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

        public StatusHudRiftActivityElement(StatusHudSystem system, StatusHudConfig config) : base(system)
        {
            riftSystem = this.system.capi.ModLoader.GetModSystem<ModSystemRiftWeather>();


            renderer = new StatusHudRiftAvtivityRenderer(system, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            textureId = this.system.textures.texturesDict["empty"].TextureId;

            active = this.system.capi.World.Config.GetString("temporalRifts") != "off" ? true : false;

            // World has to be reloaded for changes to apply
            harmony = new Harmony(harmonyId);
            harmony.Patch(typeof(ModSystemRiftWeather).GetMethod("onPacket", BindingFlags.Instance | BindingFlags.NonPublic),
                    postfix: new HarmonyMethod(typeof(StatusHudRiftActivityElement).GetMethod(nameof(StatusHudRiftActivityElement.receiveData))));

            if (!active)
            {
                Dispose();
            }
        }

        public static void receiveData(SpawnPatternPacket msg)
        {
            riftActivityData = msg.Pattern;
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
            if (!active)
            {
                return;
            }

            if (riftSystem == null || riftActivityData == null)
            {
                return;
            }

            double hours = system.capi.World.Calendar.TotalHours;
            double nextRiftChange = Math.Max(riftActivityData.UntilTotalHours - hours, 0);

            TimeSpan ts = TimeSpan.FromHours(nextRiftChange);
            string text = (int)nextRiftChange + ":" + ts.ToString("mm");

            renderer.setText(text);
            updateTexture(riftActivityData.Code);
        }

        public override void Dispose()
        {
            harmony.UnpatchAll(harmonyId);

            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }

        protected void updateTexture(string activity)
        {
            try
            {
                textureId = system.textures.texturesDict["rift_" + activity].TextureId;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                system.capi.Logger.Error("For {0} element, texture rift_{1} is not valid", name, activity);
                throw;
            }
        }
    }

    public class StatusHudRiftAvtivityRenderer : StatusHudRenderer
    {
        protected StatusHudRiftActivityElement element;

        protected StatusHudText text;

        public StatusHudRiftAvtivityRenderer(StatusHudSystem system, StatusHudRiftActivityElement element, StatusHudConfig config) : base(system)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.element.getTextKey(), config);
        }

                public override void Reload()
        {
            text.ReloadText(pos);
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
                    this.RenderHidden(system.textures.texturesDict["rift_calm"].TextureId);
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