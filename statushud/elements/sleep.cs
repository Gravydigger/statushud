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

        public StatusHudSleepElement(StatusHudSystem system) : base(system)
        {
            renderer = new StatusHudSleepRenderer(system, this);
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

        public StatusHudSleepRenderer(StatusHudSystem system, StatusHudSleepElement element) : base(system)
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
                    this.RenderHidden(System.textures.texturesDict["sleep"].TextureId);
                }
                return;
            }

            System.capi.Render.RenderTexture(System.textures.texturesDict["sleep"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            Text.Dispose();
        }
    }
}