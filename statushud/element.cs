using System;

namespace StatusHud;

public abstract class StatusHudElement
{
    private const string name = "element";

    public readonly bool fast;
    public readonly StatusHudPos pos;

    protected readonly StatusHudSystem system;

    protected StatusHudElement(StatusHudSystem system, bool fast = false)
    {
        this.system = system;
        this.fast = fast;

        pos = new StatusHudPos();
    }

    public virtual string ElementName => name;
    public virtual string ElementOption => "";

    public void SetPos(StatusHudPos.HorizAlign horizAlign, int x, StatusHudPos.VertAlign vertAlign, int y)
    {
        pos.Set(horizAlign, x, vertAlign, y);
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

    public bool Repos()
    {
        pos.Set(0, 0, 0, 0);

        GetRenderer().Pos(pos);
        return true;
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