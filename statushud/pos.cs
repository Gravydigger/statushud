namespace StatusHud;

public class StatusHudPos
{
    public enum HorizAlign
    {
        Left = -1,
        Center,
        Right
    }

    public enum VertAlign
    {
        Top = -1,
        Middle,
        Bottom
    }

    public HorizAlign horizAlign;
    public VertAlign vertAlign;
    public int x;
    public int y;

    public void Set(HorizAlign horizAlign, int x, VertAlign vertAlign, int y)
    {
        this.horizAlign = horizAlign;
        this.x = x;
        this.vertAlign = vertAlign;
        this.y = y;
    }

    public void Set(StatusHudPos pos)
    {
        Set(pos.horizAlign, pos.x, pos.vertAlign, pos.y);
    }
}