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
            this.stabilitySystem = this.system.capi.ModLoader.GetModSystem<SystemTemporalStability>();

            this.renderer = new StatusHudTempstormRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);

            this.active = false;
            this.textureId = this.system.textures.empty.TextureId;

            if (this.stabilitySystem != null)
            {
                this.harmony = new Harmony(harmonyId);

                this.harmony.Patch(typeof(SystemTemporalStability).GetMethod("onServerData", BindingFlags.Instance | BindingFlags.NonPublic),
                        postfix: new HarmonyMethod(typeof(StatusHudTempstormElement).GetMethod(nameof(StatusHudTempstormElement.receiveData))));
            }
        }

        public static void receiveData(TemporalStormRunTimeData data)
        {
            StatusHudTempstormElement.data = data;
        }

        protected override StatusHudRenderer getRenderer()
        {
            return this.renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            if (this.stabilitySystem == null)
            {
                return;
            }

            if (StatusHudTempstormElement.data == null)
            {
                return;
            }

            double nextStormDaysLeft = StatusHudTempstormElement.data.nextStormTotalDays - this.system.capi.World.Calendar.TotalDays;

            if (nextStormDaysLeft > 0 && nextStormDaysLeft < approachingThreshold)
            {
                // Preparing.
                float hoursLeft = (float)((StatusHudTempstormElement.data.nextStormTotalDays - this.system.capi.World.Calendar.TotalDays) * this.system.capi.World.Calendar.HoursPerDay);
                float approachingHours = (float)(approachingThreshold * this.system.capi.World.Calendar.HoursPerDay);

                this.active = true;
                this.textureId = this.system.textures.tempstormIncoming.TextureId;

                TimeSpan ts = TimeSpan.FromHours(Math.Max(hoursLeft, 0));
                this.renderer.setText(ts.ToString("h':'mm"));
            }
            else
            {
                // In progress.
                if (StatusHudTempstormElement.data.nowStormActive)
                {
                    // Active.
                    double hoursLeft = (StatusHudTempstormElement.data.stormActiveTotalDays - this.system.capi.World.Calendar.TotalDays) * this.system.capi.World.Calendar.HoursPerDay;

                    this.active = true;
                    this.textureId = this.system.textures.tempstormDuration.TextureId;

                    TimeSpan ts = TimeSpan.FromHours(Math.Max(hoursLeft, 0));
                    this.renderer.setText(ts.ToString("h':'mm"));
                }
                else if (this.active)
                {
                    // Ending.
                    this.active = false;
                    this.textureId = this.system.textures.empty.TextureId;

                    this.renderer.setText("");
                }
            }
        }

        public override void Dispose()
        {
            this.harmony.UnpatchAll(harmonyId);

            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudTempstormRenderer : StatusHudRenderer
    {
        protected StatusHudTempstormElement element;

        protected StatusHudText text;

        public StatusHudTempstormRenderer(StatusHudSystem system, int slot, StatusHudTempstormElement element, StatusHudTextConfig config) : base(system, slot)
        {
            this.element = element;

            this.text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
        }

        public void setText(string value)
        {
            this.text.Set(value);
        }

        protected override void update()
        {
            base.update();
            this.text.Pos(this.pos);
        }

        protected override void render()
        {
            if (!this.element.active)
            {
                if (this.system.showHidden)
                {
                    this.renderHidden(this.system.textures.tempstormIncoming.TextureId);
                }
                return;
            }

            this.system.capi.Render.RenderTexture(this.element.textureId, this.x, this.y, this.w, this.h);
        }

        public override void Dispose()
        {
            this.text.Dispose();
        }
    }
}