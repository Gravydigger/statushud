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
        public new const string desc = "The 'armour' element displays the equipped armour's durability average (in %). If no armour is equipped, it is hidden.";
        protected const string textKey = "shud-armour";

        public override string elementName => name;

        // Hard-coded.
        protected static readonly int[] slots = {
            12,		// Head.
			13,		// Body.
			14		// Legs.
		};

        public bool active;

        protected StatusHudArmourRenderer renderer;

        public StatusHudArmourElement(StatusHudSystem system, int slot, StatusHudConfig config) : base(system, slot)
        {
            renderer = new StatusHudArmourRenderer(this.system, this.slot, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            active = false;
        }

        public override StatusHudRenderer getRenderer()
        {
            return renderer;
        }

        public virtual string getTextKey()
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

            for (int i = 0; i < StatusHudArmourElement.slots.Length; i++)
            {
                ItemSlot slot = inventory[StatusHudArmourElement.slots[i]];

                if (!slot.Empty
                        && slot.Itemstack.Item is ItemWearable)
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
                renderer.setText("");
                active = false;
                return;
            }

            average /= (float)count;

            renderer.setText((int)Math.Round(average * 100, 0) + "%");
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

        protected StatusHudText text;

        public StatusHudArmourRenderer(StatusHudSystem system, int slot, StatusHudArmourElement element, StatusHudConfig config) : base(system, slot)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config);
        }

        public override void Reload()
        {
            text.ReloadText(pos);
        }

        public void setText(string value)
        {
            text.Set(value);
        }

        protected override void Update()
        {
            base.Update();
            text.Pos(pos);
        }

        protected override void Render()
        {
            if (!element.active)
            {
                if (system.ShowHidden)
                {
                    RenderHidden(system.textures.texturesDict["armour"].TextureId);
                }
                return;
            }

            system.capi.Render.RenderTexture(system.textures.texturesDict["armour"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}