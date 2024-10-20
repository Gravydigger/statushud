using System;
using Vintagestory.API.Client;

namespace StatusHud
{
    public class StatusHudWetElement : StatusHudElement
    {
        public new const string name = "wet";
        public new const string desc = "The 'wet' element displays how wet (in %) the player is. If the player is dry, it is hidden.";
        protected const string textKey = "shud-wet";

        public override string elementName => name;

        public bool active;

        protected StatusHudWetRenderer renderer;

        public StatusHudWetElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            renderer = new StatusHudWetRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            active = false;
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
            float wetness = system.capi.World.Player.Entity.WatchedAttributes.GetFloat("wetness");

            if (wetness > 0)
            {
                renderer.setText((int)Math.Round(wetness * 100f, 0) + "%");

                active = true;
            }
            else
            {
                if (active)
                {
                    // Only set text once.
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

    public class StatusHudWetRenderer : StatusHudRenderer
    {
        protected StatusHudWetElement element;

        protected StatusHudText text;

        public StatusHudWetRenderer(StatusHudSystem system, int slot, StatusHudWetElement element, StatusHudTextConfig config) : base(system, slot)
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

        protected override void Update()
        {
            base.Update();
            text.Pos(pos);
        }

        protected override void Render()
        {
            if (!element.active)
            {
                if (system.showHidden)
                {
                    this.RenderHidden(system.textures.texturesDict["wet"].TextureId);
                }
                return;
            }

            system.capi.Render.RenderTexture(system.textures.texturesDict["wet"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}