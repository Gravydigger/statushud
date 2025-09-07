using System;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace StatusHud;

public sealed class StatusHudText : HudElement
{
    private const string dialogNamePrefix = "d-";
    private readonly Vec4f colour = new(0.91f, 0.87f, 0.81f, 1);
    private readonly StatusHudConfig config;

    private readonly string dialogName;

    private readonly string key;
    private CairoFont font;
    private float height;
    private GuiElementDynamicText text;
    private float width;

    public StatusHudText(ICoreClientAPI capi, string key, StatusHudConfig config) : base(capi)
    {
        this.key = key;
        this.config = config;

        dialogName = dialogNamePrefix + this.key;
        font = InitFont();
    }

    public override EnumDialogType DialogType => EnumDialogType.HUD;
    public override bool Focusable => false;
    public override double DrawOrder => 0;

    public override void Dispose()
    {
        TryClose();
        base.Dispose();
    }

    // Fixed an issue where if the boundary box of the text element was overlapping with another, it would eat
    // the mouse event (i.e. if on the crosshair, it would not allow you to interact with blocks). 
    public override bool ShouldReceiveMouseEvents()
    {
        return false;
    }

    public void ReloadText(StatusHudPos pos)
    {
        font = InitFont();
        SetPos(pos);
    }

    public void SetPos(StatusHudPos pos)
    {
        EnumDialogArea area = EnumDialogArea.None;
        float x = pos.x;
        float y = pos.y;

        // Area.
        switch (pos.horizAlign)
        {
            case StatusHudPos.HorizAlign.Left:
            {
                switch (pos.vertAlign)
                {
                    case StatusHudPos.VertAlign.Top:
                    {
                        area = EnumDialogArea.LeftTop;
                        break;
                    }
                    case StatusHudPos.VertAlign.Middle:
                    {
                        area = EnumDialogArea.LeftMiddle;
                        break;
                    }
                    case StatusHudPos.VertAlign.Bottom:
                    {
                        area = EnumDialogArea.LeftBottom;
                        break;
                    }
                }
                break;
            }
            case StatusHudPos.HorizAlign.Center:
            {
                switch (pos.vertAlign)
                {
                    case StatusHudPos.VertAlign.Top:
                    {
                        area = EnumDialogArea.CenterTop;
                        break;
                    }
                    case StatusHudPos.VertAlign.Middle:
                    {
                        area = EnumDialogArea.CenterMiddle;
                        break;
                    }
                    case StatusHudPos.VertAlign.Bottom:
                    {
                        area = EnumDialogArea.CenterBottom;
                        break;
                    }
                }
                break;
            }
            case StatusHudPos.HorizAlign.Right:
            {
                switch (pos.vertAlign)
                {
                    case StatusHudPos.VertAlign.Top:
                    {
                        area = EnumDialogArea.RightTop;
                        break;
                    }
                    case StatusHudPos.VertAlign.Middle:
                    {
                        area = EnumDialogArea.RightMiddle;
                        break;
                    }
                    case StatusHudPos.VertAlign.Bottom:
                    {
                        area = EnumDialogArea.RightBottom;
                        break;
                    }
                }
                break;
            }
        }

        float iconSize = StatusHudSystem.iconSize * config.elementScale;
        float iconHalf = iconSize / 2f;
        float frameWidth = capi.Render.FrameWidth;
        float frameHeight = capi.Render.FrameHeight;
        width = iconSize * 3;
        height = iconSize;

        // X.
        switch (pos.horizAlign)
        {
            case StatusHudPos.HorizAlign.Left:
            {
                x = GameMath.Clamp(x, 0, frameWidth - iconSize);

                x -= (float)Math.Round((width - iconSize) / 2f);
                break;
            }
            case StatusHudPos.HorizAlign.Center:
            {
                x = GameMath.Clamp(x, -(frameWidth / 2) + iconHalf, frameWidth / 2 - iconHalf);
                break;
            }
            case StatusHudPos.HorizAlign.Right:
            {
                x = GameMath.Clamp(x, 0, frameWidth - iconSize);

                x = -x + (float)Math.Round((width - iconSize) / 2f);
                break;
            }
        }

        // Y.
        switch (pos.vertAlign)
        {
            case StatusHudPos.VertAlign.Top:
            {
                y = GameMath.Clamp(y, 0, frameHeight - iconSize);
                break;
            }
            case StatusHudPos.VertAlign.Middle:
            {
                y = GameMath.Clamp(y, -(frameHeight / 2), frameHeight / 2 - iconHalf);
                break;
            }
            case StatusHudPos.VertAlign.Bottom:
            {
                y = GameMath.Clamp(y, 0, frameHeight);

                y = -y;
                break;
            }
        }

        Compose(area, x, y);
    }

    public void Set(string value)
    {
        text.Text = value;
        text.RecomposeText();
    }

    private void Compose(EnumDialogArea area, float x, float y)
    {
        const int offsetX = 0;
        int offsetY = (int)-(StatusHudSystem.iconSize * config.elementScale / 1.5f);

        SingleComposer?.Dispose();
        ElementBounds dialogBounds = ElementBounds.Fixed(area, x + offsetX, y + offsetY, width, height);
        ElementBounds textBounds = ElementBounds.Fixed(EnumDialogArea.CenterTop, 0, 0, width, height);
        SingleComposer = capi.Gui.CreateCompo(dialogName, dialogBounds)
            .AddDynamicText("", font, textBounds, key)
            .Compose();
        text = SingleComposer.GetDynamicText(key);
        TryOpen();
    }

    private CairoFont InitFont()
    {
        const EnumTextOrientation align = EnumTextOrientation.Center;

        return new CairoFont()
            .WithColor([colour.R, colour.G, colour.B, colour.A])
            .WithFont(GuiStyle.StandardFontName)
            .WithFontSize(StatusHudSystem.iconSize * config.elementScale / 2f)
            .WithWeight(FontWeight.Bold)
            .WithOrientation(align)
            .WithStroke([0, 0, 0, 0.5], 2);
    }
}