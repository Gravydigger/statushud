using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using statushud;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

namespace StatusHud;

// ReSharper disable once ClassNeverInstantiated.Global
public class StatusHudSystem : ModSystem
{
    public const string Domain = "statushudcont";
    public const int IconSize = 32;
    private const int slowListenInterval = 1000;
    private const int fastListenInterval = 100;

    public ICoreClientAPI capi;

    private StatusHudConfigManager configManager;
    private StatusHudConfigGui dialog;

    internal List<StatusHudElement> elements;
    private List<StatusHudElement> fastElements;
    private long fastListenerId;
    private List<StatusHudElement> slowElements;
    private long slowListenerId;

    public StatusHudTextures textures { get; private set; }

    public static Dictionary<string, Type> ElementTypes { get; private set; }

    public StatusHudConfig Config => configManager.Config;
    public string Uuid { get; private set; }

    // private static Dictionary<string, Type> LoadElementTypes()
    // {
    //     return typeof(StatusHudElement).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(StatusHudElement)))
    //         .ToDictionary(t => (string)t.GetField("Name", BindingFlags.Public | BindingFlags.Static)?.GetValue(null), t => t);
    // }

    public override bool ShouldLoad(EnumAppSide side)
    {
        return side == EnumAppSide.Client;
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        base.StartClientSide(capi);
        this.capi = capi;

        configManager = new StatusHudConfigManager(this);

        elements = [];
        slowElements = [];
        fastElements = [];
        textures = new StatusHudTextures(this.capi, IconSize * Config.elementScale);

        configManager.LoadElements(this);
        configManager.Save();

        slowListenerId = this.capi.Event.RegisterGameTickListener(SlowTick, slowListenInterval);
        fastListenerId = this.capi.Event.RegisterGameTickListener(FastTick, fastListenInterval);

        capi.Event.PlayerJoin += SetUuid;
        // Used to check for new elements for other mods
        capi.Event.IsPlayerReady += PostLoad;

        capi.Input.RegisterHotKey("statushudconfiggui", "Status Hud Menu", GlKeys.U, HotkeyType.GUIOrOtherControls);
        capi.Input.SetHotKeyHandler("statushudconfiggui", ToggleConfigGui);

        this.capi.Logger.Debug(PrintModName($"Current locale set to: {Lang.CurrentLocale}"));
    }

    public override void Dispose()
    {
        base.Dispose();

        capi.Event.UnregisterGameTickListener(slowListenerId);
        capi.Event.UnregisterGameTickListener(fastListenerId);
        foreach (StatusHudElement element in elements)
        {
            element.Dispose();
        }

        textures.Dispose();
        dialog.Dispose();
        capi.Event.PlayerJoin -= SetUuid;
        capi.Event.IsPlayerReady -= PostLoad;
    }

    public static string PrintModName(string text)
    {
        return $"[Status HUD] {text}";
    }

    private void SlowTick(float dt)
    {
        slowElements.ForEach(e => e.Tick());
    }

    private void FastTick(float dt)
    {
        fastElements.ForEach(e => e.Tick());
    }

    public StatusHudElement Set(Type type)
    {
        if (type == null) return null;

        StatusHudElement element = (StatusHudElement)Activator.CreateInstance(type, this);

        if (element == null) return null;

        // Remove any other element of the same type.
        foreach (StatusHudElement elementVal in elements.Where(elementVal => elementVal.GetType() == element.GetType()))
        {
            elementVal.Dispose();
        }

        elements.Add(element);

        elements[elements.IndexOf(element)].Repos();

        (element.fast ? fastElements : slowElements).Add(element);

        return element;
    }

    public void Unset(Type type)
    {
        StatusHudElement element = elements.FirstOrDefault(e => e.GetType() == type);
        if (element == null) return;

        (element.fast ? fastElements : slowElements).Remove(element);
        element.Dispose();
        elements.Remove(element);
    }

    // Will load elements from file
    public void LoadConfig()
    {
        Clear();
        configManager.Load();
        configManager.LoadElements(this);
    }

    public void ReloadElements()
    {
        foreach (StatusHudElement element in elements)
        {
            element.GetRenderer().Reload();
            element.GetRenderer().UpdateRender();
        }
    }

    public static void SetPos(StatusHudElement element, StatusHudPos.HorizAlign horizAlign, int x,
        StatusHudPos.VertAlign vertAlign, int y, StatusHudPos.TextAlign textAlign, int orientOffset)
    {
        element.SetPos(horizAlign, x, vertAlign, y, textAlign, orientOffset);
    }

