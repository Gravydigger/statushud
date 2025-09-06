using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudTempstormElement : StatusHudElement
    {
        public new const string name = "tempstorm";
        protected const string textKey = "shud-tempstorm";
        protected const string harmonyId = "shud-tempstorm";

        public override string ElementName => name;

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
        private bool firstLoad;

        public StatusHudTempstormElement(StatusHudSystem system) : base(system)
        {
            stabilitySystem = this.system.capi.ModLoader.GetModSystem<SystemTemporalStability>();

            renderer = new StatusHudTempstormRenderer(system, this);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            active = false;
            firstLoad = true;
            textureId = this.system.textures.texturesDict["empty"].TextureId;

            if (stabilitySystem != null)
            {
                harmony = new Harmony(harmonyId);

                harmony.Patch(typeof(SystemTemporalStability).GetMethod("onServerData", BindingFlags.Instance | BindingFlags.NonPublic),
                        postfix: new HarmonyMethod(typeof(StatusHudTempstormElement).GetMethod(nameof(ReceiveData))));
            }
        }

        public static void ReceiveData(TemporalStormRunTimeData data)
        {
            StatusHudTempstormElement.data = data;
        }

        public override StatusHudRenderer GetRenderer()
        {
            return renderer;
        }

        public virtual string GetTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            if (stabilitySystem == null)
            {
                return;
            }

            if (data == null)
            {
                if (firstLoad)
                {
                    string langName = Lang.Get("statushudcont:tempstorm-name");
                    system.capi.ShowChatMessage(StatusHudSystem.PrintModName(Lang.Get($"statushudcont:harmony-nodata", langName, langName.ToLower())));
                    firstLoad = false;
                }
                return;
            }

            double nextStormDaysLeft = data.nextStormTotalDays - system.capi.World.Calendar.TotalDays;

            if (nextStormDaysLeft > 0 && nextStormDaysLeft < approachingThreshold)
            {
                // Preparing.
                float hoursLeft = (float)((data.nextStormTotalDays - system.capi.World.Calendar.TotalDays) * system.capi.World.Calendar.HoursPerDay);
                // float approachingHours = (float)(approachingThreshold * system.capi.World.Calendar.HoursPerDay);

                active = true;
                textureId = system.textures.texturesDict["tempstorm_incoming"].TextureId;

                TimeSpan ts = TimeSpan.FromHours(Math.Max(hoursLeft, 0));
                renderer.SetText(ts.ToString("h':'mm"));
            }
            else
            {
                // In progress.
                if (data.nowStormActive)
                {
                    // Active.
                    double hoursLeft = (data.stormActiveTotalDays - system.capi.World.Calendar.TotalDays) * system.capi.World.Calendar.HoursPerDay;

                    active = true;
                    textureId = system.textures.texturesDict["tempstorm_duration"].TextureId;

                    TimeSpan ts = TimeSpan.FromHours(Math.Max(hoursLeft, 0));
                    renderer.SetText(ts.ToString("h':'mm"));
                }
                else if (active)
                {
                    // Ending.
                    active = false;
                    textureId = system.textures.texturesDict["empty"].TextureId;

                    renderer.SetText("");
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

        public StatusHudTempstormRenderer(StatusHudSystem system, StatusHudTempstormElement element) : base(system)
        {
            this.element = element;
            Text = new StatusHudText(this.System.capi, this.element.GetTextKey(), system.Config);
        }

        public override void Reload()
        {
            Text.ReloadText(pos);
        }

        public void SetText(string value)
        {
            Text.Set(value);
        }

        protected override void Update()
        {
            base.Update();
            Text.Pos(pos);
        }

        protected override void Render()
        {
            if (!element.active)
            {
                if (System.ShowHidden)
                {
                    this.RenderHidden(System.textures.texturesDict["tempstorm_incoming"].TextureId);
                }
                return;
            }

            System.capi.Render.RenderTexture(element.textureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            Text.Dispose();
        }
    }
}