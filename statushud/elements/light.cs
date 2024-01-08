using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace StatusHud {
	public class StatusHudLightElement: StatusHudElement {
		public new const string name = "light";
		public new const string desc = "The 'light' element displays the selected block's light level. If no block is selected, it is hidden.";
		protected const string textKey = "shud-light";

        public override string elementName => name;

        public bool active;

		protected StatusHudLightRenderer renderer;

		public StatusHudLightElement(StatusHudSystem system, int slot, StatusHudTextConfig config): base(system, slot, true) {
			this.renderer = new StatusHudLightRenderer(system, slot, this, config);
			this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);
		}

        public override StatusHudRenderer getRenderer() {
			return this.renderer;
		}

		public virtual string getTextKey() {
			return textKey;
		}

        public override void Tick() {
			if(this.system.capi.World.Player.CurrentBlockSelection != null) {
				this.renderer.setText(this.system.capi.World.BlockAccessor.GetLightLevel(this.system.capi.World.Player.CurrentBlockSelection.Position, EnumLightLevelType.MaxTimeOfDayLight).ToString());
				this.active = true;
			} else {
				if(this.active) {
					this.renderer.setText("");
				}
				this.active = false;
			}
		}

		public override void Dispose() {
			this.renderer.Dispose();
			this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
		}
	}

	public class StatusHudLightRenderer: StatusHudRenderer {
		protected StatusHudLightElement element;

		protected StatusHudText text;

		public StatusHudLightRenderer(StatusHudSystem system, int slot, StatusHudLightElement element, StatusHudTextConfig config): base(system, slot) {
			this.element = element;

			this.text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
		}

        public override void Reload(StatusHudTextConfig config)
        {
            this.text.ReloadText(config, this.pos);
        }

        public void setText(string value) {
			this.text.Set(value);
		}

		protected override void update() {
			base.update();
			this.text.Pos(this.pos);
		}

		protected override void render() {
			if(!this.element.active) {
				if(this.system.showHidden) {
					this.renderHidden(this.system.textures.light.TextureId);
				}
				return;
			}

			this.system.capi.Render.RenderTexture(this.system.textures.light.TextureId, this.x, this.y, this.w, this.h);
		}

		public override void Dispose() {
			this.text.Dispose();
		}
	}
}