using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Common;

namespace StatusHud
{
    public class StatusHudDateElement : StatusHudElement
    {
        public new const string name = "date";
        protected const string textKey = "shud-date";

        public override string ElementName => name;

        public int textureId;

        protected StatusHudDateRenderer renderer;
        private readonly string[] monthNames = {
            Lang.Get("statushudcont:short-month-january"),
            Lang.Get("statushudcont:short-month-february"),
            Lang.Get("statushudcont:short-month-march"),
            Lang.Get("statushudcont:short-month-april"),
            Lang.Get("statushudcont:short-month-may"),
            Lang.Get("statushudcont:short-month-june"),
            Lang.Get("statushudcont:short-month-july"),
            Lang.Get("statushudcont:short-month-august"),
            Lang.Get("statushudcont:short-month-september"),
            Lang.Get("statushudcont:short-month-october"),
            Lang.Get("statushudcont:short-month-november"),
            Lang.Get("statushudcont:short-month-december")};

        public StatusHudDateElement(StatusHudSystem system, StatusHudConfig config) : base(system)
        {
            renderer = new StatusHudDateRenderer(this.system, this, config);

            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            textureId = this.system.textures.texturesDict["empty"].TextureId;
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
            // Add '+ 1' because months start on the 1st.
            int day = ((GameCalendar)system.capi.World.Calendar).DayOfMonth;

            if (system.capi.World.Calendar.Month >= 1 && system.capi.World.Calendar.Month <= 12
                    && monthNames.Length >= system.capi.World.Calendar.Month)
            {
                renderer.SetText(day + " " + monthNames[system.capi.World.Calendar.Month - 1]);
            }
            else
            {
                // Unknown month.
                renderer.SetText(day.ToString());
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

        public StatusHudDateRenderer(StatusHudSystem system, StatusHudDateElement element, StatusHudConfig config) : base(system)
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
            system.capi.Render.RenderTexture(element.textureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}
