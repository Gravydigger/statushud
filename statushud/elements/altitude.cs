using ConfigLib;
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
            this.weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            this.renderer = new StatusHudAltitudeRenderer(this.system, this.slot, this, config);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);

            this.needleOffset = 0;
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
            float altitude = (int)Math.Round(this.system.capi.World.Player.Entity.Pos.Y - this.system.capi.World.SeaLevel, 0);
            this.renderer.setText(altitude.ToString());

            float ratio = -(altitude / (this.system.capi.World.BlockAccessor.MapSizeY / 2));
            this.needleOffset = (float)(GameMath.Clamp(ratio, -1, 1) * (this.system.textures.size / 2f) * 0.75f);
        }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudAltitudeRenderer : StatusHudRenderer
    {
        protected StatusHudAltitudeElement element;
        protected StatusHudText text;

        public StatusHudAltitudeRenderer(StatusHudSystem system, int slot, StatusHudAltitudeElement element, StatusHudTextConfig config) : base(system, slot)
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
            this.system.capi.Render.RenderTexture(this.system.textures.altitude.TextureId, this.x, this.y, this.w, this.h);
            this.system.capi.Render.RenderTexture(this.system.textures.altitudeNeedle.TextureId, this.x, this.y + GuiElement.scaled(this.element.needleOffset), this.w, this.h);
        }

        public override void Dispose()
        {
            this.text.Dispose();
        }
    }
}