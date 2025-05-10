using System;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;

namespace StatusHud
{
    public class StatusHudHungerElement : StatusHudElement
    {
        public new const string name = "hunger";
        protected const string textKey = "shud-hunger";

        public override string ElementName => name;

        protected StatusHudHungerRenderer renderer;

        private ITreeAttribute hungerTree;

        public StatusHudHungerElement(StatusHudSystem system, StatusHudConfig config) : base(system)
        {
            renderer = new StatusHudHungerRenderer(system, this, config);
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
            if (hungerTree == null){
                try
                {
                    hungerTree = system.capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute("hunger");
                }
                catch (NullReferenceException)
                {
                    // likely the player has yet to have been fully loaded
                    return;
                }
            }

            float curr = hungerTree.GetFloat("currentsaturation");
            renderer.SetText(Math.Round(curr, 1).ToString());
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudHungerRenderer : StatusHudRenderer
    {
        protected StatusHudHungerElement element;

        protected StatusHudText text;

        public StatusHudHungerRenderer(StatusHudSystem system, StatusHudHungerElement element, StatusHudConfig config) : base(system)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.element.GetTextKey(), config);
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
            // TODO: update with proper texture
            system.capi.Render.RenderTexture(system.textures.texturesDict["wet"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}