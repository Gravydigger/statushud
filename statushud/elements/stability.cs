using System;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace StatusHud;

public class StatusHudStabilityElement : StatusHudElement
{
    public const string name = "stability";
    private const string textKey = "shud-stability";

    private const float maxStability = 1.5f; // Hard-coded in SystemTemporalStability.
    private readonly StatusHudStabilityRenderer renderer;

    private readonly SystemTemporalStability stabilitySystem;

    public bool active;

    public StatusHudStabilityElement(StatusHudSystem system) : base(system)
    {
        stabilitySystem = this.system.capi.ModLoader.GetModSystem<SystemTemporalStability>();

        renderer = new StatusHudStabilityRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        active = false;
    }

    public override string ElementName => name;

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
        if (stabilitySystem == null)
        {
            return;
        }

        float stability = stabilitySystem.GetTemporalStability(system.capi.World.Player.Entity.Pos.AsBlockPos);

        if (stability < maxStability)
        {
            renderer.SetText((int)Math.Floor(stability * 100) + "%");
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

public class StatusHudStabilityRenderer : StatusHudRenderer
{
    private readonly StatusHudStabilityElement element;

    public StatusHudStabilityRenderer(StatusHudSystem system, StatusHudStabilityElement element) : base(system)
    {
        this.element = element;
        text = new StatusHudText(this.system.capi, StatusHudStabilityElement.GetTextKey(), system.Config);
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
            if (system.ShowHidden)
            {
                RenderHidden(system.textures.texturesDict["stability"].TextureId);
            }
            return;
        }

        system.capi.Render.RenderTexture(system.textures.texturesDict["stability"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}