using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace StatusHud
{
    public class StatusHudText : HudElement
    {
        protected const string dialogNamePrefix = "d-";

        protected int slot;
        protected string key;
        protected StatusHudTextConfig config;
        protected Vec4f colour;
        protected int iconSize;
        protected float width;
        protected float height;

        protected string dialogName;
        protected CairoFont font;
        protected GuiElementDynamicText text;

        protected bool composed;

        public override EnumDialogType DialogType => EnumDialogType.HUD;
        public override bool Focusable => false;
        public override double DrawOrder => 0;

        public StatusHudText(ICoreClientAPI capi, int slot, string key, StatusHudTextConfig config, int iconSize) : base(capi)
        {
            this.slot = slot;
            this.key = key;
            this.config = config;
            this.iconSize = iconSize;

            colour = config.colour.ToVec4f();
            width = this.iconSize * 3;
            height = this.iconSize;

            dialogName = dialogNamePrefix + this.key;
            font = initFont();

            composed = false;
        }

        public override void Dispose()
        {
            TryClose();
            base.Dispose();
        }

        public void ReloadText(StatusHudTextConfig config, StatusHudPos pos)
        {
            colour = config.colour.ToVec4f();
            font = initFont();
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

            float iconHalf = iconSize / 2f;
            float frameWidth = capi.Render.FrameWidth;
            float frameHeight = capi.Render.FrameHeight;

            // X.
            switch (pos.halign)
            {
                case StatusHudPos.halignLeft:
                    {
                        x = GameMath.Clamp(x, 0, frameWidth - iconSize);

                        x -= (float)Math.Round((width - iconSize) / 2f);
                        break;
                    }
                case StatusHudPos.halignCenter:
                    {
                        x = GameMath.Clamp(x, -(frameWidth / 2) + iconHalf, (frameWidth / 2) - iconHalf);
                        break;
                    }
                case StatusHudPos.halignRight:
                    {
                        x = GameMath.Clamp(x, 0, frameWidth - iconSize);

                        x = -x + (float)Math.Round((width - iconSize) / 2f);
                        break;
                    }
            }

            // Y.
            switch (pos.valign)
            {
                case StatusHudPos.valignTop:
                    {
                        y = GameMath.Clamp(y, 0, frameHeight - iconSize);
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

            compose(area, x, y);
        }

        public void Set(string value)
        {
            text.Text = value;
            text.RecomposeText();
        }

        protected void compose(EnumDialogArea area, float x, float y)
        {
            if (composed)
            {
                Dispose();
            }

            ElementBounds dialogBounds = ElementBounds.Fixed(area, x + config.offsetX, y + config.offsetY, width, height);
            ElementBounds textBounds = ElementBounds.Fixed(EnumDialogArea.CenterTop, 0, 0, width, height);
            SingleComposer = capi.Gui.CreateCompo(dialogName, dialogBounds)
                    .AddDynamicText("", font, textBounds, key)
                    .Compose();
            text = SingleComposer.GetDynamicText(key);
            TryOpen();

            composed = true;
        }

        protected virtual CairoFont initFont()
        {
            return new CairoFont()
                    .WithColor(new double[] { colour.R, colour.G, colour.B, colour.A })
                    .WithFont(GuiStyle.StandardFontName)
                    .WithFontSize(config.size)
                    .WithWeight(config.bold ? Cairo.FontWeight.Bold : Cairo.FontWeight.Normal)
                    .WithOrientation(config.align)
                    .WithStroke(new double[] { 0, 0, 0, 0.5 }, 2);
        }
    }
}