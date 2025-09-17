using System;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace StatusHud;

public class StatusHudSleepElement : StatusHudElement
{
    public const string Name = "sleep";
    private const string textKey = "shud-sleep";

    private const float threshold = 8; // Hard-coded in BlockBed.
    private const float ratio = 0.75f; // Hard-coded in EntityBehaviorTiredness.

    private readonly StatusHudSleepRenderer renderer;
    public bool active;

    public StatusHudSleepElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudSleepRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        active = false;
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
        if (system.capi.World.Player.Entity.GetBehavior("tiredness") is not EntityBehaviorTiredness ebt)
        {
            return;
        }

        if (ebt.Tiredness <= threshold
            && !ebt.IsSleeping)
        {
            TimeSpan ts = TimeSpan.FromHours((threshold - ebt.Tiredness) / ratio);
            renderer.SetText(ts.ToString("h':'mm"));

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

public class StatusHudSleepRenderer : StatusHudRenderer
{
    private readonly StatusHudSleepElement element;

    public StatusHudSleepRenderer(StatusHudSystem system, StatusHudSleepElement element) : base(system)
    {
        this.element = element;
        text = new StatusHudText(this.system.capi, StatusHudSleepElement.GetTextKey(), system.Config);
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
                RenderHidden(system.textures.TexturesDict["sleep"].TextureId);
            }
            return;
        }

        system.capi.Render.RenderTexture(system.textures.TexturesDict["sleep"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}