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

            this.pos = new StatusHudPos();

            MeshData quadMesh = QuadMeshUtil.GetQuad();

            this.ping = false;
            this.pingMesh = system.capi.Render.UploadMesh(quadMesh);
            this.pingMatrix = new Matrixf();
            this.pingRgba = new Vec4f(1, 1, 1, 0);
            this.pingTime = 0;
            this.pingScale = 1;

            this.hiddenMesh = system.capi.Render.UploadMesh(quadMesh);
            this.hiddenMatrix = new Matrixf();
        }

        public void Pos(StatusHudPos pos)
        {
            this.pos.Set(pos);
            this.update();
        }

        public void Ping()
        {
            this.ping = true;
            this.pingTime = pingTimeInit;
            this.pingScale = pingScaleInit;
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (this.scale != RuntimeEnv.GUIScale)
            {
                // GUI scale changed.
                this.update();
            }

            if (this.frameWidth != this.system.capi.Render.FrameWidth
                    || this.frameHeight != this.system.capi.Render.FrameHeight)
            {
                // Resolution changed.
                this.update();
            }

            if (this.ping)
            {
                this.pingScale = Math.Max(1 + (((this.pingTime - pingTimeHalf) / (float)pingTimeHalf) * pingScaleInit), 1);
                this.pingRgba.A = (float)Math.Sin(((pingTimeInit - this.pingTime) / (float)pingTimeInit) * Math.PI);

                IShaderProgram prog = this.system.capi.Render.GetEngineShader(EnumShaderProgram.Gui);
                prog.Uniform("rgbaIn", this.pingRgba);
                prog.Uniform("extraGlow", 0);
                prog.Uniform("applyColor", 0);
                prog.Uniform("noTexture", 0f);
                prog.BindTexture2D("tex2d", this.system.textures.ping.TextureId, 0);

                float w = (float)GuiElement.scaled(this.system.textures.ping.Width) * this.pingScale;
                float h = (float)GuiElement.scaled(this.system.textures.ping.Height) * this.pingScale;

                this.pingMatrix.Set(this.system.capi.Render.CurrentModelviewMatrix)
                        .Translate(this.x + (this.w / 2f), this.y + (this.h / 2f), 50)
                        .Scale(w, h, 0)
                        .Scale(0.75f, 0.75f, 0);

                prog.UniformMatrix("projectionMatrix", this.system.capi.Render.CurrentProjectionMatrix);
                prog.UniformMatrix("modelViewMatrix", this.pingMatrix.Values);

                this.system.capi.Render.RenderMesh(this.pingMesh);

                this.pingTime--;
                if (this.pingTime <= 0)
                {
                    this.ping = false;
                }
            }

            this.render();
        }

        public virtual void Dispose()
        {
            this.pingMesh.Dispose();
            this.hiddenMesh.Dispose();
        }

        public abstract void Reload(StatusHudTextConfig config);

        protected abstract void render();

        protected virtual void update()
        {
            this.w = this.solveW();
            this.h = this.solveH();

            this.x = this.solveX(this.w);
            this.y = this.solveY(this.h);

            this.scale = RuntimeEnv.GUIScale;
            this.frameWidth = this.system.capi.Render.FrameWidth;
            this.frameHeight = this.system.capi.Render.FrameHeight;

            // Keep inside frame.
            if (this.x < 0)
            {
                this.x = 0;
            }
            else if (this.x + this.w > this.frameWidth)
            {
                this.x = this.frameWidth - this.w;
            }

            if (this.y < 0)
            {
                this.y = 0;
            }
            else if (this.y + this.h > this.frameHeight)
            {
                this.y = this.frameHeight - this.h;
            }
        }

        protected float solveX(float w)
        {
            switch (this.pos.halign)
            {
                case StatusHudPos.halignLeft:
                    return (float)GuiElement.scaled(this.pos.x);
                case StatusHudPos.halignCenter:
                    return (float)((this.system.capi.Render.FrameWidth / 2f) - (w / 2f) + GuiElement.scaled(this.pos.x));
                case StatusHudPos.halignRight:
                    return (float)(this.system.capi.Render.FrameWidth - w - GuiElement.scaled(this.pos.x));
            }
            return 0;
        }

        protected float solveY(float h)
        {
            switch (this.pos.valign)
            {
                case StatusHudPos.valignTop:
                    return (float)GuiElement.scaled(this.pos.y);
                case StatusHudPos.valignMiddle:
                    return (float)((this.system.capi.Render.FrameHeight / 2f) - (h / 2f) + GuiElement.scaled(this.pos.y));
                case StatusHudPos.valignBottom:
                    return (float)(this.system.capi.Render.FrameHeight - h - GuiElement.scaled(this.pos.y));
            }
            return 0;
        }

        protected float solveW()
        {
            return (float)GuiElement.scaled(this.system.textures.size);
        }

        protected float solveH()
        {
            return (float)GuiElement.scaled(this.system.textures.size);
        }

        protected void renderHidden(int textureId)
        {
            IShaderProgram prog = this.system.capi.Render.GetEngineShader(EnumShaderProgram.Gui);
            prog.Uniform("rgbaIn", StatusHudRenderer.hiddenRgba);
            prog.Uniform("extraGlow", 0);
            prog.Uniform("applyColor", 0);
            prog.Uniform("noTexture", 0f);
            prog.BindTexture2D("tex2d", textureId, 0);

            this.hiddenMatrix.Set(this.system.capi.Render.CurrentModelviewMatrix)
                    .Translate(this.x + (this.w / 2f), this.y + (this.h / 2f), 50)
                    .Scale(w, h, 0)
                    .Scale(0.5f, 0.5f, 0);

            prog.UniformMatrix("projectionMatrix", this.system.capi.Render.CurrentProjectionMatrix);
            prog.UniformMatrix("modelViewMatrix", this.hiddenMatrix.Values);

            this.system.capi.Render.RenderMesh(this.hiddenMesh);
        }
    }
}