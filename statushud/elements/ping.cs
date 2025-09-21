using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace StatusHud;

public class StatusHudPingElement : StatusHudElement
{
    public const string Name = "ping";

    private const int maxPing = 999;

    private readonly StatusHudPingRenderer renderer;
    private ClientPlayer player;

    public StatusHudPingElement(StatusHudSystem system) : base(system, true)
    {
        renderer = new StatusHudPingRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
    }

    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void Tick()
    {
        if (system.capi.IsSinglePlayer)
        {
            renderer.SetText("");
            return;
        }

        if (player == null)
        {
            var players = system.capi.World.AllOnlinePlayers;

            // Get the clients player object
            player = (ClientPlayer)players.FirstOrDefault(p => p.PlayerUID == system.Uuid);
            renderer.SetText("");

            return;
        }

        int ping = (int)(player.Ping * 1000f);
        string msg = ping < maxPing ? $"{Math.Min(ping, maxPing)}" : "+" + maxPing;

        renderer.SetText(msg);
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudPingRenderer : StatusHudRenderer
{
    private const string textKey = "shud-ping";
    public StatusHudPingRenderer(StatusHudSystem system, StatusHudPingElement element) : base(system)
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
        switch (showHidden)
        {
            case true when system.capi.IsSinglePlayer:
                RenderHidden(system.textures.TexturesDict["network"].TextureId);
                break;
            case false when system.capi.IsSinglePlayer:
                system.capi.Render.RenderTexture(system.textures.TexturesDict["empty"].TextureId, x, y, w, h);
                break;
            default:
                system.capi.Render.RenderTexture(system.textures.TexturesDict["network"].TextureId, x, y, w, h);
                break;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}