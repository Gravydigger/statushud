using Vintagestory.API.Client;

namespace StatusHud;

public class StatusHudPlayersElement : StatusHudElement
{
    public const string Name = "players";
    private readonly StatusHudPlayersRenderer renderer;

    public StatusHudPlayersElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudPlayersRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
    }

    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void Tick()
    {
        renderer.SetText(system.capi.World.AllOnlinePlayers.Length.ToString());
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudPlayersRenderer : StatusHudRenderer
{
    private const string textKey = "shud-players";
    public StatusHudPlayersRenderer(StatusHudSystem system, StatusHudPlayersElement element) : base(system)
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
        system.capi.Render.RenderTexture(system.textures.TexturesDict["players"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}