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

        public const string domain = "statushud";
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
        protected static readonly string[] elementNames = initElementNames();
        protected static readonly string elementList = initElementList();

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
                return this.config.Get().showHidden;
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

            this.config = new StatusHudConfigManager(this.capi);

            this.elements = new Dictionary<int, StatusHudElement>();
            this.slowElements = new List<StatusHudElement>();
            this.fastElements = new List<StatusHudElement>();
            this.textures = new StatusHudTextures(this.capi, this.config.Get().iconSize);
            this.gui = new StatusHudGui(this);

            this.config.LoadElements(this);

            capi.ChatCommands.Create("shud")
                .WithDescription("Configure Status HUD")
                    .BeginSubCommand("default")
                        .WithDescription("Reset all elements to a default layout")
                        .HandleWith(this.cmdDefault)
                    .EndSubCommand()
                    .BeginSubCommand("set")
                        .WithDescription("Set status HUD element")
                        .WithArgs(capi.ChatCommands.Parsers.IntRange("slot", StatusHudSystem.slotMin, StatusHudSystem.slotMax),
                                capi.ChatCommands.Parsers.WordRange("element", StatusHudSystem.elementNames))
                        .HandleWith(this.cmdSet)
                    .EndSubCommand()
                    .BeginSubCommand("unset")
                        .WithDescription("Unset status HUD element")
                        .WithArgs(capi.ChatCommands.Parsers.IntRange("slot", StatusHudSystem.slotMin, StatusHudSystem.slotMax))
                        .HandleWith(this.cmdUnset)
                    .EndSubCommand()
                    .BeginSubCommand("clear")
                        .WithDescription("Unset all status HUD elements")
                        .HandleWith(this.cmdClear)
                    .EndSubCommand()
                    .BeginSubCommand("pos")
                        .WithDescription("Set status HUD element's position")
                        .WithArgs(capi.ChatCommands.Parsers.IntRange("slot", StatusHudSystem.slotMin, StatusHudSystem.slotMax),
                                capi.ChatCommands.Parsers.WordRange("halign", StatusHudSystem.halignWords),
                                capi.ChatCommands.Parsers.Int("x"),
                                capi.ChatCommands.Parsers.WordRange("valign", StatusHudSystem.valignWords),
                                capi.ChatCommands.Parsers.Int("y"))
                        .HandleWith(this.cmdPos)
                    .EndSubCommand()
                    .BeginSubCommand("repos")
                        .WithDescription("Reset status HUD element's position")
                        .WithArgs(capi.ChatCommands.Parsers.IntRange("slot", StatusHudSystem.slotMin, StatusHudSystem.slotMax))
                        .HandleWith(this.cmdRepos)
                    .EndSubCommand()
                    .BeginSubCommand("list")
                        .WithDescription("List current status HUD elements")
                        .HandleWith(this.cmdList)
                    .EndSubCommand()
                    .BeginSubCommand("info")
                        .WithDescription("Show status HUD element info")
                        .WithArgs(capi.ChatCommands.Parsers.WordRange("element", StatusHudSystem.elementNames))
                        .HandleWith(this.cmdInfo)
                    .EndSubCommand()
                    .BeginSubCommand("hidden")
                        .WithDescription("Show or hide hidden elements")
                        .WithArgs(capi.ChatCommands.Parsers.WordRange("show/hide", StatusHudSystem.visWords))
                        .HandleWith(this.cmdHidden)
                    .EndSubCommand()
                    .BeginSubCommand("options")
                        .WithDescription("Change how certian elements are displayed")
                        .BeginSubCommand("timeformat")
                            .WithDescription("Change clock elements to 12-hour or 24-hour time")
                            .WithArgs(capi.ChatCommands.Parsers.WordRange("12hr/24hr", StatusHudSystem.timeFormatWords))
                            .HandleWith(this.cmdTimeFormat)
                            .EndSubCommand()
                        .BeginSubCommand("tempscale")
                            .WithDescription("Change temperature scale to °C, °F, or °K")
                            .WithArgs(capi.ChatCommands.Parsers.WordRange("C/F/K", tempScaleWords))
                            .HandleWith(this.cmdTempScale)
                            .EndSubCommand()
                    .EndSubCommand()
                    .BeginSubCommand("help")
                        .WithDescription("Show status HUD command help")
                        .HandleWith(this.cmdHelp)
                    .EndSubCommand();
