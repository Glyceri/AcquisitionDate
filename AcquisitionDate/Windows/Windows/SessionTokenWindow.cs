using AcquistionDate.PetNicknames.Windowing.Components.Labels;
using AcquistionDate.PetNicknames.Windowing.Components;
using ImGuiNET;
using System.Numerics;
using Dalamud.Interface.Utility;

namespace AcquisitionDate.Windows.Windows;

internal class SessionTokenWindow : AcquisitionWindow
{
    protected override Vector2 MinSize { get; } = new Vector2(500, 200);
    protected override Vector2 MaxSize { get; } = new Vector2(500, 1200);
    protected override Vector2 DefaultSize { get; } = new Vector2(500, 500);
    protected override bool HasHeaderBar { get; } = false;

    float BarSize => 25 * ImGuiHelpers.GlobalScale;

    public SessionTokenWindow(WindowHandler windowHandler, Configuration configuration) : base(windowHandler, configuration, "Session Token Window")
    {

    }

    protected override void OnDraw()
    {
        TextAligner.Align(TextAlignment.Left);

        Vector2 size = new Vector2(ImGui.GetContentRegionAvail().X, BarSize);

        BasicLabel.Draw("!!! Disable your VPN !!!", size);
        BasicLabel.Draw("1. Log in to the lodestone.", size);
        BasicLabel.Draw("   !!! MAKE SURE YOU ARE ON THE SAME CHARACTER/ACCOUNT\n   AS YOU ARE LOGGED IN ON IN GAME !!!", new Vector2(ImGui.GetContentRegionAvail().X, BarSize * 2));
        BasicLabel.Draw("2. Right click anything and click inspect element.", size);
        BasicLabel.Draw(" * A weird window now opens up on the side.", size);
        BasicLabel.Draw("3. Click the Network tab in that window.", size);
        BasicLabel.Draw(" * When it opens it should be empty.", size);
        BasicLabel.Draw("4. Now click on the quest section on the lodestone.", size);
        BasicLabel.Draw(" * A LOT of stuff should now show in the Network tab.", size);
        BasicLabel.Draw("5. Scroll to the top of the list and click on 'quest/'.", size);
        BasicLabel.Draw(" * A section should now pop up with the tabs:\n   Headers, Preview, Response, Initiator, Timing, Cookies", new Vector2(ImGui.GetContentRegionAvail().X, BarSize * 2));
        BasicLabel.Draw("6. Click on the 'Cookies' tab.", size);
        BasicLabel.Draw("7. Find the section that says: 'ldst_sess and copy the value.", size);
        BasicLabel.Draw(" * If you see a ... after the cookie number,\n   drag the tab to the left until it disappears.", new Vector2(ImGui.GetContentRegionAvail().X, BarSize * 2));
        BasicLabel.Draw("   !!! NEVER SHARE THIS VALUE WITH ANYONE\n   YES THIS INCLUDES MY PLUGIN TECHNICALLY !!!", new Vector2(ImGui.GetContentRegionAvail().X, BarSize * 2));
        BasicLabel.Draw("8. Now paste the session token into the field 'Session Token' in the 'Acquisition Window'", size);
        BasicLabel.Draw("9. After you are done downloading LOG OUT of the lodestone.\nDon't just click it away LOG OUT! This invalidates your session token.", new Vector2(ImGui.GetContentRegionAvail().X, BarSize * 2));

        TextAligner.PopAlignment();
    }
}
