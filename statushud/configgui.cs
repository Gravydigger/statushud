using System;
using System.Collections.Generic;
using System.Linq;
using StatusHud;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

namespace statushud;

public class StatusHudConfigGui : GuiDialog
{
    private readonly string[] elementAlignments =
    [
        Lang.Get("statushudcont:Top Left"),
        Lang.Get("statushudcont:Top Center"),
        Lang.Get("statushudcont:Top Right"),
        Lang.Get("statushudcont:Center Left"),
        Lang.Get("statushudcont:True Center"),
        Lang.Get("statushudcont:Center Right"),
        Lang.Get("statushudcont:Bottom Left"),
        Lang.Get("statushudcont:Bottom Center"),
        Lang.Get("statushudcont:Bottom Right")
    ];

    private readonly string[] elementAlignmentsValue = ["0", "1", "2", "3", "4", "5", "6", "7", "8"];
    private readonly string[] elementNames = [.. StatusHudSystem.ElementTypes.Keys];
    private readonly List<string> elementNamesTranslated = [];

    private readonly StatusHudSystem system;

    private readonly string[] translatedTextAlign =
        Enum.GetValues(typeof(StatusHudPos.TextAlign))
            .Cast<StatusHudPos.TextAlign>()
            .Select(e => Lang.Get($"statushudcont:{e}"))
            .ToArray();

    private bool elementScaleChange;
    private float scaled;
    private int selectedElementIndex;
    private string selectedElementName;

    public StatusHudConfigGui(ICoreClientAPI capi, StatusHudSystem system) : base(capi)
    {
        this.system = system;
        scaled = RuntimeEnv.GUIScale;
        elementScaleChange = false;

        selectedElementName = elementNames[0];
        selectedElementIndex = 0;

        elementNames.Foreach(name => elementNamesTranslated.Add(Lang.Get($"statushudcont:{name}-name")));

        ReloadElementInputs(GetElementFromName(selectedElementName));
    }
    public override string ToggleKeyCombinationCode => "statushudconfiggui";

    public override void OnGuiOpened()
    {
        base.OnGuiOpened();

        if (scaled == RuntimeEnv.GUIScale) return;
        scaled = RuntimeEnv.GUIScale;
        ReloadElementInputs(GetElementFromName(selectedElementName));
    }

