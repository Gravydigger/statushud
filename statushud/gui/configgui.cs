using System;
using StatusHud;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

public class StatusHudConfigGui : GuiDialog
{
    public override string ToggleKeyCombinationCode => "statushudconfiggui";

    private StatusHudSystem system;
    // private GuiDialogMoveable elementSelector;
    private string selectedElementName;
    private int selectedElementIndex;

    private readonly string[] elementAlignments = { "Top Left", "Top Center", "Top Right", "Center Left", "True Center", "Center Right", "Bottom Left", "Bottom Center", "Bottom Right" };

    public StatusHudConfigGui(ICoreClientAPI capi, StatusHudSystem system) : base(capi)
    {
        this.system = system;

        selectedElementName = StatusHudSystem.elementNames[0];
        selectedElementIndex = 0;

        ReloadElementInputs(GetElementFromName(selectedElementName));
    }

    private void ComposeDialog()
    {
        const int titleBarHeight = 25;
        const int vertOffset = 20;
        const int horzOffset = 25;

        StatusHudElement element = GetElementFromName(selectedElementName);
        // TODO: Update with GUI Scale

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
        ElementBounds showHiddenButtonBounds = ElementBounds.Fixed(0, 0, 320, 23).WithFixedPadding(10, 4).FixedUnder(fontSizeTextBounds, vertOffset);

        // Create Drop Down for Selecting Elements
        ElementBounds moveElementTextBounds = ElementBounds.Fixed(0, 0, 320, 23).WithAlignment(EnumDialogArea.CenterFixed).FixedUnder(showHiddenButtonBounds, vertOffset * 1.25f);
        ElementBounds moveElementDropdownBounds = ElementBounds.Fixed(0, 0, 340, 25).FixedUnder(moveElementTextBounds, vertOffset * 1.25f);

        // Create Selected Element Buttons
        ElementBounds enableElementButtonBounds = ElementBounds.Fixed(0, 0, 90, 23).WithFixedPadding(10, 4);
        ElementBounds alignElementDropdownBounds = ElementBounds.Fixed(0, 0, 200, 30).FixedRightOf(enableElementButtonBounds, horzOffset);
        // ElementBounds editElementPosButtonBounds = enableElementButtonBounds.RightCopy(horzOffset);

        ElementBounds xPosTextBounds = ElementBounds.Fixed(0, 0, 90, 30).FixedUnder(enableElementButtonBounds, vertOffset * 1.5f + textOffset);
        ElementBounds xPosInputBounds = ElementBounds.Fixed(xPosTextBounds.fixedWidth + horzOffset, xPosTextBounds.fixedY - textOffset, 205, 30);
        ElementBounds yPosTextBounds = ElementBounds.Fixed(0, 0, 90, 30).FixedUnder(xPosInputBounds, vertOffset / 2 + textOffset);
        ElementBounds yPosInputBounds = ElementBounds.Fixed(yPosTextBounds.fixedWidth + horzOffset, yPosTextBounds.fixedY - textOffset, 205, 30);

        ElementBounds optionalConfigTextBounds = ElementBounds.Fixed(0, 0, 100, 30).FixedUnder(yPosTextBounds, vertOffset / 2 + textOffset);
        ElementBounds optionalConfigDropdownBounds = ElementBounds.Fixed(optionalConfigTextBounds.fixedWidth + horzOffset - 10, optionalConfigTextBounds.fixedY - textOffset, 205, 30);

        // Create Element Editing Group
        ElementBounds editingBounds = ElementBounds.Fill.WithFixedPadding(10);
        editingBounds.BothSizing = ElementSizing.FitToChildren;

        if (element.ElementOption != "")
        {
            editingBounds.WithChildren(enableElementButtonBounds, alignElementDropdownBounds, xPosInputBounds, xPosInputBounds, yPosTextBounds, yPosInputBounds, optionalConfigTextBounds, optionalConfigDropdownBounds);
        }
        else
        {
            editingBounds.WithChildren(enableElementButtonBounds, alignElementDropdownBounds, xPosInputBounds, xPosInputBounds, yPosTextBounds, yPosInputBounds);
        }

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
                .AddTextInput(iconSizeInputBounds, OnIconSize, key: "shud-iconsize")
                .AddStaticTextAutoFontSize("Font Size", CairoFont.WhiteSmallishText(), fontSizeTextBounds)
                .AddTextInput(fontSizeInputBounds, OnFontSize, key: "shud-fontsize")
                .AddToggleButton("Show Hidden Elements", CairoFont.ButtonText(), OnHidden, showHiddenButtonBounds)
                .AddStaticText("Edit Element", CairoFont.ButtonText(), EnumTextOrientation.Center, moveElementTextBounds)
                .AddDropDown(StatusHudSystem.elementNames, StatusHudSystem.elementNames, selectedElementIndex, OnSelectionChange, moveElementDropdownBounds)
                .BeginChildElements(editingBgBounds)
                    .BeginChildElements(editingBounds)
                        // .AddToggleButton("Edit", CairoFont.ButtonText(), OnEdit, editElementPosButtonBounds, "editbutton")
                        .AddToggleButton("Enable", CairoFont.ButtonText(), OnEnable, enableElementButtonBounds, "shud-enablebutton")
                        .AddDropDown(elementAlignments, elementAlignments, 4, OnAlignChange, alignElementDropdownBounds, "shud-align")
                        .AddStaticTextAutoFontSize("X Position", CairoFont.WhiteSmallishText(), xPosTextBounds)
                        .AddTextInput(xPosInputBounds, OnXPos, key: "shud-xpos")
                        .AddStaticTextAutoFontSize("Y Position", CairoFont.WhiteSmallishText(), yPosTextBounds)
                        .AddTextInput(yPosInputBounds, OnYPos, key: "shud-ypos")
                        .AddIf(selectedElementName == "time" || selectedElementName == "time-local")
                            .AddStaticText("Time Format", CairoFont.WhiteSmallText(), optionalConfigTextBounds)
                            .AddDropDown(StatusHudTimeElement.timeFormatWords, StatusHudTimeElement.timeFormatWords, StatusHudTimeElement.timeFormatWords.IndexOf(element.ElementOption), OnOptionalConfig, optionalConfigDropdownBounds)
                        .EndIf()
                        .AddIf(selectedElementName == "weather")
                            .AddStaticText("Temp Scale", CairoFont.WhiteSmallText(), optionalConfigTextBounds)
                            .AddDropDown(StatusHudWeatherElement.tempFormatWords, StatusHudWeatherElement.tempFormatWords, StatusHudWeatherElement.tempFormatWords.IndexOf(element.ElementOption), OnOptionalConfig, optionalConfigDropdownBounds)
                        .EndIf()
                        .AddIf(selectedElementName == "bodyheat")
                            .AddStaticText("Temp Scale", CairoFont.WhiteSmallText(), optionalConfigTextBounds)
                            .AddDropDown(StatusHudBodyheatElement.tempFormatWords, StatusHudBodyheatElement.tempFormatWords, StatusHudBodyheatElement.tempFormatWords.IndexOf(element.ElementOption), OnOptionalConfig, optionalConfigDropdownBounds)
                        .EndIf()
                        .AddIf(selectedElementName == "compass")
                            .AddStaticText("Direction", CairoFont.WhiteSmallText(), optionalConfigTextBounds)
                            .AddDropDown(StatusHudCompassElement.compassBearingOptions, StatusHudCompassElement.compassBearingOptions, StatusHudCompassElement.compassBearingOptions.IndexOf(element.ElementOption), OnOptionalConfig, optionalConfigDropdownBounds)
                        .EndIf()
                    .EndChildElements()
                .EndChildElements()
            .EndChildElements()
            .Compose()
        ;

        SingleComposer.GetTextInput("shud-iconsize").SetValue(system.Config.iconSize);
        SingleComposer.GetTextInput("shud-iconsize").SetPlaceHolderText("Icon Size...");

        SingleComposer.GetTextInput("shud-fontsize").SetValue(system.Config.textSize);
        SingleComposer.GetTextInput("shud-fontsize").SetPlaceHolderText("Font Size...");

        // "altitude" is the first alphabetical element
        // ReloadElementInputs(GetElementFromName("altitude"));

        // SingleComposer.GetToggleButton("editbutton").SetValue(true);

        // elementSelector = new GuiDialogMoveable(capi);
    }

