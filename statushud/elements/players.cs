using Vintagestory.API.Client;

namespace StatusHud
{
    public class StatusHudPlayersElement : StatusHudElement
    {
        public new const string name = "players";
        protected const string textKey = "shud-players";

        public override string ElementName => name;

        protected StatusHudPlayersRenderer renderer;

        public StatusHudPlayersElement(StatusHudSystem system) : base(system)
        {
            renderer = new StatusHudPlayersRenderer(system, this);
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
            renderer.SetText(system.capi.World.AllOnlinePlayers.Length.ToString());
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudPlayersRenderer : StatusHudRenderer
    {
        protected StatusHudPlayersElement element;

        public StatusHudPlayersRenderer(StatusHudSystem system, StatusHudPlayersElement element) : base(system)
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
            system.capi.Render.RenderTexture(system.textures.texturesDict["players"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}