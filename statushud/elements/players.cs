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
            this.renderer = new StatusHudPlayersRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }

        protected override StatusHudRenderer getRenderer()
        {
            return this.renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            this.renderer.setText(this.system.capi.World.AllOnlinePlayers.Length.ToString());
        }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudPlayersRenderer : StatusHudRenderer
    {
        protected StatusHudPlayersElement element;

        protected StatusHudText text;

        public StatusHudPlayersRenderer(StatusHudSystem system, int slot, StatusHudPlayersElement element, StatusHudTextConfig config) : base(system, slot)
        {
            this.element = element;

            this.text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
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
            this.system.capi.Render.RenderTexture(this.system.textures.players.TextureId, this.x, this.y, this.w, this.h);
        }

        public override void Dispose()
        {
            this.text.Dispose();
        }
    }
}