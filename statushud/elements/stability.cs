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

        public StatusHudStabilityElement(StatusHudSystem system, StatusHudConfig config) : base(system)
        {
            stabilitySystem = this.system.capi.ModLoader.GetModSystem<SystemTemporalStability>();

            renderer = new StatusHudStabilityRenderer(system, this, config);
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

        public StatusHudStabilityRenderer(StatusHudSystem system, StatusHudStabilityElement element, StatusHudConfig config) : base(system)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.element.getTextKey(), config);
        }

                public override void Reload()
        {
            text.ReloadText(pos);
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
            if (!element.active)
            {
                if (system.ShowHidden)
                {
                    this.RenderHidden(system.textures.texturesDict["stability"].TextureId);
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