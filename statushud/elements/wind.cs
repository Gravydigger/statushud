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
        public new const string desc = "The 'wind' element displays the current wind speed (in %) at the player's position, and wind direction relative to the player.";
        protected const string textKey = "shud-wind";

        public override string elementName => name;

        protected WeatherSystemBase weatherSystem;
        protected StatusHudWindRenderer renderer;

        public bool directional;
        public float dirAngle;

        public StatusHudWindElement(StatusHudSystem system, int slot, StatusHudTextConfig config) : base(system, slot)
        {
            this.weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            this.renderer = new StatusHudWindRenderer(system, slot, this, config);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);

            this.directional = false;
            this.dirAngle = 0;
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
            EntityPlayer entity = this.system.capi.World.Player.Entity;

            double speed = this.weatherSystem.WeatherDataSlowAccess.GetWindSpeed(entity.Pos.AsBlockPos.ToVec3d());
            this.renderer.setText((int)Math.Round(speed * 100, 0) + "%");

            Vec3d dir = this.system.capi.World.BlockAccessor.GetWindSpeedAt(entity.Pos.AsBlockPos);
            if (speed != 0 && dir.Length() != 0)
            {
                this.dirAngle = (float)Math.Atan2(-dir.Z, dir.X);

                this.directional = true;
            }
            else
            {
                // No wind direction.
                this.directional = false;
            }
        }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudWindRenderer : StatusHudRenderer
    {
        protected StatusHudWindElement element;
        protected StatusHudText text;

        protected const float dirAdjust = 90 * GameMath.DEG2RAD;

        public StatusHudWindRenderer(StatusHudSystem system, int slot, StatusHudWindElement element, StatusHudTextConfig config) : base(system, slot)
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
            if (this.element.directional)
            {
                this.system.capi.Render.RenderTexture(this.system.textures.windDir.TextureId, this.x, this.y, this.w, this.h);

                IShaderProgram prog = this.system.capi.Render.GetEngineShader(EnumShaderProgram.Gui);
                prog.Uniform("rgbaIn", ColorUtil.WhiteArgbVec);
                prog.Uniform("extraGlow", 0);
                prog.Uniform("applyColor", 0);
                prog.Uniform("noTexture", 0f);
                prog.BindTexture2D("tex2d", this.system.textures.windDirArrow.TextureId, 0);

                float angle = this.element.dirAngle - this.system.capi.World.Player.CameraYaw + StatusHudWindRenderer.dirAdjust;

                // Use hidden matrix and mesh because this element is never hidden.
                this.hiddenMatrix.Set(this.system.capi.Render.CurrentModelviewMatrix)
                        .Translate(this.x + (this.w / 2f), this.y + (this.h / 2f), 50)
                        .Scale(this.w, this.h, 0)
                        .Scale(0.5f, 0.5f, 0)
                        .RotateZ(-angle);

                prog.UniformMatrix("projectionMatrix", this.system.capi.Render.CurrentProjectionMatrix);
                prog.UniformMatrix("modelViewMatrix", this.hiddenMatrix.Values);

                this.system.capi.Render.RenderMesh(this.hiddenMesh);
            }
            else
            {
                this.system.capi.Render.RenderTexture(this.system.textures.wind.TextureId, this.x, this.y, this.w, this.h);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            this.text.Dispose();
        }
    }
}