using System;
using System.Collections.Generic;

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
        public const int version = 2;

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
    }
}