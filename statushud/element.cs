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
            this.pos.set(halign, x, valign, y);
            this.getRenderer().Pos(this.pos);
        }

        public void Pos()
        {
            this.getRenderer().Pos(this.pos);
        }

        public bool Repos()
        {
            int sign = Math.Sign(this.slot);

            this.pos.set(StatusHudPos.halignCenter,
                    (sign * StatusHudElement.offsetX) + (int)((this.slot - sign) * (this.system.textures.size * 1.5f)),
                    StatusHudPos.valignBottom,
                    StatusHudElement.offsetY);

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