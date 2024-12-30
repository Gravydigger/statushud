using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace StatusHud
{
    public class StatusHudSystem : ModSystem
    {
        public const string halignWordLeft = "left";
        public const string halignWordCenter = "center";
        public const string halignWordRight = "right";
        public const string valignWordTop = "top";
        public const string valignWordMiddle = "middle";
        public const string valignWordBottom = "bottom";
        public static readonly string[] halignWords = new string[] { halignWordLeft, halignWordCenter, halignWordRight };
        public static readonly string[] valignWords = new string[] { valignWordTop, valignWordMiddle, valignWordBottom };

        public const string visWordShow = "show";
        public const string visWordHide = "hide";
        public static readonly string[] visWords = new string[] { visWordShow, visWordHide };

        public static readonly string[] timeFormatWords = new string[] { "12hr", "24hr" };
        public static readonly string[] tempScaleWords = new string[] { "C", "F", "K" };

        public const string domain = "statushudcont";
        protected const int slowListenInterval = 1000;
        protected const int fastListenInterval = 100;

        public static readonly Type[] elementTypes = {
            typeof(StatusHudAltitudeElement),
            typeof(StatusHudArmourElement),
            typeof(StatusHudBodyheatElement),
            typeof(StatusHudCompassElement),
            typeof(StatusHudDateElement),
            typeof(StatusHudDurabilityElement),
            typeof(StatusHudLatitudeElement),
            typeof(StatusHudLightElement),
            typeof(StatusHudPingElement),
            typeof(StatusHudPlayersElement),
            typeof(StatusHudRiftActivityElement),
            typeof(StatusHudRoomElement),
            typeof(StatusHudSleepElement),
            typeof(StatusHudSpeedElement),
            typeof(StatusHudStabilityElement),
            typeof(StatusHudTempstormElement),
            typeof(StatusHudTimeElement),
            typeof(StatusHudTimeLocalElement),
            typeof(StatusHudWeatherElement),
            typeof(StatusHudWetElement),
            typeof(StatusHudWindElement)
        };
        public static readonly string[] elementNames = InitElementNames();
        protected static readonly string elementList = InitElementList();

        private static readonly int slotMax = elementTypes.Length;

        private StatusHudConfigManager configManager;
        public StatusHudConfig Config => configManager.Config;

        public IList<StatusHudElement> elements;
        protected IList<StatusHudElement> slowElements;
        protected IList<StatusHudElement> fastElements;
        public StatusHudTextures textures;

        private GuiDialog dialog;
        private string uuid = null;
        public bool ShowHidden => configManager.Config.showHidden;

        private StatusHudElement Instantiate(string name)
        {
            StatusHudConfig config = configManager.Config;

            return name switch
            {
                StatusHudAltitudeElement.name => new StatusHudAltitudeElement(this, config),
                StatusHudArmourElement.name => new StatusHudArmourElement(this, config),
                StatusHudBodyheatElement.name => new StatusHudBodyheatElement(this, config),
                StatusHudCompassElement.name => new StatusHudCompassElement(this, config),
                StatusHudDateElement.name => new StatusHudDateElement(this, config),
                StatusHudDurabilityElement.name => new StatusHudDurabilityElement(this, config),
                StatusHudLatitudeElement.name => new StatusHudLatitudeElement(this, config),
                StatusHudLightElement.name => new StatusHudLightElement(this, config),
                StatusHudPingElement.name => new StatusHudPingElement(this, config),
                StatusHudPlayersElement.name => new StatusHudPlayersElement(this, config),
                StatusHudRiftActivityElement.name => new StatusHudRiftActivityElement(this, config),
                StatusHudRoomElement.name => new StatusHudRoomElement(this),
                StatusHudSleepElement.name => new StatusHudSleepElement(this, config),
                StatusHudSpeedElement.name => new StatusHudSpeedElement(this, config),
                StatusHudStabilityElement.name => new StatusHudStabilityElement(this, config),
                StatusHudTempstormElement.name => new StatusHudTempstormElement(this, config),
                StatusHudTimeElement.name => new StatusHudTimeElement(this, config),
                StatusHudTimeLocalElement.name => new StatusHudTimeLocalElement(this, config),
                StatusHudWeatherElement.name => new StatusHudWeatherElement(this, config),
                StatusHudWetElement.name => new StatusHudWetElement(this, config),
                StatusHudWindElement.name => new StatusHudWindElement(this, config),
                _ => null,
            };
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

            elements = new List<StatusHudElement>();
            slowElements = new List<StatusHudElement>();
            fastElements = new List<StatusHudElement>();
            textures = new StatusHudTextures(this.capi, Config.iconSize);

            configManager.LoadElements(this);

            capi.ChatCommands.Create("shud")
                .WithDescription("Configure Status HUD")
                    .BeginSubCommand("default")
                        .WithDescription("Reset all elements to a default layout")
                        .HandleWith(CmdDefault)
                    .EndSubCommand()
                    .BeginSubCommand("set")
                        .WithDescription("Set status HUD element")
                        .WithArgs(capi.ChatCommands.Parsers.IntRange("slot", 0, slotMax),
                                capi.ChatCommands.Parsers.WordRange("element", StatusHudSystem.elementNames))
                        .HandleWith(CmdSet)
                    .EndSubCommand()
                    .BeginSubCommand("unset")
                        .WithDescription("Unset status HUD element")
                        .WithArgs(capi.ChatCommands.Parsers.IntRange("slot", 0, slotMax))
                        .HandleWith(CmdUnset)
                    .EndSubCommand()
                    .BeginSubCommand("clear")
                        .WithDescription("Unset all status HUD elements")
                        .HandleWith(CmdClear)
                    .EndSubCommand()
                    .BeginSubCommand("pos")
                        .WithDescription("Set status HUD element's position")
                        .WithArgs(capi.ChatCommands.Parsers.IntRange("slot", 0, slotMax),
                                capi.ChatCommands.Parsers.WordRange("halign", StatusHudSystem.halignWords),
                                capi.ChatCommands.Parsers.Int("x"),
                                capi.ChatCommands.Parsers.WordRange("valign", StatusHudSystem.valignWords),
                                capi.ChatCommands.Parsers.Int("y"))
                        .HandleWith(CmdPos)
                    .EndSubCommand()
                    .BeginSubCommand("repos")
                        .WithDescription("Reset status HUD element's position")
                        .WithArgs(capi.ChatCommands.Parsers.IntRange("slot", 0, slotMax))
                        .HandleWith(CmdRepos)
                    .EndSubCommand()
                    .BeginSubCommand("list")
                        .WithDescription("List current status HUD elements")
                        .HandleWith(CmdList)
                    .EndSubCommand()
                    .BeginSubCommand("info")
                        .WithDescription("Show status HUD element info")
                        .WithArgs(capi.ChatCommands.Parsers.WordRange("element", StatusHudSystem.elementNames))
                        .HandleWith(CmdInfo)
                    .EndSubCommand()
                    .BeginSubCommand("hidden")
                        .WithDescription("Show or hide hidden elements")
                        .WithArgs(capi.ChatCommands.Parsers.WordRange("show/hide", StatusHudSystem.visWords))
                        .HandleWith(CmdHidden)
                    .EndSubCommand()
                    // .BeginSubCommand("options")
                    //     .WithDescription("Change how certian elements are displayed")
                    //     .BeginSubCommand("timeformat")
                    //         .WithDescription("Change clock elements to 12-hour or 24-hour time")
                    //         .WithArgs(capi.ChatCommands.Parsers.WordRange("12hr/24hr", StatusHudSystem.timeFormatWords))
                    //         .HandleWith(CmdTimeFormat)
                    //         .EndSubCommand()
                    //     .BeginSubCommand("tempscale")
                    //         .WithDescription("Change temperature scale to °C, °F, or °K")
                    //         .WithArgs(capi.ChatCommands.Parsers.WordRange("C/F/K", tempScaleWords))
                    //         .HandleWith(CmdTempScale)
                    //         .EndSubCommand()
                    // .EndSubCommand()
                    .BeginSubCommand("help")
                        .WithDescription("Show status HUD command help")
                        .HandleWith(CmdHelp)
                    .EndSubCommand();
#if DEBUG
            capi.ChatCommands.GetOrCreate("shud").BeginSubCommand("reload").HandleWith(CmdReload);
#endif
            slowListenerId = this.capi.Event.RegisterGameTickListener(SlowTick, slowListenInterval);
            fastListenerId = this.capi.Event.RegisterGameTickListener(FastTick, fastListenInterval);

            if (Config.version <= 0)
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
#if DEBUG
            this.capi.Logger.Debug(PrintModName("Debug logging Enabled"));
#endif
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

        public string UUID { get { return uuid; } }

        protected TextCommandResult CmdDefault(TextCommandCallingArgs args)
        {
            InstallDefault();

            SaveConfig();
            return TextCommandResult.Success(PrintModName("Default layout set."));
        }

        protected TextCommandResult CmdSet(TextCommandCallingArgs args)
        {
            int slot = (int)args[0];
            string element = (string)args[1];

            if (slot == 0)
            {
                return TextCommandResult.Error(PrintModName("Error: # must be positive or negative."));
            }

            Set(element);
            elements[slot].Ping();

            SaveConfig();
            return TextCommandResult.Success(PrintModName("Element #" + slot + " set to: " + element));
        }

        protected TextCommandResult CmdUnset(TextCommandCallingArgs args)
        {
            string name = (string)args[0];

            if (!Unset(name))
            {
                return TextCommandResult.Error(PrintModName("Error: # must be positive or negative."));
            }

            SaveConfig();
            return TextCommandResult.Success(PrintModName("Element #" + name + " unset."));
        }

        protected TextCommandResult CmdClear(TextCommandCallingArgs args)
        {
            Clear();

            SaveConfig();
            return TextCommandResult.Success(PrintModName("All elements unset."));
        }

        protected TextCommandResult CmdPos(TextCommandCallingArgs args)
        {
            string name = (string)args[0];
            string halignWord = (string)args[1];
            int x = (int)args[2];
            string valignWord = (string)args[3];
            int y = (int)args[4];

            foreach (var element in elements)
            {
                if (element.ElementName == name)
                {
                    int halign = HalignFromWord(halignWord);
                    int valign = ValignFromWord(valignWord);

                    Pos(element, halign, x, valign, y);
                    element.Ping();
                    SaveConfig();
                    return TextCommandResult.Success(PrintModName("#" + name + " position set."));
                }
            }

            return TextCommandResult.Error(PrintModName("Error: No element at #" + name + "."));
        }

        protected TextCommandResult CmdRepos(TextCommandCallingArgs args)
        {
            string name = (string)args[0];

            foreach (var element in elements)
            {
                if (element.ElementName == name)
                {
                    element.Repos();
                    element.Ping();

                    SaveConfig();
                    return TextCommandResult.Success(PrintModName("#" + name + " position reset."));
                }
            }

            return TextCommandResult.Error(PrintModName("Error: No element at #" + name + "."));
        }

        protected TextCommandResult CmdList(TextCommandCallingArgs args)
        {
            StringBuilder sb = new();
            sb.Append("Current elements:\n");

            foreach (var element in elements)
            {
                sb.Append('[');
                sb.Append(element.ElementName);
                sb.Append("] ");
                sb.Append('\n');
            }
            return TextCommandResult.Success(PrintModName(sb.ToString()));
        }

        protected static TextCommandResult CmdInfo(TextCommandCallingArgs args)
        {
            string element = (string)args[0];
            string message = null;



            foreach (Type type in elementTypes)
            {
                if (type.GetField("name").GetValue(null).ToString() == element)
                {
                    message = type.GetField("desc").GetValue(null).ToString();
                    break;
                }
            }

            if (message == null)
            {
                message = "Invalid element. Try: " + elementList;
            }



            return TextCommandResult.Success(PrintModName(message));
        }

        protected TextCommandResult CmdHidden(TextCommandCallingArgs args)
        {
            string vis = (string)args[0];
            string message = null;

            switch (vis)
            {
                case visWordShow:
                    {
                        message = "Showing hidden elements.";
                        Config.showHidden = true;
                        break;
                    }
                case visWordHide:
                    {
                        message = "Hiding hidden elements.";
                        Config.showHidden = false;
                        break;
                    }
            }

            SaveConfig();
            return TextCommandResult.Success(PrintModName(message));
        }

        // protected TextCommandResult CmdTimeFormat(TextCommandCallingArgs args)
        // {
        //     string timeFormat = (string)args[0];

        //     Config.options.timeFormat = timeFormat;

        //     string message = "Time format now set to " + timeFormat + " time";

        //     return TextCommandResult.Success(PrintModName(message));
        // }

        // protected TextCommandResult CmdTempScale(TextCommandCallingArgs args)
        // {
        //     string tempScale = (string)args[0];

        //     Config.options.temperatureScale = tempScale[0];

        //     string message = "Temperature scale now set to °" + tempScale;

        //     SaveConfig();
        //     return TextCommandResult.Success(PrintModName(message));
        // }

        protected static TextCommandResult CmdHelp(TextCommandCallingArgs args)
        {
            string message = "[Status HUD] Instructions:\n"
                    + "To use the default layout, use:\t.shud default\n"
                    + "To set an element, use:\t.shud set [#] [element]\n"
                    + "To unset an element, use:\t.shud unset [#]\n"
                    + "To unset all elements, use:\t.shud clear\n"
                    + "To change an element's position, use:\t.shud pos [#] [left, center, right] x [top, middle, bottom] y\n"
                    + "To reset an element's position, use:\t.shud repos [#]\n"
                    + "To list current elements, use:\t.shud list\n"
                    + "To view an element's description, use:\t.shud info [element]\n"
                    + "To show or hide hidden elements, use:\t.shud hidden [show, hide]\n"
                    + "To configure element's options, use:\t.shud options [option] [value]"
                    + "\n"
                    + "[#] is a non-zero number between 0 and " + slotMax + ".\n"
                    + "[element] is one of the following:\t" + elementList;

            return TextCommandResult.Success(message);
        }

#if DEBUG
        protected TextCommandResult CmdReload(TextCommandCallingArgs args)
        {
            string message = "Elements Reloaded";

            Reload();

            return TextCommandResult.Success(PrintModName(message));
        }
#endif
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

            Pos(Set(StatusHudStabilityElement.name), StatusHudPos.halignCenter, sideX + (int)(offset * 9f), StatusHudPos.valignBottom, bottomY);

            Pos(Set(StatusHudArmourElement.name), StatusHudPos.halignCenter, sideX + (int)(offset * 10f), StatusHudPos.valignBottom, bottomY);

            Pos(Set(StatusHudRoomElement.name), StatusHudPos.halignCenter, -1 * (sideX + (int)(offset * 9f)), StatusHudPos.valignBottom, bottomY);

            Pos(Set(StatusHudSleepElement.name), StatusHudPos.halignRight, sideMinimapX + offset, StatusHudPos.valignTop, topY);

            Pos(Set(StatusHudWetElement.name), StatusHudPos.halignRight, sideMinimapX, StatusHudPos.valignTop, topY);

            Pos(Set(StatusHudTimeLocalElement.name), StatusHudPos.halignRight, sideX, StatusHudPos.valignBottom, bottomY);
        }

        public void SaveConfig()
        {
            configManager.Save();
        }

        protected static string[] InitElementNames()
        {
            List<string> names = new();

            foreach (var type in elementTypes)
            {
                names.Add((string)type.GetField("name").GetValue(null));
            }

            return names.OrderBy(name => name).ToArray();
        }

        protected static string InitElementList()
        {
            StringBuilder sb = new();
            sb.Append('[');
            sb.Append(StatusHudSystem.elementNames[0]);
            for (int i = 1; i < StatusHudSystem.elementTypes.Length; i++)
            {
                sb.Append(", ");
                sb.Append(StatusHudSystem.elementNames[i]);
            }
            sb.Append(']');
            return sb.ToString();
        }

        protected static int HalignFromWord(string word)
        {
            return word switch
            {
                halignWordLeft => StatusHudPos.halignLeft,
                halignWordCenter => StatusHudPos.halignCenter,
                halignWordRight => StatusHudPos.halignRight,
                _ => 0,
            };
        }

        protected static int ValignFromWord(string word)
        {
            return word switch
            {
                valignWordTop => StatusHudPos.valignTop,
                valignWordMiddle => StatusHudPos.valignMiddle,
                valignWordBottom => StatusHudPos.valignBottom,
                _ => 0,
            };
        }
    }
}