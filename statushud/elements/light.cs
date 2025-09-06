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
            if (!element.active)
            {
                if (System.ShowHidden)
                {
                    RenderHidden(System.textures.texturesDict["light"].TextureId);
                }
                return;
            }

            System.capi.Render.RenderTexture(System.textures.texturesDict["light"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            Text.Dispose();
        }
    }
}