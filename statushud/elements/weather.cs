using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace StatusHud;

public class StatusHudWeatherElement : StatusHudElement
{
    public const string name = "weather";
    private const string textKey = "shud-weather";
    private const float cfratio = 9f / 5f;
    private const float cfdiff = 32;
    private const float ckdiff = 273.15f;

    public static readonly string[] TempFormatWords = ["C", "F", "K"];
    private readonly StatusHudWeatherRenderer renderer;

    private readonly WeatherSystemBase weatherSystem;
    private string tempScale;
    public int textureId;

    public StatusHudWeatherElement(StatusHudSystem system) : base(system)
    {
        weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

        renderer = new StatusHudWeatherRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        tempScale = "C";
        textureId = this.system.textures.texturesDict["empty"].TextureId;

        // Config error checking
        if (!TempFormatWords.Any(str => str.Contains(tempScale)))
        {
            system.capi.Logger.Warning("[{0}] {1} is not a valid value for temperatureFormat. Defaulting to C", textKey, tempScale);
        }
    }

    public override string ElementOption => tempScale;
    public override string ElementName => name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public virtual string GetTextKey()
    {
        return textKey;
    }

    public override void ConfigOptions(string value)
    {
        foreach (string words in TempFormatWords)
        {
            if (words == value)
            {
                tempScale = value;
            }
        }
    }

    public override void Tick()
    {
        ClimateCondition cc = system.capi.World.BlockAccessor.GetClimateAt(system.capi.World.Player.Entity.Pos.AsBlockPos);

        string temperature = tempScale switch
        {
            "F" => (int)Math.Round(cc.Temperature * cfratio + cfdiff, 0) + "°F",
            "K" => (int)Math.Round(cc.Temperature + ckdiff, 0) + "°K",
            _ => (int)Math.Round(cc.Temperature, 0) + "°C"
        };

        renderer.SetText(temperature);
        UpdateTexture(cc);
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }

    private void UpdateTexture(ClimateCondition cc)
    {
        if (cc.Rainfall > 0)
        {
            // Show precipitation.
            switch (weatherSystem.WeatherDataSlowAccess.GetPrecType(system.capi.World.Player.Entity.Pos.XYZ))
            {
                case EnumPrecipitationType.Rain:
                {
                    textureId = cc.Rainfall >= 0.5
                        ? system.textures.texturesDict["weather_rain_heavy"].TextureId
                        : system.textures.texturesDict["weather_rain_light"].TextureId;
                    break;
                }
                case EnumPrecipitationType.Snow:
                {
                    textureId = cc.Rainfall >= 0.5
                        ? system.textures.texturesDict["weather_snow_heavy"].TextureId
                        : system.textures.texturesDict["weather_snow_light"].TextureId;
                    break;
                }
                case EnumPrecipitationType.Hail:
                {
                    textureId = system.textures.texturesDict["weather_hail"].TextureId;
                    break;
                }
                case EnumPrecipitationType.Auto:
                {
                    if (cc.Temperature < weatherSystem.WeatherDataSlowAccess.BlendedWeatherData.snowThresholdTemp)
                    {
                        textureId = cc.Rainfall >= 0.5
                            ? system.textures.texturesDict["weather_snow_heavy"].TextureId
                            : system.textures.texturesDict["weather_snow_light"].TextureId;
                    }
                    else
                    {
                        textureId = cc.Rainfall >= 0.5
                            ? system.textures.texturesDict["weather_rain_heavy"].TextureId
                            : system.textures.texturesDict["weather_rain_light"].TextureId;
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            // Show clouds.
            BlockPos blockPos = system.capi.World.Player.Entity.Pos.AsBlockPos;
            int regionX = blockPos.X / system.capi.World.BlockAccessor.RegionSize;
            int regionZ = blockPos.Z / system.capi.World.BlockAccessor.RegionSize;

            long index2d = weatherSystem.MapRegionIndex2D(regionX, regionZ);
            weatherSystem.weatherSimByMapRegion.TryGetValue(index2d, out WeatherSimulationRegion weatherSim);

            if (weatherSim == null)
            {
                // Simulation not available.
                textureId = system.textures.texturesDict["empty"].TextureId;
                return;
            }

            textureId = weatherSim.NewWePattern.config.Code switch
            {
                "clearsky" => system.textures.texturesDict["weather_clear"].TextureId,
                "overcast" => system.textures.texturesDict["weather_cloudy"].TextureId,
                _ => system.textures.texturesDict["weather_fair"].TextureId
            };
        }
    }
}

public class StatusHudWeatherRenderer : StatusHudRenderer
{
    private readonly StatusHudWeatherElement element;

    public StatusHudWeatherRenderer(StatusHudSystem system, StatusHudWeatherElement element) : base(system)
    {
        this.element = element;
        text = new StatusHudText(this.system.capi, this.element.GetTextKey(), system.Config);
    }

    public override void Reload()
    {
        text.ReloadText(pos);
    }

    public void SetText(string value)
    {
        text.Set(value);
    }

    protected override void Update()
    {
        base.Update();
        text.SetPos(pos);
    }

    protected override void Render()
    {
        system.capi.Render.RenderTexture(element.textureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}