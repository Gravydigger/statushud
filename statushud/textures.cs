using System.Collections.Generic;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace StatusHud
{
    public class StatusHudTextures
    {
        public int size;

        public LoadedTexture empty;
        public LoadedTexture ping;

        public LoadedTexture altitude;
        public LoadedTexture altitudeNeedle;

        public LoadedTexture armour;

        public LoadedTexture bodyheat;
        public LoadedTexture bodyheatHot; // unused
        public LoadedTexture bodyheatCold;

        public LoadedTexture compass;
        public LoadedTexture compassNeedle;

        public LoadedTexture dateAutumn;
        public LoadedTexture dateSpring;
        public LoadedTexture dateSummer;
        public LoadedTexture dateWinter;

        public LoadedTexture durability;

        public LoadedTexture latitude;
        public LoadedTexture latitudeNeedle;

        public LoadedTexture light;

        public LoadedTexture network;

        public LoadedTexture players;

        public LoadedTexture riftCalm;
        public LoadedTexture riftLow;
        public LoadedTexture riftMedium;
        public LoadedTexture riftHigh;
        public LoadedTexture riftVeryHigh;
        public LoadedTexture riftApocalyptic;


        public LoadedTexture roomRoom;
        public LoadedTexture roomCellar;
        public LoadedTexture roomGreenhouse;

        public LoadedTexture tempstormIncoming;
        public LoadedTexture tempstormDuration;

        public LoadedTexture timeDayHigh;
        public LoadedTexture timeDayLow;
        public LoadedTexture timeDayMid;
        public LoadedTexture timeNight;
        public LoadedTexture timeTwilight;
        public LoadedTexture timeLocal;

        public LoadedTexture sleep;

        public LoadedTexture speed;

        public LoadedTexture stability;

        public LoadedTexture weatherClear;
        public LoadedTexture weatherCloudy;
        public LoadedTexture weatherFair;
        public LoadedTexture weatherHail;
        public LoadedTexture weatherRainHeavy;
        public LoadedTexture weatherRainLight;
        public LoadedTexture weatherSnowHeavy;
        public LoadedTexture weatherSnowLight;
        public LoadedTexture weatherThunder;
        public LoadedTexture weatherThunderstorm;

        public LoadedTexture wet;

        public LoadedTexture wind;
        public LoadedTexture windDir;
        public LoadedTexture windDirArrow;

        protected ICoreClientAPI capi;
        protected List<LoadedTexture> textures;

        public StatusHudTextures(ICoreClientAPI capi, int size)
        {
            this.capi = capi;
            this.size = size;

            this.textures = new List<LoadedTexture>();

            ImageSurface surface;
            Context context;

            // Generate empty texture.
            this.empty = new LoadedTexture(this.capi);
            surface = new ImageSurface(Format.Argb32, this.size, this.size);
            context = new Context(surface);

            this.capi.Gui.LoadOrUpdateCairoTexture(surface, true, ref this.empty);
            context.Dispose();
            surface.Dispose();

            // Generate ping texture.
            this.ping = new LoadedTexture(this.capi);
            surface = new ImageSurface(Format.Argb32, this.size, this.size);
            context = new Context(surface);

            context.LineWidth = 2;

            context.SetSourceRGBA(0, 0, 0, 0.5);
            context.Rectangle(0, 0, this.size, this.size);
            context.Stroke();

            context.SetSourceRGBA(1, 1, 1, 1);
            context.Rectangle(context.LineWidth, context.LineWidth, this.size - (context.LineWidth * 2), this.size - (context.LineWidth * 2));
            context.Stroke();

            this.capi.Gui.LoadOrUpdateCairoTexture(surface, true, ref this.ping);
            context.Dispose();
            surface.Dispose();

            // Load texture files.
            this.load("armour", ref this.armour);

            this.load("bodyheat", ref this.bodyheat);
            this.load("bodyheat_hot", ref this.bodyheatHot);
            this.load("bodyheat_cold", ref this.bodyheatCold);

            this.load("altitude", ref this.altitude);
            this.load("altitude_needle", ref this.altitudeNeedle);

            this.load("compass", ref this.compass);
            this.load("compass_needle", ref this.compassNeedle);

            this.load("date_autumn", ref this.dateAutumn);
            this.load("date_spring", ref this.dateSpring);
            this.load("date_summer", ref this.dateSummer);
            this.load("date_winter", ref this.dateWinter);

            this.load("durability", ref this.durability);

            this.load("latitude", ref this.latitude);
            this.load("latitude_needle", ref this.latitudeNeedle);

            this.load("light", ref this.light);

            this.load("network", ref this.network);

            this.load("players", ref this.players);

            this.load("rift_calm", ref this.riftCalm);
            this.load("rift_low", ref this.riftLow);
            this.load("rift_medium", ref this.riftMedium);
            this.load("rift_high", ref this.riftHigh);
            this.load("rift_veryhigh", ref this.riftVeryHigh);
            this.load("rift_apocalyptic", ref this.riftApocalyptic);

            this.load("room_room", ref this.roomRoom);
            this.load("room_cellar", ref this.roomCellar);
            this.load("room_greenhouse", ref this.roomGreenhouse);

            this.load("sleep", ref this.sleep);

            this.load("speed", ref this.speed);

            this.load("stability", ref this.stability);

            this.load("tempstorm_incoming", ref this.tempstormIncoming);
            this.load("tempstorm_duration", ref this.tempstormDuration);

            this.load("time_day_high", ref this.timeDayHigh);
            this.load("time_day_low", ref this.timeDayLow);
            this.load("time_day_mid", ref this.timeDayMid);
            this.load("time_night", ref this.timeNight);
            this.load("time_twilight", ref this.timeTwilight);
            this.load("time_local", ref this.timeLocal);

            this.load("weather_clear", ref this.weatherClear);
            this.load("weather_cloudy", ref this.weatherCloudy);
            this.load("weather_fair", ref this.weatherFair);
            this.load("weather_hail", ref this.weatherHail);
            this.load("weather_rain_heavy", ref this.weatherRainHeavy);
            this.load("weather_rain_light", ref this.weatherRainLight);
            this.load("weather_snow_heavy", ref this.weatherSnowHeavy);
            this.load("weather_snow_light", ref this.weatherSnowLight);
            this.load("weather_thunder", ref this.weatherThunder);
            this.load("weather_thunderstorm", ref this.weatherThunderstorm);

            this.load("wet", ref this.wet);

            this.load("wind", ref this.wind);
            this.load("wind_dir", ref this.windDir);
            this.load("wind_dir_arrow", ref this.windDirArrow);
        }

        public void Dispose()
        {
            foreach (LoadedTexture texture in this.textures)
            {
                texture.Dispose();
            }
            this.textures.Clear();
        }

        protected void load(string filename, ref LoadedTexture variable)
        {
            variable = new LoadedTexture(capi);
            this.capi.Render.GetOrLoadTexture(new AssetLocation(StatusHudSystem.domain, "textures/" + filename + ".png"), ref variable);

            this.textures.Add(variable);
        }
    }
}