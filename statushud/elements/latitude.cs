using System;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace StatusHud;

public class StatusHudLatitudeElement : StatusHudElement
{
    public const string Name = "latitude";
    private const string textKey = "shud-latitude";
    private readonly StatusHudLatitudeRenderer renderer;

    public float needleOffset;

    protected WeatherSystemBase weatherSystem;

    public StatusHudLatitudeElement(StatusHudSystem system) : base(system)
    {
        weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

        renderer = new StatusHudLatitudeRenderer(this.system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        needleOffset = 0;
    }

    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public virtual string GetTextKey()
    {
        return textKey;
    }

    public override void Tick()
    {
        double latitude = system.capi.World.Calendar.OnGetLatitude(system.capi.World.Player.Entity.Pos.Z);
        renderer.SetText((int)Math.Round(latitude * 900, 0) / 10f + "°");
        needleOffset = (float)(-latitude * (StatusHudSystem.IconSize * system.Config.elementScale / 2f) * 0.75f);
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudLatitudeRenderer : StatusHudRenderer
{
    private readonly StatusHudLatitudeElement element;

    public StatusHudLatitudeRenderer(StatusHudSystem system, StatusHudLatitudeElement element) : base(system)
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
        system.capi.Render.RenderTexture(system.textures.TexturesDict["latitude"].TextureId, x, y, w, h);
        system.capi.Render.RenderTexture(system.textures.TexturesDict["latitude_needle"].TextureId, x, y + GuiElement.scaled(element.needleOffset), w,
            h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}