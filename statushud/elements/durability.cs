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

        public StatusHudDurabilityElement(StatusHudSystem system, StatusHudConfig config) : base(system, true)
        {
            renderer = new StatusHudDurabilityRenderer(this.system, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
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
            CollectibleObject item = system.capi.World.Player.InventoryManager.ActiveHotbarSlot?.Itemstack?.Collectible;

            if (item != null
                    && item.Durability != 0)
            {
                renderer.setText(item.GetRemainingDurability(system.capi.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack).ToString());
                active = true;
            }
            else
            {
                if (active)
                {
                    renderer.setText("");
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
        protected StatusHudDurabilityElement element;

        protected StatusHudText text;

        public StatusHudDurabilityRenderer(StatusHudSystem system, StatusHudDurabilityElement element, StatusHudConfig config) : base(system)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.element.getTextKey(), config);
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
                    RenderHidden(system.textures.texturesDict["durability"].TextureId);
                }
                return;
            }

            system.capi.Render.RenderTexture(system.textures.texturesDict["durability"].TextureId, x, y, w, h);
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}