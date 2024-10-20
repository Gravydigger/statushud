using System;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudStabilityElement : StatusHudElement
    {
        public new const string name = "stability";
        public new const string desc = "The 'stability' element displays the temporal stability at the player's position if it is below 100%. Otherwise, it is hidden.";
        protected const string textKey = "shud-stability";

        public override string elementName => name;

        protected const float maxStability = 1.5f;      // Hard-coded in SystemTemporalStability.

        public bool active;

        protected SystemTemporalStability stabilitySystem;
        protected StatusHudStabilityRenderer renderer;

        public StatusHudStabilityElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            stabilitySystem = this.system.capi.ModLoader.GetModSystem<SystemTemporalStability>();

            renderer = new StatusHudStabilityRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            active = false;
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
            if (stabilitySystem == null)
            {
                return;
            }

            float stability = stabilitySystem.GetTemporalStability(system.capi.World.Player.Entity.Pos.AsBlockPos);

            if (stability < maxStability)
            {
                renderer.setText((int)Math.Floor(stability * 100) + "%");
                active = true;
            }
            else
            {
                if (active)
                {
                    // Only set text once.
                    renderer.setText("");
                }
                active = false;
            }
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudStabilityRenderer : StatusHudRenderer
    {
        protected StatusHudStabilityElement element;

        protected StatusHudText text;

        public StatusHudStabilityRenderer(StatusHudSystem system, int slot, StatusHudStabilityElement element, StatusHudTextConfig config) : base(system, slot)
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
                    this.renderHidden(system.textures.texturesDict["stability"].TextureId);
                }
                return;
            }

            system.capi.Render.RenderTexture(system.textures.texturesDict["stability"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}