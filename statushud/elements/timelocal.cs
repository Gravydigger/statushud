using System;
using System.Globalization;

namespace StatusHud;

public class StatusHudTimeLocalElement : StatusHudTimeElement
{
    public new const string Name = "timelocal";
    private const string textKey = "shud-timelocal";

    private readonly new string timeFormat;

    public StatusHudTimeLocalElement(StatusHudSystem system) : base(system)
    {
        textureId = system.textures.TexturesDict["time_local"].TextureId;
        timeFormat = base.timeFormat;
    }
    public override string ElementOption => timeFormat;

    public override string ElementName => Name;

    public override string GetTextKey()
    {
        return textKey;
    }

    public override void Tick()
    {
        string time = timeFormat == "12hr" ? DateTime.Now.ToString("h:mmtt", CultureInfo.InvariantCulture) : DateTime.Now.ToString("HH':'mm");

        renderer.SetText(time);
    }
}