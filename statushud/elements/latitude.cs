using System;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudLatitudeElement : StatusHudElement
    {
        public new const string name = "latitude";
        public new const string desc = "The 'latitude' element displays the player's current latitude (in degrees).";
        protected const string textKey = "shud-latitude";

        public override string elementName => name;

        protected WeatherSystemBase weatherSystem;
        protected StatusHudLatitudeRenderer renderer;

        public float needleOffset;

        public StatusHudLatitudeElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            this.weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            this.renderer = new StatusHudLatitudeRenderer(this.system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);

            this.needleOffset = 0;
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
            double latitude = this.system.capi.World.Calendar.OnGetLatitude(this.system.capi.World.Player.Entity.Pos.Z);
            this.renderer.setText((float)((int)Math.Round(latitude * 900, 0) / 10f) + "°");
            this.needleOffset = (float)(-latitude * (this.system.textures.size / 2f) * 0.75f);
        }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudLatitudeRenderer : StatusHudRenderer
    {
        protected StatusHudLatitudeElement element;
        protected StatusHudText text;

        public StatusHudLatitudeRenderer(StatusHudSystem system, int slot, StatusHudLatitudeElement element, StatusHudTextConfig config) : base(system, slot)
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
            this.system.capi.Render.RenderTexture(this.system.textures.latitude.TextureId, this.x, this.y, this.w, this.h);
            this.system.capi.Render.RenderTexture(this.system.textures.latitudeNeedle.TextureId, this.x, this.y + GuiElement.scaled(this.element.needleOffset), this.w, this.h);
        }

        public override void Dispose()
        {
            this.text.Dispose();
        }
    }
}