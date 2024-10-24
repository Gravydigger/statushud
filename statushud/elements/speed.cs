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
            renderer = new StatusHudSpeedRenderer(system, slot, this, config);
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

        public StatusHudSpeedRenderer(StatusHudSystem system, int slot, StatusHudSpeedElement element, StatusHudTextConfig config) : base(system, slot)
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