using Vintagestory.API.Client;

public class StatusHudConfigGui : GuiDialog
{
    public override string ToggleKeyCombinationCode => "statushudconfiggui";

    private GuiDialog elementSelector;
    protected GuiComposer configGui;

    public StatusHudConfigGui(ICoreClientAPI capi) : base(capi)
    {
        SetupDialog();
    }

    private void SetupDialog()
    {
        const int titleBarHeight = 25;
        const int vertOffset = 20;
        const int horzOffset = 5;

        // Create Config Buttons
        ElementBounds saveButtonBounds = ElementBounds.Fixed(0, 0, 90, 23).WithFixedPadding(10, 4);
        ElementBounds defaultButtonBounds = ElementBounds.Fixed(0, 0, 90, 23).WithFixedPadding(10, 4).FixedUnder(saveButtonBounds, vertOffset);
        ElementBounds restoreButtonBounds = ElementBounds.Fixed(0, 0, 90, 23).WithFixedPadding(10, 4).FixedUnder(defaultButtonBounds, vertOffset);

        // Create Number Inputs
        ElementBounds iconSizeTextBounds = ElementBounds.Fixed(0, 0, 90, 30).FixedUnder(restoreButtonBounds, vertOffset * 1.5f);
        ElementBounds iconSizeInputBounds = ElementBounds.Fixed(0, iconSizeTextBounds.fixedY, 90, 30).FixedRightOf(iconSizeTextBounds, horzOffset);
        ElementBounds fontSizeTextBounds = ElementBounds.Fixed(0, 0, 90, 30).FixedUnder(iconSizeTextBounds, vertOffset / 2);
        ElementBounds fontSizeInputBounds = ElementBounds.Fixed(0, fontSizeTextBounds.fixedY, 90, 30).FixedRightOf(fontSizeTextBounds, horzOffset);


        // ElementBounds hiddenButtonBounds = ElementBounds.Fixed(0, 0, 90, 23).WithFixedPadding(10, 4);

        // ElementBounds moveElementButtonBounds = ElementBounds.Fixed(0, 0, 90, 23).WithFixedPadding(10, 4);
        // ElementBounds moveElementDropdownBounds = ElementBounds.Fixed(0, 0, 90, 23).WithFixedPadding(10, 4);

        // Add padding around all elementBounds, and shift so is below titlebar
        ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;
        bgBounds.WithChildren(saveButtonBounds, defaultButtonBounds, restoreButtonBounds, iconSizeTextBounds, fontSizeTextBounds, iconSizeInputBounds, fontSizeInputBounds);
        bgBounds.fixedOffsetY = titleBarHeight;

        // Auto-sized dialog at the center of the screen
        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

        // Lastly, create the dialog
        SingleComposer = capi.Gui.CreateCompo("statushudconfiggui", dialogBounds)
            .AddShadedDialogBG(ElementBounds.Fill)
            .AddDialogTitleBar("Status Hud", OnTitleBarCloseClicked)
            .BeginChildElements(bgBounds)
                .AddButton("Save", OnSave, saveButtonBounds, EnumButtonStyle.Normal)
                .AddButton("Default", OnDefault, defaultButtonBounds, EnumButtonStyle.Normal)
                .AddButton("Restore", OnRestore, restoreButtonBounds, EnumButtonStyle.Normal)
                .AddStaticTextAutoFontSize("Icon Size", CairoFont.WhiteSmallishText(), iconSizeTextBounds)
                .AddNumberInput(iconSizeInputBounds, TextInput)
                .AddStaticTextAutoFontSize("Font Size", CairoFont.WhiteSmallishText(), fontSizeTextBounds)
                .AddNumberInput(fontSizeInputBounds, TextInput)
            .EndChildElements()
            .Compose()
        ;

        elementSelector = new GuiDialogMoveable(capi);
    }

    private void TextInput(string value)
    {
        capi.Logger.Notification("You typed in a number!");
    }


    private void OnTitleBarCloseClicked()
    {
        TryClose();
    }

    private bool OnSave()
    {
        capi.Logger.Notification("You pressed the Save Button!");
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
        capi.Logger.Notification("You pressed the Default Button!");
        return true;
    }

    private bool OnRestore()
    {
        capi.Logger.Notification("You pressed the Restore Button!");
        return true;
    }
}