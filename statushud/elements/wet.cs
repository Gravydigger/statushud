using System;
using Vintagestory.API.Client;

namespace StatusHud;

public class StatusHudWetElement : StatusHudElement
{
    public const string Name = "wet";
    private readonly StatusHudWetRenderer renderer;

    public bool active;

    public StatusHudWetElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudWetRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        active = false;
    }

    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void Tick()
    {
        float wetness = system.capi.World.Player.Entity.WatchedAttributes.GetFloat("wetness");

        if (wetness > 0)
        {
            renderer.SetText((int)Math.Round(wetness * 100f, 0) + "%");

            active = true;
        }
        else
        {
            if (active)
            {
                // Only set text once.
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

public class StatusHudWetRenderer : StatusHudRenderer
{
    private const string textKey = "shud-wet";
    private readonly StatusHudWetElement element;

    public StatusHudWetRenderer(StatusHudSystem system, StatusHudWetElement element) : base(system)
    {
        this.element = element;
        text = new StatusHudText(this.system.capi, textKey, system.Config);
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
                RenderHidden(system.textures.TexturesDict["wet"].TextureId);
            }
            return;
        }

        system.capi.Render.RenderTexture(system.textures.TexturesDict["wet"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}