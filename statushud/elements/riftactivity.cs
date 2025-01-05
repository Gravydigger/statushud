using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudRiftActivityElement : StatusHudElement
    {
        public new const string name = "riftactivity";
        protected const string textKey = "shud-riftactivity";
        protected const string harmonyId = "shud-riftactivity";

        public static readonly string[] riftChangeOptions = { "True", "False" };
        private string showRiftChange;

        public override string ElementName => name;
        public override string ElementOption => showRiftChange;

        public int textureId;
        public bool active;

        private ModSystemRiftWeather riftSystem;
        private StatusHudRiftActivityRenderer renderer;
        private Harmony harmony;

        private static CurrentPattern riftActivityData;

        public StatusHudRiftActivityElement(StatusHudSystem system, StatusHudConfig config) : base(system)
        {
            riftSystem = this.system.capi.ModLoader.GetModSystem<ModSystemRiftWeather>();

            renderer = new StatusHudRiftActivityRenderer(system, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            // When a player first activates this element when already in a world, the element hasn't gotten the rift data yet.
            // Until the player gets rift data, show the element with the unknown icon
            textureId = this.system.textures.texturesDict["rift_unknown"].TextureId;

            active = this.system.capi.World.Config.GetString("temporalRifts") != "off";

            showRiftChange = "false";

            // World has to be reloaded for changes to apply
            harmony = new Harmony(harmonyId);
            harmony.Patch(typeof(ModSystemRiftWeather).GetMethod("onPacket", BindingFlags.Instance | BindingFlags.NonPublic),
                    postfix: new HarmonyMethod(typeof(StatusHudRiftActivityElement).GetMethod(nameof(ReceiveData))));

            if (!active)
            {
                Dispose();
            }
        }

        public static void ReceiveData(SpawnPatternPacket msg)
        {
            riftActivityData = msg.Pattern;
        }

        public override StatusHudRenderer GetRenderer()
        {
            return renderer;
        }

        public virtual string GetTextKey()
        {
            return textKey;
        }

        public override void ConfigOptions(string value)
        {
            if (value.ToLower().ToBool())
            {
                showRiftChange = "True";
            }
            else
            {
                showRiftChange = "False";
            }
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

            if (showRiftChange.ToLower().ToBool())
            {
                double hours = system.capi.World.Calendar.TotalHours;
                double nextRiftChange = Math.Max(riftActivityData.UntilTotalHours - hours, 0);

                TimeSpan ts = TimeSpan.FromHours(nextRiftChange);
                string text = (int)nextRiftChange + ":" + ts.ToString("mm");

                renderer.SetText(text);
            }
            else
            {
                renderer.SetText("");
            }

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

    public class StatusHudRiftActivityRenderer : StatusHudRenderer
    {
        protected StatusHudRiftActivityElement element;

        protected StatusHudText text;

        public StatusHudRiftActivityRenderer(StatusHudSystem system, StatusHudRiftActivityElement element, StatusHudConfig config) : base(system)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.element.GetTextKey(), config);
        }

        public override void Reload()
        {
            text.ReloadText(pos);
        }

        public void SetText(string value)
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
            if (element.active)
            {
                system.capi.Render.RenderTexture(element.textureId, x, y, w, h);
            }
            else if (system.ShowHidden)
            {
                RenderHidden(system.textures.texturesDict["rift_calm"].TextureId);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}