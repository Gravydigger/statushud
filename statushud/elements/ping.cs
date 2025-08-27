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
        protected const string textKey = "shud-ping";

        public override string ElementName => name;

        private static readonly int maxPing = 999;
        private ClientPlayer player;

        protected StatusHudPingRenderer renderer;

        public StatusHudPingElement(StatusHudSystem system) : base(system, true)
        {
            renderer = new StatusHudPingRenderer(system, this);
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
            if (system.capi.IsSinglePlayer)
            {
                renderer.SetText("");
                return;
            }

            if (player == null)
            {
                IPlayer[] players = system.capi.World.AllOnlinePlayers;

                // Get the clients player object
                player = (ClientPlayer)players.FirstOrDefault(player => player.PlayerUID == system.UUID);
                renderer.SetText("");

                return;
            }

            int ping = (int)(player.Ping * 1000f);
            string msg = ping < maxPing ? string.Format("{0}", Math.Min(ping, maxPing)) : "+" + maxPing;

            renderer.SetText(msg);
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
        public StatusHudPingRenderer(StatusHudSystem system, StatusHudPingElement element) : base(system)
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
            if (system.ShowHidden && system.capi.IsSinglePlayer)
            {
                RenderHidden(system.textures.texturesDict["network"].TextureId);
            }
            else
            {
                system.capi.Render.RenderTexture(system.textures.texturesDict["network"].TextureId, x, y, w, h);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}