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
            this.weatherSystem = this.system.capi.ModLoader.GetModSystem<WeatherSystemBase>();

            this.renderer = new StatusHudWeatherRenderer(system, slot, this, config.text);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);

            this.config = config;

            this.tempScale = config.options.temperatureScale;
            this.textureId = this.system.textures.empty.TextureId;

            // Config error checking
            if (!tempFormatWords.Any(str => str.Contains(tempScale)))
            {
                system.capi.Logger.Warning("[" + this.getTextKey() + "] " + tempScale + " is not a valid value for temperatureFormat. Defaulting to C");
            }
        }

        protected override StatusHudRenderer getRenderer()
        {
            return this.renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            ClimateCondition cc = this.system.capi.World.BlockAccessor.GetClimateAt(this.system.capi.World.Player.Entity.Pos.AsBlockPos, EnumGetClimateMode.NowValues);
            this.tempScale = config.options.temperatureScale;

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

            this.renderer.setText(temperature);
            this.updateTexture(cc);
        }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }

        protected void updateTexture(ClimateCondition cc)
        {
            if (cc.Rainfall > 0)
            {
                // Show precipitation.
                switch (this.weatherSystem.WeatherDataSlowAccess.GetPrecType(this.system.capi.World.Player.Entity.Pos.XYZ))
                {
                    case EnumPrecipitationType.Rain:
                        {
                            this.textureId = cc.Rainfall >= 0.5
                                    ? this.system.textures.weatherRainHeavy.TextureId
                                    : this.system.textures.weatherRainLight.TextureId;
                            break;
                        }
                    case EnumPrecipitationType.Snow:
                        {
                            this.textureId = cc.Rainfall >= 0.5
                                    ? this.system.textures.weatherSnowHeavy.TextureId
                                    : this.system.textures.weatherSnowLight.TextureId;
                            break;
                        }
                    case EnumPrecipitationType.Hail:
                        {
                            this.textureId = this.system.textures.weatherHail.TextureId;
                            break;
                        }
                    case EnumPrecipitationType.Auto:
                        {
                            if (cc.Temperature < this.weatherSystem.WeatherDataSlowAccess.BlendedWeatherData.snowThresholdTemp)
                            {
                                this.textureId = cc.Rainfall >= 0.5
                                        ? this.system.textures.weatherSnowHeavy.TextureId
                                        : this.system.textures.weatherSnowLight.TextureId;
                            }
                            else
                            {
                                this.textureId = cc.Rainfall >= 0.5
                                        ? this.system.textures.weatherRainHeavy.TextureId
                                        : this.system.textures.weatherRainLight.TextureId;
                            }
                            break;
                        }
                }
            }
            else
            {
                // Show clouds.
                BlockPos pos = this.system.capi.World.Player.Entity.Pos.AsBlockPos;
                int regionX = (int)pos.X / this.system.capi.World.BlockAccessor.RegionSize;
                int regionZ = (int)pos.Z / this.system.capi.World.BlockAccessor.RegionSize;

                WeatherSimulationRegion weatherSim;
                long index2d = this.weatherSystem.MapRegionIndex2D(regionX, regionZ);
                this.weatherSystem.weatherSimByMapRegion.TryGetValue(index2d, out weatherSim);

                if (weatherSim == null)
                {
                    // Simulation not available.
                    this.textureId = this.system.textures.empty.TextureId;
                    return;
                }

                switch (weatherSim.NewWePattern.config.Code)
                {
                    case "clearsky":
                        {
                            this.textureId = this.system.textures.weatherClear.TextureId;
                            break;
                        }
                    case "overcast":
                        {
                            this.textureId = this.system.textures.weatherCloudy.TextureId;
                            break;
                        }
                    default:
                        {
                            this.textureId = this.system.textures.weatherFair.TextureId;
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

            this.text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
        }

        public void setText(string value)
        {
            this.text.Set(value);
        }

        protected override void update()
        {
            base.update();
            this.text.Pos(this.pos);
        }

        protected override void render()
        {
            this.system.capi.Render.RenderTexture(this.element.textureId, this.x, this.y, this.w, this.h);
        }

        public override void Dispose()
        {
            this.text.Dispose();
        }
    }
}