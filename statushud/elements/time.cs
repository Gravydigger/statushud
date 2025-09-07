using System;
using System.Globalization;
using System.Linq;
using Vintagestory.API.Client;

namespace StatusHud;

public class StatusHudTimeElement : StatusHudElement
{
    public const string name = "time";
    private const string textKey = "shud-time";
    public static readonly string[] TimeFormatWords = ["12hr", "24hr"];

    protected readonly StatusHudTimeRenderer renderer;

    public int textureId;
    protected string timeFormat;

    // ReSharper disable once MemberCanBeProtected.Global
    public StatusHudTimeElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudTimeRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        textureId = this.system.textures.texturesDict["empty"].TextureId;
        timeFormat = "24hr";

        // Config error checking
        if (!TimeFormatWords.Any(str => str.Contains(timeFormat)))
        {
            system.capi.Logger.Warning("[{0}] {1} is not a valid value for timeFormat. Defaulting to 24hr", textKey, timeFormat);
        }
    }

    public override string ElementName => name;
    public override string ElementOption => timeFormat;

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
        if (TimeFormatWords.Any(word => value == word))
        {
            timeFormat = value;
        }
    }

    public override void Tick()
    {
        TimeSpan ts = TimeSpan.FromHours(system.capi.World.Calendar.HourOfDay);

        string time;

        if (timeFormat == "12hr")
        {
            DateTime dateTime = new(ts.Ticks);
            time = dateTime.ToString("h:mmtt", CultureInfo.InvariantCulture);
        }
        else
        {
            time = ts.ToString("hh':'mm");
        }

        renderer.SetText(time);

        textureId = system.capi.World.Calendar.SunPosition.Y switch
        {
            < -5 => system.textures.texturesDict["time_night"].TextureId,
            < 5 => system.textures.texturesDict["time_twilight"].TextureId,
            < 15 => system.textures.texturesDict["time_day_low"].TextureId,
            < 30 => system.textures.texturesDict["time_day_mid"].TextureId,
            _ => system.textures.texturesDict["time_day_high"].TextureId
        };
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudTimeRenderer : StatusHudRenderer
{
    private readonly StatusHudTimeElement element;

    public StatusHudTimeRenderer(StatusHudSystem system, StatusHudTimeElement element) : base(system)
    {
        this.element = element;
        text = new StatusHudText(this.system.capi, this.element.GetTextKey(), system.Config);
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
        text.SetPos(pos);
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