    private void OnOptionalConfig(string code, bool selected)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);
        if (element == null) return;

        element.ConfigOptions(code);
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

    private void ReloadElementInputs(StatusHudElement element)
    {
        ComposeDialog();
        if (element == null)
        {
            SingleComposer.GetToggleButton("shud-enablebutton").SetValue(false);
            SingleComposer.GetTextInput("shud-xpos").SetValue(0);
            SingleComposer.GetTextInput("shud-ypos").SetValue(0);
            SingleComposer.GetDropDown("shud-align").SetSelectedValue("True Center");
        }
        else
        {
            SingleComposer.GetToggleButton("shud-enablebutton").SetValue(true);
            SingleComposer.GetTextInput("shud-xpos").SetValue(element.pos.x);
            SingleComposer.GetTextInput("shud-ypos").SetValue(element.pos.y);

            string value = "";

            switch (element.pos.valign)
            {
                case -1:
                    switch (element.pos.valign)
                    {
                        case -1:
                            value = "Top Left";
                            break;
                        case 0:
                            value = "Top Center";
                            break;
                        case 1:
                            value = "Top Right";
                            break;
                    }
                    break;
                case 0:
                    switch (element.pos.valign)
                    {
                        case -1:
                            value = "Center Left";
                            break;
                        case 0:
                            value = "True Center";
                            break;
                        case 1:
                            value = "Center Right";
                            break;
                    }
                    break;
                case 1:
                    switch (element.pos.valign)
                    {
                        case -1:
                            value = "Bottom Left";
                            break;
                        case 0:
                            value = "Bottom Center";
                            break;
                        case 1:
                            value = "Bottom Right";
                            break;
                    }
                    break;
            }

            SingleComposer.GetDropDown("shud-align").SetSelectedValue(value);
        }
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
        ReloadElementInputs(GetElementFromName(selectedElementName));

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

        element.pos.x = value.ToInt(0);
        element.Pos();
        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element X Position changed to {element.pos.x}"));
    }

    private void OnYPos(string value)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);
        if (element == null) return;

        element.pos.y = value.ToInt(0);
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
            capi.Logger.Debug(StatusHudSystem.PrintModName("Showing hidden elements"));
        }
        else
        {
            capi.Logger.Debug(StatusHudSystem.PrintModName("Hiding hidden elements"));
        }
    }

    private void OnSelectionChange(string name, bool selected)
    {
        selectedElementName = name;

        for (int i = 0; i < StatusHudSystem.elementNames.Length; i++)
        {
            if (StatusHudSystem.elementNames[i] == name)
            {
                selectedElementIndex = i;
                break;
            }
        }

        ReloadElementInputs(GetElementFromName(name));

        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element {name} selected"));
    }

    private void OnAlignChange(string name, bool selected)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);
        if (element == null) return;

        switch (name)
        {
            case "Top Left":
                element.pos.valign = -1;
                element.pos.halign = -1;
                break;
            case "Top Center":
                element.pos.valign = -1;
                element.pos.halign = 0;
                break;
            case "Top Right":
                element.pos.valign = -1;
                element.pos.halign = 1;
                break;
            case "Center Left":
                element.pos.valign = 0;
                element.pos.halign = -1;
                break;
            case "True Center":
                element.pos.valign = 0;
                element.pos.halign = 0;
                break;
            case "Center Right":
                element.pos.valign = 0;
                element.pos.halign = 1;
                break;
            case "Bottom Left":
                element.pos.valign = 1;
                element.pos.halign = -1;
                break;
            case "Bottom Center":
                element.pos.valign = 1;
                element.pos.halign = 0;
                break;
            case "Bottom Right":
                element.pos.valign = 1;
                element.pos.halign = 1;
                break;
            default:
                break;
        }

        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element {selectedElementName} set to {name} alignment"));
    }

    // private void OnEdit(bool on)
    // {
    //     StatusHudElement element = GetElementFromName(selectedElementName);

    //     if (element == null) return;

    //     if (on)
    //     {
    //         elementSelector.UpdateSelectedElement(element);
    //         elementSelector.TryOpen();

    //         capi.Logger.Debug(StatusHudSystem.PrintModName($"Editing Element {selectedElementName}'s position"));
    //     }
    //     else
    //     {
    //         if (elementSelector.IsOpened())
    //         {
    //             elementSelector.TryClose();
    //         }

    //         capi.Logger.Debug(StatusHudSystem.PrintModName($"Setting Element {selectedElementName}'s position"));
    //     }
    // }

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