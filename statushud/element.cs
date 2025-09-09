using System;

namespace StatusHud;

public abstract class StatusHudElement(StatusHudSystem system, bool fast = false)
{
    private const string name = "element";

    public readonly bool fast = fast;
    public readonly StatusHudPos pos = new();

    protected readonly StatusHudSystem system = system;

    public virtual string ElementName => name;
    public virtual string ElementOption => "";
    public virtual string[] ElementOptionList => null;

    public void SetPos(StatusHudPos.HorizAlign horizAlign, int x, StatusHudPos.VertAlign vertAlign, int y, StatusHudPos.TextAlign textAlign, int orientOffset)
    {
        pos.Set(horizAlign, x, vertAlign, y, textAlign, orientOffset);
        SetPos();
    }

    private void CheckPosBounds()
    {
        int frameWidthMax = system.capi.Render.FrameWidth;
        int frameHeightMax = system.capi.Render.FrameHeight;
        int frameWidthMin = 0;
        int frameHeightMin = 0;

        switch (pos.horizAlign)
        {
            case StatusHudPos.HorizAlign.Left:
                frameWidthMax /= 2;
                break;
            case StatusHudPos.HorizAlign.Center:
                frameWidthMax /= 2;
                frameWidthMin = -1 * frameWidthMax;
                break;
            case StatusHudPos.HorizAlign.Right:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (pos.vertAlign)
        {
            case StatusHudPos.VertAlign.Top:
                frameHeightMax /= 2;
                break;
            case StatusHudPos.VertAlign.Middle:
                frameHeightMax /= 2;
                frameHeightMin = -1 * frameHeightMax;
                break;
            case StatusHudPos.VertAlign.Bottom:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        pos.x = Math.Min(frameWidthMax, Math.Max(frameWidthMin, pos.x));
        pos.y = Math.Min(frameHeightMax, Math.Max(frameHeightMin, pos.y));
    }

    public void SetPos()
    {
        CheckPosBounds();
        GetRenderer().Pos(pos);
    }

    public void Repos()
    {
        pos.Set(StatusHudPos.HorizAlign.Center, 0, StatusHudPos.VertAlign.Middle, 0, StatusHudPos.TextAlign.Up, 0);

        GetRenderer().Pos(pos);
    }

    public void Ping()
    {
        GetRenderer().Ping();
    }

    public virtual void ConfigOptions(string value)
    {
    }

    public abstract void Tick();
    public abstract void Dispose();

    public abstract StatusHudRenderer GetRenderer();
}