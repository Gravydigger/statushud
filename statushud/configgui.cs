using System;
using System.Collections.Generic;
using StatusHud;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

public class StatusHudConfigGui : GuiDialog
{
    public override string ToggleKeyCombinationCode => "statushudconfiggui";

    private StatusHudSystem system;
    private string selectedElementName;
    private int selectedElementIndex;
    private readonly List<string> elementNamesTranslated = new();

    private readonly string[] elementAlignments = {
        Lang.Get("statushudcont:Top Left"),
        Lang.Get("statushudcont:Top Center"),
        Lang.Get("statushudcont:Top Right"),
        Lang.Get("statushudcont:Center Left"),
        Lang.Get("statushudcont:True Center"),
        Lang.Get("statushudcont:Center Right"),
        Lang.Get("statushudcont:Bottom Left"),
        Lang.Get("statushudcont:Bottom Center"),
        Lang.Get("statushudcont:Bottom Right")};
    private readonly string[] elementAlignmentsValue = { "0", "1", "2", "3", "4", "5", "6", "7", "8" };

    public StatusHudConfigGui(ICoreClientAPI capi, StatusHudSystem system) : base(capi)
    {
        this.system = system;

        selectedElementName = StatusHudSystem.elementNames[0];
        selectedElementIndex = 0;

        foreach (var name in StatusHudSystem.elementNames)
        {
            elementNamesTranslated.Add(Lang.Get($"statushudcont:{name}-name"));
        }

        ReloadElementInputs(GetElementFromName(selectedElementName));
    }

