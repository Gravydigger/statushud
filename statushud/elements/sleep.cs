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

        public override string elementName => name;

        protected const float threshold = 8;        // Hard-coded in BlockBed.
        protected const float ratio = 0.75f;        // Hard-coded in EntityBehaviorTiredness.
        public bool active;

        protected StatusHudSleepRenderer renderer;

        public StatusHudSleepElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            renderer = new StatusHudSleepRenderer(system, slot, this, config);
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
            EntityBehaviorTiredness ebt = system.capi.World.Player.Entity.GetBehavior("tiredness") as EntityBehaviorTiredness;

            if (ebt == null)
            {
                return;
            }

            if (ebt.Tiredness <= threshold
                    && !ebt.IsSleeping)
            {
                TimeSpan ts = TimeSpan.FromHours((threshold - ebt.Tiredness) / ratio);
                renderer.setText(ts.ToString("h':'mm"));

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

    public class StatusHudSleepRenderer : StatusHudRenderer
    {
        protected StatusHudSleepElement element;

        protected StatusHudText text;

        public StatusHudSleepRenderer(StatusHudSystem system, int slot, StatusHudSleepElement element, StatusHudTextConfig config) : base(system, slot)
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
                    this.renderHidden(system.textures.texturesDict["sleep"].TextureId);
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