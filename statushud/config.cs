using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace StatusHud
{
    public class StatusHudConfig
    {
        public int iconSize = 32;
        public bool showHidden = false;
        public StatusHudTextConfig text = new StatusHudTextConfig(new StatusHudColour(0.91f, 0.87f, 0.81f, 1), 16, true, 0, -19, EnumTextOrientation.Center);
        public string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        public StatusHudOptions options = new StatusHudOptions('C', "24hr");
        public bool compassAbsolute = false;
        public IDictionary<int, StatusHudConfigElement> elements = new Dictionary<int, StatusHudConfigElement>();
        public bool installed = false;
    }

    public class StatusHudTextConfig
    {
        public StatusHudColour colour;
        public float size;
        public bool bold;
        public float offsetX;
        public float offsetY;
        public EnumTextOrientation align;

        public StatusHudTextConfig(StatusHudColour colour, float size, bool bold, float offsetX, float offsetY, EnumTextOrientation align)
        {
            this.colour = colour;
            this.size = size;
            this.bold = bold;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.align = align;
        }
    }

    public class StatusHudColour
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public StatusHudColour(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Vec4f ToVec4f()
        {
            return new Vec4f(r, g, b, a);
        }
    }

    public class StatusHudOptions
    {
        public char temperatureScale = 'C';
        public string timeFormat = "24hr";

        public StatusHudOptions(char temperatureScale, string timeFormat)
        {
            this.temperatureScale = temperatureScale;
            this.timeFormat = timeFormat;
        }
    }

    public class StatusHudConfigManager
    {
        public const string filename = "statushud.json";

        protected StatusHudConfig config;

        protected ICoreClientAPI capi;

        public StatusHudConfigManager(ICoreClientAPI capi)
        {
            this.capi = capi;

            // Load or create config file.
            Load();

            if (config == null)
            {
                config = new StatusHudConfig();
            }
            // Create new config file, or update current file to generate missing fields.
            this.capi.StoreModConfig<StatusHudConfig>(config, StatusHudConfigManager.filename);
        }

        public StatusHudConfig Get()
        {
            return config;
        }

        public void Load()
        {
            try
            {
                config = capi.LoadModConfig<StatusHudConfig>(StatusHudConfigManager.filename);
            }
            catch (Exception) { }
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

        public void Save(IDictionary<int, StatusHudElement> elements)
        {
            // Save element data to config.
            config.elements.Clear();
            foreach (KeyValuePair<int, StatusHudElement> kvp in elements)
            {
                config.elements.Add(kvp.Key, new StatusHudConfigElement((string)kvp.Value.GetType().GetField("name").GetValue(null),
                        kvp.Value.pos.halign,
                        kvp.Value.pos.x,
                        kvp.Value.pos.valign,
                        kvp.Value.pos.y));
            }

            // Save config file.
            capi.StoreModConfig<StatusHudConfig>(config, StatusHudConfigManager.filename);
        }

        public void Save()
        {
            capi.StoreModConfig<StatusHudConfig>(config, StatusHudConfigManager.filename);
        }
    }

    public class StatusHudConfigElement
    {
        public string name;
        public int halign;
        public int x;
        public int valign;
        public int y;

        public StatusHudConfigElement(string name, int halign, int x, int valign, int y)
        {
            this.name = name;
            this.halign = halign;
            this.x = x;
            this.valign = valign;
            this.y = y;
        }
    }
}