using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace StatusHud;

public class StatusHudLightElement : StatusHudElement
{
    public const string Name = "light";
    private const string textKey = "shud-light";

    private readonly StatusHudLightRenderer renderer;

    public bool active;

    public StatusHudLightElement(StatusHudSystem system) : base(system, true)
    {
        renderer = new StatusHudLightRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
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
        if (system.capi.World.Player.CurrentBlockSelection != null)
        {
            renderer.SetText(system.capi.World.BlockAccessor
                .GetLightLevel(system.capi.World.Player.CurrentBlockSelection.Position, EnumLightLevelType.MaxTimeOfDayLight)
                .ToString());
            active = true;
        }
        else
        {
            if (active)
            {
                renderer.SetText("");
            }
            active = false;
        }
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudLightRenderer : StatusHudRenderer
{
    private readonly StatusHudLightElement element;

    public StatusHudLightRenderer(StatusHudSystem system, StatusHudLightElement element) : base(system)
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
        if (!element.active)
        {
            if (showHidden)
            {
                RenderHidden(system.textures.TexturesDict["light"].TextureId);
            }
            return;
        }

        system.capi.Render.RenderTexture(system.textures.TexturesDict["light"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}