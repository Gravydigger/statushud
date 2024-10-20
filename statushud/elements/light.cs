using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace StatusHud
{
    public class StatusHudLightElement : StatusHudElement
    {
        public new const string name = "light";
        public new const string desc = "The 'light' element displays the selected block's light level. If no block is selected, it is hidden.";
        protected const string textKey = "shud-light";

        public override string elementName => name;

        public bool active;

        protected StatusHudLightRenderer renderer;

        public StatusHudLightElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot, true)
        {
            renderer = new StatusHudLightRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
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
            if (system.capi.World.Player.CurrentBlockSelection != null)
            {
                renderer.setText(system.capi.World.BlockAccessor.GetLightLevel(system.capi.World.Player.CurrentBlockSelection.Position, EnumLightLevelType.MaxTimeOfDayLight).ToString());
                active = true;
            }
            else
            {
                if (active)
                {
                    renderer.setText("");
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

        protected StatusHudText text;

        public StatusHudLightRenderer(StatusHudSystem system, int slot, StatusHudLightElement element, StatusHudTextConfig config) : base(system, slot)
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
            if (!element.active)
            {
                if (system.showHidden)
                {
                    this.renderHidden(system.textures.texturesDict["light"].TextureId);
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