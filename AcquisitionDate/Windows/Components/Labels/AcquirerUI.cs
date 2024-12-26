using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Acquisition.Interfaces;
using AcquistionDate.PetNicknames.TranslatorSystem;
using AcquistionDate.PetNicknames.Windowing.Components.Labels;
using AcquistionDate.PetNicknames.Windowing.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface;
using ImGuiNET;
using System.Numerics;
using Dalamud.Utility;

namespace AcquisitionDate.Windows.Components.Labels;

internal static class AcquirerUI
{
    public static void Draw(IAcquirer acquirer, IDatableData data, string acquirerName, Vector2 size)
    {
        string? errorString = acquirer.AcquisitionError;

        bool hasError = !errorString.IsNullOrWhitespace();
        bool hasSuccess = acquirer.HasSucceeded;

        float completionAmount = acquirer.CompletionRate / (float)255;

        if (!hasError && !acquirer.IsAcquiring)
        {
            completionAmount = 0;
        }

        if (hasSuccess)
        {
            completionAmount = 1;
        }

        int completionPercentageNumber = (int)(completionAmount * 100);

        if (hasError) ImGui.NewLine();

        ImGuiStylePtr style = ImGui.GetStyle();

        float actualWidth = 140 * ImGuiHelpers.GlobalScale;
        float height = size.Y;

        TextAligner.Align(TextAlignment.Left);
        BasicLabel.Draw(acquirerName, new Vector2(actualWidth, size.Y), acquirerName);
        TextAligner.PopAlignment();

        ImGui.SameLine();

        ImGui.BeginDisabled(acquirer.IsAcquiring);
        ImGui.PushFont(UiBuilder.IconFont);

        FontAwesomeIcon iconToUse = FontAwesomeIcon.Play;

        if (hasError || hasSuccess)
        {
            iconToUse = FontAwesomeIcon.Redo;
        }

        if (ImGui.Button($"{iconToUse.ToIconString()}##saveButton_{WindowHandler.InternalCounter}", new Vector2(height, height)))
        {
            acquirer.Acquire(data);
        }

        ImGui.PopFont();
        ImGui.EndDisabled();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip(string.Format(Translator.GetLine("Acquiry.Start"), acquirerName));
        }

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.BeginDisabled(!acquirer.IsAcquiring);
        if (ImGui.Button($"{FontAwesomeIcon.Stop.ToIconString()}##cancelButton_{WindowHandler.InternalCounter}", new Vector2(height, height)))
        {
            acquirer.Cancel();
        }
        ImGui.PopFont();
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip(Translator.GetLine("Acquiry.Stop"));
        }

        ImGui.EndDisabled();
        

        ImGui.SameLine();

        BasicLabel.Draw($"{completionPercentageNumber}%", new Vector2(40 * ImGuiHelpers.GlobalScale, size.Y), string.Format(Translator.GetLine("Acquiry.CompPerc"), completionPercentageNumber));



        float availableWidth = ImGui.GetContentRegionAvail().X;

        uint colour = hasError ? 0xFF0000FF : 0xFF00FF00;

        if (completionAmount > float.Epsilon)
        {
            ImGui.SameLine();

            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, colour);
            ImGui.PushStyleColor(ImGuiCol.Button, colour);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, colour);

            ImGui.Button(string.Empty, new Vector2(ImGui.GetContentRegionAvail().X * completionAmount, size.Y));

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
               
                ImGui.SetTooltip(string.Format(Translator.GetLine("Acquiry.CompPerc"), completionPercentageNumber));
            }

            ImGui.PopStyleColor(3);
        }

        if (hasError)
        {
            LabledLabel.Draw("Error: ", errorString!, new Vector2(ImGui.GetContentRegionAvail().X, size.Y));
            ImGui.NewLine();
        }
    }
}
