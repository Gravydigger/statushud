using System;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudSleepElement : StatusHudElement
    {
        public new const string name = "sleep";
        public new const string desc = "The 'sleep' element displays a countdown until the next time the player is able to sleep. If the player can sleep, it is hidden.";
        protected const string textKey = "shud-sleep";

        protected const float threshold = 8;        // Hard-coded in BlockBed.
        protected const float ratio = 0.75f;        // Hard-coded in EntityBehaviorTiredness.
        public bool active;

        protected StatusHudSleepRenderer renderer;

        public StatusHudSleepElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            this.renderer = new StatusHudSleepRenderer(system, slot, this, config);
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
            EntityBehaviorTiredness ebt = this.system.capi.World.Player.Entity.GetBehavior("tiredness") as EntityBehaviorTiredness;

            if (ebt == null)
            {
                return;
            }

            if (ebt.Tiredness <= threshold
                    && !ebt.IsSleeping)
            {
                TimeSpan ts = TimeSpan.FromHours((threshold - ebt.Tiredness) / ratio);
                this.renderer.setText(ts.ToString("h':'mm"));

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

    public class StatusHudSleepRenderer : StatusHudRenderer
    {
        protected StatusHudSleepElement element;

        protected StatusHudText text;

        public StatusHudSleepRenderer(StatusHudSystem system, int slot, StatusHudSleepElement element, StatusHudTextConfig config) : base(system, slot)
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
                    this.renderHidden(this.system.textures.sleep.TextureId);
                }
                return;
            }

            this.system.capi.Render.RenderTexture(this.system.textures.sleep.TextureId, this.x, this.y, this.w, this.h);
        }

        public override void Dispose()
        {
            this.text.Dispose();
        }
    }
}