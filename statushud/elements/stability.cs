using System;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudStabilityElement : StatusHudElement
    {
        public new const string name = "stability";
        protected const string textKey = "shud-stability";

        public override string ElementName => name;

        protected const float maxStability = 1.5f; // Hard-coded in SystemTemporalStability.

        public bool active;

        protected SystemTemporalStability stabilitySystem;
        protected StatusHudStabilityRenderer renderer;

        public StatusHudStabilityElement(StatusHudSystem system) : base(system)
        {
            stabilitySystem = this.system.capi.ModLoader.GetModSystem<SystemTemporalStability>();

            renderer = new StatusHudStabilityRenderer(system, this);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            active = false;
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
            if (stabilitySystem == null)
            {
                return;
            }

            float stability = stabilitySystem.GetTemporalStability(system.capi.World.Player.Entity.Pos.AsBlockPos);

            if (stability < maxStability)
            {
                renderer.SetText((int)Math.Floor(stability * 100) + "%");
                active = true;
            }
            else
            {
                if (active)
                {
                    // Only set text once.
                    renderer.SetText("");
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

        public StatusHudStabilityRenderer(StatusHudSystem system, StatusHudStabilityElement element) : base(system)
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
            if (!element.active)
            {
                if (System.ShowHidden)
                {
                    this.RenderHidden(System.textures.texturesDict["stability"].TextureId);
                }
                return;
            }

            System.capi.Render.RenderTexture(System.textures.texturesDict["stability"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            Text.Dispose();
        }
    }
}