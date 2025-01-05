using System;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudSleepElement : StatusHudElement
    {
        public new const string name = "sleep";
        protected const string textKey = "shud-sleep";

        public override string ElementName => name;

        protected const float threshold = 8;        // Hard-coded in BlockBed.
        protected const float ratio = 0.75f;        // Hard-coded in EntityBehaviorTiredness.
        public bool active;

        protected StatusHudSleepRenderer renderer;

        public StatusHudSleepElement(StatusHudSystem system, StatusHudConfig config) : base(system)
        {
            renderer = new StatusHudSleepRenderer(system, this, config);
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
            if (system.capi.World.Player.Entity.GetBehavior("tiredness") is not EntityBehaviorTiredness ebt)
            {
                return;
            }

            if (ebt.Tiredness <= threshold
                    && !ebt.IsSleeping)
            {
                TimeSpan ts = TimeSpan.FromHours((threshold - ebt.Tiredness) / ratio);
                renderer.SetText(ts.ToString("h':'mm"));

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

    public class StatusHudSleepRenderer : StatusHudRenderer
    {
        protected StatusHudSleepElement element;

        protected StatusHudText text;

        public StatusHudSleepRenderer(StatusHudSystem system, StatusHudSleepElement element, StatusHudConfig config) : base(system)
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
            if (!element.active)
            {
                if (system.ShowHidden)
                {
                    this.RenderHidden(system.textures.texturesDict["sleep"].TextureId);
                }
                return;
            }

            system.capi.Render.RenderTexture(system.textures.texturesDict["sleep"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}