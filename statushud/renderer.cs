using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace StatusHud
{
    public abstract class StatusHudRenderer : IRenderer
    {
        private const float pingScaleInit = 8;
        private const int pingTimeInit = 30;
        private const int pingTimeHalf = (int)(pingTimeInit / 2f);

        protected static readonly Vec4f hiddenRgba = new(1, 1, 1, 0.25f);

        public double RenderOrder => 1;
        public int RenderRange => 0;

        protected StatusHudSystem system;

        // Position values.
        protected StatusHudPos pos;
        // Text Element
        protected StatusHudText text;

        // Render values.
        protected float x;
        protected float y;
        protected float w;
        protected float h;

        private float scale;
        private float frameWidth;
        private float frameHeight;

        // Ping.
        private bool ping;
        private readonly MeshRef pingMesh;
        private readonly Matrixf pingMatrix;
        private readonly Vec4f pingRgba;
        private int pingTime;
        private float pingScale;

        // Hidden.
        protected MeshRef hiddenMesh;
        protected Matrixf hiddenMatrix;

        public StatusHudRenderer(StatusHudSystem system)
        {
            this.system = system;

            pos = new StatusHudPos();
            text = new StatusHudText(system.capi, "", system.Config);

            MeshData quadMesh = QuadMeshUtil.GetQuad();

            ping = false;
            pingMesh = system.capi.Render.UploadMesh(quadMesh);
            pingMatrix = new Matrixf();
            pingRgba = new Vec4f(1, 1, 1, 0);
            pingTime = 0;
            pingScale = 1;

            hiddenMesh = system.capi.Render.UploadMesh(quadMesh);
            hiddenMatrix = new Matrixf();
        }

        public void Pos(StatusHudPos pos)
        {
            this.pos.Set(pos);
            Update();
        }

        public void Ping()
        {
            ping = true;
            pingTime = pingTimeInit;
            pingScale = pingScaleInit;
        }
        public void UpdateRender()
        {
            Update();
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (scale != RuntimeEnv.GUIScale)
            {
                // GUI scale changed.
                Update();
            }

            if (frameWidth != system.capi.Render.FrameWidth
                    || frameHeight != system.capi.Render.FrameHeight)
            {
                // Resolution changed.
                Update();
            }

            if (ping)
            {
                pingScale = Math.Max(1 + ((pingTime - pingTimeHalf) / (float)pingTimeHalf * pingScaleInit), 1);
                pingRgba.A = (float)Math.Sin((pingTimeInit - pingTime) / (float)pingTimeInit * Math.PI);

                IShaderProgram prog = system.capi.Render.GetEngineShader(EnumShaderProgram.Gui);
                prog.Uniform("rgbaIn", pingRgba);
                prog.Uniform("extraGlow", 0);
                prog.Uniform("applyColor", 0);
                prog.Uniform("noTexture", 0f);
                prog.BindTexture2D("tex2d", system.textures.texturesDict["ping"].TextureId, 0);

                float w = (float)GuiElement.scaled(system.textures.texturesDict["ping"].Width) * pingScale;
                float h = (float)GuiElement.scaled(system.textures.texturesDict["ping"].Height) * pingScale;

                pingMatrix.Set(system.capi.Render.CurrentModelviewMatrix)
                        .Translate(x + (this.w / 2f), y + (this.h / 2f), 50)
                        .Scale(w, h, 0)
                        .Scale(0.75f, 0.75f, 0);

                prog.UniformMatrix("projectionMatrix", system.capi.Render.CurrentProjectionMatrix);
                prog.UniformMatrix("modelViewMatrix", pingMatrix.Values);

                system.capi.Render.RenderMesh(pingMesh);

                pingTime--;
                if (pingTime <= 0)
                {
                    ping = false;
                }
            }

            Render();
        }

        public virtual void Dispose()
        {
            pingMesh.Dispose();
            hiddenMesh.Dispose();
        }

        public abstract void Reload();

        protected abstract void Render();

        protected virtual void Update()
        {
            w = SolveW();
            h = SolveH();

            x = SolveX(w);
            y = SolveY(h);

            scale = RuntimeEnv.GUIScale;
            frameWidth = system.capi.Render.FrameWidth;
            frameHeight = system.capi.Render.FrameHeight;

            // Keep inside frame.
            if (x < 0)
            {
                x = 0;
            }
            else if (x + w > frameWidth)
            {
                x = frameWidth - w;
            }

            if (y < 0)
            {
                y = 0;
            }
            else if (y + h > frameHeight)
            {
                y = frameHeight - h;
            }
        }

        protected float SolveX(float w)
        {
            return pos.horzAlign switch
            {
                StatusHudPos.HorzAlign.Left => (float)GuiElement.scaled(pos.x),
                StatusHudPos.HorzAlign.Center => (float)((system.capi.Render.FrameWidth / 2f) - (w / 2f) + GuiElement.scaled(pos.x)),
                StatusHudPos.HorzAlign.Right => (float)(system.capi.Render.FrameWidth - w - GuiElement.scaled(pos.x)),
                _ => 0,
            };
        }

        protected float SolveY(float h)
        {
            return pos.vertAlign switch
            {
                StatusHudPos.VertAlign.Top => (float)GuiElement.scaled(pos.y),
                StatusHudPos.VertAlign.Middle => (float)((system.capi.Render.FrameHeight / 2f) - (h / 2f) + GuiElement.scaled(pos.y)),
                StatusHudPos.VertAlign.Bottom => (float)(system.capi.Render.FrameHeight - h - GuiElement.scaled(pos.y)),
                _ => 0,
            };
        }

        protected float SolveW()
        {
            return (float)GuiElement.scaled(system.Config.iconSize);
        }

        protected float SolveH()
        {
            return (float)GuiElement.scaled(system.Config.iconSize);
        }

        protected void RenderHidden(int textureId)
        {
            IShaderProgram prog = system.capi.Render.GetEngineShader(EnumShaderProgram.Gui);
            prog.Uniform("rgbaIn", hiddenRgba);
            prog.Uniform("extraGlow", 0);
            prog.Uniform("applyColor", 0);
            prog.Uniform("noTexture", 0f);
            prog.BindTexture2D("tex2d", textureId, 0);

            hiddenMatrix.Set(system.capi.Render.CurrentModelviewMatrix)
                    .Translate(x + (w / 2f), y + (h / 2f), 50)
                    .Scale(w, h, 0)
                    .Scale(0.5f, 0.5f, 0);

            prog.UniformMatrix("projectionMatrix", system.capi.Render.CurrentProjectionMatrix);
            prog.UniformMatrix("modelViewMatrix", hiddenMatrix.Values);

            system.capi.Render.RenderMesh(hiddenMesh);
        }
    }
}