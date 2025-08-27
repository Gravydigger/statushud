namespace StatusHud
{
    public class StatusHudPos
    {
        public enum HorzAlign
        {
            Left = -1,
            Center,
            Right,
        }

        public enum VertAlign
        {
            Top = -1,
            Middle,
            Bottom
        }

        public HorzAlign horzAlign;
        public int x;
        public VertAlign vertAlign;
        public int y;

        public void Set(HorzAlign horzAlign, int x, VertAlign vertAlign, int y)
        {
            this.horzAlign = horzAlign;
            this.x = x;
            this.vertAlign = vertAlign;
            this.y = y;
        }

        public void Set(StatusHudPos pos)
        {
            Set(pos.horzAlign, pos.x, pos.vertAlign, pos.y);
        }
    }
}