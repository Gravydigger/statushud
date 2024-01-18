using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudRiftActivityElement : StatusHudElement
    {
        public new const string name = "riftactivity";
        public new const string desc = "The 'riftactivity' element displays the current rift activity.";
        protected const string textKey = "shud-riftactivity";
        protected const string harmonyId = "shud-riftactivity";

        public override string elementName => name;

        public int textureId;
        public bool active;

        protected ModSystemRiftWeather riftSystem;
        protected StatusHudRiftAvtivityRenderer renderer;
        protected Harmony harmony;

        protected static CurrentPattern riftActivityData;

        public StatusHudRiftActivityElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            this.riftSystem = this.system.capi.ModLoader.GetModSystem<ModSystemRiftWeather>();


            this.renderer = new StatusHudRiftAvtivityRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);

            this.textureId = this.system.textures.empty.TextureId;

            string riftsWorldConf = this.system.capi.World.Config.GetString("temporalRifts");

            // World has to be reloaded for changes to apply
            this.active = riftsWorldConf != "off" ? true : false;

            if (!this.active)
            {
                this.renderer.setText("");
            }

            if (this.riftSystem != null && this.active)
            {
                this.harmony = new Harmony(harmonyId);

                this.harmony.Patch(typeof(ModSystemRiftWeather).GetMethod("onPacket", BindingFlags.Instance | BindingFlags.NonPublic),
                        postfix: new HarmonyMethod(typeof(StatusHudRiftActivityElement).GetMethod(nameof(StatusHudRiftActivityElement.receiveData))));
            }
        }

        public static void receiveData(SpawnPatternPacket msg)
        {
            riftActivityData = msg.Pattern;
        }

        public override StatusHudRenderer getRenderer()
        {
            return this.renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            if (!this.active)
            {
                return;
            }

            if (this.riftSystem == null || riftActivityData == null)
            {
                return;
            }

            double hours = this.system.capi.World.Calendar.TotalHours;
            double nextRiftChange = Math.Max(riftActivityData.UntilTotalHours - hours, 0);

            TimeSpan ts = TimeSpan.FromHours(nextRiftChange);
            string text = (int)nextRiftChange + ":" + ts.ToString("mm");

            this.renderer.setText(text);
            updateTexture(riftActivityData.Code);
        }

        public override void Dispose()
        {
            this.harmony.UnpatchAll(harmonyId);

            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }

        protected void updateTexture(string activity)
        {
            switch (activity)
            {
                case "calm":
                    this.textureId = this.system.textures.riftCalm.TextureId;
                    break;
                case "low":
                    this.textureId = this.system.textures.riftLow.TextureId;
                    break;
                case "medium":
                    this.textureId = this.system.textures.riftMedium.TextureId;
                    break;
                case "high":
                    this.textureId = this.system.textures.riftHigh.TextureId;
                    break;
                case "veryhigh":
                    this.textureId = this.system.textures.riftVeryHigh.TextureId;
                    break;
                case "apocalyptic":
                    this.textureId = this.system.textures.riftApocalyptic.TextureId;
                    break;
                default:
                    break;
            }
        }
    }

    public class StatusHudRiftAvtivityRenderer : StatusHudRenderer
    {
        protected StatusHudRiftActivityElement element;

        protected StatusHudText text;

        public StatusHudRiftAvtivityRenderer(StatusHudSystem system, int slot, StatusHudRiftActivityElement element, StatusHudTextConfig config) : base(system, slot)
        {
            this.element = element;

            this.text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
        }

        public override void Reload(StatusHudTextConfig config)
        {
            this.text.ReloadText(config, this.pos);
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
                    this.renderHidden(this.system.textures.riftCalm.TextureId);
                }
                return;
            }

            this.system.capi.Render.RenderTexture(this.element.textureId, this.x, this.y, this.w, this.h);
        }

        public override void Dispose()
        {
            base.Dispose();
            this.text.Dispose();
        }
    }
}