using System;
using System.Linq;
using StatusHud;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

namespace statushud;

public class StatusHudConfigGui : GuiDialog
{
    private readonly string[] elementNames = [.. StatusHudSystem.ElementTypes.Keys];

    private readonly StatusHudSystem system;
    private readonly string[] translatedElementNames;

    private readonly string[] translatedHudAlign = Enum.GetValues<HudAlign>()
        .Select(e => Lang.Get($"statushudcont:{e}"))
        .ToArray();

    private readonly string[] translatedTextAlign =
        Enum.GetValues<StatusHudPos.TextAlign>()
            .Select(e => Lang.Get($"statushudcont:{e}"))
            .ToArray();

    private bool elementScaleChange;
    private bool offsetChange;
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

        translatedElementNames = elementNames.Select(e => Lang.Get($"statushudcont:{e}-name")).ToArray();

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
        string optionsText = "";
        string optionsTooltip = "";
        string[] optionsValue = [];
        string[] optionsName = [];

        StatusHudElement element = (StatusHudElement)Activator.CreateInstance(StatusHudSystem.ElementTypes[selectedElementName], system);

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
        ElementBounds moveElementTextBounds = ElementBounds.Fixed(0, 0, 350, 23).WithAlignment(EnumDialogArea.CenterFixed)
            .FixedUnder(showHiddenButtonBounds, vertOffset * 1.25f);
        ElementBounds moveElementDropdownBounds = ElementBounds.Fixed(0, 0, 370, 25).FixedUnder(moveElementTextBounds, vertOffset * 1.25f)
            .WithAlignment(EnumDialogArea.CenterFixed);

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

        if (element == null || element.ElementOption == "" || element.ElementOptionList == null)
        {
            editingBounds.WithChildren(enableElementButtonBounds, alignElementDropdownBounds, xPosInputBounds, xPosInputBounds, yPosTextBounds, yPosInputBounds,
                textAlignTextBounds, textAlignInputBounds, textAlignOffsetTextBounds, textAlignOffsetInputBounds);
        }
        else
        {
            editingBounds.WithChildren(enableElementButtonBounds, alignElementDropdownBounds, xPosInputBounds, xPosInputBounds, yPosTextBounds, yPosInputBounds,
                textAlignTextBounds, textAlignInputBounds, textAlignOffsetTextBounds, textAlignOffsetInputBounds, optionalConfigTextBounds,
                optionalConfigDropdownBounds);

            string elementName = element.ElementName;

            // TimeLocal uses the same lang options as Time
            if (element.GetType() == typeof(StatusHudTimeLocalElement))
            {
                elementName = StatusHudTimeElement.Name;
            }

            optionsValue = element.ElementOptionList;
            optionIndex = Math.Max(optionsName.IndexOf(elementName), 0);

            optionsText = Lang.Get($"statushudcont:{elementName}-opt-text");
            optionsTooltip = Lang.Get($"statushudcont:{elementName}-opt-tooltip");

            for (int i = 0; i < optionsValue.Length; i++)
            {
                string key = $"statushudcont:{elementName}-opt-{i + 1}";
                optionsName = optionsName.Append(Lang.HasTranslation(key) ? Lang.Get(key) : optionsValue[i]);
            }
        }

        // Delete element so it isn't displayed on the screen
        element?.Dispose();

        // Create Element Editing Group Background
        ElementBounds editingBgBounds = ElementBounds.Fill.FixedUnder(moveElementDropdownBounds, vertOffset / 2).WithAlignment(EnumDialogArea.CenterFixed);
        editingBgBounds.BothSizing = ElementSizing.FitToChildren;
        editingBgBounds.WithChildren(editingBounds);

