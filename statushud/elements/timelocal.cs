using System;
using System.Globalization;
using System.Linq;

namespace StatusHud;

public class StatusHudTimeLocalElement : StatusHudTimeElement
{
    public new const string name = "timelocal";
    private const string textKey = "shud-timelocal";

    private new string timeFormat;

    public StatusHudTimeLocalElement(StatusHudSystem system) : base(system)
    {
        textureId = this.system.textures.texturesDict["time_local"].TextureId;
        timeFormat = base.timeFormat;
    }
    public override string ElementOption => timeFormat;

    public override string ElementName => name;

    public override string GetTextKey()
    {
        return textKey;
    }

    public override void ConfigOptions(string value)
    {
        if (TimeFormatWords.Any(word => value == word))
        {
            timeFormat = value;
        }
    }

    public override void Tick()
    {
        string time = timeFormat == "12hr" ? DateTime.Now.ToString("h:mmtt", CultureInfo.InvariantCulture) : DateTime.Now.ToString("HH':'mm");

        renderer.SetText(time);
    }
}