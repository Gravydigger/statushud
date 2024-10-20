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
        private static readonly int maxPing = 999;
        private bool noRenderText;
        private ClientPlayer player;

        protected StatusHudPingRenderer renderer;

        public StatusHudPingElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot, true)
        {
            renderer = new StatusHudPingRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

#if DEBUG
            active = true;
#else
            this.active = this.system.capi.IsSinglePlayer ? false : true;
#endif
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
            if (!active)
            {
                // render text only once
                if (!noRenderText)
                {
                    renderer.setText("");
                    noRenderText = false;
                }
                return;
            }

            if (player == null)
            {
                IPlayer[] players = system.capi.World.AllOnlinePlayers;

                // Get the mod users player object
                player = (ClientPlayer)players.FirstOrDefault(player => player.PlayerUID == system.UUID);
                renderer.setText("");

                return;
            }

            int ping = (int)(player.Ping * 1000f);
            string msg = ping < maxPing ? string.Format("{0}", Math.Min(ping, maxPing)) : "+" + maxPing;

            renderer.setText(msg);
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudPingRenderer : StatusHudRenderer
    {
        protected StatusHudPingElement element;

        protected StatusHudText text;

        public StatusHudPingRenderer(StatusHudSystem system, int slot, StatusHudPingElement element, StatusHudTextConfig config) : base(system, slot)
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
            if (!element.active)
            {
                if (system.showHidden)
                {
                    this.renderHidden(system.textures.texturesDict["network"].TextureId);
                }
                return;
            }

            system.capi.Render.RenderTexture(system.textures.texturesDict["network"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}