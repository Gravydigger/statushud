using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudWeatherElement : StatusHudElement
    {
        public new const string name = "weather";
        protected const string textKey = "shud-weather";
        private const float cfratio = 9f / 5f;
        private const float cfdiff = 32;
        private const float ckdiff = 273.15f;

        public static readonly string[] tempFormatWords = new string[] { "C", "F", "K" };
        private string tempScale;
        public int textureId;

        public override string ElementOption => tempScale;
        public override string ElementName => name;

        protected WeatherSystemBase weatherSystem;
        protected StatusHudWeatherRenderer renderer;

        public StatusHudWeatherElement(StatusHudSystem system) : base(system)
        {
            weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            renderer = new StatusHudWeatherRenderer(system, this);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            tempScale = "C";
            textureId = this.system.textures.texturesDict["empty"].TextureId;

            // Config error checking
            if (!tempFormatWords.Any(str => str.Contains(tempScale)))
            {
                system.capi.Logger.Warning("[{0}] {1} is not a valid value for temperatureFormat. Defaulting to C", GetTextKey(), tempScale);
            }
        }

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
            foreach (var words in tempFormatWords)
            {
                if (words == value)
                {
                    tempScale = value;
                }
            }
        }

        public override void Tick()
        {
            ClimateCondition cc = system.capi.World.BlockAccessor.GetClimateAt(system.capi.World.Player.Entity.Pos.AsBlockPos, EnumGetClimateMode.NowValues);

            string temperature = tempScale switch
            {
                "F" => (int)Math.Round((cc.Temperature * cfratio) + cfdiff, 0) + "°F",
                "K" => (int)Math.Round(cc.Temperature + ckdiff, 0) + "°K",
                _ => (int)Math.Round(cc.Temperature, 0) + "°C",
            };

            renderer.SetText(temperature);
            UpdateTexture(cc);
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }

        protected void UpdateTexture(ClimateCondition cc)
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
                }
            }
            else
            {
                // Show clouds.
                BlockPos pos = system.capi.World.Player.Entity.Pos.AsBlockPos;
                int regionX = (int)pos.X / system.capi.World.BlockAccessor.RegionSize;
                int regionZ = (int)pos.Z / system.capi.World.BlockAccessor.RegionSize;

                long index2d = weatherSystem.MapRegionIndex2D(regionX, regionZ);
                weatherSystem.weatherSimByMapRegion.TryGetValue(index2d, out WeatherSimulationRegion weatherSim);

                if (weatherSim == null)
                {
                    // Simulation not available.
                    textureId = system.textures.texturesDict["empty"].TextureId;
                    return;
                }

                switch (weatherSim.NewWePattern.config.Code)
                {
                    case "clearsky":
                        {
                            textureId = system.textures.texturesDict["weather_clear"].TextureId;
                            break;
                        }
                    case "overcast":
                        {
                            textureId = system.textures.texturesDict["weather_cloudy"].TextureId;
                            break;
                        }
                    default:
                        {
                            textureId = system.textures.texturesDict["weather_fair"].TextureId;
                            break;
                        }
                }
            }
        }
    }

    public class StatusHudWeatherRenderer : StatusHudRenderer
    {
        protected StatusHudWeatherElement element;

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
            text.Pos(pos);
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
}