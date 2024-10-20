using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudAltitudeElement : StatusHudElement
    {
        public new const string name = "altitude";
        public new const string desc = "The 'altitude' element displays the player's current height (in meters) in relation to sea level.";
        protected const string textKey = "shud-altitude";

        public override string elementName => name;

        protected WeatherSystemBase weatherSystem;
        protected StatusHudAltitudeRenderer renderer;

        public float needleOffset;

        public StatusHudAltitudeElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            renderer = new StatusHudAltitudeRenderer(this.system, this.slot, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            needleOffset = 0;
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
            float altitude = (int)Math.Round(system.capi.World.Player.Entity.Pos.Y - system.capi.World.SeaLevel, 0);
            renderer.SetText(altitude.ToString());

            float ratio = -(altitude / (system.capi.World.BlockAccessor.MapSizeY / 2));
            needleOffset = (float)(GameMath.Clamp(ratio, -1, 1) * (system.textures.size / 2f) * 0.75f);
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudAltitudeRenderer : StatusHudRenderer
    {
        protected StatusHudAltitudeElement element;
        protected StatusHudText text;

        public StatusHudAltitudeRenderer(StatusHudSystem system, int slot, StatusHudAltitudeElement element, StatusHudTextConfig config) : base(system, slot)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
        }

        public override void Reload(StatusHudTextConfig config)
        {
            text.ReloadText(config, pos);
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
            system.capi.Render.RenderTexture(system.textures.texturesDict["altitude"].TextureId, x, y, w, h);
            system.capi.Render.RenderTexture(system.textures.texturesDict["altitude_needle"].TextureId, x, y + GuiElement.scaled(element.needleOffset), w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}