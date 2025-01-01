using AcquistionDate.PetNicknames.TranslatorSystem;
using Dalamud.Interface.Utility;
using ImGuiNET;
using System.Numerics;


namespace AcquisitionDate.Windows.Windows;

internal class AcquisitionConfigWindow : AcquisitionWindow
{
    protected override Vector2 MinSize { get; } = new Vector2(400, 200);
    protected override Vector2 MaxSize { get; } = new Vector2(400, 1200);
    protected override Vector2 DefaultSize { get; } = new Vector2(400, 500);

    protected override bool HasHeaderBar { get; } = true;

    public AcquisitionConfigWindow(WindowHandler windowHandler, Configuration configuration) : base(windowHandler, configuration, Translator.GetLine("Config.Title"))
    {
        
    }

    protected override void OnDraw()
    {
        if (ImGui.CollapsingHeader(Translator.GetLine("Config.Header.GeneralSettings")))
        {
            DrawMenu(Translator.GetLine("Config.DateFormat"), Configuration.DateFormatString, ref Configuration.DateType, 120);
            DrawMenu(Translator.GetLine("Config.PluginLanguage"), Configuration.Languages, ref Configuration.AcquisitionLanuage, 120);
            if (ImGui.Checkbox(Translator.GetLine("Config.ShowPlaceholderDates") + " [??/??/????]", ref Configuration.ShowPlaceholderDates)) Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawAlts"), ref Configuration.ShowDatesFromAlts)) Configuration.Save();
        }

        if (ImGui.CollapsingHeader(Translator.GetLine("Config.Header.UISettings")))
        {
            if (ImGui.Checkbox(Translator.GetLine("Config.Kofi"), ref Configuration.showKofiButton)) Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.Toggle"), ref Configuration.quickButtonsToggle)) Configuration.Save();
        }

        if (ImGui.CollapsingHeader(Translator.GetLine("Config.Header.NativeSettings")))
        {
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawDatesOnAchievements"),    ref Configuration.DrawDatesOnAchievements))         Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawDatesOnLevelScreen"),     ref Configuration.DrawDatesOnLevelScreen))          Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawDatesOnCutsceneReplay"),  ref Configuration.DrawDatesOnCutsceneReplay))       Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawDatesOnEorzeaIncognita"), ref Configuration.DrawDatesOnEorzeaIncognita))      Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawDatesOnFishGuide"),       ref Configuration.DrawDatesOnFishGuide))            Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawDatesOnGlassesSelect"),   ref Configuration.DrawDatesOnGlassesSelect))        Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawDatesOnMinionNotebook"),  ref Configuration.DrawDatesOnMinionNotebook))       Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawDatesOnMountNotebook"),   ref Configuration.DrawDatesOnMountNotebook))        Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawDatesOnOrchestrion"),     ref Configuration.DrawDatesOnOrchestrion))          Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawDatesOnFashionSelect"),   ref Configuration.DrawDatesOnFashionSelect))        Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawDatesOnQuestJournal"),    ref Configuration.DrawDatesOnQuestJournal))         Configuration.Save();
            if (ImGui.Checkbox(Translator.GetLine("Config.DrawDatesOnDutyFinder"),      ref Configuration.DrawDatesOnDutyFinder))           Configuration.Save();
        }

        if (ImGui.CollapsingHeader(Translator.GetLine("Debug")))
        {
            bool keyComboPressed = ImGui.IsKeyDown(ImGuiKey.LeftCtrl) && ImGui.IsKeyDown(ImGuiKey.LeftShift);

            ImGui.BeginDisabled(!keyComboPressed && !Configuration.debugModeActive);
            if (ImGui.Checkbox("Enable Debug Mode.", ref Configuration.debugModeActive)) Configuration.Save();
            if (ImGui.Checkbox("Open Debug Window On Start.", ref Configuration.openDebugWindowOnStart)) Configuration.Save();
            ImGui.EndDisabled();
        }
    }

    void DrawMenu(string title, string[] elements, ref int configurationInt, float width = 0)
    {
        if (configurationInt < 0 || configurationInt >= elements.Length)
        {
            configurationInt = 0;
        }

        if (width <= 0) width = ImGui.GetContentRegionAvail().X;
        else width = width * ImGuiHelpers.GlobalScale;

        ImGui.SetNextItemWidth(width);

        if (ImGui.BeginCombo(title, elements[configurationInt], ImGuiComboFlags.PopupAlignLeft))
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (ImGui.Selectable(elements[i], i == configurationInt, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    configurationInt = i;
                    Configuration.Save();
                }
            }

            ImGui.EndCombo();
        }
    }
}
