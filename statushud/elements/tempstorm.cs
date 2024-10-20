using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudTempstormElement : StatusHudElement
    {
        public new const string name = "tempstorm";
        public new const string desc = "The 'tempstorm' element displays a timer when a temporal storm is approaching or in progress. Otherwise, it is hidden.";
        protected const string textKey = "shud-tempstorm";
        protected const string harmonyId = "shud-tempstorm";

        public override string elementName => name;

        // Hard-coded values from SystemTemporalStability.
        protected const double approachingThreshold = 0.35;
        protected const double imminentThreshold = 0.02;
        protected const double waningThreshold = 0.02;

        public bool active;
        public int textureId;

        protected SystemTemporalStability stabilitySystem;
        protected StatusHudTempstormRenderer renderer;
        protected Harmony harmony;

        protected static TemporalStormRunTimeData data;

        public StatusHudTempstormElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            stabilitySystem = this.system.capi.ModLoader.GetModSystem<SystemTemporalStability>();

            renderer = new StatusHudTempstormRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            active = false;
            textureId = this.system.textures.texturesDict["empty"].TextureId;

            if (stabilitySystem != null)
            {
                harmony = new Harmony(harmonyId);

                harmony.Patch(typeof(SystemTemporalStability).GetMethod("onServerData", BindingFlags.Instance | BindingFlags.NonPublic),
                        postfix: new HarmonyMethod(typeof(StatusHudTempstormElement).GetMethod(nameof(StatusHudTempstormElement.receiveData))));
            }
        }

        public static void receiveData(TemporalStormRunTimeData data)
        {
            StatusHudTempstormElement.data = data;
        }

        public override StatusHudRenderer getRenderer()
        {
            return renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            if (stabilitySystem == null)
            {
                return;
            }

            if (StatusHudTempstormElement.data == null)
            {
                return;
            }

            double nextStormDaysLeft = StatusHudTempstormElement.data.nextStormTotalDays - system.capi.World.Calendar.TotalDays;

            if (nextStormDaysLeft > 0 && nextStormDaysLeft < approachingThreshold)
            {
                // Preparing.
                float hoursLeft = (float)((StatusHudTempstormElement.data.nextStormTotalDays - system.capi.World.Calendar.TotalDays) * system.capi.World.Calendar.HoursPerDay);
                float approachingHours = (float)(approachingThreshold * system.capi.World.Calendar.HoursPerDay);

                active = true;
                textureId = system.textures.texturesDict["tempstorm_incoming"].TextureId;

                TimeSpan ts = TimeSpan.FromHours(Math.Max(hoursLeft, 0));
                renderer.setText(ts.ToString("h':'mm"));
            }
            else
            {
                // In progress.
                if (StatusHudTempstormElement.data.nowStormActive)
                {
                    // Active.
                    double hoursLeft = (StatusHudTempstormElement.data.stormActiveTotalDays - system.capi.World.Calendar.TotalDays) * system.capi.World.Calendar.HoursPerDay;

                    active = true;
                    textureId = system.textures.texturesDict["tempstorm_duration"].TextureId;

                    TimeSpan ts = TimeSpan.FromHours(Math.Max(hoursLeft, 0));
                    renderer.setText(ts.ToString("h':'mm"));
                }
                else if (active)
                {
                    // Ending.
                    active = false;
                    textureId = system.textures.texturesDict["empty"].TextureId;

                    renderer.setText("");
                }
            }
        }

        public override void Dispose()
        {
            harmony.UnpatchAll(harmonyId);

            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudTempstormRenderer : StatusHudRenderer
    {
        protected StatusHudTempstormElement element;

        protected StatusHudText text;

        public StatusHudTempstormRenderer(StatusHudSystem system, int slot, StatusHudTempstormElement element, StatusHudTextConfig config) : base(system, slot)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
        }

        public override void Reload(StatusHudTextConfig config)
        {
            text.ReloadText(config, pos);
        }

        public void setText(string value)
        {
            text.Set(value);
        }

        protected override void Update()
        {
            base.Update();
            text.Pos(pos);
        }

        protected override void Render()
        {
            if (!element.active)
            {
                if (system.ShowHidden)
                {
                    this.RenderHidden(system.textures.texturesDict["tempstorm_incoming"].TextureId);
                }
                return;
            }

            system.capi.Render.RenderTexture(element.textureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}