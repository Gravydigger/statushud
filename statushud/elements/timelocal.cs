using System;
using System.Globalization;

namespace StatusHud
{
    public class StatusHudTimeLocalElement : StatusHudTimeElement
    {
        public new const string name = "time-local";
        public new const string desc = "The 'time-local' element displays the system's local time.";
        protected new const string textKey = "shud-timelocal";

        private new string timeFormat;
        public override string ElementOption => timeFormat;

        public override string ElementName => name;

        public StatusHudTimeLocalElement(StatusHudSystem system, StatusHudConfig config) : base(system, config)
        {
            textureId = this.system.textures.texturesDict["time_local"].TextureId;
            timeFormat = base.timeFormat;
        }

        public override string GetTextKey()
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
            string time;

            if (timeFormat == "12hr")
            {
                time = DateTime.Now.ToString("h:mmtt", CultureInfo.InvariantCulture);
            }
            else
            {
                time = DateTime.Now.ToString("HH':'mm");
            }

            renderer.SetText(time);
        }
    }
}