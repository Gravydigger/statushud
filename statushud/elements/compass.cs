using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudCompassElement : StatusHudElement
    {
        public new const string name = "compass";
        protected const string textKey = "shud-compass";

        public static readonly string[] compassBearingOptions = { "relative", "absolute" };
        private string compassBearing;

        public override string ElementName => name;
        public override string ElementOption => compassBearing;

        protected WeatherSystemBase weatherSystem;
        protected StatusHudCompassRenderer renderer;

        public StatusHudCompassElement(StatusHudSystem system) : base(system)
        {
            weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            renderer = new StatusHudCompassRenderer(this.system, this);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            compassBearing = "relative";
        }

        public override StatusHudRenderer GetRenderer()
        {
            return renderer;
        }

        public virtual string GetTextKey()
        {
            return textKey;
        }

        public override void ConfigOptions(string value)
        {
            foreach (var option in compassBearingOptions)
            {
                if (option == value)
                {
                    compassBearing = value;
                }
            }
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

        public StatusHudCompassRenderer(StatusHudSystem system, StatusHudCompassElement element) : base(system)
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
            text.Pos(pos);
        }

        protected override void Render()
        {
            int direction = (Modulo((int)Math.Round(-system.capi.World.Player.CameraYaw * GameMath.RAD2DEG), 360) + 180) % 360;
            text.Set(direction + "°");

            system.capi.Render.RenderTexture(system.textures.texturesDict["compass"].TextureId, x, y, w, h);

            IShaderProgram prog = system.capi.Render.GetEngineShader(EnumShaderProgram.Gui);
            prog.Uniform("rgbaIn", ColorUtil.WhiteArgbVec);
            prog.Uniform("extraGlow", 0);
            prog.Uniform("applyColor", 0);
            prog.Uniform("noTexture", 0f);
            prog.BindTexture2D("tex2d", system.textures.texturesDict["compass_needle"].TextureId, 0);

            float angle = system.capi.World.Player.CameraYaw;

            if (element.ElementOption == "absolute")
            {
                // Show player's absolute direction instead of relation to north.
                angle = GameMath.PIHALF - angle;
            }
            else
            {
                angle += GameMath.PIHALF;
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

        private static int Modulo(int n, int m)
        {
            return ((n % m) + m) % m;
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}