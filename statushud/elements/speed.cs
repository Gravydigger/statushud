using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;

namespace StatusHud
{
    public class StatusHudSpeedElement : StatusHudElement
    {
        public new const string name = "speed";
        public new const string desc = "The 'speed' element displays the player's current speed (in m/s).";
        protected const string textKey = "shud-speed";

        public override string ElementName => name;

        protected StatusHudSpeedRenderer renderer;

        public StatusHudSpeedElement(StatusHudSystem system, StatusHudConfig config) : base(system)
        {
            renderer = new StatusHudSpeedRenderer(system, this, config);
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

        protected StatusHudText text;

        public StatusHudSpeedRenderer(StatusHudSystem system, StatusHudSpeedElement element, StatusHudConfig config) : base(system)
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
            Entity mount = system.capi.World.Player.Entity.MountedOn?.MountSupplier as Entity;
            if (mount != null)
            {
                text.Set(((int)Math.Round(mount.Pos.Motion.Length() * 1000, 0) / 10f).ToString());
            }
            else
            {
                text.Set(((int)Math.Round(system.capi.World.Player.Entity.Pos.Motion.Length() * 1000) / 10f).ToString());
            }
            system.capi.Render.RenderTexture(system.textures.texturesDict["speed"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}