using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace StatusHud
{
    public class StatusHudLightElement : StatusHudElement
    {
        public new const string name = "light";
        protected const string textKey = "shud-light";

        public override string ElementName => name;

        public bool active;

        protected StatusHudLightRenderer renderer;

        public StatusHudLightElement(StatusHudSystem system) : base(system, true)
        {
            renderer = new StatusHudLightRenderer(system, this);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
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
            if (system.capi.World.Player.CurrentBlockSelection != null)
            {
                renderer.SetText(system.capi.World.BlockAccessor.GetLightLevel(system.capi.World.Player.CurrentBlockSelection.Position, EnumLightLevelType.MaxTimeOfDayLight).ToString());
                active = true;
            }
            else
            {
                if (active)
                {
                    renderer.SetText("");
                }
                active = false;
            }
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudLightRenderer : StatusHudRenderer
    {
        protected StatusHudLightElement element;

        public StatusHudLightRenderer(StatusHudSystem system, StatusHudLightElement element) : base(system)
        {
            this.element = element;
            text = new StatusHudText(this.system.capi, this.element.GetTextKey(), system.Config);
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
            if (!element.active)
            {
                if (system.ShowHidden)
                {
                    RenderHidden(system.textures.texturesDict["light"].TextureId);
                }
                return;
            }

            system.capi.Render.RenderTexture(system.textures.texturesDict["light"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}