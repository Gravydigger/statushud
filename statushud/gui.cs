using System;
using ImGuiNET;
using VSImGui;
using Vintagestory.API.Client;
using ConfigLib;
using System.Numerics;
using System.Xml.Linq;

namespace StatusHud
{
    public class StatusHudGui : StatusHudSystem
    {
        StatusHudSystem system;

        public StatusHudGui(StatusHudSystem system)
        {
            this.system = system;
        }

        public void DrawConfigLibSettings(string id, ConfigLib.ControlButtons controlButtons)
        {
            if (controlButtons.Save) saveConfig();
            bool textChanged = DrawConfigEditor(id);

            foreach ((int elementId, StatusHudElement element) in elements)
            {
                DrawElementSettings($"{id}{elementId}", element);
            }
        }

        private bool DrawConfigEditor(string id)
        {
            Vector4 value = new()
            {
                X = config.Get().text.colour.r,
                Y = config.Get().text.colour.g,
                Z = config.Get().text.colour.b,
                W = config.Get().text.colour.a
            };
            ImGui.ColorEdit4($"Text color##{id}", ref value);
            bool changed = config.Get().text.colour.r != value.X || config.Get().text.colour.g != value.Y || config.Get().text.colour.b != value.Z || config.Get().text.colour.a != value.W;
            config.Get().text.colour.r = value.X;
            config.Get().text.colour.g = value.Y;
            config.Get().text.colour.b = value.Z;
            config.Get().text.colour.a = value.W;
            return changed;
        }

        private void DrawElementSettings(string elementId, StatusHudElement element)
        {
            if (!ImGui.CollapsingHeader($"{element.Name}##{elementId}")) return;
            if (StatusHudPosEditor(element.pos, $"hudelement{elementId}"))
            {
                element.Pos();
                element.Ping();
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
