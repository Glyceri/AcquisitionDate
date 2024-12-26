using AcquistionDate.PetNicknames.TranslatorSystem;
using AcquistionDate.PetNicknames.Windowing.Components.Labels;
using AcquistionDate.PetNicknames.Windowing.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface;
using Dalamud.Utility;
using ImGuiNET;
using System.Numerics;

namespace AcquisitionDate.Windows.Components.Labels;

internal static class RenameLabel
{
    public static bool Draw(string label, bool activeSave, ref string value, Vector2 size, string tooltipLabel = "", float labelWidth = 140)
    {
        ImGuiStylePtr style = ImGui.GetStyle();

        float actualWidth = labelWidth * ImGuiHelpers.GlobalScale;
        float height = size.Y;

        TextAligner.Align(TextAlignment.Left);
        BasicLabel.Draw(label, new Vector2(actualWidth, size.Y), tooltipLabel);
        TextAligner.PopAlignment();

        ImGui.SameLine();

        bool shouldActivate = false;

        ImGui.BeginDisabled(activeSave);
        ImGui.PushFont(UiBuilder.IconFont);

        shouldActivate |= ImGui.Button($"{FontAwesomeIcon.Download.ToIconString()}##saveButton_{WindowHandler.InternalCounter}", new Vector2(height, height));

        ImGui.PopFont();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip(Translator.GetLine("Acquiry.Save"));
        }

        ImGui.EndDisabled();

        ImGui.SameLine();

        if (EraserButton.Draw(new Vector2(height, height), Translator.GetLine("ClearButton.Label"), Translator.GetLine("Acquiry.Clear")))
        {
            value = string.Empty;
            shouldActivate |= true;
        }
        ImGui.SameLine();

        bool valueNullOrWhitespace = value.IsNullOrWhitespace();

        if (valueNullOrWhitespace)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
            ImGui.PushStyleColor(ImGuiCol.Border, 0xFF404040);
        }

        shouldActivate |= ImGui.InputTextMultiline($"##RenameBar_{WindowHandler.InternalCounter}", ref value, 64, size - new Vector2(actualWidth + style.ItemSpacing.X * 3 + height * 2, 0), ImGuiInputTextFlags.CtrlEnterForNewLine | ImGuiInputTextFlags.EnterReturnsTrue);

        if (valueNullOrWhitespace)
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
        }

        return shouldActivate;
    }
}
