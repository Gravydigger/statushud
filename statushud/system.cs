using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace StatusHud
{
    public class StatusHudSystem : ModSystem
    {
        public const string domain = "statushudcont";
        private const int slowListenInterval = 1000;
        private const int fastListenInterval = 100;

        public static readonly Dictionary<string, Type> elementTypes =
            typeof(StatusHudElement).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(StatusHudElement)))
            .ToDictionary(
                t => (string)t.GetField("name", BindingFlags.Public | BindingFlags.Static)?.GetValue(null),
                t => t
            );

        private StatusHudConfigManager configManager;
        public StatusHudConfig Config => configManager.Config;

        public List<StatusHudElement> elements;
        private List<StatusHudElement> slowElements;
        private List<StatusHudElement> fastElements;
        public StatusHudTextures textures;
        private StatusHudConfigGui dialog;

        private string uuid = null;
        public string UUID => uuid;
        public bool ShowHidden => configManager.Config.showHidden;

        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Client;
        }

        public ICoreClientAPI capi;
        private long slowListenerId;
        private long fastListenerId;

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);
            this.capi = capi;

            configManager = new StatusHudConfigManager(this);

            elements = [];
            slowElements = [];
            fastElements = [];
            textures = new StatusHudTextures(this.capi, Config.iconSize);

            configManager.LoadElements(this);

            slowListenerId = this.capi.Event.RegisterGameTickListener(SlowTick, slowListenInterval);
            fastListenerId = this.capi.Event.RegisterGameTickListener(FastTick, fastListenInterval);

            if (Config.version < StatusHudConfigManager.version)
            {
                if (Config.elements.Count == 0)
                {
                    // Install default layout
                    InstallDefault();
                }

                Config.version = StatusHudConfigManager.version;
                SaveConfig();
            }

            capi.Event.PlayerJoin += SetUUID;

            dialog = new StatusHudConfigGui(capi, this);
            capi.Input.RegisterHotKey("statushudconfiggui", "Status Hud Menu", GlKeys.U, HotkeyType.GUIOrOtherControls);
            capi.Input.SetHotKeyHandler("statushudconfiggui", ToggleConfigGui);

            this.capi.Logger.Debug(PrintModName($"Current locale set to: {Lang.CurrentLocale}"));
        }

        public override void Dispose()
        {
            base.Dispose();

            capi.Event.UnregisterGameTickListener(slowListenerId);
            capi.Event.UnregisterGameTickListener(fastListenerId);
            foreach (var element in elements)
            {
                element.Dispose();
            }

            textures.Dispose();
            dialog.Dispose();
            capi.Event.PlayerJoin -= SetUUID;
        }

        public static string PrintModName(string text)
        {
            return $"[Status HUD] {text}";
        }

        public void SlowTick(float dt) => slowElements.ForEach(e => e.Tick());

        public void FastTick(float dt) => fastElements.ForEach(e => e.Tick());

        public StatusHudElement Set(Type type)
        {
            if (type == null) return null;

            StatusHudElement element = (StatusHudElement)Activator.CreateInstance(type, this, Config);

            if (element == null) return null;

            // Remove any other element of the same type.
            foreach (StatusHudElement elementVal in elements)
            {
                if (elementVal.GetType() == element.GetType())
                {
                    elementVal.Dispose();
                }
            }

            elements.Add(element);

            elements[elements.IndexOf(element)].Repos();

            if (element.fast)
            {
                fastElements.Add(element);
            }
            else
            {
                slowElements.Add(element);
            }
            return element;
        }

        public bool Unset(Type type)
        {
            StatusHudElement element = elements.FirstOrDefault(e => e.GetType() == type);
            if (element == null) return false;

            (element.fast ? fastElements : slowElements).Remove(element);
            element.Dispose();
            elements.Remove(element);

            return false;
        }

        // Will load elements from file
        public void LoadConfig()
        {
            Clear();
            configManager.Load();
            configManager.LoadElements(this);
        }

        public void Reload()
        {
            foreach (var element in elements)
            {
                element.GetRenderer().Reload();
                element.GetRenderer().UpdateRender();
            }
        }

        public void Reload(IClientPlayer byPlayer)
        {
            if (byPlayer != null && byPlayer.PlayerUID == UUID)
            {
                Reload();
            }
        }

        public static void Pos(StatusHudElement element, StatusHudPos.HorzAlign horzAlign, int x, StatusHudPos.VertAlign vertAlign, int y)
        {
            element.Pos(horzAlign, x, vertAlign, y);
        }

        protected void Clear()
        {
            fastElements.Clear();
            slowElements.Clear();

            foreach (var element in elements)
            {
                element.Dispose();
            }
            elements.Clear();
        }

        private void SetUUID(IClientPlayer byPlayer)
        {
            if (uuid == null && byPlayer != null)
            {
                uuid = byPlayer.PlayerUID;
            }
        }

        private bool ToggleConfigGui(KeyCombination comb)
        {
            if (dialog.IsOpened()) dialog.TryClose();
            else dialog.TryOpen();

            return true;
        }

        public void InstallDefault()
        {
            Clear();

            int size = Config.iconSize;
            int sideX = (int)Math.Round(size * 0.75f);
            int sideMinimapX = sideX + 256;
            int topY = size;
            int bottomY = (int)Math.Round(size * 0.375f);
            int offset = (int)Math.Round(size * 1.5f);

            Pos(Set(typeof(StatusHudDateElement)), StatusHudPos.HorzAlign.Left, sideX, StatusHudPos.VertAlign.Bottom, bottomY);

            Pos(Set(typeof(StatusHudTimeElement)), StatusHudPos.HorzAlign.Left, sideX + (int)(offset * 1.3f), StatusHudPos.VertAlign.Bottom, bottomY);

            Pos(Set(typeof(StatusHudWeatherElement)), StatusHudPos.HorzAlign.Left, sideX + (int)(offset * 2.5f), StatusHudPos.VertAlign.Bottom, bottomY);

            Pos(Set(typeof(StatusHudWindElement)), StatusHudPos.HorzAlign.Left, sideX + (int)(offset * 3.5f), StatusHudPos.VertAlign.Bottom, bottomY);

            Pos(Set(typeof(StatusHudArmourElement)), StatusHudPos.HorzAlign.Right, sideX + (int)(offset * 9f), StatusHudPos.VertAlign.Bottom, bottomY);

            Pos(Set(typeof(StatusHudStabilityElement)), StatusHudPos.HorzAlign.Right, sideX + (int)(offset * 10f), StatusHudPos.VertAlign.Bottom, bottomY);

            Pos(Set(typeof(StatusHudRoomElement)), StatusHudPos.HorzAlign.Right, -1 * (sideX + (int)(offset * 9f)), StatusHudPos.VertAlign.Bottom, bottomY);

            Pos(Set(typeof(StatusHudSleepElement)), StatusHudPos.HorzAlign.Right, sideMinimapX + offset, StatusHudPos.VertAlign.Top, topY);

            Pos(Set(typeof(StatusHudWetElement)), StatusHudPos.HorzAlign.Right, sideMinimapX, StatusHudPos.VertAlign.Top, topY);

            Pos(Set(typeof(StatusHudTimeLocalElement)), StatusHudPos.HorzAlign.Right, sideX, StatusHudPos.VertAlign.Bottom, bottomY);
        }

        public void SaveConfig()
        {
            configManager.Save();
        }
    }
}