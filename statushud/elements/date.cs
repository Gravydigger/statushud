using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace StatusHud
{
    public class StatusHudDateElement : StatusHudElement
    {
        public new const string name = "date";
        public new const string desc = "The 'date' element displays the current date and an icon for the current season.";
        protected const string textKey = "shud-date";

        public override string elementName => name;

        public int textureId;

        protected StatusHudDateRenderer renderer;
        protected string[] monthNames;

        public StatusHudDateElement(StatusHudSystem system, int slot, StatusHudTextConfig config, string[] monthNames) : base(system, slot)
        {
            this.renderer = new StatusHudDateRenderer(this.system, this.slot, this, config);
            this.monthNames = monthNames;

            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);

            this.textureId = this.system.textures.empty.TextureId;
        }

        public override StatusHudRenderer getRenderer()
        {
            return this.renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            // Add '+ 1' because months start on the 1st.
            int day = (int)(this.system.capi.World.Calendar.DayOfYear % this.system.capi.World.Calendar.DaysPerMonth) + 1;

            if (this.system.capi.World.Calendar.Month >= 1 && this.system.capi.World.Calendar.Month <= 12
                    && this.monthNames.Length >= this.system.capi.World.Calendar.Month)
            {
                this.renderer.setText(day + " " + this.monthNames[this.system.capi.World.Calendar.Month - 1]);
            }
            else
            {
                // Unknown month.
                this.renderer.setText(day.ToString());
            }

            // Season.
            switch (this.system.capi.World.Calendar.GetSeason(this.system.capi.World.Player.Entity.Pos.AsBlockPos))
            {
                case EnumSeason.Spring:
                    {
                        this.textureId = this.system.textures.dateSpring.TextureId;
                        break;
                    }
                case EnumSeason.Summer:
                    {
                        this.textureId = this.system.textures.dateSummer.TextureId;
                        break;
                    }
                case EnumSeason.Fall:
                    {
                        this.textureId = this.system.textures.dateAutumn.TextureId;
                        break;
                    }
                case EnumSeason.Winter:
                    {
                        this.textureId = this.system.textures.dateWinter.TextureId;
                        break;
                    }
            }
        }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudDateRenderer : StatusHudRenderer
    {
        protected StatusHudDateElement element;

        protected StatusHudText text;

        public StatusHudDateRenderer(StatusHudSystem system, int slot, StatusHudDateElement element, StatusHudTextConfig config) : base(system, slot)
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
            this.system.capi.Render.RenderTexture(this.element.textureId, this.x, this.y, this.w, this.h);
        }

        public override void Dispose()
        {
            base.Dispose();
            this.text.Dispose();
        }
    }
}