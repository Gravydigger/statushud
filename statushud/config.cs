using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Vintagestory.API.Config;

namespace StatusHud;

// For config versions 3 or lower
public class StatusHudConfigV3
{
    // ReSharper disable once CollectionNeverUpdated.Global
    public IList<StatusHudConfigElementV3> elements = [];
    public int iconSize = 32;
    public bool showHidden = false;
    public int textSize = 16;
    public int version;
}

// For config versions 3 or lower
public class StatusHudConfigElementV3(string name, int x, int y, int halign, int valign, string elementOptions)
{
    public int halign = halign;
    public string name = name;
    public string options = elementOptions;
    public int valign = valign;
    public int x = x;
    public int y = y;
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
    public IList<StatusHudConfigElement> elements = [];
    public float elementScale = 1;
    public bool showHidden;
    public int version;
}

public class StatusHudConfigElement(string name, int x, int y, int horzAlign, int vertAlign, Orientation orientation, int orientationOffset, string elementOptions)
{
    public int horzAlign = horzAlign;
    public string name = name;
    public int offset = orientationOffset;
    public string options = elementOptions;
    public Orientation orientation = orientation;
    public int vertAlign = vertAlign;
    public int x = x;
    public int y = y;
}

public class StatusHudConfigManager
{
    private const string filename = "statushud.json";
    private const int version = 4;
    private readonly StatusHudSystem system;

    public StatusHudConfigManager(StatusHudSystem system)
    {
        this.system = system;

        Load();

        if (Config == null)
        {
            Config = new StatusHudConfig();
            system.capi.Logger.Debug(StatusHudSystem.PrintModName($"Generated new config file {filename}"));
        }

        // Someone is loading a new config version for an old mod version
        if (Config.version > version)
        {
            system.capi.Logger.Error(StatusHudSystem.PrintModName($"Expected mod config version is {version}, got {Config.version}."
                                                                  + "\nOverwriting config with default config."));
            system.InstallDefault();
        }

        else if (Config.version <= 1 || Config.elements.Count == 0)
        {
            // Install default layout
            Config.version = version;
            system.InstallDefault();
        }

        Config.version = version;
    }

    public StatusHudConfig Config { get; private set; }

    public void Load()
    {
        int modConfigVersion = GetVersion();

        switch (modConfigVersion)
        {
            case <= 0:
                break;
            case <= 3:
            {
                // Convert the old config type to the new one
                StatusHudConfigV3 v3Config = system.capi.LoadModConfig<StatusHudConfigV3>(filename);
                if (v3Config.version == 2)
                {
                    v3Config = ConfigToV3(v3Config);
                }
                if (v3Config.version == 3)
                {
                    Config = ConfigToV4(v3Config);
                }
                break;
            }
            default:
                Config = system.capi.LoadModConfig<StatusHudConfig>(filename);
                break;
        }
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
        foreach (StatusHudConfigElement configElement in Config.elements)
        {
            StatusHudElement element = system.Set(Type.GetType(configElement.name));

            if (element == null) continue;

            element.ConfigOptions(configElement.options);
            StatusHudSystem.SetPos(element, (StatusHudPos.HorizAlign)configElement.horzAlign, configElement.x, (StatusHudPos.VertAlign)configElement.vertAlign, configElement.y);
        }
    }

    public void Save()
    {
        Config.elements.Clear();

        foreach (StatusHudElement element in system.elements)
        {
            Config.elements.Add(new StatusHudConfigElement(
                element.GetType().ToString(),
                element.pos.x,
                element.pos.y,
                (int)element.pos.horizAlign,
                (int)element.pos.vertAlign,
                Orientation.Up, // Placeholder
                0, // Placeholder
                element.ElementOption)
            );
        }

        system.capi.StoreModConfig(Config, filename);
    }

    private static StatusHudConfigV3 ConfigToV3(StatusHudConfigV3 configV3)
    {
        configV3.version = 3;

        foreach (StatusHudConfigElementV3 element in configV3.elements)
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
                    element.name = StatusHudBodyHeatElement.name;
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
                    element.options = element.options == "True" ? StatusHudRiftActivityElement.RiftChangeOptions[0] : StatusHudRiftActivityElement.RiftChangeOptions[1];
                    break;
                case "Altitude":
                    element.name = StatusHudAltitudeElement.name;
                    break;
                case "Compass":
                    element.name = StatusHudCompassElement.name;
                    element.options = element.options == "Relative" ? StatusHudCompassElement.CompassBearingOptions[0] : StatusHudCompassElement.CompassBearingOptions[1];
                    break;
            }
        }

        return configV3;
    }
    private static StatusHudConfig ConfigToV4(StatusHudConfigV3 configV3)
    {
        StatusHudConfig newConfig = new()
        {
            version = 4,
            elementScale = (float)Math.Clamp(Math.Round((double)(configV3.iconSize / StatusHudSystem.iconSize)), 0.5f, 2f),
            showHidden = configV3.showHidden
        };

        foreach (StatusHudConfigElementV3 oldElement in configV3.elements)
        {
            StatusHudConfigElement newElement = new(
                StatusHudSystem.ElementTypes.TryGetValue(oldElement.name, out Type value) ? value.ToString() : "invalid",
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