#if DEBUG
            capi.ChatCommands.GetOrCreate("shud").BeginSubCommand("reload").HandleWith(this.cmdReload);
#endif
            this.slowListenerId = this.capi.Event.RegisterGameTickListener(this.SlowTick, slowListenInterval);
            this.fastListenerId = this.capi.Event.RegisterGameTickListener(this.FastTick, fastListenInterval);

            if (!this.config.Get().installed)
            {
                if (this.config.Get().elements.Count == 0)
                {
                    // Install default layout.
                    this.installDefault();
                }

                this.config.Get().installed = true;
                this.saveConfig();
            }

            capi.Event.PlayerJoin += SetUUID;
            capi.Event.PlayerJoin += Reload;

            capi.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig("statushudcont", gui.DrawConfigLibSettings);
#if DEBUG
            this.capi.Logger.Debug(print("Debug logging Enabled"));
#endif
        }


        public override void Dispose()
        {
            base.Dispose();

            this.capi.Event.UnregisterGameTickListener(this.slowListenerId);
            this.capi.Event.UnregisterGameTickListener(this.fastListenerId);
            foreach (KeyValuePair<int, StatusHudElement> element in this.elements)
            {
                element.Value.Dispose();
            }

            this.textures.Dispose();
            capi.Event.PlayerJoin -= SetUUID;
            capi.Event.PlayerJoin -= Reload;
        }

        public void SlowTick(float dt)
        {
            foreach (StatusHudElement element in this.slowElements)
            {
                element.Tick();
            }
        }

        public void FastTick(float dt)
        {
            foreach (StatusHudElement element in this.fastElements)
            {
                element.Tick();
            }
        }

        public bool Set(int slot, string name)
        {
            StatusHudElement element = this.instantiate(slot, name);

            if (element == null)
            {
                // Invalid element.
                return false;
            }

            StatusHudPos pos = null;

            // Remove any existing element in that slot.
            if (this.elements.ContainsKey(slot))
            {
                pos = this.elements[slot].pos;
                this.elements[slot].Dispose();
                this.elements.Remove(slot);
            }

            // Remove any other element of the same type.
            int key = 0;
            foreach (KeyValuePair<int, StatusHudElement> kvp in this.elements)
            {
                if (kvp.Value.GetType() == element.GetType())
                {
                    key = kvp.Key;
                }
            }

            if (key != 0)
            {
                this.elements[key].Dispose();
                this.elements.Remove(key);
            }

            this.elements.Add(slot, element);
            if (pos != null)
            {
                // Retain previous position.
                this.elements[slot].Pos(pos.halign, pos.x, pos.valign, pos.y);
            }
            else
            {
                this.elements[slot].Repos();
            }

            if (element.fast)
            {
                this.fastElements.Add(element);
            }
            else
            {
                this.slowElements.Add(element);
            }
            return true;
        }

        public bool Unset(int slot)
        {
            if (this.elements.ContainsKey(slot))
            {
                StatusHudElement element = this.elements[slot];

                if (element.fast)
                {
                    this.fastElements.Remove(element);
                }
                else
                {
                    this.slowElements.Remove(element);
                }

                this.elements[slot].Dispose();
                this.elements.Remove(slot);
                return true;
            }

            return false;
        }

        // Will reload elements in memory, but not in file.
        public void Reload()
        {
            //config.Load();
            this.clear();
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
            this.elements[slot].Pos(halign, x, valign, y);
        }

        protected void clear()
        {
            this.fastElements.Clear();
            this.slowElements.Clear();

            foreach (KeyValuePair<int, StatusHudElement> kvp in this.elements)
            {
                this.elements[kvp.Key].Dispose();
            }
            for (int i = 0; i < this.elements.Count; i++)
            {
                this.elements.Clear();
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

        protected TextCommandResult cmdDefault(TextCommandCallingArgs args)
        {
            this.installDefault();

            this.saveConfig();
            return TextCommandResult.Success(print("Default layout set."));
        }

        protected TextCommandResult cmdSet(TextCommandCallingArgs args)
        {
            int slot = (int)args[0];
            string element = (string)args[1];

            if (slot == 0)
            {
                return TextCommandResult.Error(print("Error: # must be positive or negative."));
            }

            this.Set(slot, element);
            this.elements[slot].Ping();

            this.saveConfig();
            return TextCommandResult.Success(print("Element #" + slot + " set to: " + element));
        }

        protected TextCommandResult cmdUnset(TextCommandCallingArgs args)
        {
            int slot = (int)args[0];

            if (!Unset(slot))
            {
                return TextCommandResult.Error(print("Error: # must be positive or negative."));
            }

            this.saveConfig();
            return TextCommandResult.Success(print("Element #" + slot + " unset."));
        }

        protected TextCommandResult cmdClear(TextCommandCallingArgs args)
        {
            this.clear();

            this.saveConfig();
            return TextCommandResult.Success(print("All elements unset."));
        }

        protected TextCommandResult cmdPos(TextCommandCallingArgs args)
        {
            int slot = (int)args[0];
            string halignWord = (string)args[1];
            int x = (int)args[2];
            string valignWord = (string)args[3];
            int y = (int)args[4];

            if (slot == 0)
            {
                return TextCommandResult.Error(print("Error: # must be positive or negative."));
            }
            if (!this.elements.ContainsKey(slot))
            {
                return TextCommandResult.Error(print("Error: No element at #" + slot + "."));
            }

            int halign = StatusHudSystem.halignFromWord(halignWord);
            int valign = StatusHudSystem.valignFromWord(valignWord);

            this.Pos(slot, halign, x, valign, y);
            this.elements[slot].Ping();

            this.saveConfig();
            return TextCommandResult.Success(print("#" + slot + " position set."));
        }

        protected TextCommandResult cmdRepos(TextCommandCallingArgs args)
        {
            int slot = (int)args[0];

            if (slot == 0)
            {
                return TextCommandResult.Error(print("Error: # must be positive or negative."));
            }
            if (!this.elements.ContainsKey(slot))
            {
                return TextCommandResult.Error(print("Error: No element at #" + slot + "."));
            }

            this.elements[slot].Repos();
            this.elements[slot].Ping();

            this.saveConfig();
            return TextCommandResult.Success(print("#" + slot + " position reset."));
        }

        protected TextCommandResult cmdList(TextCommandCallingArgs args)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Current elements:\n");

            foreach (KeyValuePair<int, StatusHudElement> kvp in this.elements)
            {
                sb.Append("[");
                sb.Append(kvp.Key);
                sb.Append("] ");
                sb.Append((string)kvp.Value.GetType().GetField("name").GetValue(null));
                sb.Append("\n");
            }
            return TextCommandResult.Success(print(sb.ToString()));
        }

        protected TextCommandResult cmdInfo(TextCommandCallingArgs args)
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

            return TextCommandResult.Success(print(message));
        }

        protected TextCommandResult cmdHidden(TextCommandCallingArgs args)
        {
            string vis = (string)args[0];
            string message = null;

            switch (vis)
            {
                case visWordShow:
                    {
                        message = "Showing hidden elements.";
                        this.config.Get().showHidden = true;
                        break;
                    }
                case visWordHide:
                    {
                        message = "Hiding hidden elements.";
                        this.config.Get().showHidden = false;
                        break;
                    }
            }

            this.saveConfig();
            return TextCommandResult.Success(print(message));
        }

        protected TextCommandResult cmdTimeFormat(TextCommandCallingArgs args)
        {
            string timeFormat = (string)args[0];

            this.config.Get().options.timeFormat = timeFormat;

            string message = "Time format now set to " + timeFormat + " time";

            return TextCommandResult.Success(print(message));
        }

        protected TextCommandResult cmdTempScale(TextCommandCallingArgs args)
        {
            string tempScale = (string)args[0];

            this.config.Get().options.temperatureScale = tempScale[0];

            string message = "Temperature scale now set to °" + tempScale;


            this.saveConfig();
            return TextCommandResult.Success(print(message));
        }

        protected TextCommandResult cmdHelp(TextCommandCallingArgs args)
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
        protected TextCommandResult cmdReload(TextCommandCallingArgs args)
        {
            string message = "Elements Reloaded";

            Reload();

            return TextCommandResult.Success(print(message));
        }
