using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace StatusHud
{
    public abstract class StatusHudRenderer : IRenderer
    {
        protected const float pingScaleInit = 8;
        protected const int pingTimeInit = 30;
        protected const int pingTimeHalf = (int)(pingTimeInit / 2f);

        protected static readonly Vec4f hiddenRgba = new Vec4f(1, 1, 1, 0.25f);

        public double RenderOrder
        {
            get
            {
                return 1;
            }
        }
        public int RenderRange
        {
            get
            {
                return 0;
            }
        }

        protected StatusHudSystem system;
        protected int slot;

        // Position values.
        protected StatusHudPos pos;

        // Render values.
        protected float x;
        protected float y;
        protected float w;
        protected float h;

        protected float scale;
        protected float frameWidth;
        protected float frameHeight;

        // Ping.
        protected bool ping;
        protected MeshRef pingMesh;
        protected Matrixf pingMatrix;
        protected Vec4f pingRgba;
        protected int pingTime;
        protected float pingScale;

        // Hidden.
        protected MeshRef hiddenMesh;
        protected Matrixf hiddenMatrix;

        public StatusHudRenderer(StatusHudSystem system, int slot)
        {
            this.system = system;
            this.slot = slot;

            pos = new StatusHudPos();

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
            switch (pos.halign)
            {
                case StatusHudPos.halignLeft:
                    return (float)GuiElement.scaled(pos.x);
                case StatusHudPos.halignCenter:
                    return (float)((system.capi.Render.FrameWidth / 2f) - (w / 2f) + GuiElement.scaled(pos.x));
                case StatusHudPos.halignRight:
                    return (float)(system.capi.Render.FrameWidth - w - GuiElement.scaled(pos.x));
            }
            return 0;
        }

        protected float SolveY(float h)
        {
            switch (pos.valign)
            {
                case StatusHudPos.valignTop:
                    return (float)GuiElement.scaled(pos.y);
                case StatusHudPos.valignMiddle:
                    return (float)((system.capi.Render.FrameHeight / 2f) - (h / 2f) + GuiElement.scaled(pos.y));
                case StatusHudPos.valignBottom:
                    return (float)(system.capi.Render.FrameHeight - h - GuiElement.scaled(pos.y));
            }
            return 0;
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
            prog.Uniform("rgbaIn", StatusHudRenderer.hiddenRgba);
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