    private void ComposeDialog()
    {
        const int titleBarHeight = 25;
        const int vertOffset = 20;
        const int horizOffset = 25;
        const int tooltipLength = 350;
        int optionIndex = 0;

        StatusHudElement element = GetElementFromName(selectedElementName);

        // Create Config Buttons
        ElementBounds saveButtonBounds = ElementBounds.Fixed(0, 0, 100, 23).WithFixedPadding(10, 4);
        ElementBounds defaultButtonBounds = saveButtonBounds.FlatCopy().FixedRightOf(saveButtonBounds, horizOffset);
        ElementBounds restoreButtonBounds = saveButtonBounds.FlatCopy().FixedRightOf(defaultButtonBounds, horizOffset);

        // Create Number Inputs
        ElementBounds elementScaleTextBounds = ElementBounds.Fixed(0, 0, 150, 30).FixedUnder(saveButtonBounds, vertOffset * 1.5f);
        ElementBounds elementScaleInputBounds = ElementBounds.Fixed(0, elementScaleTextBounds.fixedY, 205, 30).WithAlignment(EnumDialogArea.RightFixed);

        // Create Show Hidden Button
        ElementBounds showHiddenButtonBounds = ElementBounds.Fixed(0, 0, 350, 23).WithFixedPadding(10, 4).FixedUnder(elementScaleTextBounds, vertOffset)
            .WithAlignment(EnumDialogArea.CenterFixed);

        // Create Drop Down for Selecting Elements
        ElementBounds moveElementTextBounds = ElementBounds.Fixed(0, 0, 350, 23).WithAlignment(EnumDialogArea.CenterFixed).FixedUnder(showHiddenButtonBounds, vertOffset * 1.25f);
        ElementBounds moveElementDropdownBounds = ElementBounds.Fixed(0, 0, 370, 25).FixedUnder(moveElementTextBounds, vertOffset * 1.25f).WithAlignment(EnumDialogArea.CenterFixed);

        // Create Selected Element Buttons
        ElementBounds enableElementButtonBounds = ElementBounds.Fixed(0, 0, 120, 23).WithFixedPadding(10, 4);
        ElementBounds alignElementDropdownBounds = ElementBounds.Fixed(0, 0, 205, 30).FixedRightOf(enableElementButtonBounds, horizOffset);

        ElementBounds xPosTextBounds = ElementBounds.Fixed(0, 0, 150, 30).FixedUnder(enableElementButtonBounds, vertOffset * 1.5f);
        ElementBounds xPosInputBounds = ElementBounds.Fixed(0, xPosTextBounds.fixedY, 205, 30).WithAlignment(EnumDialogArea.RightFixed);
        ElementBounds yPosTextBounds = ElementBounds.Fixed(0, 0, 150, 30).FixedUnder(xPosInputBounds, vertOffset / 2);
        ElementBounds yPosInputBounds = ElementBounds.Fixed(0, yPosTextBounds.fixedY, 205, 30).WithAlignment(EnumDialogArea.RightFixed);

        ElementBounds textAlignTextBounds = ElementBounds.Fixed(0, 0, 150, 30).FixedUnder(yPosTextBounds, vertOffset / 2);
        ElementBounds textAlignInputBounds = ElementBounds.Fixed(0, textAlignTextBounds.fixedY, 205, 30).WithAlignment(EnumDialogArea.RightFixed);
        ElementBounds textAlignOffsetTextBounds = ElementBounds.Fixed(0, 0, 150, 30).FixedUnder(textAlignTextBounds, vertOffset / 2);
        ElementBounds textAlignOffsetInputBounds = ElementBounds.Fixed(0, textAlignOffsetTextBounds.fixedY, 205, 30).WithAlignment(EnumDialogArea.RightFixed);

        ElementBounds optionalConfigTextBounds = ElementBounds.Fixed(0, 0, 150, 30).FixedUnder(textAlignOffsetTextBounds, vertOffset / 2);
        ElementBounds optionalConfigDropdownBounds = ElementBounds.Fixed(0, optionalConfigTextBounds.fixedY, 205, 30).WithAlignment(EnumDialogArea.RightFixed);

        // Create Element Editing Group
        ElementBounds editingBounds = ElementBounds.Fill.WithFixedPadding(10);
        editingBounds.BothSizing = ElementSizing.FitToChildren;

        if (element == null || element.ElementOption == "")
        {
            editingBounds.WithChildren(enableElementButtonBounds, alignElementDropdownBounds, xPosInputBounds, xPosInputBounds, yPosTextBounds, yPosInputBounds,
                textAlignTextBounds, textAlignInputBounds, textAlignOffsetTextBounds, textAlignOffsetInputBounds);
        }
        else
        {
            editingBounds.WithChildren(enableElementButtonBounds, alignElementDropdownBounds, xPosInputBounds, xPosInputBounds, yPosTextBounds, yPosInputBounds,
                textAlignTextBounds, textAlignInputBounds, textAlignOffsetTextBounds, textAlignOffsetInputBounds, optionalConfigTextBounds, optionalConfigDropdownBounds);

            string[] options = [];

            switch (element.ElementName)
            {
                case StatusHudTimeElement.name:
                case StatusHudTimeLocalElement.name:
                    options = StatusHudTimeElement.TimeFormatWords;
                    break;
                case StatusHudWeatherElement.name:
                    options = StatusHudWeatherElement.TempFormatWords;
                    break;
                case StatusHudBodyHeatElement.name:
                    options = StatusHudBodyHeatElement.TempFormatWords;
                    break;
                case StatusHudCompassElement.name:
                    options = StatusHudCompassElement.CompassBearingOptions;
                    break;
                case StatusHudRiftActivityElement.name:
                    options = StatusHudRiftActivityElement.RiftChangeOptions;
                    break;
                default:
                    capi.Logger.Warning(StatusHudSystem.PrintModName($"Tried to get config options from {element.ElementName}, but it doesn't have any!"));
                    break;
            }

            optionIndex = Math.Max(options.IndexOf(element.ElementOption), 0);
        }

        // Create Element Editing Group Background
        ElementBounds editingBgBounds = ElementBounds.Fill.FixedUnder(moveElementDropdownBounds, vertOffset / 2).WithAlignment(EnumDialogArea.CenterFixed);
        editingBgBounds.BothSizing = ElementSizing.FitToChildren;
        editingBgBounds.WithChildren(editingBounds);

        // Add padding around all elementBounds, and shift so is below titlebar
        ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;
        bgBounds.WithChildren(saveButtonBounds, defaultButtonBounds, restoreButtonBounds, elementScaleTextBounds, elementScaleInputBounds, showHiddenButtonBounds,
            moveElementDropdownBounds, editingBgBounds);
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
                .AddStaticTextAutoFontSize(Lang.Get("statushudcont:Element Scale"), CairoFont.WhiteSmallishText(), elementScaleTextBounds)
                .AddNumberInput(elementScaleInputBounds, OnElementScale, key: "shud-elementscale")
                .AddToggleButton(Lang.Get("statushudcont:Show Hidden Elements"), CairoFont.ButtonText(), OnHidden, showHiddenButtonBounds, "shud-hidden")
                .AddStaticText(Lang.Get("statushudcont:Edit Element"), CairoFont.ButtonText(), EnumTextOrientation.Center, moveElementTextBounds)
                .AddDropDown(elementNames, elementNamesTranslated.ToArray(), selectedElementIndex, OnElementChange, moveElementDropdownBounds)
                .AddAutoSizeHoverText(Lang.Get($"statushudcont:{selectedElementName}-desc"), CairoFont.WhiteSmallText(), tooltipLength, moveElementDropdownBounds.FlatCopy())
                .BeginChildElements(editingBgBounds)
                .BeginChildElements(editingBounds)
                .AddToggleButton(Lang.Get("statushudcont:Enable"), CairoFont.ButtonText(), OnEnable, enableElementButtonBounds, "shud-enablebutton")
                .AddDropDown(elementAlignmentsValue, elementAlignments, 4, OnAlignChange, alignElementDropdownBounds, "shud-align")
                .AddStaticText(Lang.Get("statushudcont:X Position"), CairoFont.WhiteSmallishText(), xPosTextBounds)
                .AddNumberInput(xPosInputBounds, OnXPos, key: "shud-xpos")
                .AddStaticText(Lang.Get("statushudcont:Y Position"), CairoFont.WhiteSmallishText(), yPosTextBounds)
                .AddNumberInput(yPosInputBounds, OnYPos, key: "shud-ypos")
                .AddStaticText(Lang.Get("statushudcont:Alignment"), CairoFont.WhiteSmallishText(), textAlignTextBounds)
                .AddDropDown(Enum.GetNames(typeof(StatusHudPos.TextAlign)), translatedTextAlign, 0, OnTextAlignChange,
                    textAlignInputBounds, "shud-textalign")
                .AddStaticText(Lang.Get("statushudcont:Offset"), CairoFont.WhiteSmallishText(), textAlignOffsetTextBounds)
                .AddNumberInput(textAlignOffsetInputBounds, OnTextAlignOffsetChange, key: "shud-textalignoffset")
                .AddIf(selectedElementName is StatusHudTimeElement.name or StatusHudTimeLocalElement.name)
                .AddStaticText(Lang.Get("statushudcont:Time Format"), CairoFont.WhiteSmallText(), optionalConfigTextBounds)
                .AddDropDown(StatusHudTimeElement.TimeFormatWords, [Lang.Get("statushudcont:time-opt-1"), Lang.Get("statushudcont:time-opt-2")], optionIndex, OnOptionalConfig,
                    optionalConfigDropdownBounds)
                .AddAutoSizeHoverText(Lang.Get("statushudcont:time-format-tooltip"), CairoFont.WhiteSmallText(), tooltipLength, optionalConfigDropdownBounds.FlatCopy())
                .EndIf()
                .AddIf(selectedElementName == StatusHudWeatherElement.name)
                .AddStaticText(Lang.Get("statushudcont:Temp Scale"), CairoFont.WhiteSmallText(), optionalConfigTextBounds)
                .AddDropDown(StatusHudWeatherElement.TempFormatWords, StatusHudWeatherElement.TempFormatWords, optionIndex, OnOptionalConfig, optionalConfigDropdownBounds)
                .AddAutoSizeHoverText(Lang.Get("statushudcont:temp-scale-weather-tooltip"), CairoFont.WhiteSmallText(), tooltipLength, optionalConfigDropdownBounds.FlatCopy())
                .EndIf()
                .AddIf(selectedElementName == StatusHudBodyHeatElement.name)
                .AddStaticText(Lang.Get("statushudcont:Temp Scale"), CairoFont.WhiteSmallText(), optionalConfigTextBounds)
                .AddDropDown(StatusHudBodyHeatElement.TempFormatWords, StatusHudBodyHeatElement.TempFormatWords, optionIndex, OnOptionalConfig, optionalConfigDropdownBounds)
                .AddAutoSizeHoverText(Lang.Get("statushudcont:temp-scale-bodyheat-tooltip"), CairoFont.WhiteSmallText(), tooltipLength, optionalConfigDropdownBounds.FlatCopy())
                .EndIf()
                .AddIf(selectedElementName == StatusHudCompassElement.name)
                .AddStaticText(Lang.Get("statushudcont:Heading"), CairoFont.WhiteSmallishText(), optionalConfigTextBounds)
                .AddDropDown(StatusHudCompassElement.CompassBearingOptions, [Lang.Get("statushudcont:compass-opt-1"), Lang.Get("statushudcont:compass-opt-2")], optionIndex,
                    OnOptionalConfig, optionalConfigDropdownBounds)
                .AddAutoSizeHoverText(Lang.Get("statushudcont:compass-heading-tooltip"), CairoFont.WhiteSmallText(), tooltipLength, optionalConfigDropdownBounds.FlatCopy())
                .EndIf()
                .AddIf(selectedElementName == StatusHudRiftActivityElement.name)
                .AddStaticText(Lang.Get("statushudcont:Display Time"), CairoFont.WhiteSmallText(), optionalConfigTextBounds)
                .AddDropDown(StatusHudRiftActivityElement.RiftChangeOptions, [Lang.Get("statushudcont:riftactivity-opt-1"), Lang.Get("statushudcont:riftactivity-opt-2")],
                    optionIndex, OnOptionalConfig, optionalConfigDropdownBounds)
                .AddAutoSizeHoverText(Lang.Get("statushudcont:riftactivity-tooltip"), CairoFont.WhiteSmallText(), tooltipLength, optionalConfigDropdownBounds.FlatCopy())
                .EndIf()
                .EndChildElements()
                .EndChildElements()
                .EndChildElements()
                .Compose()
            ;

        SingleComposer.GetNumberInput("shud-elementscale").SetValue(system.Config.elementScale);
        SingleComposer.GetNumberInput("shud-elementscale").Interval = 0.25f;
        SingleComposer.GetNumberInput("shud-xpos").Interval = 100f;
        SingleComposer.GetNumberInput("shud-ypos").Interval = 100f;
    }