#endif
        protected void installDefault()
        {
            this.clear();

            int size = this.config.Get().iconSize;
            int sideX = (int)Math.Round(size * 0.75f);
            int sideMinimapX = sideX + 256;
            int topY = size;
            int bottomY = (int)Math.Round(size * 0.375f);
            int offset = (int)Math.Round(size * 1.5f);

            this.Set(-6, StatusHudDateElement.name);
            this.Pos(-6, StatusHudPos.halignLeft, sideX, StatusHudPos.valignBottom, bottomY);

            this.Set(-5, StatusHudTimeElement.name);
            this.Pos(-5, StatusHudPos.halignLeft, sideX + (int)(offset * 1.3f), StatusHudPos.valignBottom, bottomY);

            this.Set(-4, StatusHudWeatherElement.name);
            this.Pos(-4, StatusHudPos.halignLeft, sideX + (int)(offset * 2.5f), StatusHudPos.valignBottom, bottomY);

            this.Set(-3, StatusHudWindElement.name);
            this.Pos(-3, StatusHudPos.halignLeft, sideX + (int)(offset * 3.5f), StatusHudPos.valignBottom, bottomY);

            this.Set(-2, StatusHudStabilityElement.name);

            this.Set(-1, StatusHudArmourElement.name);

            this.Set(1, StatusHudRoomElement.name);

            this.Set(2, StatusHudSleepElement.name);
            this.Pos(2, StatusHudPos.halignRight, sideMinimapX + offset, StatusHudPos.valignTop, topY);

            this.Set(3, StatusHudWetElement.name);
            this.Pos(3, StatusHudPos.halignRight, sideMinimapX, StatusHudPos.valignTop, topY);

            this.Set(4, StatusHudTimeLocalElement.name);
            this.Pos(4, StatusHudPos.halignRight, sideX, StatusHudPos.valignBottom, bottomY);
        }

        protected void saveConfig()
        {
            this.config.Save(this.elements);
            this.config.Save();
        }

        protected static string print(string text)
        {
            return "[Status HUD] " + text;
        }

        protected static string[] initElementNames()
        {
            string[] names = new string[StatusHudSystem.elementTypes.Length];

            for (int i = 0; i < names.Length; i++)
            {
                names[i] = (string)StatusHudSystem.elementTypes[i].GetField("name").GetValue(null);
            }

            return names;
        }

        protected static string initElementList()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(StatusHudSystem.elementNames[0]);
            for (int i = 1; i < StatusHudSystem.elementTypes.Length; i++)
            {
                sb.Append(", ");
                sb.Append(StatusHudSystem.elementNames[i]);
            }
            sb.Append("]");
            return sb.ToString();
        }

        protected static int halignFromWord(string word)
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

        protected static int valignFromWord(string word)
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