using System;

namespace StatusHud
{
    public abstract class StatusHudElement
    {
        public const int offsetX = 457;
        public const int offsetY = 12;

        public const string name = "element";
        public const string desc = "No description available.";

        public virtual string elementName => name;

        protected StatusHudSystem system;
        protected int slot;

        public bool fast;
        public StatusHudPos pos;

        public StatusHudElement(StatusHudSystem system, int slot, bool fast = false)
        {
            this.system = system;
            this.slot = slot;
            this.fast = fast;

            this.pos = new StatusHudPos();
        }

        public void Pos(int halign, int x, int valign, int y)
        {
            this.pos.Set(halign, x, valign, y);
            Pos();
        }

        private void CheckPosBounds()
        {
            int frameWidthMax = this.system.capi.Render.FrameWidth;
            int frameHeightMax = this.system.capi.Render.FrameHeight;
            int frameWidthMin = 0;
            int frameHeightMin = 0;

            switch (this.pos.halign)
            {
                case -1: // Left
                    frameWidthMax /= 2;
                    break;
                case 0: // Centre
                    frameWidthMax /= 2;
                    frameWidthMin = -1 * frameWidthMax;
                    break;
                case 1: // Right
                    break;
                default:
                    break;
            }

            switch (this.pos.valign)
            {
                case -1: // Top
                    frameHeightMax /= 2;
                    break;
                case 0: // Centre
                    frameHeightMax /= 2;
                    frameHeightMin = -1 * frameHeightMax;
                    break;
                case 1: // Bottom
                    break;
                default:
                    break;
            }

            this.pos.x = Math.Min(frameWidthMax, Math.Max(frameWidthMin, this.pos.x));
            this.pos.y = Math.Min(frameHeightMax, Math.Max(frameHeightMin, this.pos.y));
        }

        public void Pos()
        {
            CheckPosBounds();
            this.getRenderer().Pos(this.pos);
        }

        public bool Repos()
        {
            //int sign = Math.Sign(this.slot);

            //this.pos.Set(StatusHudPos.halignCenter,
            //        (sign * StatusHudElement.offsetX) + (int)((this.slot - sign) * (this.system.textures.size * 1.5f)),
            //        StatusHudPos.valignBottom,
            //        StatusHudElement.offsetY);

            this.pos.Set(0,0,0,0);

            this.getRenderer().Pos(this.pos);
            return true;
        }

        public void Ping()
        {
            this.getRenderer().Ping();
        }

        public abstract void Tick();
        public abstract void Dispose();

        public abstract StatusHudRenderer getRenderer();
    }
}