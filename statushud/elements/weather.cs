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
        public new const string desc = "The 'weather' element displays the current temperature and an icon for the current condition.";
        protected const string textKey = "shud-weather";

        public override string elementName => name;

        protected WeatherSystemBase weatherSystem;
        protected StatusHudWeatherRenderer renderer;
        protected StatusHudConfig config;

        protected char tempScale;
        static readonly string[] tempFormatWords = new string[] { "C", "F", "K" };

        protected const float cfratio = (9f / 5f);
        protected const float cfdiff = 32;
        protected const float ckdiff = 273.15f;

        public int textureId;

        public StatusHudWeatherElement(StatusHudSystem system, int slot, StatusHudConfig config) : base(system, slot)
        {
            weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            renderer = new StatusHudWeatherRenderer(system, slot, this, config.text);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            this.config = config;

            tempScale = config.options.temperatureScale;
            textureId = this.system.textures.texturesDict["empty"].TextureId;

            // Config error checking
            if (!tempFormatWords.Any(str => str.Contains(tempScale)))
            {
                system.capi.Logger.Warning("[" + getTextKey() + "] " + tempScale + " is not a valid value for temperatureFormat. Defaulting to C");
            }
        }

        public override StatusHudRenderer getRenderer()
        {
            return renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            ClimateCondition cc = system.capi.World.BlockAccessor.GetClimateAt(system.capi.World.Player.Entity.Pos.AsBlockPos, EnumGetClimateMode.NowValues);
            tempScale = config.options.temperatureScale;

            string temperature;

            switch (tempScale)
            {
                case 'F':
                    temperature = (int)Math.Round((cc.Temperature * cfratio) + cfdiff, 0) + "°F";
                    break;
                case 'K':
                    temperature = (int)Math.Round(cc.Temperature + ckdiff, 0) + "°K";
                    break;
                default:
                    temperature = (int)Math.Round(cc.Temperature, 0) + "°C";
                    break;
            }

            renderer.setText(temperature);
            updateTexture(cc);
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }

        protected void updateTexture(ClimateCondition cc)
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

                WeatherSimulationRegion weatherSim;
                long index2d = weatherSystem.MapRegionIndex2D(regionX, regionZ);
                weatherSystem.weatherSimByMapRegion.TryGetValue(index2d, out weatherSim);

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
        protected StatusHudText text;

        public StatusHudWeatherRenderer(StatusHudSystem system, int slot, StatusHudWeatherElement element, StatusHudTextConfig config) : base(system, slot)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
        }

        public override void Reload(StatusHudTextConfig config)
        {
            text.ReloadText(config, pos);
        }

        public void setText(string value)
        {
            text.Set(value);
        }

        protected override void update()
        {
            base.update();
            text.Pos(pos);
        }

        protected override void render()
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