using System;
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
        ElementBounds defaultButtonBounds = saveButtonBounds.FlatCopy().FixedRightOf(saveButtonBounds, horzOffset + 20);
        ElementBounds restoreButtonBounds = saveButtonBounds.FlatCopy().FixedRightOf(defaultButtonBounds, horzOffset + 20);

        // Create Number Inputs
        const int textOffset = 4; // Used to help vertically center the text to the text inputs

        ElementBounds iconSizeTextBounds = ElementBounds.Fixed(0, 0, 90, 30).FixedUnder(saveButtonBounds, vertOffset * 1.5f + textOffset);
        ElementBounds iconSizeInputBounds = ElementBounds.Fixed(0, iconSizeTextBounds.fixedY - textOffset, 245, 30).WithAlignment(EnumDialogArea.RightFixed);
        ElementBounds fontSizeTextBounds = ElementBounds.Fixed(0, 0, 90, 30).FixedUnder(iconSizeTextBounds, vertOffset / 2 + textOffset);
        ElementBounds fontSizeInputBounds = ElementBounds.Fixed(0, fontSizeTextBounds.fixedY - textOffset, 245, 30).WithAlignment(EnumDialogArea.RightFixed);

        // Create Show Hidden Button
        ElementBounds showHiddenButtonBounds = ElementBounds.Fixed(0, 0, 320, 23).WithFixedPadding(10, 4).FixedUnder(fontSizeTextBounds, vertOffset * 1.5f);

        // Create Drop Down for Selecting Elements
        int selectedIndex = 0;
        string[] names = { "nOne", "ntwo", "nthree" };
        string[] values = { "vOne", "vtwo", "vthree" };

        ElementBounds moveElementDropdownBounds = ElementBounds.Fixed(0, 0, 340, 25).FixedUnder(showHiddenButtonBounds, vertOffset * 1.5f);

        // Create Selected Element Buttons
        ElementBounds editElementPosButtonBounds = ElementBounds.Fixed(0, 0, 90, 23).WithFixedPadding(10, 4);
        ElementBounds enableElementButtonBounds = editElementPosButtonBounds.RightCopy(horzOffset);

        // Create Element Editing Group
        ElementBounds editingBounds = ElementBounds.Fill.WithFixedPadding(10);
        editingBounds.BothSizing = ElementSizing.FitToChildren;
        editingBounds.WithChildren(editElementPosButtonBounds, enableElementButtonBounds);

        // Create Element Editing Group Background
        ElementBounds editingBgBounds = ElementBounds.Fill.FixedUnder(moveElementDropdownBounds, vertOffset);
        editingBgBounds.BothSizing = ElementSizing.FitToChildren;
        editingBgBounds.WithChildren(editingBounds);

        // Add padding around all elementBounds, and shift so is below titlebar
        ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;
        bgBounds.WithChildren(saveButtonBounds, defaultButtonBounds, restoreButtonBounds, iconSizeTextBounds, fontSizeTextBounds, iconSizeInputBounds, fontSizeInputBounds, showHiddenButtonBounds, moveElementDropdownBounds, editingBgBounds);
        bgBounds.fixedOffsetY = titleBarHeight;

        // Auto-sized dialog at the center of the screen
        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

        // Lastly, create the dialog
        SingleComposer = capi.Gui.CreateCompo("statushudconfiggui", dialogBounds)
            .AddShadedDialogBG(ElementBounds.Fill)
            .AddInset(editingBgBounds, 0, 0.7f)
            .AddDialogTitleBar("Status Hud", OnTitleBarCloseClicked)
            .BeginChildElements(bgBounds)
                .AddButton("Save", OnSave, saveButtonBounds)
                .AddButton("Default", OnDefault, defaultButtonBounds)
                .AddButton("Restore", OnRestore, restoreButtonBounds)
                .AddStaticTextAutoFontSize("Icon Size", CairoFont.WhiteSmallishText(), iconSizeTextBounds)
                .AddTextInput(iconSizeInputBounds, TextInput)
                .AddStaticTextAutoFontSize("Font Size", CairoFont.WhiteSmallishText(), fontSizeTextBounds)
                .AddTextInput(fontSizeInputBounds, TextInput)
                .AddToggleButton("Show Hidden Elements", CairoFont.ButtonText(), OnHidden, showHiddenButtonBounds)
                .AddDropDown(values, names, selectedIndex, OnSelectionChange, moveElementDropdownBounds)
                .BeginChildElements(editingBgBounds)
                    .BeginChildElements(editingBounds)
                        .AddToggleButton("Edit", CairoFont.ButtonText(), OnEdit, editElementPosButtonBounds)
                        .AddToggleButton("Enable", CairoFont.ButtonText(), OnEnable, enableElementButtonBounds)
                    .EndChildElements()
                .EndChildElements()
            .EndChildElements()
            .Compose()
        ;

        elementSelector = new GuiDialogMoveable(capi);
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

    private void OnSelectionChange(string code, bool selected)
    {
        capi.Logger.Notification("You changed the drop down value! code: {0}, selected {1}", code, selected);
    }

    private void TextInput(string value)
    {
        capi.Logger.Notification("You typed in a number!");
    }

    private void OnHidden(bool on)
    {
        if (on)
        {
            capi.Logger.Notification("Showing Hidden Elements");
        }
        else
        {
            capi.Logger.Notification("Hiding Hidden Elements");
        }

    }

    private void OnEdit(bool on)
    {
        if (on)
        {
            capi.Logger.Notification("Editing Element");
        }
        else
        {
            capi.Logger.Notification("Stopped Editing Element");
        }

    }

    private void OnEnable(bool on)
    {
        if (on)
        {
            capi.Logger.Notification("Enabling Element");
        }
        else
        {
            capi.Logger.Notification("Disabling Element");
        }

    }
}