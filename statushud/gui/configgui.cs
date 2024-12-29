using System;
using StatusHud;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

public class StatusHudConfigGui : GuiDialog
{
    public override string ToggleKeyCombinationCode => "statushudconfiggui";

    private StatusHudSystem system;
    private GuiDialogMoveable elementSelector;
    private string selectedElementName;

    public StatusHudConfigGui(ICoreClientAPI capi, StatusHudSystem system) : base(capi)
    {
        this.system = system;

        selectedElementName = StatusHudSystem.elementNames[0];

        SetupDialog();
    }

    private void SetupDialog()
    {
        const int titleBarHeight = 25;
        const int vertOffset = 20;
        const int horzOffset = 25;

        // Create Config Buttons
        ElementBounds saveButtonBounds = ElementBounds.Fixed(0, 0, 90, 23).WithFixedPadding(10, 4);
        ElementBounds defaultButtonBounds = saveButtonBounds.FlatCopy().FixedRightOf(saveButtonBounds, horzOffset);
        ElementBounds restoreButtonBounds = saveButtonBounds.FlatCopy().FixedRightOf(defaultButtonBounds, horzOffset);

        // Create Number Inputs
        const int textOffset = 4; // Used to help vertically center the text to the text inputs

        ElementBounds iconSizeTextBounds = ElementBounds.Fixed(0, 0, 90, 30).FixedUnder(saveButtonBounds, vertOffset * 1.5f + textOffset);
        ElementBounds iconSizeInputBounds = ElementBounds.Fixed(0, iconSizeTextBounds.fixedY - textOffset, 245, 30).WithAlignment(EnumDialogArea.RightFixed);
        ElementBounds fontSizeTextBounds = ElementBounds.Fixed(0, 0, 90, 30).FixedUnder(iconSizeTextBounds, vertOffset / 2 + textOffset);
        ElementBounds fontSizeInputBounds = ElementBounds.Fixed(0, fontSizeTextBounds.fixedY - textOffset, 245, 30).WithAlignment(EnumDialogArea.RightFixed);

        // Create Show Hidden Button
        ElementBounds showHiddenButtonBounds = ElementBounds.Fixed(0, 0, 320, 23).WithFixedPadding(10, 4).FixedUnder(fontSizeTextBounds, vertOffset * 1.5f);

        // Create Drop Down for Selecting Elements
        ElementBounds moveElementDropdownBounds = ElementBounds.Fixed(0, 0, 340, 25).FixedUnder(showHiddenButtonBounds, vertOffset * 1.5f);

        // Create Selected Element Buttons
        ElementBounds enableElementButtonBounds = ElementBounds.Fixed(0, 0, 90, 23).WithFixedPadding(10, 4);
        // ElementBounds editElementPosButtonBounds = enableElementButtonBounds.RightCopy(horzOffset);

        ElementBounds xPosTextBounds = ElementBounds.Fixed(0, 0, 90, 30).FixedUnder(enableElementButtonBounds, vertOffset * 1.5f + textOffset);
        ElementBounds xPosInputBounds = ElementBounds.Fixed(xPosTextBounds.fixedWidth + horzOffset, xPosTextBounds.fixedY - textOffset, 205, 30);
        ElementBounds yPosTextBounds = ElementBounds.Fixed(0, 0, 90, 30).FixedUnder(xPosInputBounds, vertOffset / 2 + textOffset);
        ElementBounds yPosInputBounds = ElementBounds.Fixed(yPosTextBounds.fixedWidth + horzOffset, yPosTextBounds.fixedY - textOffset, 205, 30);

        // Create Element Editing Group
        ElementBounds editingBounds = ElementBounds.Fill.WithFixedPadding(10);
        editingBounds.BothSizing = ElementSizing.FitToChildren;
        editingBounds.WithChildren(/*editElementPosButtonBounds,*/xPosInputBounds, xPosInputBounds, yPosTextBounds, yPosInputBounds, enableElementButtonBounds);

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
                .AddTextInput(iconSizeInputBounds, OnIconSize, key: "iconsize")
                .AddStaticTextAutoFontSize("Font Size", CairoFont.WhiteSmallishText(), fontSizeTextBounds)
                .AddTextInput(fontSizeInputBounds, OnFontSize, key: "fontsize")
                .AddToggleButton("Show Hidden Elements", CairoFont.ButtonText(), OnHidden, showHiddenButtonBounds)
                .AddDropDown(StatusHudSystem.elementNames, StatusHudSystem.elementNames, 0, OnSelectionChange, moveElementDropdownBounds)
                .BeginChildElements(editingBgBounds)
                    .BeginChildElements(editingBounds)
                        // .AddToggleButton("Edit", CairoFont.ButtonText(), OnEdit, editElementPosButtonBounds, "editbutton")
                        .AddToggleButton("Enable", CairoFont.ButtonText(), OnEnable, enableElementButtonBounds, "enablebutton")
                        .AddStaticTextAutoFontSize("X Position", CairoFont.WhiteSmallishText(), xPosTextBounds)
                        .AddTextInput(xPosInputBounds, OnXPos, key: "xpos")
                        .AddStaticTextAutoFontSize("Y Position", CairoFont.WhiteSmallishText(), yPosTextBounds)
                        .AddTextInput(yPosInputBounds, OnYPos, key: "ypos")
                    .EndChildElements()
                .EndChildElements()
            .EndChildElements()
            .Compose()
        ;

        SingleComposer.GetTextInput("iconsize").SetValue(system.Config.iconSize);
        SingleComposer.GetTextInput("iconsize").SetPlaceHolderText("Icon Size...");

        SingleComposer.GetTextInput("fontsize").SetValue(system.Config.textSize);
        SingleComposer.GetTextInput("fontsize").SetPlaceHolderText("Font Size...");

        // SingleComposer.GetToggleButton("editbutton").SetValue(true);

        elementSelector = new GuiDialogMoveable(capi);
    }

