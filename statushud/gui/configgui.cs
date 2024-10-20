using Vintagestory.API.Client;

public class StatusHudConfigGui : GuiDialog
{
    public override string ToggleKeyCombinationCode => "statushudconfiggui";

    private GuiDialog elementSelector;

    public StatusHudConfigGui(ICoreClientAPI capi) : base(capi)
    {
        SetupDialog();
    }

    private void SetupDialog()
    {
        // Auto-sized dialog at the center of the screen
        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        ElementBounds buttonBounds = ElementBounds.Fixed(0, 0, 90, 23).WithFixedPadding(10, 4);


        // Lastly, create the dialog
        SingleComposer = capi.Gui.CreateCompo("statushudconfiggui", dialogBounds)
            .AddShadedDialogBG(ElementBounds.Fill)
            .AddDialogTitleBar("Heck yeah!", OnTitleBarCloseClicked)
            .AddButton("New", OnNew, ElementBounds.Fixed(10, 30, 90, 23).WithFixedPadding(10, 4), CairoFont.WhiteSmallishText().WithOrientation(EnumTextOrientation.Center))
            .AddButton("Edit", OnEdit, ElementBounds.Fixed(130, 30, 90, 23).WithFixedPadding(10, 4), CairoFont.WhiteSmallishText().WithOrientation(EnumTextOrientation.Center))
            .AddButton("Delete", OnDefault, ElementBounds.Fixed(250, 30, 90, 23).WithFixedPadding(10, 4), CairoFont.WhiteSmallishText().WithOrientation(EnumTextOrientation.Center))
            .Compose()
        ;

        elementSelector = new GuiDialogMoveable(capi);
    }

    private void OnTitleBarCloseClicked()
    {
        TryClose();
    }

    private bool OnNew()
    {
        capi.Logger.Notification("You Pressed the New Button!");
        return true;
    }

    private bool OnEdit()
    {
        if (elementSelector.IsOpened())
        {
            elementSelector.TryClose();
        }
        else
        {
            elementSelector.TryOpen();
        }

        return true;
    }

    private bool OnDefault()
    {
        capi.Logger.Notification("You Pressed the Default Button!");
        return true;
    }
}