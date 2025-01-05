using System;
using System.Collections.Generic;
using Vintagestory.API.Util;

namespace StatusHud
{
    public class StatusHudConfig
    {
        public int version = 0;
        public int iconSize = 32;
        public int textSize = 16;
        public bool showHidden = false;
        public IList<StatusHudConfigElement> elements = new List<StatusHudConfigElement>();
    }

    public class StatusHudConfigElement
    {
        public string name;
        public int x;
        public int y;
        public int halign;
        public int valign;
        public string options;

        public StatusHudConfigElement(string name, int x, int y, int halign, int valign, string elementOptions)
        {
            this.name = name;
            this.x = x;
            this.halign = halign;
            this.y = y;
            this.valign = valign;
            this.options = elementOptions;
        }
    }

    public class StatusHudConfigManager
    {
        private const string filename = "statushud.json";
        public const int version = 3;

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
                this.system.capi.StoreModConfig(config, filename);
            }
            else if (config.version == 2)
            {
                ConfigToV3();
            }
        }

        public void Load()
        {
            try
            {
                config = system.capi.LoadModConfig<StatusHudConfig>(filename);
            }
            catch (Exception)
            {
                system.capi.Logger.Debug(StatusHudSystem.PrintModName("Config file does not exist"));
            }
        }

        public void LoadElements(StatusHudSystem system)
        {
            foreach (var configElement in config.elements)
            {
                StatusHudElement element = system.Set(configElement.name);

                if (element != null)
                {
                    element.ConfigOptions(configElement.options);
                    StatusHudSystem.Pos(element, configElement.halign, configElement.x, configElement.valign, configElement.y);
                }
            }
        }

        public void Save()
        {
            config.elements.Clear();

            foreach (var element in system.elements)
            {
                config.elements.Add(new StatusHudConfigElement((string)element.GetType().GetField("name").GetValue(null),
                    element.pos.x,
                    element.pos.y,
                    element.pos.halign,
                    element.pos.valign,
                    element.ElementOption)
                );
            }

            system.capi.StoreModConfig(config, filename);
        }

        private void ConfigToV3()
        {
            foreach (var element in config.elements)
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
        }
    }
}