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

        public override string elementName => name;

        protected StatusHudSpeedRenderer renderer;

        public StatusHudSpeedElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            this.renderer = new StatusHudSpeedRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }

        public override StatusHudRenderer getRenderer()
        {
            return this.renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
        }

        public override void Tick() { }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudSpeedRenderer : StatusHudRenderer
    {
        protected StatusHudSpeedElement element;

        protected StatusHudText text;

        public StatusHudSpeedRenderer(StatusHudSystem system, int slot, StatusHudSpeedElement element, StatusHudTextConfig config) : base(system, slot)
        {
            this.element = element;

            this.text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
        }

        public override void Reload(StatusHudTextConfig config)
        {
            this.text.ReloadText(config, this.pos);
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
            Entity mount = this.system.capi.World.Player.Entity.MountedOn?.MountSupplier as Entity;
            if (mount != null)
            {
                this.text.Set(((int)Math.Round(mount.Pos.Motion.Length() * 1000, 0) / 10f).ToString());
            }
            else
            {
                this.text.Set(((int)Math.Round(this.system.capi.World.Player.Entity.Pos.Motion.Length() * 1000) / 10f).ToString());
            }
            this.system.capi.Render.RenderTexture(this.system.textures.speed.TextureId, this.x, this.y, this.w, this.h);
        }

        public override void Dispose()
        {
            this.text.Dispose();
        }
    }
}