    private StatusHudElement GetElementFromName(string name)
    {
        foreach (var element in system.elements)
        {
            if (element.ElementName == name)
            {
                return element;
            }
        }
        return null;
    }

    private void OnTitleBarCloseClicked()
    {
        TryClose();
    }

    private bool OnSave()
    {
        capi.Logger.Debug(StatusHudSystem.PrintModName("Saving configuration to disk"));
        system.SaveConfig();
        return true;
    }

    private bool OnDefault()
    {
        capi.Logger.Debug(StatusHudSystem.PrintModName("Setting configuration to default layout"));
        system.InstallDefault();
        return true;
    }

    private bool OnRestore()
    {
        capi.Logger.Debug(StatusHudSystem.PrintModName("Restoring configuration from disk"));
        system.LoadConfig();
        return true;
    }

    private void OnIconSize(string value)
    {
        system.Config.iconSize = SanitiseIconInt(value, system.Config.iconSize);
        system.Reload();
        capi.Logger.Debug(StatusHudSystem.PrintModName($"Icon size changed to {system.Config.iconSize}"));
    }

    private void OnFontSize(string value)
    {
        system.Config.textSize = SanitiseIconInt(value, system.Config.textSize);
        system.Reload();
        capi.Logger.Debug(StatusHudSystem.PrintModName($"Font size changed to {system.Config.textSize}"));
    }

    private void OnXPos(string value)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);
        if (element == null) return;

        // This may need to be changed in a different alignment is used, currently using left alignment
        element.pos.x = Math.Max(0, Math.Min(system.capi.Render.FrameWidth, value.ToInt(0)));
        element.Pos();
        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element X Position changed to {element.pos.x}"));
    }

    private void OnYPos(string value)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);
        if (element == null) return;

        // This may need to be changed in a different alignment is used, currently using top alignment
        element.pos.y = Math.Max(0, Math.Min(system.capi.Render.FrameHeight, value.ToInt(0)));
        element.Pos();
        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element Y Position changed to {element.pos.y}"));
    }



    private int SanitiseIconInt(string value, int defaultInt)
    {
        const int minSize = 8;
        const int maxSize = 100;

        return Math.Max(minSize, Math.Min(maxSize, value.ToInt(defaultInt)));
    }

    private void OnHidden(bool on)
    {
        system.Config.showHidden = on;

        if (on)
        {
            capi.Logger.Debug(StatusHudSystem.PrintModName("Showing Hidden Elements"));

        }
        else
        {
            capi.Logger.Debug(StatusHudSystem.PrintModName("Hiding Hidden Elements"));
        }

    }

    private void OnSelectionChange(string name, bool selected)
    {
        StatusHudElement element = GetElementFromName(name);

        if (element == null)
        {
            SingleComposer.GetToggleButton("enablebutton").SetValue(false);
            SingleComposer.GetTextInput("xpos").SetValue(0);
            SingleComposer.GetTextInput("ypos").SetValue(0);
        }
        else
        {
            SingleComposer.GetToggleButton("enablebutton").SetValue(true);
            SingleComposer.GetTextInput("xpos").SetValue(element.pos.x);
            SingleComposer.GetTextInput("ypos").SetValue(element.pos.y);
        }

        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element {name} Selected"));
    }

    private void OnEdit(bool on)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);

        if (element == null) return;

        if (on)
        {
            elementSelector.UpdateSelectedElement(element);
            elementSelector.TryOpen();

            capi.Logger.Debug(StatusHudSystem.PrintModName($"Editing Element {selectedElementName}'s position"));
        }
        else
        {
            if (elementSelector.IsOpened())
            {
                elementSelector.TryClose();
            }

            capi.Logger.Debug(StatusHudSystem.PrintModName($"Setting Element {selectedElementName}'s position"));
        }

    }

    private void OnEnable(bool on)
    {
        if (on)
        {
            system.Set(selectedElementName);
            capi.Logger.Debug(StatusHudSystem.PrintModName($"Enabling Element {selectedElementName}"));
        }
        else
        {
            system.Unset(selectedElementName);
            capi.Logger.Debug(StatusHudSystem.PrintModName($"Disabling Element {selectedElementName}"));
        }

    }
}