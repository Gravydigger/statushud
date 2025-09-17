using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Common;

namespace StatusHud;

public class StatusHudDateElement : StatusHudElement
{
    public const string Name = "date";
    private const string textKey = "shud-date";

    private readonly string[] monthNames =
    [
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
        Lang.Get("statushudcont:short-month-december")
    ];

    private readonly StatusHudDateRenderer renderer;

    public int textureId;

    public StatusHudDateElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudDateRenderer(this.system, this);

        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        textureId = system.textures.TexturesDict["empty"].TextureId;
    }

    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public static string GetTextKey()
    {
        return textKey;
    }

    public override void Tick()
    {
        int day = ((GameCalendar)system.capi.World.Calendar).DayOfMonth;

        if (system.capi.World.Calendar.Month is >= 1 and <= 12
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
        textureId = system.capi.World.Calendar.GetSeason(system.capi.World.Player.Entity.Pos.AsBlockPos) switch
        {
            EnumSeason.Spring => system.textures.TexturesDict["date_spring"].TextureId,
            EnumSeason.Summer => system.textures.TexturesDict["date_summer"].TextureId,
            EnumSeason.Fall => system.textures.TexturesDict["date_autumn"].TextureId,
            EnumSeason.Winter => system.textures.TexturesDict["date_winter"].TextureId,
            _ => textureId
        };
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudDateRenderer : StatusHudRenderer
{
    private readonly StatusHudDateElement element;

    public StatusHudDateRenderer(StatusHudSystem system, StatusHudDateElement element) : base(system)
    {
        this.element = element;
        text = new StatusHudText(this.system.capi, StatusHudDateElement.GetTextKey(), system.Config);
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