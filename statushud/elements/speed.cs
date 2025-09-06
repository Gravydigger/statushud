using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;

namespace StatusHud
{
    public class StatusHudSpeedElement : StatusHudElement
    {
        public new const string name = "speed";
        protected const string textKey = "shud-speed";

        public override string ElementName => name;

        protected StatusHudSpeedRenderer renderer;

        public StatusHudSpeedElement(StatusHudSystem system) : base(system)
        {
            renderer = new StatusHudSpeedRenderer(system, this);
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

        public override void Tick() { }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudSpeedRenderer : StatusHudRenderer
    {
        protected StatusHudSpeedElement element;

        public StatusHudSpeedRenderer(StatusHudSystem system, StatusHudSpeedElement element) : base(system)
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
            Entity mount = System.capi.World.Player.Entity.MountedOn?.Entity;

            if (mount != null)
            {
                Text.Set(((int)Math.Round(mount.Pos.Motion.Length() * 1000) / 10f).ToString());
            }
            else
            {
                Text.Set(((int)Math.Round(System.capi.World.Player.Entity.Pos.Motion.Length() * 1000) / 10f).ToString());
            }
            System.capi.Render.RenderTexture(System.textures.texturesDict["speed"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            Text.Dispose();
        }
    }
}