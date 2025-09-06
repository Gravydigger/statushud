using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Vintagestory.API.Config;

namespace StatusHud
{
    // For config versions 3 or lower
    public class StatusHudConfigOld
    {
        public int version = 0;
        public int iconSize = 32;
        public int textSize = 16;
        public bool showHidden = false;
        public IList<StatusHudConfigElementOld> elements = [];
    }

    // For config versions 3 or lower
    public class StatusHudConfigElementOld(string name, int x, int y, int halign, int valign, string elementOptions)
    {
        public string name = name;
        public int x = x;
        public int y = y;
        public int halign = halign;
        public int valign = valign;
        public string options = elementOptions;
    }

    public enum Orientation
    {
        Up,
        Left,
        Right,
        Down
    }

    public class StatusHudConfig
    {
        public int version = 0;
        public float elementScale = 1;
        public bool showHidden = false;
        public IList<StatusHudConfigElement> elements = [];
    }

    public class StatusHudConfigElement(string name, int x, int y, int horzAlign, int vertAlign, Orientation orientation, int orientationOffset, string elementOptions)
    {
        public string name = name;
        public int x = x;
        public int y = y;
        public int horzAlign = horzAlign;
        public int vertAlign = vertAlign;
        public Orientation orientation = orientation;
        public int offset = orientationOffset;
        public string options = elementOptions;
    }

    public class StatusHudConfigManager
    {
        private const string filename = "statushud.json";
        private const int version = 4;

        private StatusHudConfig config;
        private readonly StatusHudSystem system;

        public StatusHudConfig Config => config;

        public StatusHudConfigManager(StatusHudSystem system)
        {
            this.system = system;

            Load();

            if (config == null)
            {
                config = new StatusHudConfig();
                system.capi.Logger.Debug(StatusHudSystem.PrintModName($"Generated new config file {filename}"));
            }

            // Someone is loading a new config version for an old mod version
            if (Config.version > version)
            {
                system.capi.Logger.Error(StatusHudSystem.PrintModName($"Expected mod config version is {version}, got {Config.version}."
                + "\nOverwriting config with default config."));
                system.InstallDefault();
            }

            else if (config.version <= 1 || Config.elements.Count == 0)
            {
                // Install default layout
                Config.version = version;
                system.InstallDefault();
            }

            Config.version = version;
        }

        public void Load()
        {
            int modConfigVersion = GetVersion();

            if (modConfigVersion <= 0) { return; }
            else if (modConfigVersion <= 3)
            {
                // Convert the old config type to the new one
                StatusHudConfigOld oldConfig = system.capi.LoadModConfig<StatusHudConfigOld>(filename);
                if (oldConfig.version == 2)
                {
                    oldConfig = ConfigToV3(oldConfig);
                }
                if (oldConfig.version == 3)
                {
                    config = ConfigToV4(oldConfig);
                }
            }
            else { config = system.capi.LoadModConfig<StatusHudConfig>(filename); }
        }

        private int GetVersion()
        {
            string modConfigPath = Path.Combine(GamePaths.ModConfig, filename);

            if (!File.Exists(modConfigPath))
            {
                system.capi.Logger.Debug(StatusHudSystem.PrintModName("Config file does not exist"));
                return 0;
            }

            try
            {
                return JsonDocument.Parse(File.ReadAllText(modConfigPath)).RootElement.GetProperty("version").GetInt32();
            }
            catch (Exception)
            {
                system.capi.Logger.Error(StatusHudSystem.PrintModName("Config file is malformed"));
                return 0;
            }
        }

        public void LoadElements(StatusHudSystem system)
        {
            foreach (var configElement in config.elements)
            {
                StatusHudElement element = system.Set(Type.GetType(configElement.name));

                if (element != null)
                {
                    element.ConfigOptions(configElement.options);
                    StatusHudSystem.Pos(element, (StatusHudPos.HorzAlign)configElement.horzAlign, configElement.x, (StatusHudPos.VertAlign)configElement.vertAlign, configElement.y);
                }
            }
        }

