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
            System.capi.Render.RenderTexture(System.textures.texturesDict["players"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            Text.Dispose();
        }
    }
}