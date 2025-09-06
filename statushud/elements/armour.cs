using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudArmourElement : StatusHudElement
    {
        public new const string name = "armour";
        protected const string textKey = "shud-armour";

        public override string ElementName => name;

        // Hard-coded.
        protected static readonly int[] slots = {
            12,		// Head.
			13,		// Body.
			14		// Legs.
		};

        public bool active;

        protected StatusHudArmourRenderer renderer;

        public StatusHudArmourElement(StatusHudSystem system) : base(system)
        {
            renderer = new StatusHudArmourRenderer(this.system, this);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            active = false;
        }

        public override StatusHudRenderer GetRenderer()
        {
            return renderer;
        }

        public virtual string GetTextKey()
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

            for (int i = 0; i < slots.Length; i++)
            {
                ItemSlot slot = inventory[slots[i]];

                if (!slot.Empty && slot.Itemstack.Item is ItemWearable)
                {
                    int max = slot.Itemstack.Collectible.GetMaxDurability(slot.Itemstack);

                    // For cases like the night vision mask, where the armour has no durability
                    if (max <= 0) continue;

                    average += slot.Itemstack.Collectible.GetRemainingDurability(slot.Itemstack) / (float)max;
                    count++;
                }
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
        protected StatusHudArmourElement element;

        public StatusHudArmourRenderer(StatusHudSystem system, StatusHudArmourElement element) : base(system)
        {
            this.element = element;
            Text = new StatusHudText(this.System.capi, this.element.GetTextKey(), system.Config);
        }

        public override void Reload()
        {
            Text.ReloadText(pos);
        }

        public void SetText(string value)
        {
            Text.Set(value);
        }

        protected override void Update()
        {
            base.Update();
            Text.Pos(pos);
        }

        protected override void Render()
        {
            if (!element.active)
            {
                if (System.ShowHidden)
                {
                    RenderHidden(System.textures.texturesDict["armour"].TextureId);
                }
                return;
            }

            System.capi.Render.RenderTexture(System.textures.texturesDict["armour"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            Text.Dispose();
        }
    }
}