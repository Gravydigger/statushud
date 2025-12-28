using System;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;

namespace StatusHud;

public class StatusHudHealthElement : StatusHudElement
{
    public const string Name = "health";

    private readonly StatusHudHealthRenderer renderer;

    public StatusHudHealthElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudHealthRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
    }

    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void Tick()
    {
        ITreeAttribute healthTree = system.capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute("health");

        if (healthTree == null) return;

        float? health = healthTree.TryGetFloat("currenthealth");
        float? maxHealth = healthTree.TryGetFloat("maxhealth");

        if (health == null || maxHealth == null) return;

        health = (float)Math.Round((float)health, 1);
        maxHealth = (float)Math.Round((float)maxHealth, 1);

        renderer.SetText(health + " / " + maxHealth);
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudHealthRenderer : StatusHudRenderer
{
    private const string textKey = "shud-health";

    public StatusHudHealthRenderer(StatusHudSystem system, StatusHudHealthElement element) : base(system)
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
        system.capi.Render.RenderTexture(system.textures.TexturesDict["health"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}