using Vintagestory.API.Client;

namespace StatusHud
{
    public class StatusHudPlayersElement : StatusHudElement
    {
        public new const string name = "players";
        public new const string desc = "The 'players' element displays the number of players currently online.";
        protected const string textKey = "shud-players";

        public override string elementName => name;

        protected StatusHudPlayersRenderer renderer;

        public StatusHudPlayersElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            renderer = new StatusHudPlayersRenderer(system, slot, this, config);
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
            renderer.setText(system.capi.World.AllOnlinePlayers.Length.ToString());
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

        protected StatusHudText text;

        public StatusHudPlayersRenderer(StatusHudSystem system, int slot, StatusHudPlayersElement element, StatusHudTextConfig config) : base(system, slot)
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
            system.capi.Render.RenderTexture(system.textures.texturesDict["players"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}