using System;
using System.Globalization;
using System.Linq;
using Vintagestory.API.Client;

namespace StatusHud
{
    public class StatusHudTimeElement : StatusHudElement
    {
        public new const string name = "time";
        public new const string desc = "The 'time' element displays the current time and an icon for the position of the sun relative to the horizon.";
        protected const string textKey = "shud-time";

        public override string elementName => name;

        public int textureId;
        protected string timeFormat;
        public static readonly string[] timeFormatWords = new string[] { "12hr", "24hr" };

        protected StatusHudTimeRenderer renderer;
        protected StatusHudConfig config;

        public StatusHudTimeElement(StatusHudSystem system, int slot, StatusHudConfig config) : base(system, slot)
        {
            renderer = new StatusHudTimeRenderer(system, slot, this, config.text);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            this.config = config;

            textureId = this.system.textures.texturesDict["empty"].TextureId;
            timeFormat = config.options.timeFormat;

            // Config error checking
            if (!timeFormatWords.Any(str => str.Contains(timeFormat)))
            {
                system.capi.Logger.Warning("[{0}] {1} is not a valid value for timeFormat. Defaulting to 24hr", getTextKey(), timeFormat);
            }
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
            TimeSpan ts = TimeSpan.FromHours(system.capi.World.Calendar.HourOfDay);
            timeFormat = config.options.timeFormat;

            string time;

            if (timeFormat == "12hr")
            {
                DateTime dateTime = new DateTime(ts.Ticks);
                time = dateTime.ToString("h:mmtt", CultureInfo.InvariantCulture);
            }
            else
            {
                time = ts.ToString("hh':'mm");
            }

            renderer.setText(time);

            if (system.capi.World.Calendar.SunPosition.Y < -5)
            {
                // Night
                textureId = system.textures.texturesDict["time_night"].TextureId;
            }
            else if (system.capi.World.Calendar.SunPosition.Y < 5)
            {
                // Twilight
                textureId = system.textures.texturesDict["time_twilight"].TextureId;
            }
            else if (system.capi.World.Calendar.SunPosition.Y < 15)
            {
                // Low
                textureId = system.textures.texturesDict["time_day_low"].TextureId;
            }
            else if (system.capi.World.Calendar.SunPosition.Y < 30)
            {
                // Mid
                textureId = system.textures.texturesDict["time_day_mid"].TextureId;
            }
            else
            {
                // High
                textureId = system.textures.texturesDict["time_day_high"].TextureId;
            }
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudTimeRenderer : StatusHudRenderer
    {
        protected StatusHudTimeElement element;
        protected StatusHudText text;

        public StatusHudTimeRenderer(StatusHudSystem system, int slot, StatusHudTimeElement element, StatusHudTextConfig config) : base(system, slot)
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