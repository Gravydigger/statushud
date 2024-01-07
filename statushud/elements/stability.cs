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

        protected const float maxStability = 1.5f;      // Hard-coded in SystemTemporalStability.

        public bool active;

        protected SystemTemporalStability stabilitySystem;
        protected StatusHudStabilityRenderer renderer;

        public StatusHudStabilityElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            this.stabilitySystem = this.system.capi.ModLoader.GetModSystem<SystemTemporalStability>();

            this.renderer = new StatusHudStabilityRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);

            this.active = false;
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
            if (this.stabilitySystem == null)
            {
                return;
            }

            float stability = this.stabilitySystem.GetTemporalStability(this.system.capi.World.Player.Entity.Pos.AsBlockPos);

            if (stability < maxStability)
            {
                this.renderer.setText((int)Math.Floor(stability * 100) + "%");
                this.active = true;
            }
            else
            {
                if (this.active)
                {
                    // Only set text once.
                    this.renderer.setText("");
                }
                this.active = false;
            }
        }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudStabilityRenderer : StatusHudRenderer
    {
        protected StatusHudStabilityElement element;

        protected StatusHudText text;

        public StatusHudStabilityRenderer(StatusHudSystem system, int slot, StatusHudStabilityElement element, StatusHudTextConfig config) : base(system, slot)
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
                    this.renderHidden(this.system.textures.stability.TextureId);
                }
                return;
            }

            this.system.capi.Render.RenderTexture(this.system.textures.stability.TextureId, this.x, this.y, this.w, this.h);
        }

        public override void Dispose()
        {
            this.text.Dispose();
        }
    }
}