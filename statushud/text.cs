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

            this.colour = config.colour.ToVec4f();
            this.width = this.iconSize * 3;
            this.height = this.iconSize;

            this.dialogName = dialogNamePrefix + this.key;
            this.font = this.initFont();

            this.composed = false;
        }

        public override void Dispose()
        {
            this.TryClose();
            base.Dispose();
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

            float iconHalf = this.iconSize / 2f;
            float frameWidth = this.capi.Render.FrameWidth;
            float frameHeight = this.capi.Render.FrameHeight;

            // X.
            switch (pos.halign)
            {
                case StatusHudPos.halignLeft:
                    {
                        x = GameMath.Clamp(x, 0, frameWidth - this.iconSize);

                        x -= (float)Math.Round((this.width - this.iconSize) / 2f);
                        break;
                    }
                case StatusHudPos.halignCenter:
                    {
                        x = GameMath.Clamp(x, -(frameWidth / 2) + iconHalf, (frameWidth / 2) - iconHalf);
                        break;
                    }
                case StatusHudPos.halignRight:
                    {
                        x = GameMath.Clamp(x, 0, frameWidth - this.iconSize);

                        x = -x + (float)Math.Round((this.width - this.iconSize) / 2f);
                        break;
                    }
            }

            // Y.
            switch (pos.valign)
            {
                case StatusHudPos.valignTop:
                    {
                        y = GameMath.Clamp(y, 0, frameHeight - this.iconSize);
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

            this.compose(area, x, y);
        }

        public void Set(string value)
        {
            this.text.Text = value;
            this.text.RecomposeText();
        }

        protected void compose(EnumDialogArea area, float x, float y)
        {
            if (this.composed)
            {
                this.Dispose();
            }

            ElementBounds dialogBounds = ElementBounds.Fixed(area, x + this.config.offsetX, y + this.config.offsetY, this.width, this.height);
            ElementBounds textBounds = ElementBounds.Fixed(EnumDialogArea.CenterTop, 0, 0, this.width, this.height);
            this.SingleComposer = this.capi.Gui.CreateCompo(this.dialogName, dialogBounds)
                    .AddDynamicText("", this.font, textBounds, this.key)
                    .Compose();
            this.text = this.SingleComposer.GetDynamicText(key);
            this.TryOpen();

            this.composed = true;
        }

        protected virtual CairoFont initFont()
        {
            return new CairoFont()
                    .WithColor(new double[] { this.colour.R, this.colour.G, this.colour.B, this.colour.A })
                    .WithFont(GuiStyle.StandardFontName)
                    .WithFontSize(this.config.size)
                    .WithWeight(this.config.bold ? Cairo.FontWeight.Bold : Cairo.FontWeight.Normal)
                    .WithOrientation(this.config.align)
                    .WithStroke(new double[] { 0, 0, 0, 0.5 }, 2);
        }
    }
}