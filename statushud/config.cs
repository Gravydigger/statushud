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

public class StatusHudConfig
{
    public IList<StatusHudConfigElement> elements = [];
    public float elementScale = 1;
    public bool showHidden;
    public int version;
}

public class StatusHudConfigElement(
    string name,
    int x,
    int y,
    StatusHudPos.HorizAlign horizAlign,
    StatusHudPos.VertAlign vertAlign,
    StatusHudPos.TextAlign textAlign,
    int orientOffset,
    string elementOptions)
{
    public StatusHudPos.HorizAlign horizAlign = horizAlign;
    public string name = name;
    public string options = elementOptions;
    public int orientOffset = orientOffset;
    public StatusHudPos.TextAlign textAlign = textAlign;
    public StatusHudPos.VertAlign vertAlign = vertAlign;
    public int x = x;
    public int y = y;
}

internal class StatusHudConfigManager
{
    private const string filename = "statushud.json";
    private const int configVersion = 4;
    private readonly StatusHudSystem system;

    internal StatusHudConfigManager(StatusHudSystem system)
    {
        this.system = system;

        Load();
    }

    public StatusHudConfig Config { get; private set; }

    internal void Load()
    {
        int modConfigVersion = GetVersion();

        switch (modConfigVersion)
        {
            case <= 0:
                Config = new StatusHudConfig
                {
                    version = configVersion
                };
                system.capi.Logger.Debug(StatusHudSystem.PrintModName($"Generated new config file {filename}"));
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
            case configVersion:
                Config = system.capi.LoadModConfig<StatusHudConfig>(filename);
                break;
            default:
                // Someone is loading a new config version for an old mod version
                system.capi.Logger.Error(
                    StatusHudSystem.PrintModName(
                        $"Expected mod config version is {configVersion}, got {Config.version}. Overwriting config with default config."));
                Config = new StatusHudConfig
                {
                    version = configVersion
                };
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

    internal void LoadElements(StatusHudSystem system)
    {
        foreach (StatusHudConfigElement configElement in Config.elements)
        {
            StatusHudElement element = system.Set(Type.GetType(configElement.name));

            if (element == null) continue;

            element.ConfigOptions(configElement.options);
            StatusHudSystem.SetPos(element, configElement.horizAlign, configElement.x, configElement.vertAlign, configElement.y,
                configElement.textAlign, configElement.orientOffset);
        }
    }

    internal void Save()
    {
        Config.elements.Clear();

        foreach (StatusHudElement element in system.elements)
        {
            Config.elements.Add(new StatusHudConfigElement(
                element.GetType().ToString(),
                element.pos.x,
                element.pos.y,
                element.pos.horizAlign,
                element.pos.vertAlign,
                element.pos.textAlign,
                element.pos.textAlignOffset,
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
                    element.name = StatusHudDateElement.Name;
                    break;
                case "Time":
                    element.name = StatusHudTimeElement.Name;
                    break;
                case "Weather":
                    element.name = StatusHudWeatherElement.Name;
                    break;
                case "Wind":
                    element.name = StatusHudWindElement.Name;
                    break;
                case "Stability":
                    element.name = StatusHudStabilityElement.Name;
                    break;
                case "Armour":
                    element.name = StatusHudArmourElement.Name;
                    break;
                case "Room":
                    element.name = StatusHudRoomElement.Name;
                    break;
                case "Sleep":
                    element.name = StatusHudSleepElement.Name;
                    break;
                case "Wetness":
                    element.name = StatusHudWetElement.Name;
                    break;
                case "Time (Local)":
                    element.name = StatusHudTimeLocalElement.Name;
                    break;
                case "Body heat":
                    element.name = StatusHudBodyHeatElement.Name;
                    break;
                case "Durability":
                    element.name = StatusHudDurabilityElement.Name;
                    break;
                case "Latitude":
                    element.name = StatusHudLatitudeElement.Name;
                    break;
                case "Light":
                    element.name = StatusHudLightElement.Name;
                    break;
                case "Ping":
                    element.name = StatusHudPingElement.Name;
                    break;
                case "Players":
                    element.name = StatusHudPlayersElement.Name;
                    break;
                case "Speed":
                    element.name = StatusHudSpeedElement.Name;
                    break;
                case "Temporal Storm":
                    element.name = StatusHudTempstormElement.Name;
                    break;
                case "Rift Activity":
                    element.name = StatusHudRiftActivityElement.Name;
                    element.options = element.options == "True" ? "true" : "false";
                    break;
                case "Altitude":
                    element.name = StatusHudAltitudeElement.Name;
                    break;
                case "Compass":
                    element.name = StatusHudCompassElement.Name;
                    element.options = element.options == "Relative" ? "relative" : "absolute";
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
            elementScale = (float)Math.Clamp(Math.Round((double)(configV3.iconSize / StatusHudSystem.IconSize)), 0.5f, 2f),
            showHidden = configV3.showHidden
        };

        foreach (StatusHudConfigElementV3 oldElement in configV3.elements)
        {
            StatusHudConfigElement newElement = new(
                StatusHudSystem.ElementTypes.TryGetValue(oldElement.name, out Type value) ? value.ToString() : "invalid",
                oldElement.x,
                oldElement.y,
                (StatusHudPos.HorizAlign)oldElement.halign,
                (StatusHudPos.VertAlign)oldElement.valign,
                StatusHudPos.TextAlign.Up,
                0,
                oldElement.options
            );

            newConfig.elements.Add(newElement);
        }

        return newConfig;
    }
}