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

        public static readonly Type[] elementTypes =
            [.. Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(StatusHudElement)) && !t.IsAbstract)];
        public static readonly string[] elementNames = InitElementNames();

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

        private StatusHudElement Instantiate(string name)
        {
            Type type = elementTypes.FirstOrDefault(t =>
                (string)t.GetField("name", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) == name);

            return type != null ? (StatusHudElement)Activator.CreateInstance(type, this, Config) : null;
        }

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

        public void SlowTick(float dt)
        {
            foreach (StatusHudElement element in slowElements)
            {
                element.Tick();
            }
        }

        public void FastTick(float dt)
        {
            foreach (StatusHudElement element in fastElements)
            {
                element.Tick();
            }
        }

        public StatusHudElement Set(string name)
        {
            StatusHudElement element = Instantiate(name);

            if (element == null)
            {
                // Invalid element.
                return null;
            }

            StatusHudPos pos = null;

            // Remove any other element of the same type.
            foreach (StatusHudElement elementVal in elements)
            {
                if (elementVal.GetType() == element.GetType())
                {
                    elementVal.Dispose();
                }
            }

            elements.Add(element);
            int index = elements.IndexOf(element);

            if (pos != null)
            {
                // Retain previous position.
                elements[index].Pos(pos.halign, pos.x, pos.valign, pos.y);
            }
            else
            {
                elements[index].Repos();
            }

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

        public bool Unset(string name)
        {
            foreach (var element in elements)
            {
                if (element.ElementName == name)
                {
                    if (element.fast)
                    {
                        fastElements.Remove(element);
                    }
                    else
                    {
                        slowElements.Remove(element);
                    }

                    element.Dispose();
                    elements.Remove(element);
                    return true;
                }
            }

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

        public static void Pos(StatusHudElement element, int halign, int x, int valign, int y)
        {
            element.Pos(halign, x, valign, y);
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

            Pos(Set(StatusHudDateElement.name), StatusHudPos.halignLeft, sideX, StatusHudPos.valignBottom, bottomY);

            Pos(Set(StatusHudTimeElement.name), StatusHudPos.halignLeft, sideX + (int)(offset * 1.3f), StatusHudPos.valignBottom, bottomY);

            Pos(Set(StatusHudWeatherElement.name), StatusHudPos.halignLeft, sideX + (int)(offset * 2.5f), StatusHudPos.valignBottom, bottomY);

            Pos(Set(StatusHudWindElement.name), StatusHudPos.halignLeft, sideX + (int)(offset * 3.5f), StatusHudPos.valignBottom, bottomY);

            Pos(Set(StatusHudArmourElement.name), StatusHudPos.halignCenter, sideX + (int)(offset * 9f), StatusHudPos.valignBottom, bottomY);

            Pos(Set(StatusHudStabilityElement.name), StatusHudPos.halignCenter, sideX + (int)(offset * 10f), StatusHudPos.valignBottom, bottomY);

            Pos(Set(StatusHudRoomElement.name), StatusHudPos.halignCenter, -1 * (sideX + (int)(offset * 9f)), StatusHudPos.valignBottom, bottomY);

            Pos(Set(StatusHudSleepElement.name), StatusHudPos.halignRight, sideMinimapX + offset, StatusHudPos.valignTop, topY);

            Pos(Set(StatusHudWetElement.name), StatusHudPos.halignRight, sideMinimapX, StatusHudPos.valignTop, topY);

            Pos(Set(StatusHudTimeLocalElement.name), StatusHudPos.halignRight, sideX, StatusHudPos.valignBottom, bottomY);
        }

        public void SaveConfig()
        {
            configManager.Save();
        }

        private static string[] InitElementNames()
        {
            List<string> names = [];

            foreach (var type in elementTypes)
            {
                names.Add((string)type.GetField("name").GetValue(null));
            }

            return [.. names.OrderBy(name => name)];
        }
    }
}