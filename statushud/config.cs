using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace StatusHud
{
    public class StatusHudConfig
    {
        public int version = 0;
        public int iconSize = 32;
        public int textSize = 16;
        public bool showHidden = false;
        // public StatusHudTextConfig text = new StatusHudTextConfig(new StatusHudColour(0.91f, 0.87f, 0.81f, 1), 16, true, 0, -19, EnumTextOrientation.Center);
        // public string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        // public StatusHudOptions options = new StatusHudOptions('C', "24hr");
        // public bool compassAbsolute = false;
        public IDictionary<int, StatusHudConfigElement> elements = new Dictionary<int, StatusHudConfigElement>();
        // public bool installed = false;
    }

    public class StatusHudConfigElement
    {
        public string name;
        public int x;
        public int y;
        public int halign;
        public int valign;
        public string elementOptions;

        public StatusHudConfigElement(string name, int x, int y, int halign, int valign, string elementOptions)
        {
            this.name = name;
            this.x = x;
            this.halign = halign;
            this.y = y;
            this.valign = valign;
            this.elementOptions = elementOptions;
        }
    }

    // public class StatusHudTextConfig
    // {
    //     public StatusHudColour colour;
    //     public float size;
    //     public bool bold;
    //     public float offsetX;
    //     public float offsetY;
    //     public EnumTextOrientation align;

    //     public StatusHudTextConfig(StatusHudColour colour, float size, bool bold, float offsetX, float offsetY, EnumTextOrientation align)
    //     {
    //         this.colour = colour;
    //         this.size = size;
    //         this.bold = bold;
    //         this.offsetX = offsetX;
    //         this.offsetY = offsetY;
    //         this.align = align;
    //     }
    // }

    // public class StatusHudColour
    // {
    //     public float r;
    //     public float g;
    //     public float b;
    //     public float a;

    //     public StatusHudColour(float r, float g, float b, float a)
    //     {
    //         this.r = r;
    //         this.g = g;
    //         this.b = b;
    //         this.a = a;
    //     }

    //     public Vec4f ToVec4f()
    //     {
    //         return new Vec4f(r, g, b, a);
    //     }
    // }

    // public class StatusHudOptions
    // {
    //     public char temperatureScale = 'C';
    //     public string timeFormat = "24hr";

    //     public StatusHudOptions(char temperatureScale, string timeFormat)
    //     {
    //         this.temperatureScale = temperatureScale;
    //         this.timeFormat = timeFormat;
    //     }
    // }

    public class StatusHudConfigManager
    {
        private const string filename = "statushud.json";
        public const int version = 2;

        private StatusHudConfig config;
        private readonly StatusHudSystem system;

        public StatusHudConfig Config => config;

        public StatusHudConfigManager(StatusHudSystem system)
        {
            this.system = system;

            // Load config file
            Load();

            // Create new config file if none is present
            if (config == null)
            {
                config = new StatusHudConfig();
                system.capi.Logger.Debug(StatusHudSystem.PrintModName($"Generated new config file {filename}"));
                this.system.capi.StoreModConfig(config, filename);
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
            foreach (KeyValuePair<int, StatusHudConfigElement> kvp in config.elements)
            {
                if (system.Set(kvp.Key, kvp.Value.name))
                {
                    system.Pos(kvp.Key, kvp.Value.halign, kvp.Value.x, kvp.Value.valign, kvp.Value.y);
                }
            }
        }

        public void Save()
        {
            // Save element data to config
            config.elements.Clear();

            foreach (KeyValuePair<int, StatusHudElement> kvp in system.elements)
            {
                config.elements.Add(kvp.Key, new StatusHudConfigElement((string)kvp.Value.GetType().GetField("name").GetValue(null),
                    kvp.Value.pos.x,
                    kvp.Value.pos.y,
                    kvp.Value.pos.halign,
                    kvp.Value.pos.valign,
                    kvp.Value.ElementOption)
                );
            }
            // Save config file
            system.capi.StoreModConfig(config, filename);
        }

        // public void Save()
        // {
        //     system.capi.StoreModConfig(config, filename);
        // }
    }
}