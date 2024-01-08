using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace StatusHud
{
    public class StatusHudDurabilityElement : StatusHudElement
    {
        public new const string name = "durability";
        public new const string desc = "The 'durability' element displays the selected item's remaining durability. If there is no durability, it is hidden.";
        protected const string textKey = "shud-durability";

        public override string elementName => name;

        public bool active;

        protected StatusHudDurabilityRenderer renderer;

        public StatusHudDurabilityElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot, true)
        {
            this.renderer = new StatusHudDurabilityRenderer(this.system, this.slot, this, config);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }

        public override StatusHudRenderer getRenderer()
        {
            return this.renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            CollectibleObject item = this.system.capi.World.Player.InventoryManager.ActiveHotbarSlot?.Itemstack?.Collectible;

            if (item != null
                    && item.Durability != 0)
            {
                this.renderer.setText(item.GetRemainingDurability(this.system.capi.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack).ToString());
                this.active = true;
            }
            else
            {
                if (this.active)
                {
                    this.renderer.setText("");
                }
                this.active = false;
            }
        }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudDurabilityRenderer : StatusHudRenderer
    {
        protected StatusHudDurabilityElement element;

        protected StatusHudText text;

        public StatusHudDurabilityRenderer(StatusHudSystem system, int slot, StatusHudDurabilityElement element, StatusHudTextConfig config) : base(system, slot)
        {
            this.element = element;

            this.text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
        }

        public override void Reload(StatusHudTextConfig config)
        {
            this.text.ReloadText(config, this.pos);
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
                    this.renderHidden(this.system.textures.durability.TextureId);
                }
                return;
            }

            this.system.capi.Render.RenderTexture(this.system.textures.durability.TextureId, this.x, this.y, this.w, this.h);
        }

        public override void Dispose()
        {
            this.text.Dispose();
        }
    }
}