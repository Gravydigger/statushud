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

            pos = new StatusHudPos();
        }

        public void Pos(int halign, int x, int valign, int y)
        {
            pos.Set(halign, x, valign, y);
            Pos();
        }

        private void CheckPosBounds()
        {
            int frameWidthMax = system.capi.Render.FrameWidth;
            int frameHeightMax = system.capi.Render.FrameHeight;
            int frameWidthMin = 0;
            int frameHeightMin = 0;

            switch (pos.halign)
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

            switch (pos.valign)
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

            pos.x = Math.Min(frameWidthMax, Math.Max(frameWidthMin, pos.x));
            pos.y = Math.Min(frameHeightMax, Math.Max(frameHeightMin, pos.y));
        }

        public void Pos()
        {
            CheckPosBounds();
            getRenderer().Pos(pos);
        }

        public bool Repos()
        {
            pos.Set(0,0,0,0);

            getRenderer().Pos(pos);
            return true;
        }

        public void Ping()
        {
            getRenderer().Ping();
        }

        public abstract void Tick();
        public abstract void Dispose();

        public abstract StatusHudRenderer getRenderer();
    }
}