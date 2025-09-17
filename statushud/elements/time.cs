using System;
using System.Globalization;
using System.Linq;
using Vintagestory.API.Client;

namespace StatusHud;

public class StatusHudTimeElement : StatusHudElement
{
    public const string Name = "time";
    private const string textKey = "shud-time";

    protected readonly StatusHudTimeRenderer renderer;

    public int textureId;
    protected string timeFormat;

    // ReSharper disable once MemberCanBeProtected.Global
    public StatusHudTimeElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudTimeRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        textureId = system.textures.TexturesDict["empty"].TextureId;
        timeFormat = "24hr";

        // Config error checking
        if (!ElementOptionList.Any(str => str.Contains(timeFormat)))
        {
            system.capi.Logger.Warning(StatusHudSystem.PrintModName("[{0}] {1} is not a valid value for timeFormat. Defaulting to 24hr"), textKey, timeFormat);
        }
    }
    public sealed override string[] ElementOptionList => ["12hr", "24hr"];

    public override string ElementName => Name;
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
        if (ElementOptionList.Any(word => value == word))
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
            < -5 => system.textures.TexturesDict["time_night"].TextureId,
            < 5 => system.textures.TexturesDict["time_twilight"].TextureId,
            < 15 => system.textures.TexturesDict["time_day_low"].TextureId,
            < 30 => system.textures.TexturesDict["time_day_mid"].TextureId,
            _ => system.textures.TexturesDict["time_day_high"].TextureId
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