        public void Save()
        {
            config.elements.Clear();

            foreach (var element in system.elements)
            {
                config.elements.Add(new StatusHudConfigElement(
                    element.GetType().ToString(),
                    element.pos.x,
                    element.pos.y,
                    (int)element.pos.horzAlign,
                    (int)element.pos.vertAlign,
                    Orientation.Up, // Placeholder
                    0, // Placeholder
                    element.ElementOption)
                );
            }

            system.capi.StoreModConfig(config, filename);
        }

        private static StatusHudConfigOld ConfigToV3(StatusHudConfigOld configOld)
        {
            configOld.version = 3;

            foreach (var element in configOld.elements)
            {
                switch (element.name)
                {
                    case "Date":
                        element.name = StatusHudDateElement.name;
                        break;
                    case "Time":
                        element.name = StatusHudTimeElement.name;
                        break;
                    case "Weather":
                        element.name = StatusHudWeatherElement.name;
                        break;
                    case "Wind":
                        element.name = StatusHudWindElement.name;
                        break;
                    case "Stability":
                        element.name = StatusHudStabilityElement.name;
                        break;
                    case "Armour":
                        element.name = StatusHudArmourElement.name;
                        break;
                    case "Room":
                        element.name = StatusHudRoomElement.name;
                        break;
                    case "Sleep":
                        element.name = StatusHudSleepElement.name;
                        break;
                    case "Wetness":
                        element.name = StatusHudWetElement.name;
                        break;
                    case "Time (Local)":
                        element.name = StatusHudTimeLocalElement.name;
                        break;
                    case "Body heat":
                        element.name = StatusHudBodyheatElement.name;
                        break;
                    case "Durability":
                        element.name = StatusHudDurabilityElement.name;
                        break;
                    case "Latitude":
                        element.name = StatusHudLatitudeElement.name;
                        break;
                    case "Light":
                        element.name = StatusHudLightElement.name;
                        break;
                    case "Ping":
                        element.name = StatusHudPingElement.name;
                        break;
                    case "Players":
                        element.name = StatusHudPlayersElement.name;
                        break;
                    case "Speed":
                        element.name = StatusHudSpeedElement.name;
                        break;
                    case "Temporal Storm":
                        element.name = StatusHudTempstormElement.name;
                        break;
                    case "Rift Activity":
                        element.name = StatusHudRiftActivityElement.name;
                        if (element.options == "True")
                        {
                            element.options = StatusHudRiftActivityElement.riftChangeOptions[0];
                        }
                        else
                        {
                            element.options = StatusHudRiftActivityElement.riftChangeOptions[1];
                        }
                        break;
                    case "Altitude":
                        element.name = StatusHudAltitudeElement.name;
                        break;
                    case "Compass":
                        element.name = StatusHudCompassElement.name;
                        if (element.options == "Relative")
                        {
                            element.options = StatusHudCompassElement.compassBearingOptions[0];
                        }
                        else
                        {
                            element.options = StatusHudCompassElement.compassBearingOptions[1];
                        }
                        break;
                    default:
                        break;
                }
            }

            return configOld;
        }
        private StatusHudConfig ConfigToV4(StatusHudConfigOld configOld)
        {
            StatusHudConfig newConfig = new()
            {
                version = 4,
                elementScale = (float)Math.Clamp(Math.Round((double)(configOld.iconSize / StatusHudSystem.iconSize)), 0.5f, 2f),
                showHidden = configOld.showHidden
            };

            foreach (var oldElement in configOld.elements)
            {
                StatusHudConfigElement newElement = new(
                    StatusHudSystem.elementTypes.TryGetValue(oldElement.name, out Type value) ? value.ToString() : "invalid",
                    oldElement.x,
                    oldElement.y,
                    oldElement.halign,
                    oldElement.valign,
                    Orientation.Up,
                    0,
                    oldElement.options
                );

                newConfig.elements.Add(newElement);
            }

            return newConfig;
        }
    }
}