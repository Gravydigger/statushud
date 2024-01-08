using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Linq;

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

        public StatusHudGui(StatusHudSystem system, StatusHudConfig config, IDictionary<int, StatusHudElement> elements, string[] elementNames)
        {
            this.system = system;
            this.config = config;
            this.elements = elements;
            this.elementNames = elementNames;
        }

        public void DrawConfigLibSettings(string id, ConfigLib.ControlButtons controlButtons)
        {
            if (controlButtons.Save) this.system.saveConfig();
            bool textChanged = DrawConfigEditor(id);

            DrawAddElement();

            foreach ((int elementId, StatusHudElement element) in elements)
            {
                DrawElementSettings($"{id}{elementId}", element);
            }
        }

        private void DrawAddElement()
        {
            ImGui.Combo("ElementList", ref selectedElement, elementNames, elementNames.Length);
            if (ImGui.Button("ElementSet"))
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
                    }
                    else
                    {
                        elementExists = 2;
                    }
                }
            }

            ImGui.SameLine();
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
            bool changed = config.text.colour.r != value.X || config.text.colour.g != value.Y || config.text.colour.b != value.Z || config.text.colour.a != value.W;
            config.text.colour.r = value.X;
            config.text.colour.g = value.Y;
            config.text.colour.b = value.Z;
            config.text.colour.a = value.W;
            return changed;
        }

        private void DrawElementSettings(string elementId, StatusHudElement element)
        {
            if (!ImGui.CollapsingHeader($"{element.elementName}##{elementId}")) return;
            if (StatusHudPosEditor(element.pos, $"hudelement{elementId}"))
            {
                element.Pos();
            }
        }

        private bool StatusHudPosEditor(StatusHudPos value, string id)
        {
            bool changed = false;
            if (IntEditor($"Horizontal offset##{id}", ref value.x)) changed = true;
            if (IntEditor($"Vertical offset##{id}", ref value.y)) changed = true;
            if (AlignEditor($"Horizontal align##{id}", ref value.halign, horizontal: true)) changed = true;
            if (AlignEditor($"Vertical align##{id}", ref value.valign, horizontal: false)) changed = true;
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
        private static bool AlignEditor(string title, ref int value, bool horizontal)
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
