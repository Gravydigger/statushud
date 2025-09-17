using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace StatusHud;

public sealed class StatusHudArmourElement : StatusHudElement
{
    public const string Name = "armour";
    private const string textKey = "shud-armour";

    // Hard-coded.
    // https://github.com/anegostudios/vssurvivalmod/blob/master/Systems/WearableStats.cs#L135-L152
    private static readonly int[] Slots =
    [
        12, // Head.
        13, // Body.
        14 // Legs.
    ];

    private readonly StatusHudArmourRenderer renderer;

    public bool active;

    public StatusHudArmourElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudArmourRenderer(this.system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        active = false;
    }

    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public string GetTextKey()
    {
        return textKey;
    }

    public override void Tick()
    {
        IInventory inventory = system.capi.World.Player.InventoryManager.GetOwnInventory(GlobalConstants.characterInvClassName);

        if (inventory == null)
        {
            return;
        }

        int count = 0;
        float average = 0;

        foreach (int i in Slots)
        {
            ItemSlot slot = inventory[i];

            if (slot.Empty || slot.Itemstack.Item is not ItemWearable) continue;

            int max = slot.Itemstack.Collectible.GetMaxDurability(slot.Itemstack);

            // For cases like the night vision mask, where the armour has no durability
            if (max <= 0) continue;

            average += slot.Itemstack.Collectible.GetRemainingDurability(slot.Itemstack) / (float)max;
            count++;
        }

        if (count == 0)
        {
            renderer.SetText("");
            active = false;
            return;
        }

        average /= count;

        renderer.SetText((int)Math.Round(average * 100, 0) + "%");
        active = true;
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudArmourRenderer : StatusHudRenderer
{
    private readonly StatusHudArmourElement element;

    public StatusHudArmourRenderer(StatusHudSystem system, StatusHudArmourElement element) : base(system)
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
                RenderHidden(system.textures.TexturesDict["armour"].TextureId);
            }
            return;
        }

        system.capi.Render.RenderTexture(system.textures.TexturesDict["armour"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}