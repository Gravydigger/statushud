using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudCompassElement : StatusHudElement
    {
        public new const string name = "compass";
        public new const string desc = "The 'compass' element displays the player's facing direction (in degrees) in relation to the north.";
        protected const string textKey = "shud-compass";

        protected WeatherSystemBase weatherSystem;
        protected StatusHudCompassRenderer renderer;

        public StatusHudCompassElement(StatusHudSystem system, int slot, StatusHudTextConfig config, bool absolute) : base(system, slot)
        {
            this.weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            this.renderer = new StatusHudCompassRenderer(this.system, slot, this, config, absolute);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }

        protected override StatusHudRenderer getRenderer()
        {
            return this.renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
        }

        public override void Tick() { }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudCompassRenderer : StatusHudRenderer
    {
        protected StatusHudCompassElement element;
        protected StatusHudText text;
        protected bool absolute;

        protected const float dirAdjust = 180 * GameMath.DEG2RAD;

        public StatusHudCompassRenderer(StatusHudSystem system, int slot, StatusHudCompassElement element, StatusHudTextConfig config, bool absolute) : base(system, slot)
        {
            this.element = element;
            this.text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
            this.absolute = absolute;
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
            int direction = (this.mod((int)Math.Round(-this.system.capi.World.Player.CameraYaw * GameMath.RAD2DEG, 0), 360) + 90) % 360;
            this.text.Set(direction + "°");

            this.system.capi.Render.RenderTexture(this.system.textures.compass.TextureId, this.x, this.y, this.w, this.h);

            IShaderProgram prog = this.system.capi.Render.GetEngineShader(EnumShaderProgram.Gui);
            prog.Uniform("rgbaIn", ColorUtil.WhiteArgbVec);
            prog.Uniform("extraGlow", 0);
            prog.Uniform("applyColor", 0);
            prog.Uniform("noTexture", 0f);
            prog.BindTexture2D("tex2d", this.system.textures.compassNeedle.TextureId, 0);

            float angle = this.system.capi.World.Player.CameraYaw;

            if (this.absolute)
            {
                // Show player's absolute direction instead of relation to north.
                angle *= -1;
            }
            else
            {
                angle += StatusHudCompassRenderer.dirAdjust;
            }

            // Use hidden matrix and mesh because this element is never hidden.
            this.hiddenMatrix.Set(this.system.capi.Render.CurrentModelviewMatrix)
                    .Translate(this.x + (this.w / 2f), this.y + (this.h / 2f), 50)
                    .Scale(this.w, this.h, 0)
                    .Scale(0.5f, 0.5f, 0)
                    .RotateZ(angle);

            prog.UniformMatrix("projectionMatrix", this.system.capi.Render.CurrentProjectionMatrix);
            prog.UniformMatrix("modelViewMatrix", this.hiddenMatrix.Values);

            this.system.capi.Render.RenderMesh(this.hiddenMesh);
        }

        private int mod(int n, int m)
        {
            int r = n % m;
            return r < 0 ? r + m : r;
        }

        public override void Dispose()
        {
            this.text.Dispose();
        }
    }
}