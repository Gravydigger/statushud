using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudWindElement : StatusHudElement
    {
        public new const string name = "wind";
        protected const string textKey = "shud-wind";

        public override string ElementName => name;

        protected WeatherSystemBase weatherSystem;
        protected StatusHudWindRenderer renderer;

        public bool directional;
        public float dirAngle;

        public StatusHudWindElement(StatusHudSystem system, StatusHudConfig config) : base(system)
        {
            weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            renderer = new StatusHudWindRenderer(system, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            directional = false;
            dirAngle = 0;
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
            EntityPlayer entity = system.capi.World.Player.Entity;

            double speed = weatherSystem.WeatherDataSlowAccess.GetWindSpeed(entity.Pos.AsBlockPos.ToVec3d());
            renderer.SetText((int)Math.Round(speed * 100, 0) + "%");

            Vec3d dir = system.capi.World.BlockAccessor.GetWindSpeedAt(entity.Pos.AsBlockPos);
            if (speed != 0 && dir.Length() != 0)
            {
                dirAngle = (float)Math.Atan2(-dir.Z, dir.X);

                directional = true;
            }
            else
            {
                // No wind direction.
                directional = false;
            }
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudWindRenderer : StatusHudRenderer
    {
        protected StatusHudWindElement element;
        protected StatusHudText text;

        public StatusHudWindRenderer(StatusHudSystem system, StatusHudWindElement element, StatusHudConfig config) : base(system)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.element.GetTextKey(), config);
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
            if (element.directional)
            {
                system.capi.Render.RenderTexture(system.textures.texturesDict["wind_dir"].TextureId, x, y, w, h);

                IShaderProgram prog = system.capi.Render.GetEngineShader(EnumShaderProgram.Gui);
                prog.Uniform("rgbaIn", ColorUtil.WhiteArgbVec);
                prog.Uniform("extraGlow", 0);
                prog.Uniform("applyColor", 0);
                prog.Uniform("noTexture", 0f);
                prog.BindTexture2D("tex2d", system.textures.texturesDict["wind_dir_arrow"].TextureId, 0);

                float angle = element.dirAngle - system.capi.World.Player.CameraYaw + GameMath.PI;

                // Use hidden matrix and mesh because this element is never hidden.
                hiddenMatrix.Set(system.capi.Render.CurrentModelviewMatrix)
                        .Translate(x + (w / 2f), y + (h / 2f), 50)
                        .Scale(w, h, 0)
                        .Scale(0.5f, 0.5f, 0)
                        .RotateZ(-angle);

                prog.UniformMatrix("projectionMatrix", system.capi.Render.CurrentProjectionMatrix);
                prog.UniformMatrix("modelViewMatrix", hiddenMatrix.Values);

                system.capi.Render.RenderMesh(hiddenMesh);
            }
            else
            {
                system.capi.Render.RenderTexture(system.textures.texturesDict["wind"].TextureId, x, y, w, h);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            text.Dispose();
        }
    }
}