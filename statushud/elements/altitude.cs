using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudAltitudeElement : StatusHudElement
    {
        public new const string name = "altitude";
        protected const string textKey = "shud-altitude";

        public override string ElementName => name;

        protected WeatherSystemBase weatherSystem;
        protected StatusHudAltitudeRenderer renderer;

        public float needleOffset;

        public StatusHudAltitudeElement(StatusHudSystem system, StatusHudConfig config) : base(system)
        {
            weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            renderer = new StatusHudAltitudeRenderer(this.system, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            needleOffset = 0;
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
            float altitude = (int)Math.Round(system.capi.World.Player.Entity.Pos.Y - system.capi.World.SeaLevel, 0);
            renderer.SetText(altitude.ToString());

            float ratio = -(altitude / (system.capi.World.BlockAccessor.MapSizeY / 2));
            needleOffset = GameMath.Clamp(ratio, -1, 1) * (system.Config.iconSize / 2f) * 0.75f;
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

        public StatusHudAltitudeRenderer(StatusHudSystem system, StatusHudAltitudeElement element, StatusHudConfig config) : base(system)
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