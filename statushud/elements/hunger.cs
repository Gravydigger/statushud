using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;

namespace StatusHud;

public class StatusHudHungerElement : StatusHudElement
{
    public const string Name = "hunger";

    private readonly StatusHudHungerRenderer renderer;

    public StatusHudHungerElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudHungerRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
    }

    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void Tick()
    {
        ITreeAttribute hungerTree = system.capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute("hunger");

        float? hunger = hungerTree?.TryGetFloat("currentsaturation");
        float? maxHunger = hungerTree?.TryGetFloat("maxsaturation");

        if (hunger == null || maxHunger == null) return;

        renderer.SetText((int)hunger + " / " + (int)maxHunger);
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudHungerRenderer : StatusHudRenderer
{
    private const string textKey = "shud-hunger";

    public StatusHudHungerRenderer(StatusHudSystem system, StatusHudHungerElement element) : base(system)
    {
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
        system.capi.Render.RenderTexture(system.textures.TexturesDict["hunger"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}