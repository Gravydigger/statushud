using System;
using Vintagestory.API.Client;

namespace StatusHud {
	public class StatusHudTimeElement: StatusHudElement {
		public new const string name = "time";
		public new const string desc = "The 'time' element displays the current time and an icon for the position of the sun relative to the horizon.";
		protected const string textKey = "shud-time";

		public int textureId;

		protected StatusHudTimeRenderer renderer;

		public StatusHudTimeElement(StatusHudSystem system, int slot, StatusHudTextConfig config): base(system, slot) {
			this.renderer = new StatusHudTimeRenderer(system, slot, this, config);
			this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);

			this.textureId = this.system.textures.empty.TextureId;
		}

		protected override StatusHudRenderer getRenderer() {
			return this.renderer;
		}

		public virtual string getTextKey() {
			return textKey;
		}

		public override void Tick() {
			TimeSpan ts = TimeSpan.FromHours(this.system.capi.World.Calendar.HourOfDay);
			this.renderer.setText(ts.ToString("hh':'mm"));

			if(this.system.capi.World.Calendar.SunPosition.Y < -5) {
				// Night.
				this.textureId = this.system.textures.timeNight.TextureId;
			} else if(this.system.capi.World.Calendar.SunPosition.Y < 5) {
				// Twilight.
				this.textureId = this.system.textures.timeTwilight.TextureId;
			} else if(this.system.capi.World.Calendar.SunPosition.Y < 15) {
				// Low.
				this.textureId = this.system.textures.timeDayLow.TextureId;
			} else if(this.system.capi.World.Calendar.SunPosition.Y < 30) {
				// Mid.
				this.textureId = this.system.textures.timeDayMid.TextureId;
			} else {
				// High.
				this.textureId = this.system.textures.timeDayHigh.TextureId;
			}
		}

		public override void Dispose() {
			this.renderer.Dispose();
			this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
		}
	}

	public class StatusHudTimeRenderer: StatusHudRenderer {
		protected StatusHudTimeElement element;
		protected StatusHudText text;

		public StatusHudTimeRenderer(StatusHudSystem system, int slot, StatusHudTimeElement element, StatusHudTextConfig config): base(system, slot) {
			this.element = element;

			this.text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
		}

		public void setText(string value) {
			this.text.Set(value);
		}

		protected override void update() {
			base.update();
			this.text.Pos(this.pos);
		}

		protected override void render() {
			this.system.capi.Render.RenderTexture(this.element.textureId, this.x, this.y, this.w, this.h);
		}

		public override void Dispose() {
			this.text.Dispose();
		}
	}
}