    private void Clear()
    {
        fastElements?.Clear();
        slowElements?.Clear();

        foreach (StatusHudElement element in elements)
        {
            element.Dispose();
        }

        elements.Clear();
    }

    private void SetUuid(IClientPlayer byPlayer)
    {
        if (Uuid == null && byPlayer != null)
        {
            Uuid = byPlayer.PlayerUID;
        }
    }

    private bool ToggleConfigGui(KeyCombination comb)
    {
        if (dialog.IsOpened()) dialog.TryClose();
        else dialog.TryOpen();

        return true;
    }

    private bool PostLoad(ref EnumHandling handled)
    {
        handled = EnumHandling.PassThrough;

        textures.LoadAllTextures();
        ElementTypes = typeof(StatusHudElement).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(StatusHudElement)))
            .ToDictionary(t => (string)t.GetField("Name", BindingFlags.Public | BindingFlags.Static)?.GetValue(null), t => t);

        dialog = new StatusHudConfigGui(capi, this);

        capi.Logger.VerboseDebug(PrintModName("Currently loaded element types:"));
        ElementTypes.Foreach(e => capi.Logger.VerboseDebug($"\t{e.Value}"));

        capi.Logger.VerboseDebug(PrintModName("Textures Loaded:"));
        textures.TexturesDict.Foreach(e => capi.Logger.VerboseDebug($"\t{e.Key}"));

        return true;
    }

    public void InstallDefault()
    {
        Clear();

        int sideX = (int)Math.Round(IconSize * 0.75f);
        int sideMinimapX = sideX + 256;
        int toolbarMidpoint = (int)(310 * RuntimeEnv.GUIScale);
        int yOffset = (int)Math.Round(IconSize * 0.375f);
        int offset = (int)Math.Round(IconSize * Config.elementScale * 1.5f);

        SetPos(Set(typeof(StatusHudDateElement)), StatusHudPos.HorizAlign.Left, sideX, StatusHudPos.VertAlign.Bottom, yOffset, StatusHudPos.TextAlign.Up, 0);

        SetPos(Set(typeof(StatusHudTimeElement)), StatusHudPos.HorizAlign.Left, sideX + offset, StatusHudPos.VertAlign.Bottom, yOffset,
            StatusHudPos.TextAlign.Up, 0);

        SetPos(Set(typeof(StatusHudWeatherElement)), StatusHudPos.HorizAlign.Left, sideX + (int)(offset * 2f), StatusHudPos.VertAlign.Bottom, yOffset,
            StatusHudPos.TextAlign.Up,
            0);

        SetPos(Set(typeof(StatusHudWindElement)), StatusHudPos.HorizAlign.Left, sideX + (int)(offset * 3f), StatusHudPos.VertAlign.Bottom, yOffset,
            StatusHudPos.TextAlign.Up, 0);

        SetPos(Set(typeof(StatusHudArmourElement)), StatusHudPos.HorizAlign.Center, sideX + toolbarMidpoint + offset, StatusHudPos.VertAlign.Bottom, yOffset,
            StatusHudPos.TextAlign.Up, 0);

        SetPos(Set(typeof(StatusHudStabilityElement)), StatusHudPos.HorizAlign.Center, sideX + toolbarMidpoint + offset * 2, StatusHudPos.VertAlign.Bottom,
            yOffset,
            StatusHudPos.TextAlign.Up, 0);

        SetPos(Set(typeof(StatusHudRoomElement)), StatusHudPos.HorizAlign.Center, -1 * (sideX + toolbarMidpoint + offset), StatusHudPos.VertAlign.Bottom,
            yOffset,
            StatusHudPos.TextAlign.Up, 0);

        SetPos(Set(typeof(StatusHudSleepElement)), StatusHudPos.HorizAlign.Right, sideMinimapX + offset, StatusHudPos.VertAlign.Top, yOffset,
            StatusHudPos.TextAlign.Down, 0);

        SetPos(Set(typeof(StatusHudWetElement)), StatusHudPos.HorizAlign.Right, sideMinimapX, StatusHudPos.VertAlign.Top, yOffset, StatusHudPos.TextAlign.Down,
            0);

        SetPos(Set(typeof(StatusHudTimeLocalElement)), StatusHudPos.HorizAlign.Right, sideX, StatusHudPos.VertAlign.Bottom, yOffset, StatusHudPos.TextAlign.Up,
            0);
    }

    public void SaveConfig()
    {
        configManager.Save();
    }
}