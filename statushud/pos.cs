namespace StatusHud
{
    public class StatusHudPos
    {
        public const int halignLeft = -1;
        public const int halignCenter = 0;
        public const int halignRight = 1;
        public const int valignTop = -1;
        public const int valignMiddle = 0;
        public const int valignBottom = 1;

        public int halign;
        public int x;
        public int valign;
        public int y;

        public void set(int halign, int x, int valign, int y)
        {
            this.halign = halign;
            this.x = x;
            this.valign = valign;
            this.y = y;
        }

        public void set(StatusHudPos pos)
        {
            this.halign = pos.halign;
            this.x = pos.x;
            this.valign = pos.valign;
            this.y = pos.y;
        }
    }
}