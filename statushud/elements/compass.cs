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

        public override string elementName => name;

        protected WeatherSystemBase weatherSystem;
        protected StatusHudCompassRenderer renderer;

        public StatusHudCompassElement(StatusHudSystem system, int slot, StatusHudConfig config, bool absolute) : base(system, slot)
        {
            weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            renderer = new StatusHudCompassRenderer(this.system, slot, this, config, absolute);
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

        public override void Tick() { }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudCompassRenderer : StatusHudRenderer
    {
        protected StatusHudCompassElement element;
        protected StatusHudText text;
        protected bool absolute;

        protected const float dirAdjust = 180 * GameMath.DEG2RAD;

        public StatusHudCompassRenderer(StatusHudSystem system, int slot, StatusHudCompassElement element, StatusHudConfig config, bool absolute) : base(system, slot)
        {
            this.element = element;
            this.absolute = absolute;
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
            int direction = (mod((int)Math.Round(-system.capi.World.Player.CameraYaw * GameMath.RAD2DEG, 0), 360) + 90) % 360;
            text.Set(direction + "°");

            system.capi.Render.RenderTexture(system.textures.texturesDict["compass"].TextureId, x, y, w, h);

            IShaderProgram prog = system.capi.Render.GetEngineShader(EnumShaderProgram.Gui);
            prog.Uniform("rgbaIn", ColorUtil.WhiteArgbVec);
            prog.Uniform("extraGlow", 0);
            prog.Uniform("applyColor", 0);
            prog.Uniform("noTexture", 0f);
            prog.BindTexture2D("tex2d", system.textures.texturesDict["compass_needle"].TextureId, 0);

            float angle = system.capi.World.Player.CameraYaw;

            if (absolute)
            {
                // Show player's absolute direction instead of relation to north.
                angle *= -1;
            }
            else
            {
                angle += StatusHudCompassRenderer.dirAdjust;
            }

            // Use hidden matrix and mesh because this element is never hidden.
            hiddenMatrix.Set(system.capi.Render.CurrentModelviewMatrix)
                    .Translate(x + (w / 2f), y + (h / 2f), 50)
                    .Scale(w, h, 0)
                    .Scale(0.5f, 0.5f, 0)
                    .RotateZ(angle);

            prog.UniformMatrix("projectionMatrix", system.capi.Render.CurrentProjectionMatrix);
            prog.UniformMatrix("modelViewMatrix", hiddenMatrix.Values);

            system.capi.Render.RenderMesh(hiddenMesh);
        }

        private int mod(int n, int m)
        {
            int r = n % m;
            return r < 0 ? r + m : r;
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}