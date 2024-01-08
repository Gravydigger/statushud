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
            this.textureId = this.system.textures.timeLocal.TextureId;
        }

        public override string getTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            this.timeFormat = config.options.timeFormat;

            string time;

            if (this.timeFormat == "12hr")
            {
                time = DateTime.Now.ToString("h:mmtt", CultureInfo.InvariantCulture);
            }
            else
            {
                time = DateTime.Now.ToString("HH':'mm");
            }

            this.renderer.setText(time);
        }
    }
}