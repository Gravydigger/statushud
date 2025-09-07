using Vintagestory.API.Client;

namespace StatusHud;

public class StatusHudPlayersElement : StatusHudElement
{
    public const string name = "players";
    private const string textKey = "shud-players";

    private readonly StatusHudPlayersRenderer renderer;

    public StatusHudPlayersElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudPlayersRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
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
    public StatusHudPlayersRenderer(StatusHudSystem system, StatusHudPlayersElement element) : base(system)
    {
        text = new StatusHudText(this.system.capi, StatusHudPlayersElement.GetTextKey(), system.Config);
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
        system.capi.Render.RenderTexture(system.textures.texturesDict["players"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}