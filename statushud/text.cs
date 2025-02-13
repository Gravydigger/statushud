using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace StatusHud
{
    public class StatusHudText : HudElement
    {
        private const string dialogNamePrefix = "d-";

        private readonly string key;
        private readonly StatusHudConfig config;
        private readonly Vec4f colour = new(0.91f, 0.87f, 0.81f, 1);
        private float width;
        private float height;

        private readonly string dialogName;
        private CairoFont font;
        private GuiElementDynamicText text;

        public override EnumDialogType DialogType => EnumDialogType.HUD;
        public override bool Focusable => false;
        public override double DrawOrder => 0;

        public StatusHudText(ICoreClientAPI capi, string key, StatusHudConfig config) : base(capi)
        {
            this.key = key;
            this.config = config;

            dialogName = dialogNamePrefix + this.key;
            font = InitFont();
        }

        public override void Dispose()
        {
            TryClose();
            base.Dispose();
        }

        // Fixed an issue where if the boundry box of the text element was overlapping with another, it would eat
        // the mouse event (i.e. if on the crosshair, it would not allow you to interact with blocks). 
        public override bool ShouldReceiveMouseEvents()
        {
            return false;
        }

        public void ReloadText(StatusHudPos pos)
        {
            font = InitFont();
            Pos(pos);
        }

        public void Pos(StatusHudPos pos)
        {
            EnumDialogArea area = EnumDialogArea.None;
            float x = pos.x;
            float y = pos.y;

            // Area.
            switch (pos.halign)
            {
                case StatusHudPos.halignLeft:
                    {
                        switch (pos.valign)
                        {
                            case StatusHudPos.valignTop:
                                {
                                    area = EnumDialogArea.LeftTop;
                                    break;
                                }
                            case StatusHudPos.valignMiddle:
                                {
                                    area = EnumDialogArea.LeftMiddle;
                                    break;
                                }
                            case StatusHudPos.valignBottom:
                                {
                                    area = EnumDialogArea.LeftBottom;
                                    break;
                                }
                        }
                        break;
                    }
                case StatusHudPos.halignCenter:
                    {
                        switch (pos.valign)
                        {
                            case StatusHudPos.valignTop:
                                {
                                    area = EnumDialogArea.CenterTop;
                                    break;
                                }
                            case StatusHudPos.valignMiddle:
                                {
                                    area = EnumDialogArea.CenterMiddle;
                                    break;
                                }
                            case StatusHudPos.valignBottom:
                                {
                                    area = EnumDialogArea.CenterBottom;
                                    break;
                                }
                        }
                        break;
                    }
                case StatusHudPos.halignRight:
                    {
                        switch (pos.valign)
                        {
                            case StatusHudPos.valignTop:
                                {
                                    area = EnumDialogArea.RightTop;
                                    break;
                                }
                            case StatusHudPos.valignMiddle:
                                {
                                    area = EnumDialogArea.RightMiddle;
                                    break;
                                }
                            case StatusHudPos.valignBottom:
                                {
                                    area = EnumDialogArea.RightBottom;
                                    break;
                                }
                        }
                        break;
                    }
            }

            float iconHalf = config.iconSize / 2f;
            float frameWidth = capi.Render.FrameWidth;
            float frameHeight = capi.Render.FrameHeight;
            width = config.iconSize * 3;
            height = config.iconSize;

            // X.
            switch (pos.halign)
            {
                case StatusHudPos.halignLeft:
                    {
                        x = GameMath.Clamp(x, 0, frameWidth - config.iconSize);

                        x -= (float)Math.Round((width - config.iconSize) / 2f);
                        break;
                    }
                case StatusHudPos.halignCenter:
                    {
                        x = (float)GameMath.Clamp(x, -(frameWidth / 2) + iconHalf, (frameWidth / 2) - iconHalf);
                        break;
                    }
                case StatusHudPos.halignRight:
                    {
                        x = GameMath.Clamp(x, 0, frameWidth - config.iconSize);

                        x = -x + (float)Math.Round((width - config.iconSize) / 2f);
                        break;
                    }
            }

            // Y.
            switch (pos.valign)
            {
                case StatusHudPos.valignTop:
                    {
                        y = GameMath.Clamp(y, 0, frameHeight - config.iconSize);
                        break;
                    }
                case StatusHudPos.valignMiddle:
                    {
                        y = GameMath.Clamp(y, -(frameHeight / 2), (frameHeight / 2) - iconHalf);
                        break;
                    }
                case StatusHudPos.valignBottom:
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

        protected void Compose(EnumDialogArea area, float x, float y)
        {
            const int offsetX = 0;
            int offsetY = -config.textSize;

            SingleComposer?.Dispose();
            ElementBounds dialogBounds = ElementBounds.Fixed(area, x + offsetX, y + offsetY, width, height);
            ElementBounds textBounds = ElementBounds.Fixed(EnumDialogArea.CenterTop, 0, 0, width, height);
            SingleComposer = capi.Gui.CreateCompo(dialogName, dialogBounds)
                    .AddDynamicText("", font, textBounds, key)
                    .Compose();
            text = SingleComposer.GetDynamicText(key);
            TryOpen();
        }

        protected virtual CairoFont InitFont()
        {
            const bool bold = true;
            const EnumTextOrientation align = EnumTextOrientation.Center;

            return new CairoFont()
                .WithColor(new double[] { colour.R, colour.G, colour.B, colour.A })
                .WithFont(GuiStyle.StandardFontName)
                .WithFontSize(config.textSize)
                .WithWeight(bold ? Cairo.FontWeight.Bold : Cairo.FontWeight.Normal)
                .WithOrientation(align)
                .WithStroke(new double[] { 0, 0, 0, 0.5 }, 2);
        }
    }
}