using System;
using System.Globalization;
using System.Linq;
using Vintagestory.API.Client;

namespace StatusHud
{
    public class StatusHudTimeElement : StatusHudElement
    {
        public new const string name = "time";
        protected const string textKey = "shud-time";
        public static readonly string[] timeFormatWords = new string[] { "12hr", "24hr" };

        public int textureId;
        protected string timeFormat;

        public override string ElementName => name;
        public override string ElementOption => timeFormat;

        protected StatusHudTimeRenderer renderer;

        public StatusHudTimeElement(StatusHudSystem system) : base(system)
        {
            renderer = new StatusHudTimeRenderer(system, this);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            textureId = this.system.textures.texturesDict["empty"].TextureId;
            timeFormat = "24hr";

            // Config error checking
            if (!timeFormatWords.Any(str => str.Contains(timeFormat)))
            {
                system.capi.Logger.Warning("[{0}] {1} is not a valid value for timeFormat. Defaulting to 24hr", GetTextKey(), timeFormat);
            }
        }

        public override StatusHudRenderer GetRenderer()
        {
            return renderer;
        }

        public virtual string GetTextKey()
        {
            return textKey;
        }

        public override void ConfigOptions(string value)
        {
            foreach (var word in timeFormatWords)
            {
                if (value == word)
                {
                    timeFormat = value;
                    return;
                }
            }
        }

        public override void Tick()
        {
            TimeSpan ts = TimeSpan.FromHours(system.capi.World.Calendar.HourOfDay);

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

            renderer.SetText(time);

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

        public StatusHudTimeRenderer(StatusHudSystem system, StatusHudTimeElement element) : base(system)
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
            System.capi.Render.RenderTexture(element.textureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            Text.Dispose();
        }
    }
}