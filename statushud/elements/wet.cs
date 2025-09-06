using System;
using Vintagestory.API.Client;

namespace StatusHud
{
    public class StatusHudWetElement : StatusHudElement
    {
        public new const string name = "wet";
        protected const string textKey = "shud-wet";

        public override string ElementName => name;

        public bool active;

        protected StatusHudWetRenderer renderer;

        public StatusHudWetElement(StatusHudSystem system) : base(system)
        {
            renderer = new StatusHudWetRenderer(system, this);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            active = false;
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
            float wetness = system.capi.World.Player.Entity.WatchedAttributes.GetFloat("wetness");

            if (wetness > 0)
            {
                renderer.SetText((int)Math.Round(wetness * 100f, 0) + "%");

                active = true;
            }
            else
            {
                if (active)
                {
                    // Only set text once.
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

    public class StatusHudWetRenderer : StatusHudRenderer
    {
        protected StatusHudWetElement element;

        public StatusHudWetRenderer(StatusHudSystem system, StatusHudWetElement element) : base(system)
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
                    this.RenderHidden(System.textures.texturesDict["wet"].TextureId);
                }
                return;
            }

            System.capi.Render.RenderTexture(System.textures.texturesDict["wet"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            Text.Dispose();
        }
    }
}