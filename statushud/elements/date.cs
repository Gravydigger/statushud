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
        private string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        public StatusHudDateElement(StatusHudSystem system, int slot, StatusHudConfig config) : base(system, slot)
        {
            renderer = new StatusHudDateRenderer(this.system, this.slot, this, config);

            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            textureId = this.system.textures.texturesDict["empty"].TextureId;
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
            // Add '+ 1' because months start on the 1st.
            int day = (int)(system.capi.World.Calendar.DayOfYear % system.capi.World.Calendar.DaysPerMonth) + 1;

            if (system.capi.World.Calendar.Month >= 1 && system.capi.World.Calendar.Month <= 12
                    && monthNames.Length >= system.capi.World.Calendar.Month)
            {
                renderer.setText(day + " " + monthNames[system.capi.World.Calendar.Month - 1]);
            }
            else
            {
                // Unknown month.
                renderer.setText(day.ToString());
            }

            // Season.
            switch (system.capi.World.Calendar.GetSeason(system.capi.World.Player.Entity.Pos.AsBlockPos))
            {
                case EnumSeason.Spring:
                    {
                        textureId = system.textures.texturesDict["date_spring"].TextureId;
                        break;
                    }
                case EnumSeason.Summer:
                    {
                        textureId = system.textures.texturesDict["date_summer"].TextureId;
                        break;
                    }
                case EnumSeason.Fall:
                    {
                        textureId = system.textures.texturesDict["date_autumn"].TextureId;
                        break;
                    }
                case EnumSeason.Winter:
                    {
                        textureId = system.textures.texturesDict["date_winter"].TextureId;
                        break;
                    }
            }
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudDateRenderer : StatusHudRenderer
    {
        protected StatusHudDateElement element;

        protected StatusHudText text;

        public StatusHudDateRenderer(StatusHudSystem system, int slot, StatusHudDateElement element, StatusHudConfig config) : base(system, slot)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config);
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
            system.capi.Render.RenderTexture(element.textureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}