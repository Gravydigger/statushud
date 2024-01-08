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
            this.renderer = new StatusHudWetRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);

            this.active = false;
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
            float wetness = this.system.capi.World.Player.Entity.WatchedAttributes.GetFloat("wetness");

            if (wetness > 0)
            {
                this.renderer.setText((int)Math.Round(wetness * 100f, 0) + "%");

                this.active = true;
            }
            else
            {
                if (this.active)
                {
                    // Only set text once.
                    this.renderer.setText("");
                }
                this.active = false;
            }
        }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudWetRenderer : StatusHudRenderer
    {
        protected StatusHudWetElement element;

        protected StatusHudText text;

        public StatusHudWetRenderer(StatusHudSystem system, int slot, StatusHudWetElement element, StatusHudTextConfig config) : base(system, slot)
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
            if (!this.element.active)
            {
                if (this.system.showHidden)
                {
                    this.renderHidden(this.system.textures.wet.TextureId);
                }
                return;
            }

            this.system.capi.Render.RenderTexture(this.system.textures.wet.TextureId, this.x, this.y, this.w, this.h);
        }

        public override void Dispose()
        {
            base.Dispose();
            this.text.Dispose();
        }
    }
}