    private void OnOptionalConfig(string code, bool selected)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);

        element?.ConfigOptions(code);
    }

    private StatusHudElement GetElementFromName(string name)
    {
        return system.elements.FirstOrDefault(e => e.ElementName == name);
    }

    private void ReloadElementInputs(StatusHudElement element)
    {
        ComposeDialog();
        if (element == null)
        {
            SingleComposer.GetToggleButton("shud-enablebutton").SetValue(false);
            SingleComposer.GetNumberInput("shud-xpos").SetValue(0);
            SingleComposer.GetNumberInput("shud-ypos").SetValue(0);
            SingleComposer.GetNumberInput("shud-textalignoffset").SetValue(0);
            SingleComposer.GetDropDown("shud-textalign").SetSelectedValue(nameof(StatusHudPos.TextAlign.Up));
            SingleComposer.GetDropDown("shud-align").SetSelectedValue(((int)HudAlign.TrueCenter).ToString());
        }
        else
        {
            SingleComposer.GetToggleButton("shud-enablebutton").SetValue(true);
            SingleComposer.GetTextInput("shud-xpos").SetValue(element.pos.x);
            SingleComposer.GetTextInput("shud-ypos").SetValue(element.pos.y);
            SingleComposer.GetDropDown("shud-textalign").SetSelectedValue(element.pos.textAlign.ToString());
            SingleComposer.GetNumberInput("shud-textalignoffset").SetValue(element.pos.textAlignOffset);

            int alignDisplayVal = (int)HudAlign.TrueCenter;

            alignDisplayVal = element.pos.vertAlign switch
            {
                StatusHudPos.VertAlign.Top => element.pos.horizAlign switch
                {
                    StatusHudPos.HorizAlign.Left => (int)HudAlign.TopLeft,
                    StatusHudPos.HorizAlign.Center => (int)HudAlign.TopCenter,
                    StatusHudPos.HorizAlign.Right => (int)HudAlign.TopRight,
                    _ => alignDisplayVal
                },
                StatusHudPos.VertAlign.Middle => element.pos.horizAlign switch
                {
                    StatusHudPos.HorizAlign.Left => (int)HudAlign.CenterLeft,
                    StatusHudPos.HorizAlign.Center => (int)HudAlign.TopCenter,
                    StatusHudPos.HorizAlign.Right => (int)HudAlign.CenterRight,
                    _ => alignDisplayVal
                },
                StatusHudPos.VertAlign.Bottom => element.pos.horizAlign switch
                {
                    StatusHudPos.HorizAlign.Left => (int)HudAlign.BottomLeft,
                    StatusHudPos.HorizAlign.Center => (int)HudAlign.BottomCenter,
                    StatusHudPos.HorizAlign.Right => (int)HudAlign.BottomRight,
                    _ => alignDisplayVal
                },
                _ => alignDisplayVal
            };

            SingleComposer.GetDropDown("shud-align").SetSelectedValue(alignDisplayVal.ToString());
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

    private void OnElementScale(string value)
    {
        if (elementScaleChange)
        {
            elementScaleChange = false;
            return;
        }

        elementScaleChange = true;
        system.Config.elementScale = Math.Max(0, SanitiseIconFloat(value, system.Config.elementScale));
        system.ReloadElements();
        SingleComposer.GetNumberInput("shud-elementscale").SetValue(system.Config.elementScale);
    }

    private void OnXPos(string value)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);
        if (element == null) return;

        element.pos.x = value.ToInt();
        element.SetPos();
        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element X Position changed to {element.pos.x}"));
    }

    private void OnYPos(string value)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);
        if (element == null) return;

        element.pos.y = value.ToInt();
        element.SetPos();
        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element Y Position changed to {element.pos.y}"));
    }

    private static float SanitiseIconFloat(string value, float defaultFloat)
    {
        const float minSize = 0.5f;
        const float maxSize = 2f;

        return (float)Math.Round(Math.Clamp(value.ToFloat(defaultFloat), minSize, maxSize), 2);
    }

    private void OnHidden(bool on)
    {
        system.Config.showHidden = on;
        system.ReloadElements();

        capi.Logger.Debug(StatusHudSystem.PrintModName(on
            ? "Showing hidden elements"
            : "Hiding hidden elements"));
    }

    private void OnElementChange(string name, bool selected)
    {
        selectedElementName = name;

        for (int i = 0; i < elementNames.Length; i++)
        {
            if (elementNames[i] != name) continue;
            selectedElementIndex = i;
            break;
        }

        ReloadElementInputs(GetElementFromName(name));

        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element {name} selected"));
    }

    private void OnTextAlignChange(string name, bool selected)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);
        if (element == null) return;

        if (Enum.TryParse(name, out StatusHudPos.TextAlign textAlign))
        {
            element.SetPos(element.pos.horizAlign, element.pos.x, element.pos.vertAlign, element.pos.y, textAlign, element.pos.textAlignOffset);
            capi.Logger.Debug(StatusHudSystem.PrintModName($"Element text alignment changed to {name}"));
        }
        else
        {
            system.capi.Logger.Debug(StatusHudSystem.PrintModName($"Could not parse element {name} into an text alignment"));
        }
    }

    private void OnTextAlignOffsetChange(string value)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);
        if (element == null) return;

        element.pos.textAlignOffset = value.ToInt();
        element.SetPos();
        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element text alignment offset changed to {element.pos.textAlignOffset}"));
    }

    private static Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign> AlignmentNameToPos(string name)
    {
        Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign> pos;
        HudAlign align = (HudAlign)name.ToInt((int)HudAlign.TrueCenter);

        switch (align)
        {
            case HudAlign.TopLeft:
                pos = new Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign>(StatusHudPos.VertAlign.Top, StatusHudPos.HorizAlign.Left);
                break;
            case HudAlign.TopCenter:
                pos = new Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign>(StatusHudPos.VertAlign.Top, StatusHudPos.HorizAlign.Center);
                break;
            case HudAlign.TopRight:
                pos = new Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign>(StatusHudPos.VertAlign.Top, StatusHudPos.HorizAlign.Right);
                break;
            case HudAlign.CenterLeft:
                pos = new Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign>(StatusHudPos.VertAlign.Middle, StatusHudPos.HorizAlign.Left);
                break;
            case HudAlign.CenterRight:
                pos = new Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign>(StatusHudPos.VertAlign.Middle, StatusHudPos.HorizAlign.Right);
                break;
            case HudAlign.BottomLeft:
                pos = new Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign>(StatusHudPos.VertAlign.Bottom, StatusHudPos.HorizAlign.Left);
                break;
            case HudAlign.BottomCenter:
                pos = new Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign>(StatusHudPos.VertAlign.Bottom, StatusHudPos.HorizAlign.Center);
                break;
            case HudAlign.BottomRight:
                pos = new Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign>(StatusHudPos.VertAlign.Bottom, StatusHudPos.HorizAlign.Right);
                break;
            case HudAlign.TrueCenter:
            default:
                pos = new Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign>(StatusHudPos.VertAlign.Middle, StatusHudPos.HorizAlign.Center);
                break;
        }
        return pos;
    }

    private void OnAlignChange(string value, bool selected)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);
        if (element == null) return;

        var align = AlignmentNameToPos(value);
        element.SetPos(align.Item2, element.pos.x, align.Item1, element.pos.y, element.pos.textAlign, element.pos.textAlignOffset);
        element.Ping();

        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element {selectedElementName} set to {Lang.GetL("en", elementAlignments[int.Parse(value)])} alignment"));
    }

    private void OnEnable(bool on)
    {
        if (on)
        {
            StatusHudElement element = system.Set(StatusHudSystem.ElementTypes[selectedElementName]);

            int x = SingleComposer.GetTextInput("shud-xpos").GetText().ToInt();
            int y = SingleComposer.GetTextInput("shud-ypos").GetText().ToInt();
            var align = AlignmentNameToPos(SingleComposer.GetDropDown("shud-align").SelectedValue);
            element.SetPos(align.Item2, x, align.Item1, y, element.pos.textAlign, element.pos.textAlignOffset);
            element.Ping();

            capi.ShowChatMessage(StatusHudSystem.PrintModName(Lang.Get("statushudcont:Element {0} created", Lang.Get($"statushudcont:{selectedElementName}-name"))));
            capi.Logger.Debug(StatusHudSystem.PrintModName($"Element {selectedElementName} created"));
        }
        else
        {
            system.Unset(StatusHudSystem.ElementTypes[selectedElementName]);
            capi.ShowChatMessage(StatusHudSystem.PrintModName(Lang.Get("statushudcont:Element {0} removed", Lang.Get($"statushudcont:{selectedElementName}-name"))));
            capi.Logger.Debug(StatusHudSystem.PrintModName($"Element {selectedElementName} removed"));
        }
    }

    private enum HudAlign
    {
        TopLeft,
        TopCenter,
        TopRight,
        CenterLeft,
        TrueCenter,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}