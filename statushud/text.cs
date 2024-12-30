using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace StatusHud
{
    public class StatusHudText : HudElement
    {
        protected const string dialogNamePrefix = "d-";

        private readonly string key;
        private readonly StatusHudConfig config;
        private readonly Vec4f colour = new(0.91f, 0.87f, 0.81f, 1);
        private readonly float width;
        private readonly float height;

        private readonly string dialogName;
        private CairoFont font;
        private GuiElementDynamicText text;

        private bool composed;

        public override EnumDialogType DialogType => EnumDialogType.HUD;
        public override bool Focusable => false;
        public override double DrawOrder => 0;

        public StatusHudText(ICoreClientAPI capi, string key, StatusHudConfig config) : base(capi)
        {
            this.key = key;
            this.config = config;

            width = this.config.iconSize * 3;
            height = this.config.iconSize;

            dialogName = dialogNamePrefix + this.key;
            font = InitFont();

            composed = false;
        }

        public override void Dispose()
        {
            TryClose();
            base.Dispose();
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
            // While boundaryScale shouldn't need to be set to anything other than 1f,
            // the center alignment seems to leave the screen way before it would hit the actual frame.
            // 0.8f seems to be the right scale to fix this issue.
            // const float boundaryScale = 0.8f;

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
                        x = (float)GameMath.Clamp(x, -(frameWidth / 2 /** boundaryScale*/) + iconHalf, (frameWidth / 2 /** boundaryScale*/) - iconHalf);
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
                        y = GameMath.Clamp(y, -(frameHeight / 2 /** boundaryScale*/), (frameHeight / 2 /** boundaryScale*/) - iconHalf);
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
            const int offsetY = -19;

            if (composed)
            {
                Dispose();
            }

            ElementBounds dialogBounds = ElementBounds.Fixed(area, x + offsetX, y + offsetY, width, height);
            ElementBounds textBounds = ElementBounds.Fixed(EnumDialogArea.CenterTop, 0, 0, width, height);
            SingleComposer = capi.Gui.CreateCompo(dialogName, dialogBounds)
                    .AddDynamicText("", font, textBounds, key)
                    .Compose();
            text = SingleComposer.GetDynamicText(key);
            TryOpen();

            composed = true;
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