    private void ComposeDialog()
    {
        const int titleBarHeight = 25;
        const int vertOffset = 20;
        const int horzOffset = 25;
        const int tooltipLength = 350;
        const int textOffset = 4; // Used to help vertically center the text to the text inputs
        int optionIndex = 0;

        StatusHudElement element = GetElementFromName(selectedElementName);
        // TODO: Update with GUI Scale

        // Create Config Buttons
        ElementBounds saveButtonBounds = ElementBounds.Fixed(0, 0, 100, 23).WithFixedPadding(10, 4);
        ElementBounds defaultButtonBounds = saveButtonBounds.FlatCopy().FixedRightOf(saveButtonBounds, horzOffset);
        ElementBounds restoreButtonBounds = saveButtonBounds.FlatCopy().FixedRightOf(defaultButtonBounds, horzOffset);

        // Create Number Inputs
        ElementBounds iconSizeTextBounds = ElementBounds.Fixed(0, 0, 150, 30).FixedUnder(saveButtonBounds, vertOffset * 1.5f + textOffset);
        ElementBounds iconSizeInputBounds = ElementBounds.Fixed(0, iconSizeTextBounds.fixedY - textOffset, 220, 30).WithAlignment(EnumDialogArea.RightFixed);
        ElementBounds fontSizeTextBounds = ElementBounds.Fixed(0, 0, 150, 30).FixedUnder(iconSizeTextBounds, vertOffset / 2 + textOffset);
        ElementBounds fontSizeInputBounds = ElementBounds.Fixed(0, fontSizeTextBounds.fixedY - textOffset, 220, 30).WithAlignment(EnumDialogArea.RightFixed);

        // Create Show Hidden Button
        ElementBounds showHiddenButtonBounds = ElementBounds.Fixed(0, 0, 350, 23).WithFixedPadding(10, 4).FixedUnder(fontSizeTextBounds, vertOffset).WithAlignment(EnumDialogArea.CenterFixed);

        // Create Drop Down for Selecting Elements
        ElementBounds moveElementTextBounds = ElementBounds.Fixed(0, 0, 350, 23).WithAlignment(EnumDialogArea.CenterFixed).FixedUnder(showHiddenButtonBounds, vertOffset * 1.25f);
        ElementBounds moveElementDropdownBounds = ElementBounds.Fixed(0, 0, 370, 25).FixedUnder(moveElementTextBounds, vertOffset * 1.25f).WithAlignment(EnumDialogArea.CenterFixed);

        // Create Selected Element Buttons
        ElementBounds enableElementButtonBounds = ElementBounds.Fixed(0, 0, 120, 23).WithFixedPadding(10, 4);
        ElementBounds alignElementDropdownBounds = ElementBounds.Fixed(0, 0, 205, 30).FixedRightOf(enableElementButtonBounds, horzOffset);

        ElementBounds xPosTextBounds = ElementBounds.Fixed(0, 0, 150, 30).FixedUnder(enableElementButtonBounds, vertOffset * 1.5f + textOffset);
        ElementBounds xPosInputBounds = ElementBounds.Fixed(0, xPosTextBounds.fixedY - textOffset, 205, 30).WithAlignment(EnumDialogArea.RightFixed);
        ElementBounds yPosTextBounds = ElementBounds.Fixed(0, 0, 150, 30).FixedUnder(xPosInputBounds, vertOffset / 2 + textOffset);
        ElementBounds yPosInputBounds = ElementBounds.Fixed(0, yPosTextBounds.fixedY - textOffset, 205, 30).WithAlignment(EnumDialogArea.RightFixed);

        ElementBounds optionalConfigTextBounds = ElementBounds.Fixed(0, 0, 150, 30).FixedUnder(yPosTextBounds, vertOffset / 2 + textOffset);
        ElementBounds optionalConfigDropdownBounds = ElementBounds.Fixed(0, optionalConfigTextBounds.fixedY - textOffset, 205, 30).WithAlignment(EnumDialogArea.RightFixed);

        // Create Element Editing Group
        ElementBounds editingBounds = ElementBounds.Fill.WithFixedPadding(10);
        editingBounds.BothSizing = ElementSizing.FitToChildren;

        if (element == null || element.ElementOption == "")
        {
            editingBounds.WithChildren(enableElementButtonBounds, alignElementDropdownBounds, xPosInputBounds, xPosInputBounds, yPosTextBounds, yPosInputBounds);
        }
        else
        {
            editingBounds.WithChildren(enableElementButtonBounds, alignElementDropdownBounds, xPosInputBounds, xPosInputBounds, yPosTextBounds, yPosInputBounds, optionalConfigTextBounds, optionalConfigDropdownBounds);

            string[] options = Array.Empty<string>();

            switch (element.ElementName)
            {
                case StatusHudTimeElement.name:
                case StatusHudTimeLocalElement.name:
                    options = StatusHudTimeElement.timeFormatWords;
                    break;
                case StatusHudWeatherElement.name:
                    options = StatusHudWeatherElement.tempFormatWords;
                    break;
                case StatusHudBodyheatElement.name:
                    options = StatusHudBodyheatElement.tempFormatWords;
                    break;
                case StatusHudCompassElement.name:
                    options = StatusHudCompassElement.compassBearingOptions;
                    break;
                case StatusHudRiftActivityElement.name:
                    options = StatusHudRiftActivityElement.riftChangeOptions;
                    break;
                default:
                    capi.Logger.Warning(StatusHudSystem.PrintModName($"Tried to get config options from {element.ElementName}, but it doesn't have any!"));
                    break;
            }

            optionIndex = Math.Max(options.IndexOf(element.ElementOption), 0);
        }

        // Create Element Editing Group Background
        ElementBounds editingBgBounds = ElementBounds.Fill.FixedUnder(moveElementDropdownBounds, vertOffset).WithAlignment(EnumDialogArea.CenterFixed);
        editingBgBounds.BothSizing = ElementSizing.FitToChildren;
        editingBgBounds.WithChildren(editingBounds);

        // Add padding around all elementBounds, and shift so is below titlebar
        ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;
        bgBounds.WithChildren(saveButtonBounds, defaultButtonBounds, restoreButtonBounds, iconSizeTextBounds, fontSizeTextBounds, iconSizeInputBounds, fontSizeInputBounds, showHiddenButtonBounds, moveElementDropdownBounds, editingBgBounds);
        bgBounds.fixedOffsetY = titleBarHeight;

        // Auto-sized dialog at the center of the screen
        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

        SingleComposer?.Dispose();

        SingleComposer = capi.Gui.CreateCompo("statushudconfiggui", dialogBounds)
            .AddShadedDialogBG(ElementBounds.Fill)
            .AddInset(editingBgBounds, 0, 0.7f)
            .AddDialogTitleBar("Status Hud", OnTitleBarCloseClicked)
            .BeginChildElements(bgBounds)
                .AddButton(Lang.Get("statushudcont:Save"), OnSave, saveButtonBounds)
                .AddButton(Lang.Get("statushudcont:Default"), OnDefault, defaultButtonBounds)
                .AddButton(Lang.Get("statushudcont:Restore"), OnRestore, restoreButtonBounds)
                .AddStaticTextAutoFontSize(Lang.Get("statushudcont:Icon Size"), CairoFont.WhiteSmallishText(), iconSizeTextBounds)
                .AddTextInput(iconSizeInputBounds, OnIconSize, key: "shud-iconsize")
                .AddStaticTextAutoFontSize(Lang.Get("statushudcont:Font Size"), CairoFont.WhiteSmallishText(), fontSizeTextBounds)
                .AddTextInput(fontSizeInputBounds, OnFontSize, key: "shud-fontsize")
                .AddToggleButton(Lang.Get("statushudcont:Show Hidden Elements"), CairoFont.ButtonText(), OnHidden, showHiddenButtonBounds, "shud-hidden")
                .AddStaticText(Lang.Get("statushudcont:Edit Element"), CairoFont.ButtonText(), EnumTextOrientation.Center, moveElementTextBounds)
                .AddDropDown(StatusHudSystem.elementNames, elementNamesTranslated.ToArray(), selectedElementIndex, OnSelectionChange, moveElementDropdownBounds)
                .AddAutoSizeHoverText(Lang.Get($"statushudcont:{selectedElementName}-desc"), CairoFont.WhiteSmallText(), tooltipLength, moveElementDropdownBounds.FlatCopy())
                .BeginChildElements(editingBgBounds)
                    .BeginChildElements(editingBounds)
                        .AddToggleButton(Lang.Get("statushudcont:Enable"), CairoFont.ButtonText(), OnEnable, enableElementButtonBounds, "shud-enablebutton")
                        .AddDropDown(elementAlignmentsValue, elementAlignments, 4, OnAlignChange, alignElementDropdownBounds, "shud-align")
                        .AddStaticTextAutoFontSize(Lang.Get("statushudcont:X Position"), CairoFont.WhiteSmallishText(), xPosTextBounds)
                        .AddTextInput(xPosInputBounds, OnXPos, key: "shud-xpos")
                        .AddStaticTextAutoFontSize(Lang.Get("statushudcont:Y Position"), CairoFont.WhiteSmallishText(), yPosTextBounds)
                        .AddTextInput(yPosInputBounds, OnYPos, key: "shud-ypos")
                        .AddIf(selectedElementName == StatusHudTimeElement.name || selectedElementName == StatusHudTimeLocalElement.name)
                            .AddStaticText(Lang.Get("statushudcont:Time Format"), CairoFont.WhiteSmallText(), optionalConfigTextBounds)
                            .AddDropDown(StatusHudTimeElement.timeFormatWords, new string[] { Lang.Get("statushudcont:time-opt-1"), Lang.Get("statushudcont:time-opt-2") }, optionIndex, OnOptionalConfig, optionalConfigDropdownBounds)
                            .AddAutoSizeHoverText(Lang.Get("statushudcont:time-format-tooltip"), CairoFont.WhiteSmallText(), tooltipLength, optionalConfigDropdownBounds.FlatCopy())
                        .EndIf()
                        .AddIf(selectedElementName == StatusHudWeatherElement.name)
                            .AddStaticText(Lang.Get("statushudcont:Temp Scale"), CairoFont.WhiteSmallText(), optionalConfigTextBounds)
                            .AddDropDown(StatusHudWeatherElement.tempFormatWords, StatusHudWeatherElement.tempFormatWords, optionIndex, OnOptionalConfig, optionalConfigDropdownBounds)
                            .AddAutoSizeHoverText(Lang.Get("statushudcont:temp-scale-weather-tooltip"), CairoFont.WhiteSmallText(), tooltipLength, optionalConfigDropdownBounds.FlatCopy())
                        .EndIf()
                        .AddIf(selectedElementName == StatusHudBodyheatElement.name)
                            .AddStaticText(Lang.Get("statushudcont:Temp Scale"), CairoFont.WhiteSmallText(), optionalConfigTextBounds)
                            .AddDropDown(StatusHudBodyheatElement.tempFormatWords, StatusHudBodyheatElement.tempFormatWords, optionIndex, OnOptionalConfig, optionalConfigDropdownBounds)
                            .AddAutoSizeHoverText(Lang.Get("statushudcont:temp-scale-bodyheat-tooltip"), CairoFont.WhiteSmallText(), tooltipLength, optionalConfigDropdownBounds.FlatCopy())
                        .EndIf()
                        .AddIf(selectedElementName == StatusHudCompassElement.name)
                            .AddStaticText(Lang.Get("statushudcont:Heading"), CairoFont.WhiteSmallishText(), optionalConfigTextBounds)
                            .AddDropDown(StatusHudCompassElement.compassBearingOptions, new string[] { Lang.Get("statushudcont:compass-opt-1"), Lang.Get("statushudcont:compass-opt-2") }, optionIndex, OnOptionalConfig, optionalConfigDropdownBounds)
                            .AddAutoSizeHoverText(Lang.Get("statushudcont:compass-heading-tooltip"), CairoFont.WhiteSmallText(), tooltipLength, optionalConfigDropdownBounds.FlatCopy())
                        .EndIf()
                        .AddIf(selectedElementName == StatusHudRiftActivityElement.name)
                            .AddStaticText(Lang.Get("statushudcont:Display Time"), CairoFont.WhiteSmallText(), optionalConfigTextBounds)
                            .AddDropDown(StatusHudRiftActivityElement.riftChangeOptions, new string[] { Lang.Get("statushudcont:riftactivity-opt-1"), Lang.Get("statushudcont:riftactivity-opt-2") }, optionIndex, OnOptionalConfig, optionalConfigDropdownBounds)
                            .AddAutoSizeHoverText(Lang.Get("statushudcont:riftactivity-tooltip"), CairoFont.WhiteSmallText(), tooltipLength, optionalConfigDropdownBounds.FlatCopy())
                        .EndIf()
                    .EndChildElements()
                .EndChildElements()
            .EndChildElements()
            .Compose()
        ;

        SingleComposer.GetTextInput("shud-iconsize").SetValue(system.Config.iconSize);
        SingleComposer.GetTextInput("shud-fontsize").SetValue(system.Config.textSize);
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
            SingleComposer.GetDropDown("shud-align").SetSelectedValue("4");
        }
        else
        {
            SingleComposer.GetToggleButton("shud-enablebutton").SetValue(true);
            SingleComposer.GetTextInput("shud-xpos").SetValue(element.pos.x);
            SingleComposer.GetTextInput("shud-ypos").SetValue(element.pos.y);

            int value = 4;

            switch (element.pos.valign)
            {
                case -1:
                    switch (element.pos.halign)
                    {
                        case -1:
                            value = 0; // Top Left
                            break;
                        case 0:
                            value = 1; // Top Center
                            break;
                        case 1:
                            value = 2; // Top Right
                            break;
                    }
                    break;
                case 0:
                    switch (element.pos.halign)
                    {
                        case -1:
                            value = 3; // Center Left
                            break;
                        case 0:
                            value = 4; // True Center
                            break;
                        case 1:
                            value = 5; // Center Right
                            break;
                    }
                    break;
                case 1:
                    switch (element.pos.halign)
                    {
                        case -1:
                            value = 6; // Bottom Left
                            break;
                        case 0:
                            value = 7; // Bottom Center
                            break;
                        case 1:
                            value = 8; // Bottom Right
                            break;
                    }
                    break;
            }

            SingleComposer.GetDropDown("shud-align").SetSelectedValue(value.ToString());
        }

