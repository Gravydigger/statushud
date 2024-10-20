using ConfigLib;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace StatusHud
{
    public class StatusHudSystem : ModSystem
    {
        public const int slotMin = -16;
        public const int slotMax = 16;

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

        protected static readonly Type[] elementTypes = {
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
        protected static readonly string[] elementNames = InitElementNames();
        protected static readonly string elementList = InitElementList();

        protected StatusHudConfigManager config;

        protected IDictionary<int, StatusHudElement> elements;
        protected IList<StatusHudElement> slowElements;
        protected IList<StatusHudElement> fastElements;
        public StatusHudTextures textures;

        protected StatusHudGui gui;
        private string uuid = null;
        public bool showHidden
        {
            get
            {
                return config.Get().showHidden;
            }
        }

        protected StatusHudElement instantiate(int slot, string name)
        {
            StatusHudConfig config = this.config.Get();
            StatusHudTextConfig textConfig = config.text;

            switch (name)
            {
                case StatusHudAltitudeElement.name:
                    return new StatusHudAltitudeElement(this, slot, textConfig);
                case StatusHudArmourElement.name:
                    return new StatusHudArmourElement(this, slot, textConfig);
                case StatusHudBodyheatElement.name:
                    return new StatusHudBodyheatElement(this, slot, config);
                case StatusHudCompassElement.name:
                    return new StatusHudCompassElement(this, slot, textConfig, config.compassAbsolute);
                case StatusHudDateElement.name:
                    return new StatusHudDateElement(this, slot, textConfig, config.months);
                case StatusHudDurabilityElement.name:
                    return new StatusHudDurabilityElement(this, slot, textConfig);
                case StatusHudLatitudeElement.name:
                    return new StatusHudLatitudeElement(this, slot, textConfig);
                case StatusHudLightElement.name:
                    return new StatusHudLightElement(this, slot, textConfig);
                case StatusHudPingElement.name:
                    return new StatusHudPingElement(this, slot, textConfig);
                case StatusHudPlayersElement.name:
                    return new StatusHudPlayersElement(this, slot, textConfig);
                case StatusHudRiftActivityElement.name:
                    return new StatusHudRiftActivityElement(this, slot, textConfig);
                case StatusHudRoomElement.name:
                    return new StatusHudRoomElement(this, slot);
                case StatusHudSleepElement.name:
                    return new StatusHudSleepElement(this, slot, textConfig);
                case StatusHudSpeedElement.name:
                    return new StatusHudSpeedElement(this, slot, textConfig);
                case StatusHudStabilityElement.name:
                    return new StatusHudStabilityElement(this, slot, textConfig);
                case StatusHudTempstormElement.name:
                    return new StatusHudTempstormElement(this, slot, textConfig);
                case StatusHudTimeElement.name:
                    return new StatusHudTimeElement(this, slot, config);
                case StatusHudTimeLocalElement.name:
                    return new StatusHudTimeLocalElement(this, slot, config);
                case StatusHudWeatherElement.name:
                    return new StatusHudWeatherElement(this, slot, config);
                case StatusHudWetElement.name:
                    return new StatusHudWetElement(this, slot, textConfig);
                case StatusHudWindElement.name:
                    return new StatusHudWindElement(this, slot, textConfig);
                default:
                    return null;
            }
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

            config = new StatusHudConfigManager(this.capi);

            elements = new Dictionary<int, StatusHudElement>();
            slowElements = new List<StatusHudElement>();
            fastElements = new List<StatusHudElement>();
            textures = new StatusHudTextures(this.capi, config.Get().iconSize);

            gui = new StatusHudGui(this, config.Get(), elements, elementNames);

            config.LoadElements(this);

            capi.ChatCommands.Create("shud")
                .WithDescription("Configure Status HUD")
                    .BeginSubCommand("default")
                        .WithDescription("Reset all elements to a default layout")
                        .HandleWith(CmdDefault)
                    .EndSubCommand()
                    .BeginSubCommand("set")
                        .WithDescription("Set status HUD element")
                        .WithArgs(capi.ChatCommands.Parsers.IntRange("slot", StatusHudSystem.slotMin, StatusHudSystem.slotMax),
                                capi.ChatCommands.Parsers.WordRange("element", StatusHudSystem.elementNames))
                        .HandleWith(CmdSet)
                    .EndSubCommand()
                    .BeginSubCommand("unset")
                        .WithDescription("Unset status HUD element")
                        .WithArgs(capi.ChatCommands.Parsers.IntRange("slot", StatusHudSystem.slotMin, StatusHudSystem.slotMax))
                        .HandleWith(CmdUnset)
                    .EndSubCommand()
                    .BeginSubCommand("clear")
                        .WithDescription("Unset all status HUD elements")
                        .HandleWith(CmdClear)
                    .EndSubCommand()
                    .BeginSubCommand("pos")
                        .WithDescription("Set status HUD element's position")
                        .WithArgs(capi.ChatCommands.Parsers.IntRange("slot", StatusHudSystem.slotMin, StatusHudSystem.slotMax),
                                capi.ChatCommands.Parsers.WordRange("halign", StatusHudSystem.halignWords),
                                capi.ChatCommands.Parsers.Int("x"),
                                capi.ChatCommands.Parsers.WordRange("valign", StatusHudSystem.valignWords),
                                capi.ChatCommands.Parsers.Int("y"))
                        .HandleWith(CmdPos)
                    .EndSubCommand()
                    .BeginSubCommand("repos")
                        .WithDescription("Reset status HUD element's position")
                        .WithArgs(capi.ChatCommands.Parsers.IntRange("slot", StatusHudSystem.slotMin, StatusHudSystem.slotMax))
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
                    .BeginSubCommand("options")
                        .WithDescription("Change how certian elements are displayed")
                        .BeginSubCommand("timeformat")
                            .WithDescription("Change clock elements to 12-hour or 24-hour time")
                            .WithArgs(capi.ChatCommands.Parsers.WordRange("12hr/24hr", StatusHudSystem.timeFormatWords))
                            .HandleWith(CmdTimeFormat)
                            .EndSubCommand()
                        .BeginSubCommand("tempscale")
                            .WithDescription("Change temperature scale to �C, �F, or �K")
                            .WithArgs(capi.ChatCommands.Parsers.WordRange("C/F/K", tempScaleWords))
                            .HandleWith(CmdTempScale)
                            .EndSubCommand()
                    .EndSubCommand()
                    .BeginSubCommand("help")
                        .WithDescription("Show status HUD command help")
                        .HandleWith(CmdHelp)
                    .EndSubCommand();
#if DEBUG
            capi.ChatCommands.GetOrCreate("shud").BeginSubCommand("reload").HandleWith(CmdReload);
#endif
            slowListenerId = this.capi.Event.RegisterGameTickListener(SlowTick, slowListenInterval);
            fastListenerId = this.capi.Event.RegisterGameTickListener(FastTick, fastListenInterval);

            if (!config.Get().installed)
            {
                if (config.Get().elements.Count == 0)
                {
                    // Install default layout.
                    InstallDefault();
                }

                config.Get().installed = true;
                SaveConfig();
            }

            capi.Event.PlayerJoin += SetUUID;
            capi.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig(domain, gui.DrawConfigLibSettings);
#if DEBUG
            this.capi.Logger.Debug(Print("Debug logging Enabled"));
#endif
        }

        public override void Dispose()
        {
            base.Dispose();

            capi.Event.UnregisterGameTickListener(slowListenerId);
            capi.Event.UnregisterGameTickListener(fastListenerId);
            foreach (KeyValuePair<int, StatusHudElement> element in elements)
            {
                element.Value.Dispose();
            }

            textures.Dispose();
            capi.Event.PlayerJoin -= SetUUID;
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

        public bool Set(int slot, string name)
        {
            StatusHudElement element = instantiate(slot, name);

            if (element == null)
            {
                // Invalid element.
                return false;
            }

            StatusHudPos pos = null;

            // Remove any existing element in that slot.
            if (elements.ContainsKey(slot))
            {
                pos = elements[slot].pos;
                elements[slot].Dispose();
                elements.Remove(slot);
            }

            // Remove any other element of the same type.
            int key = 0;
            foreach (KeyValuePair<int, StatusHudElement> kvp in elements)
            {
                if (kvp.Value.GetType() == element.GetType())
                {
                    key = kvp.Key;
                }
            }

            if (key != 0)
            {
                elements[key].Dispose();
                elements.Remove(key);
            }

            elements.Add(slot, element);
            if (pos != null)
            {
                // Retain previous position.
                elements[slot].Pos(pos.halign, pos.x, pos.valign, pos.y);
            }
            else
            {
                elements[slot].Repos();
            }

            if (element.fast)
            {
                fastElements.Add(element);
            }
            else
            {
                slowElements.Add(element);
            }
            return true;
        }

        public bool Unset(int slot)
        {
            if (elements.ContainsKey(slot))
            {
                StatusHudElement element = elements[slot];

                if (element.fast)
                {
                    fastElements.Remove(element);
                }
                else
                {
                    slowElements.Remove(element);
                }

                elements[slot].Dispose();
                elements.Remove(slot);
                return true;
            }

            return false;
        }

        // Will reload elements in memory, but not in file.
        public void Reload()
        {
            Clear();
            config.LoadElements(this);
        }

        public void Reload(IClientPlayer byPlayer)
        {
            if (byPlayer != null && byPlayer.PlayerUID == UUID)
            {
                Reload();
            }
        }

        public void Pos(int slot, int halign, int x, int valign, int y)
        {
            elements[slot].Pos(halign, x, valign, y);
        }

        protected void Clear()
        {
            fastElements.Clear();
            slowElements.Clear();

            foreach (KeyValuePair<int, StatusHudElement> kvp in elements)
            {
                elements[kvp.Key].Dispose();
            }
            for (int i = 0; i < elements.Count; i++)
            {
                elements.Clear();
            }
        }

        private void SetUUID(IClientPlayer byPlayer)
        {
            if (uuid == null && byPlayer != null)
            {
                uuid = byPlayer.PlayerUID;
            }
        }

        public string UUID { get { return uuid; } }

        protected TextCommandResult CmdDefault(TextCommandCallingArgs args)
        {
            InstallDefault();

            SaveConfig();
            return TextCommandResult.Success(Print("Default layout set."));
        }

        protected TextCommandResult CmdSet(TextCommandCallingArgs args)
        {
            int slot = (int)args[0];
            string element = (string)args[1];

            if (slot == 0)
            {
                return TextCommandResult.Error(Print("Error: # must be positive or negative."));
            }

            Set(slot, element);
            elements[slot].Ping();

            SaveConfig();
            return TextCommandResult.Success(Print("Element #" + slot + " set to: " + element));
        }

        protected TextCommandResult CmdUnset(TextCommandCallingArgs args)
        {
            int slot = (int)args[0];

            if (!Unset(slot))
            {
                return TextCommandResult.Error(Print("Error: # must be positive or negative."));
            }

            SaveConfig();
            return TextCommandResult.Success(Print("Element #" + slot + " unset."));
        }

        protected TextCommandResult CmdClear(TextCommandCallingArgs args)
        {
            Clear();

            SaveConfig();
            return TextCommandResult.Success(Print("All elements unset."));
        }

        protected TextCommandResult CmdPos(TextCommandCallingArgs args)
        {
            int slot = (int)args[0];
            string halignWord = (string)args[1];
            int x = (int)args[2];
            string valignWord = (string)args[3];
            int y = (int)args[4];

            if (slot == 0)
            {
                return TextCommandResult.Error(Print("Error: # must be positive or negative."));
            }
            if (!elements.ContainsKey(slot))
            {
                return TextCommandResult.Error(Print("Error: No element at #" + slot + "."));
            }

            int halign = StatusHudSystem.HalignFromWord(halignWord);
            int valign = StatusHudSystem.ValignFromWord(valignWord);

            Pos(slot, halign, x, valign, y);
            elements[slot].Ping();

            SaveConfig();
            return TextCommandResult.Success(Print("#" + slot + " position set."));
        }

        protected TextCommandResult CmdRepos(TextCommandCallingArgs args)
        {
            int slot = (int)args[0];

            if (slot == 0)
            {
                return TextCommandResult.Error(Print("Error: # must be positive or negative."));
            }
            if (!elements.ContainsKey(slot))
            {
                return TextCommandResult.Error(Print("Error: No element at #" + slot + "."));
            }

            elements[slot].Repos();
            elements[slot].Ping();

            SaveConfig();
            return TextCommandResult.Success(Print("#" + slot + " position reset."));
        }

        protected TextCommandResult CmdList(TextCommandCallingArgs args)
        {
            StringBuilder sb = new();
            sb.Append("Current elements:\n");

            foreach (KeyValuePair<int, StatusHudElement> kvp in elements)
            {
                sb.Append('[');
                sb.Append(kvp.Key);
                sb.Append("] ");
                sb.Append((string)kvp.Value.GetType().GetField("name").GetValue(null));
                sb.Append('\n');
            }
            return TextCommandResult.Success(Print(sb.ToString()));
        }

        protected static TextCommandResult CmdInfo(TextCommandCallingArgs args)
        {
            string element = (string)args[0];
            string message = null;

            foreach (Type type in StatusHudSystem.elementTypes)
            {
                if (type.GetField("name").GetValue(null).ToString() == element)
                {
                    message = type.GetField("desc").GetValue(null).ToString();
                    break;
                }
            }

            if (message == null)
            {
                message = "Invalid element. Try: " + StatusHudSystem.elementList;
            }

            return TextCommandResult.Success(Print(message));
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
                        config.Get().showHidden = true;
                        break;
                    }
                case visWordHide:
                    {
                        message = "Hiding hidden elements.";
                        config.Get().showHidden = false;
                        break;
                    }
            }

            SaveConfig();
            return TextCommandResult.Success(Print(message));
        }

        protected TextCommandResult CmdTimeFormat(TextCommandCallingArgs args)
        {
            string timeFormat = (string)args[0];

            config.Get().options.timeFormat = timeFormat;

            string message = "Time format now set to " + timeFormat + " time";

            return TextCommandResult.Success(Print(message));
        }

        protected TextCommandResult CmdTempScale(TextCommandCallingArgs args)
        {
            string tempScale = (string)args[0];

            config.Get().options.temperatureScale = tempScale[0];

            string message = "Temperature scale now set to °" + tempScale;

            SaveConfig();
            return TextCommandResult.Success(Print(message));
        }

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
                    + "[#] is a non-zero number between " + StatusHudSystem.slotMin + " and " + StatusHudSystem.slotMax + ".\n"
                    + "[element] is one of the following:\t" + StatusHudSystem.elementList;

            return TextCommandResult.Success(message);
        }

#if DEBUG
        protected TextCommandResult CmdReload(TextCommandCallingArgs args)
        {
            string message = "Elements Reloaded";

            Reload();

            return TextCommandResult.Success(Print(message));
        }
#endif
        public void InstallDefault()
        {
            Clear();

            int size = config.Get().iconSize;
            int sideX = (int)Math.Round(size * 0.75f);
            int sideMinimapX = sideX + 256;
            int topY = size;
            int bottomY = (int)Math.Round(size * 0.375f);
            int offset = (int)Math.Round(size * 1.5f);

            Set(-6, StatusHudDateElement.name);
            Pos(-6, StatusHudPos.halignLeft, sideX, StatusHudPos.valignBottom, bottomY);

            Set(-5, StatusHudTimeElement.name);
            Pos(-5, StatusHudPos.halignLeft, sideX + (int)(offset * 1.3f), StatusHudPos.valignBottom, bottomY);

            Set(-4, StatusHudWeatherElement.name);
            Pos(-4, StatusHudPos.halignLeft, sideX + (int)(offset * 2.5f), StatusHudPos.valignBottom, bottomY);

            Set(-3, StatusHudWindElement.name);
            Pos(-3, StatusHudPos.halignLeft, sideX + (int)(offset * 3.5f), StatusHudPos.valignBottom, bottomY);

            Set(-2, StatusHudStabilityElement.name);
            Pos(-2, StatusHudPos.halignCenter, sideX + (int)(offset * 9f), StatusHudPos.valignBottom, bottomY);

            Set(-1, StatusHudArmourElement.name);
            Pos(-1, StatusHudPos.halignCenter, sideX + (int)(offset * 10f), StatusHudPos.valignBottom, bottomY);

            Set(1, StatusHudRoomElement.name);
            Pos(1, StatusHudPos.halignCenter, -1* (sideX + (int)(offset * 9f)), StatusHudPos.valignBottom, bottomY);

            Set(2, StatusHudSleepElement.name);
            Pos(2, StatusHudPos.halignRight, sideMinimapX + offset, StatusHudPos.valignTop, topY);

            Set(3, StatusHudWetElement.name);
            Pos(3, StatusHudPos.halignRight, sideMinimapX, StatusHudPos.valignTop, topY);

            Set(4, StatusHudTimeLocalElement.name);
            Pos(4, StatusHudPos.halignRight, sideX, StatusHudPos.valignBottom, bottomY);
        }

        public void SaveConfig()
        {
            config.Save(elements);
            config.Save();
        }

        protected static string Print(string text)
        {
            return "[Status HUD] " + text;
        }

        protected static string[] InitElementNames()
        {
            string[] names = new string[StatusHudSystem.elementTypes.Length];

            for (int i = 0; i < names.Length; i++)
            {
                names[i] = (string)StatusHudSystem.elementTypes[i].GetField("name").GetValue(null);
            }

            return names;
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
            switch (word)
            {
                case halignWordLeft:
                    return StatusHudPos.halignLeft;
                case halignWordCenter:
                    return StatusHudPos.halignCenter;
                case halignWordRight:
                    return StatusHudPos.halignRight;
            }
            return 0;
        }

        protected static int ValignFromWord(string word)
        {
            switch (word)
            {
                case valignWordTop:
                    return StatusHudPos.valignTop;
                case valignWordMiddle:
                    return StatusHudPos.valignMiddle;
                case valignWordBottom:
                    return StatusHudPos.valignBottom;
            }
            return 0;
        }
    }
}