using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Vintagestory.API.Util;

namespace StatusHud
{
    public class StatusHudGui
    {
        StatusHudSystem system;
        StatusHudConfig config;
        IDictionary<int, StatusHudElement> elements;
        string[] elementNames;

        static int elementExists = -1;
        static int selectedElement = 0;
        static int selectedTempScale;
        static int selectedTimeFormat;

        public StatusHudGui(StatusHudSystem system, StatusHudConfig config, IDictionary<int, StatusHudElement> elements, string[] elementNames)
        {
            this.system = system;
            this.config = config;
            this.elements = elements;
            this.elementNames = elementNames;

            selectedTempScale = Array.IndexOf(StatusHudSystem.tempScaleWords, config.options.temperatureScale.ToString());
            selectedTimeFormat = Array.IndexOf(StatusHudSystem.timeFormatWords, config.options.timeFormat);
        }

        public void DrawConfigLibSettings(string id, ConfigLib.ControlButtons controlButtons)
        {
            if (controlButtons.Save) this.system.saveConfig();

            ImGui.Text("Element Visuals");

            if (DrawConfigEditor(id)){
                foreach ((int elementSlot, StatusHudElement element) in elements)
                {
                    element.getRenderer().Reload(config.text);
                }
            }

            if (ImGui.Button($"Reset to Defaults##{id}"))
            {
                this.system.installDefault();
            }
            ImGui.SameLine();
            if (ImGui.Button($"Show Hidden Elements##{id}"))
            {
                config.showHidden = !config.showHidden;
            }


            ImGui.NewLine();
            ImGui.Text("Element Options");

            DrawConfigOptions(id);

            ImGui.NewLine();
            ImGui.Text("Element Placement");

            DrawAddElement(id);

            var sortedElementsByName = elements.OrderBy(name => name.Value.elementName)
                .ToDictionary(element => element.Key, x => x.Value);

            foreach ((int elementSlot, StatusHudElement element) in sortedElementsByName)
            {
                DrawElementSettings(id, elementSlot, element);
            }
        }

        private void DrawConfigOptions(string id)
        {
            int selectedTempScaleLocal = selectedTempScale;
            int selectedTimeFormatLocal = selectedTimeFormat;

            ImGui.Combo($"Temperature Scale##{id}", ref selectedTempScale, StatusHudSystem.tempScaleWords, StatusHudSystem.tempScaleWords.Length);
            ImGui.Combo($"Time Format##{id}", ref selectedTimeFormat, StatusHudSystem.timeFormatWords, StatusHudSystem.timeFormatWords.Length);

            if (selectedTempScaleLocal != selectedTempScale)
            {
                this.config.options.temperatureScale = StatusHudSystem.tempScaleWords[selectedTempScale][0];
            }
            if (selectedTimeFormatLocal != selectedTimeFormat)
            {
                this.config.options.timeFormat = StatusHudSystem.timeFormatWords[selectedTimeFormat];
            }
        }

        private void DrawAddElement(string id)
        {
            ImGui.Combo($"##{id}", ref selectedElement, elementNames, elementNames.Length);
            ImGui.SameLine();
            if (ImGui.Button($"Add Element##{id}"))
            {
                elementExists = 0;
                // Find if elemnt is already displayed
                foreach ((int elementId, StatusHudElement element) in elements)
                {
                    if (element.elementName == elementNames[selectedElement])
                    {
                        elementExists = 1;
                        break;
                    }
                }

                // If not, add it to the hud in the next avaliable slot
                if (elementExists == 0)
                {
                    int slot = 0;
                    for (int i = StatusHudSystem.slotMin; i <= StatusHudSystem.slotMin; i++)
                    {
                        // Element can't be set to zero
                        if (i == 0 || elements.ContainsKey(i)) continue;
                        slot = i;
                        break;
                    }

                    // Create the item
                    if (slot != 0)
                    {
                        system.Set(slot, elementNames[selectedElement]);
                        this.elements[slot].Pos();
                        this.elements[slot].Ping();
                    }
                    else
                    {
                        elementExists = 2;
                    }
                }
            }

            switch (elementExists)
            {
                case 0:
                    ImGui.Text("Element Created!");
                    break;
                case 1:
                    ImGui.Text("Selected element already exists");
                    break;
                case 2:
                    ImGui.Text("Element limit reached! Please delete a pre-existing element");
                    break;
                default:
                    ImGui.NewLine();
                    break;
            }
        }

        private bool DrawConfigEditor(string id)
        {
            Vector4 value = new()
            {
                X = config.text.colour.r,
                Y = config.text.colour.g,
                Z = config.text.colour.b,
                W = config.text.colour.a
            };
            ImGui.ColorEdit4($"Text color##{id}", ref value);
            bool changed = config.text.colour.r != value.X |
                config.text.colour.g != value.Y |
                config.text.colour.b != value.Z |
                config.text.colour.a != value.W;

            if (changed)
            {
                config.text.colour.r = value.X;
                config.text.colour.g = value.Y;
                config.text.colour.b = value.Z;
                config.text.colour.a = value.W;
            }

            return changed;
        }

        private void DrawElementSettings(string id, int elementSlot, StatusHudElement element)
        {
            string elementId = $"{id}{elementSlot}";

            if (!ImGui.CollapsingHeader($"{element.elementName}##{elementId}")) return;
            if (StatusHudPosEditor(element.pos, $"hudelement{elementId}"))
            {
                element.Pos();
            }
            if (ImGui.Button($"Remove Element##{elementId}"))
            {
                system.Unset(elementSlot);
            }
        }

        private bool StatusHudPosEditor(StatusHudPos value, string id)
        {
            bool changed = false;

            changed =
                IntEditor($"Horizontal offset##{id}", ref value.x) |
                IntEditor($"Vertical offset##{id}", ref value.y) |
                AlignEditor($"Horizontal align##{id}", ref value.halign) |
                AlignEditor($"Vertical align##{id}", ref value.valign, false);

            return changed;
        }

        private static bool IntEditor(string title, ref int value)
        {
            int prev = value;
            ImGui.DragInt(title, ref value);
            return prev != value;
        }

        private static readonly string[] alignTypesHorizontal = new string[]
        {
            "Left",
            "Center",
            "Right"
        };
        private static readonly string[] alignTypesVertical = new string[]
        {
            "Top",
            "Center",
            "Bottom"
        };
        private static bool AlignEditor(string title, ref int value, bool horizontal = true)
        {
            int shiftedValue = value + 1;
            if (horizontal)
            {
                ImGui.Combo(title, ref shiftedValue, alignTypesHorizontal, alignTypesHorizontal.Length);
            }
            else
            {
                ImGui.Combo(title, ref shiftedValue, alignTypesVertical, alignTypesVertical.Length);
            }
            bool changed = shiftedValue != value + 1;
            value = shiftedValue - 1;
            return changed;
        }
    }
}