        // Add padding around all elementBounds, and shift so is below titlebar
        ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;
        bgBounds.WithChildren(saveButtonBounds, defaultButtonBounds, restoreButtonBounds, elementScaleTextBounds, elementScaleInputBounds,
            showHiddenButtonBounds,
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
                .AddDropDown(elementNames, translatedElementNames, selectedElementIndex, OnElementChange, moveElementDropdownBounds)
                .AddAutoSizeHoverText(Lang.Get($"statushudcont:{selectedElementName}-desc"), CairoFont.WhiteSmallText(), tooltipLength,
                    moveElementDropdownBounds.FlatCopy())
                .BeginChildElements(editingBgBounds)
                .BeginChildElements(editingBounds)
                .AddToggleButton(Lang.Get("statushudcont:Enable"), CairoFont.ButtonText(), OnEnable, enableElementButtonBounds, "shud-enablebutton")
                .AddDropDown(Enum.GetNames(typeof(HudAlign)), translatedHudAlign, 4, OnAlignChange, alignElementDropdownBounds, "shud-align")
                .AddStaticText(Lang.Get("statushudcont:X Position"), CairoFont.WhiteSmallishText(), xPosTextBounds)
                .AddNumberInput(xPosInputBounds, OnXPos, key: "shud-xpos")
                .AddStaticText(Lang.Get("statushudcont:Y Position"), CairoFont.WhiteSmallishText(), yPosTextBounds)
                .AddNumberInput(yPosInputBounds, OnYPos, key: "shud-ypos")
                .AddStaticText(Lang.Get("statushudcont:Alignment"), CairoFont.WhiteSmallishText(), textAlignTextBounds)
                .AddDropDown(Enum.GetNames(typeof(StatusHudPos.TextAlign)), translatedTextAlign, 0, OnTextAlignChange,
                    textAlignInputBounds, "shud-textalign")
                .AddStaticText(Lang.Get("statushudcont:Offset"), CairoFont.WhiteSmallishText(), textAlignOffsetTextBounds)
                .AddNumberInput(textAlignOffsetInputBounds, OnTextAlignOffsetChange, key: "shud-textalignoffset")
                .AddIf(optionsValue.Length > 0)
                .AddStaticText(optionsText, CairoFont.WhiteSmallishText(), optionalConfigTextBounds)
                .AddDropDown(optionsValue, optionsName, optionIndex, OnOptionalConfig, optionalConfigDropdownBounds)
                .AddAutoSizeHoverText(optionsTooltip, CairoFont.WhiteSmallText(), tooltipLength, optionalConfigDropdownBounds.FlatCopy())
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
            SingleComposer.GetDropDown("shud-align").SetSelectedValue(nameof(HudAlign.TrueCenter));
        }
        else
        {
            SingleComposer.GetToggleButton("shud-enablebutton").SetValue(true);
            SingleComposer.GetTextInput("shud-xpos").SetValue(element.pos.x);
            SingleComposer.GetTextInput("shud-ypos").SetValue(element.pos.y);
            SingleComposer.GetDropDown("shud-textalign").SetSelectedValue(element.pos.textAlign.ToString());
            SingleComposer.GetNumberInput("shud-textalignoffset").SetValue(element.pos.textAlignOffset);

            HudAlign alignDisplayVal = HudAlign.TrueCenter;

            alignDisplayVal = element.pos.vertAlign switch
            {
                StatusHudPos.VertAlign.Top => element.pos.horizAlign switch
                {
                    StatusHudPos.HorizAlign.Left => HudAlign.TopLeft,
                    StatusHudPos.HorizAlign.Center => HudAlign.TopCenter,
                    StatusHudPos.HorizAlign.Right => HudAlign.TopRight,
                    _ => alignDisplayVal
                },
                StatusHudPos.VertAlign.Middle => element.pos.horizAlign switch
                {
                    StatusHudPos.HorizAlign.Left => HudAlign.CenterLeft,
                    StatusHudPos.HorizAlign.Center => HudAlign.TopCenter,
                    StatusHudPos.HorizAlign.Right => HudAlign.CenterRight,
                    _ => alignDisplayVal
                },
                StatusHudPos.VertAlign.Bottom => element.pos.horizAlign switch
                {
                    StatusHudPos.HorizAlign.Left => HudAlign.BottomLeft,
                    StatusHudPos.HorizAlign.Center => HudAlign.BottomCenter,
                    StatusHudPos.HorizAlign.Right => HudAlign.BottomRight,
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
        // Avoid infinite loop when setting value for number input
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

        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element {name} selected"));
        ReloadElementInputs(GetElementFromName(name));
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
        // Avoid infinite loop when setting value for number input
        if (offsetChange)
        {
            offsetChange = false;
            return;
        }
        offsetChange = true;

        int offset = Math.Clamp(value.ToInt(), -StatusHudSystem.IconSize,
            Math.Max(system.capi.Render.FrameWidth, system.capi.Render.FrameHeight));
        SingleComposer.GetNumberInput("shud-textalignoffset").SetValue(offset);

        StatusHudElement element = GetElementFromName(selectedElementName);
        if (element == null) return;

        element.pos.textAlignOffset = offset;
        element.SetPos();
        capi.Logger.Debug(StatusHudSystem.PrintModName($"Element text alignment offset changed to {offset}"));
    }

    private static Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign> AlignmentNameToPos(string name)
    {
        Tuple<StatusHudPos.VertAlign, StatusHudPos.HorizAlign> pos;
        HudAlign align = Enum.Parse<HudAlign>(name);

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

        capi.Logger.Debug(
            StatusHudSystem.PrintModName($"Element {selectedElementName} set to {value} alignment"));
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

            capi.ShowChatMessage(StatusHudSystem.PrintModName(Lang.Get("statushudcont:Element {0} created",
                Lang.Get($"statushudcont:{selectedElementName}-name"))));
            capi.Logger.Debug(StatusHudSystem.PrintModName($"Element {selectedElementName} created"));
        }
        else
        {
            system.Unset(StatusHudSystem.ElementTypes[selectedElementName]);
            capi.ShowChatMessage(StatusHudSystem.PrintModName(Lang.Get("statushudcont:Element {0} removed",
                Lang.Get($"statushudcont:{selectedElementName}-name"))));
            capi.Logger.Debug(StatusHudSystem.PrintModName($"Element {selectedElementName} removed"));
        }
    }

    private void OnOptionalConfig(string code, bool selected)
    {
        StatusHudElement element = GetElementFromName(selectedElementName);

        element?.ConfigOptions(code);
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