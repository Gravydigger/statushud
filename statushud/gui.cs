using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace StatusHud
{
    public class StatusHudGui : GuiDialog
    {
        //protected ICoreClientAPI capi;

        public override string ToggleKeyCombinationCode => "statushudgui";
        public StatusHudGui(ICoreClientAPI capi) : base(capi)
        {
            //this.capi = capi;
            StatusHudConfigGui();
        }

        private void StatusHudConfigGui()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds textBounds = ElementBounds.Fixed(0, 400, 300, 300);

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(textBounds);

            SingleComposer = capi.Gui.CreateCompo("StatusHudConfigGui", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddStaticText("Example text", CairoFont.WhiteMediumText(), textBounds)
                .AddDialogTitleBar("Example Title", OnTitleClose)
                .Compose();
        }

        private void OnTitleClose()
        {
            TryClose();
        }
    }
}
