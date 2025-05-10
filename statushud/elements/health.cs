using System;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;

namespace StatusHud
{
    public class StatusHudHealthElement : StatusHudElement
    {
        public new const string name = "health";
        protected const string textKey = "shud-health";

        public override string ElementName => name;

        protected StatusHudHealthRenderer renderer;

        private ITreeAttribute healthTree;

        public StatusHudHealthElement(StatusHudSystem system, StatusHudConfig config) : base(system)
        {
            renderer = new StatusHudHealthRenderer(system, this, config);
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
            if (healthTree == null){
                try
                {
                    healthTree = system.capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute("health");
                }
                catch (NullReferenceException)
                {
                    return;
                }
            }
            
            // This doesn't always update correctly when taking injuries from metal spikes for example
            // rejoining the world will re-sync the value
            float curr = healthTree.GetFloat("currenthealth");
            float max = healthTree.GetFloat("maxhealth");

            renderer.SetText(Math.Round(curr, 1) + "/" + Math.Round(max, 1));
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudHealthRenderer : StatusHudRenderer
    {
        protected StatusHudHealthElement element;

        protected StatusHudText text;

        public StatusHudHealthRenderer(StatusHudSystem system, StatusHudHealthElement element, StatusHudConfig config) : base(system)
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
            system.capi.Render.RenderTexture(system.textures.texturesDict["health"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}