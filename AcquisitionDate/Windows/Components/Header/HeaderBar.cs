using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using AcquistionDate.PetNicknames.TranslatorSystem;
using System.Numerics;
using AcquisitionDate.Windows;
using AcquisitionDate;
using AcquisitionDate.Windows.Windows;
using AcquisitionDate.PetNicknames.Windowing.Windows;

namespace AcquistionDate.PetNicknames.Windowing.Components.Header;

internal static class HeaderBar
{
    const float HEADER_BAR_HEIGHT = 35;
    public static float HeaderBarWidth = 0;

    public static void Draw(WindowHandler windowHandler, Configuration configuration, AcquisitionWindow window)
    {
        Vector2 contentSize = ImGui.GetContentRegionAvail();
        contentSize.Y = HEADER_BAR_HEIGHT * ImGuiHelpers.GlobalScale;

        if (Listbox.Begin($"##headerbar_{WindowHandler.InternalCounter}", contentSize))
        {
            Vector2 lastPos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPos().X, lastPos.Y));

            HeaderBarWidth = 0;

            WindowStruct<AcquisitionDebugWindow> petDevWindow = new WindowStruct<AcquisitionDebugWindow>(in windowHandler, in configuration, FontAwesomeIcon.Biohazard, "Acquisition Dev (Messy NGL)", configuration.debugModeActive);
            WindowStruct<KofiWindow> kofiWindow = new WindowStruct<KofiWindow>(in windowHandler, in configuration, FontAwesomeIcon.Coffee, Translator.GetLine("Kofi.Title"), configuration.showKofiButton && window is not KofiWindow);
            WindowStruct<AcquisitionConfigWindow> petConfigWindow = new WindowStruct<AcquisitionConfigWindow>(in windowHandler, in configuration, FontAwesomeIcon.Cogs, Translator.GetLine("Config.Title"), window is not AcquisitionConfigWindow || configuration.quickButtonsToggle);
            WindowStruct<AcquiryWindow> petListWindow = new WindowStruct<AcquiryWindow>(in windowHandler, in configuration, FontAwesomeIcon.FileImport, Translator.GetLine("Acquiry.Title"), true);
            //WindowStruct<PetListWindow> actualPetListWindow = new WindowStruct<PetListWindow>(in windowHandler, in configuration, FontAwesomeIcon.List, Translator.GetLine("PetList.Title"), (window is not PetListWindow) && (configuration.listButtonLayout == 0 || configuration.listButtonLayout == 2));
            //WindowStruct<PetRenameWindow> petRenameWindow = new WindowStruct<PetRenameWindow>(in windowHandler, in configuration, FontAwesomeIcon.PenSquare, Translator.GetLine("ContextMenu.Rename"), window is not PetRenameWindow || configuration.quickButtonsToggle);
            
            float availableWidth = ImGui.GetContentRegionAvail().X;
            availableWidth -= HeaderBarWidth;

            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(availableWidth, 0));

            petDevWindow.Draw();
            kofiWindow.Draw();
            petConfigWindow.Draw();
            petListWindow.Draw();
            //actualPetListWindow.Draw();
            //petRenameWindow.Draw();

            Listbox.End();
        }
    }
}

ref struct WindowStruct<T> where T : AcquisitionWindow
{
    readonly WindowHandler WindowHandler;
    readonly Configuration Configuration;
    readonly FontAwesomeIcon Icon;
    readonly string Tooltip;
    readonly bool Active;

    public WindowStruct(in WindowHandler handler, in Configuration configuration, FontAwesomeIcon icon, string tooltip, bool active = true)
    {
        Active = active;

        if (Active) HeaderBar.HeaderBarWidth += WindowButton.Width;

        WindowHandler = handler;
        Configuration = configuration;
        Icon = icon;
        Tooltip = tooltip;
    }

    public void Draw() 
    {
        if (!Active) return;

        WindowButton.Draw<T>(WindowHandler, Configuration, Icon, Tooltip);
        ImGui.SameLine(0, 0);
    }
}
