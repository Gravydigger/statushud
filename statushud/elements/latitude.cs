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
            weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            renderer = new StatusHudLatitudeRenderer(this.system, slot, this, config);
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
            double latitude = system.capi.World.Calendar.OnGetLatitude(system.capi.World.Player.Entity.Pos.Z);
            renderer.setText((float)((int)Math.Round(latitude * 900, 0) / 10f) + "°");
            needleOffset = (float)(-latitude * (system.textures.size / 2f) * 0.75f);
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudLatitudeRenderer : StatusHudRenderer
    {
        protected StatusHudLatitudeElement element;
        protected StatusHudText text;

        public StatusHudLatitudeRenderer(StatusHudSystem system, int slot, StatusHudLatitudeElement element, StatusHudTextConfig config) : base(system, slot)
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

        protected override void update()
        {
            base.update();
            text.Pos(pos);
        }

        protected override void render()
        {
            system.capi.Render.RenderTexture(system.textures.texturesDict["latitude"].TextureId, x, y, w, h);
            system.capi.Render.RenderTexture(system.textures.texturesDict["latitude_needle"].TextureId, x, y + GuiElement.scaled(element.needleOffset), w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}