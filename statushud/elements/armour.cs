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

        public StatusHudArmourElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            this.renderer = new StatusHudArmourRenderer(this.system, this.slot, this, config);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);

            this.active = false;
        }

        protected override StatusHudRenderer getRenderer()
        {
            return this.renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            IInventory inventory = this.system.capi.World.Player.InventoryManager.GetOwnInventory(GlobalConstants.characterInvClassName);

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
                this.renderer.setText("");
                this.active = false;
                return;
            }

            average /= (float)count;

            this.renderer.setText((int)Math.Round(average * 100, 0) + "%");
            this.active = true;
        }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudArmourRenderer : StatusHudRenderer
    {
        protected StatusHudArmourElement element;

        protected StatusHudText text;

        public StatusHudArmourRenderer(StatusHudSystem system, int slot, StatusHudArmourElement element, StatusHudTextConfig config) : base(system, slot)
        {
            this.element = element;

            this.text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
        }

        public void setText(string value)
        {
            this.text.Set(value);
        }

        protected override void update()
        {
            base.update();
            this.text.Pos(this.pos);
        }

        protected override void render()
        {
            if (!this.element.active)
            {
                if (this.system.showHidden)
                {
                    this.renderHidden(this.system.textures.armour.TextureId);
                }
                return;
            }

            this.system.capi.Render.RenderTexture(this.system.textures.armour.TextureId, this.x, this.y, this.w, this.h);
        }

        public override void Dispose()
        {
            this.text.Dispose();
        }
    }
}