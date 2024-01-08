using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace StatusHud
{
    public class StatusHudPingElement : StatusHudElement
    {
        public new const string name = "ping";
        public new const string desc = "The 'ping' element displays your current ping to the server.";
        protected const string textKey = "shud-ping";

        public override string elementName => name;

        public bool active;
        private bool noRenderText;
        private string uuid;
        private ClientPlayer player;

        protected StatusHudPingRenderer renderer;

        public StatusHudPingElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            this.renderer = new StatusHudPingRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);

            this.uuid = system.UUID;
            this.player = null;

#if DEBUG
            this.active = true;
#else
            this.active = this.system.capi.IsSinglePlayer ? false : true;
#endif
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
            if (!this.active)
            {
                // render text only once
                if (!this.noRenderText)
                {
                    this.renderer.setText("");
                    this.noRenderText = false;
                }
                return;
            }

            if (this.player == null)
            {
                IPlayer[] players = this.system.capi.World.AllOnlinePlayers;

                // Get the mod users player object
                this.player = (ClientPlayer)players.FirstOrDefault(player => player.PlayerUID == this.uuid);
                this.renderer.setText("");

                return;
            }

#if DEBUG
            IPlayer[] tplayers = this.system.capi.World.AllOnlinePlayers;
            ClientPlayer tplayer = (ClientPlayer)tplayers.FirstOrDefault(player => player.PlayerUID == this.uuid);
#endif

            int ping = (int)player.Ping * 1000;
            string msg = string.Format("{0}", Math.Min(ping, 9999));

            this.renderer.setText(msg);
        }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudPingRenderer : StatusHudRenderer
    {
        protected StatusHudPingElement element;

        protected StatusHudText text;

        public StatusHudPingRenderer(StatusHudSystem system, int slot, StatusHudPingElement element, StatusHudTextConfig config) : base(system, slot)
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
            if (!this.element.active)
            {
                if (this.system.showHidden)
                {
                    this.renderHidden(this.system.textures.network.TextureId);
                }
                return;
            }

            this.system.capi.Render.RenderTexture(this.system.textures.network.TextureId, this.x, this.y, this.w, this.h);
        }

        public override void Dispose()
        {
            this.text.Dispose();
        }
    }
}