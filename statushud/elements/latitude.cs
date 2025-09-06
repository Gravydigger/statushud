using System;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudLatitudeElement : StatusHudElement
    {
        public new const string name = "latitude";
        protected const string textKey = "shud-latitude";

        public override string ElementName => name;

        protected WeatherSystemBase weatherSystem;
        protected StatusHudLatitudeRenderer renderer;

        public float needleOffset;

        public StatusHudLatitudeElement(StatusHudSystem system) : base(system)
        {
            weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            renderer = new StatusHudLatitudeRenderer(this.system, this);
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
            double latitude = system.capi.World.Calendar.OnGetLatitude(system.capi.World.Player.Entity.Pos.Z);
            renderer.SetText((int)Math.Round(latitude * 900, 0) / 10f + "°");
            needleOffset = (float)(-latitude * (StatusHudSystem.iconSize * system.Config.elementScale / 2f) * 0.75f);
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

        public StatusHudLatitudeRenderer(StatusHudSystem system, StatusHudLatitudeElement element) : base(system)
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
            System.capi.Render.RenderTexture(System.textures.texturesDict["latitude"].TextureId, x, y, w, h);
            System.capi.Render.RenderTexture(System.textures.texturesDict["latitude_needle"].TextureId, x, y + GuiElement.scaled(element.needleOffset), w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            Text.Dispose();
        }
    }
}