using System;
using System.Globalization;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace StatusHud;

public class StatusHudAltitudeElement : StatusHudElement
{
    public const string Name = "altitude";
    private const string textKey = "shud-altitude";
    private readonly StatusHudAltitudeRenderer renderer;

    public float needleOffset;

    public StatusHudAltitudeElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudAltitudeRenderer(this.system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        needleOffset = 0;
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
        float altitude = (int)Math.Round(system.capi.World.Player.Entity.Pos.Y - system.capi.World.SeaLevel, 0);
        renderer.SetText(altitude.ToString(CultureInfo.InvariantCulture));

        float ratio = -(altitude / (system.capi.World.BlockAccessor.MapSizeY / 2));
        needleOffset = GameMath.Clamp(ratio, -1, 1) * (StatusHudSystem.IconSize * system.Config.elementScale / 2f) * 0.75f;
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudAltitudeRenderer : StatusHudRenderer
{
    private readonly StatusHudAltitudeElement element;
    public StatusHudAltitudeRenderer(StatusHudSystem system, StatusHudAltitudeElement element) : base(system)
    {
        this.element = element;
        text = new StatusHudText(this.system.capi, StatusHudAltitudeElement.GetTextKey(), system.Config);
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
        system.capi.Render.RenderTexture(system.textures.TexturesDict["altitude"].TextureId, x, y, w, h);
        system.capi.Render.RenderTexture(system.textures.TexturesDict["altitude_needle"].TextureId, x, y + GuiElement.scaled(element.needleOffset), w,
            h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}