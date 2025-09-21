using System;
using System.Globalization;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;

namespace StatusHud;

public class StatusHudSpeedElement : StatusHudElement
{
    public const string Name = "speed";
    private readonly StatusHudSpeedRenderer renderer;

    public StatusHudSpeedElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudSpeedRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
    }

    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void Tick()
    {
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudSpeedRenderer : StatusHudRenderer
{
    private const string textKey = "shud-speed";

    public StatusHudSpeedRenderer(StatusHudSystem system, StatusHudSpeedElement element) : base(system)
    {
        text = new StatusHudText(this.system.capi, textKey, system.Config);
    }

    public override void Reload()
    {
        text.ReloadText(pos);
    }

    protected override void Update()
    {
        base.Update();
        text.SetPos(pos);
    }

    protected override void Render()
    {
        Entity mount = system.capi.World.Player.Entity.MountedOn?.Entity;

        text.Set(mount != null
            ? ((int)Math.Round(mount.Pos.Motion.Length() * 1000) / 10f).ToString(CultureInfo.InvariantCulture)
            : ((int)Math.Round(system.capi.World.Player.Entity.Pos.Motion.Length() * 1000) / 10f).ToString(CultureInfo.InvariantCulture));
        system.capi.Render.RenderTexture(system.textures.TexturesDict["speed"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}