        SingleComposer.GetToggleButton("shud-hidden").SetValue(system.Config.showHidden);
    }

    private void OnTitleBarCloseClicked()
    {
        TryClose();
    }

    private bool OnSave()
    {
        system.SaveConfig();

        const string message = "Saved HUD to disk";
        capi.ShowChatMessage(StatusHudSystem.PrintModName(Lang.Get($"statushudcont:{message}")));
        capi.Logger.Debug(StatusHudSystem.PrintModName(message));

        return true;
    }

    private bool OnDefault()
    {
        system.InstallDefault();
        ReloadElementInputs(GetElementFromName(selectedElementName));

        const string message = "HUD set to default layout";
        capi.ShowChatMessage(StatusHudSystem.PrintModName(Lang.Get($"statushudcont:{message}")));
        capi.Logger.Debug(StatusHudSystem.PrintModName(message));

        return true;
    }

    private bool OnRestore()
    {
        system.LoadConfig();
        ReloadElementInputs(GetElementFromName(selectedElementName));

        const string message = "HUD restored from disk";
        capi.ShowChatMessage(StatusHudSystem.PrintModName(Lang.Get($"statushudcont:{message}")));
        capi.Logger.Debug(StatusHudSystem.PrintModName(message));

        return true;
    }

    private void OnIconSize(string value)
    {
        system.Config.iconSize = Math.Max(0, SanitiseIconInt(value, system.Config.iconSize));
        system.Reload();
        capi.Logger.Debug(StatusHudSystem.PrintModName($"Icon size changed to {system.Config.iconSize}"));
    }

    private void OnFontSize(string value)
    {
        system.Config.textSize = Math.Clamp(SanitiseIconInt(value, system.Config.textSize), 0, system.Config.iconSize);
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

    private Vec2i AlignmentNameToValues(string name)
    {
        Vec2i align = new();

        switch (name.ToInt(4))
        {
            case 0:// Top Left
                align.Y = -1;
                align.X = -1;
                break;
            case 1: // Top Center
                align.Y = -1;
                align.X = 0;
                break;
            case 2: // Top Right
                align.Y = -1;
                align.X = 1;
                break;
            case 3: // Center Left
                align.Y = 0;
                align.X = -1;
                break;
            case 5: // Center Right
                align.Y = 0;
                align.X = 1;
                break;
            case 6: // Bottom Left
                align.Y = 1;
                align.X = -1;
                break;
            case 7: // Bottom Center
                align.Y = 1;
                align.X = 0;
                break;
            case 8: // Bottom Right
                align.Y = 1;
                align.X = 1;
                break;
            default: // True Center
                align.Y = 0;
                align.X = 0;
                break;
        }
        return align;
    }

    private void OnAlignChange(string value, bool selected)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);
        if (element == null) return;

        Vec2i align = AlignmentNameToValues(value);
        element.Pos(align.X, element.pos.x, align.Y, element.pos.y);
        element.Ping();

        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element {selectedElementName} set to {Lang.GetL("en", elementAlignments[int.Parse(value)])} alignment"));
    }

    private void OnEnable(bool on)
    {
        if (on)
        {
            StatusHudElement element = system.Set(selectedElementName);

            int x = SingleComposer.GetTextInput("shud-xpos").GetText().ToInt(0);
            int y = SingleComposer.GetTextInput("shud-ypos").GetText().ToInt(0);
            Vec2i align = AlignmentNameToValues(SingleComposer.GetDropDown("shud-align").SelectedValue);
            element.Pos(align.X, x, align.Y, y);
            element.Ping();

            capi.ShowChatMessage(StatusHudSystem.PrintModName(Lang.Get("statushudcont:Element {0} created", Lang.Get($"statushudcont:{selectedElementName}-name"))));
            capi.Logger.Debug(StatusHudSystem.PrintModName(string.Format("Element {0} created", selectedElementName)));
        }
        else
        {
            system.Unset(selectedElementName);
            capi.ShowChatMessage(StatusHudSystem.PrintModName(Lang.Get("statushudcont:Element {0} removed", Lang.Get($"statushudcont:{selectedElementName}-name"))));
            capi.Logger.Debug(StatusHudSystem.PrintModName(string.Format("Element {0} removed", selectedElementName)));
        }
    }
}