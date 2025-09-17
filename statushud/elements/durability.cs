using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace StatusHud;

public class StatusHudDurabilityElement : StatusHudElement
{
    public const string Name = "durability";
    private const string textKey = "shud-durability";

    private readonly StatusHudDurabilityRenderer renderer;

    public bool active;

    public StatusHudDurabilityElement(StatusHudSystem system) : base(system, true)
    {
        renderer = new StatusHudDurabilityRenderer(this.system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
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
        CollectibleObject item = system.capi.World.Player.InventoryManager.ActiveHotbarSlot?.Itemstack?.Collectible;

        if (item != null && item.Durability != 0)
        {
            renderer.SetText(item.GetRemainingDurability(system.capi.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack).ToString());
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

public class StatusHudDurabilityRenderer : StatusHudRenderer
{
    private readonly StatusHudDurabilityElement element;

    public StatusHudDurabilityRenderer(StatusHudSystem system, StatusHudDurabilityElement element) : base(system)
    {
        this.element = element;
        text = new StatusHudText(this.system.capi, StatusHudDurabilityElement.GetTextKey(), system.Config);
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
                RenderHidden(system.textures.TexturesDict["durability"].TextureId);
            }
            return;
        }

        system.capi.Render.RenderTexture(system.textures.TexturesDict["durability"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}