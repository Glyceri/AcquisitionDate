using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Database.Structs;
using AcquisitionDate.Services.Interfaces;
using AcquistionDate.PetNicknames.Windowing.Components.Labels;
using Dalamud.Interface.Utility;
using ImGuiNET;

namespace AcquisitionDate.Windows.Windows.ListTabs.Tabs;

internal class AchievementTab : DataTab
{
    public AchievementTab(ISheets sheets) : base(sheets) { }

    public override AcquirableDateType MyType => AcquirableDateType.Achievement;

    public override void Draw(IDatableList dataList, string searchComment)
    {
        for (int i = 0; i < dataList.Length; i++)
        {
            uint id = dataList.GetID(i);
            UnlockedDate? currentDate = dataList.GetDate(id);

            if (currentDate == null) continue;

            LabledLabel.Draw("Achievement:", Sheets.GetAchievementByID(id)?.Name.ExtractText() ?? string.Empty, new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, 35 * ImGuiHelpers.GlobalScale));
        }
    }
}
