using System;
using System.Globalization;

namespace StatusHud
{
    public class StatusHudTimeLocalElement : StatusHudTimeElement
    {
        public new const string name = "time-local";
        public new const string desc = "The 'time-local' element displays the system's local time.";
        protected new const string textKey = "shud-timelocal";

        public override string elementName => name;

        public StatusHudTimeLocalElement(StatusHudSystem system, int slot, StatusHudConfig config) : base(system, slot, config)
        {
            textureId = this.system.textures.texturesDict["time_local"].TextureId;
        }

        public override string getTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            // timeFormat = config.options.timeFormat;
            // TODO
            timeFormat = "12hr";

            string time;

            if (timeFormat == "12hr")
            {
                time = DateTime.Now.ToString("h:mmtt", CultureInfo.InvariantCulture);
            }
            else
            {
                time = DateTime.Now.ToString("HH':'mm");
            }

            renderer.setText(time);
        }
    }
}