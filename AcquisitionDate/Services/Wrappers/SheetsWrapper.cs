using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Services.Interfaces;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System.Linq;

namespace AcquisitionDate.Services.Wrappers;

internal class SheetsWrapper : ISheets
{
    readonly ExcelSheet<World> worlds;
    readonly ExcelSheet<Achievement> Achievements;
    readonly Quest[] Quests;

    public SheetsWrapper()
    {
        worlds = PluginHandlers.DataManager.GetExcelSheet<World>();
        Achievements = PluginHandlers.DataManager.GetExcelSheet<Achievement>();
        Quests = [
            ..PluginHandlers.DataManager.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.English).ToArray(),
            ..PluginHandlers.DataManager.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.German).ToArray(),
            ..PluginHandlers.DataManager.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.French).ToArray(),
            ..PluginHandlers.DataManager.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.Japanese).ToArray(),
            ];
    }

    public Quest[] AllQuests => Quests;

    public Achievement? GetAchievement(string name)
    {
        foreach (Achievement achievement in Achievements)
        {
            string achiName = achievement.Name.ExtractText();
            if (achiName != name) continue;

            return achievement;
        }

        return null;
    }

    public Quest? GetQuest(string name, string journalGroup)
    {
        int questCount = Quests.Length;
        for (int i = 0; i < questCount; i++)
        {
            try
            {
                Quest quest = Quests[i];
                string journalGenre = quest.JournalGenre.Value.Name.ExtractText();
                string questName = quest.Name.ExtractText();

                if (!name.Equals(questName, System.StringComparison.InvariantCultureIgnoreCase)) continue;
                if (!journalGroup.Equals(journalGenre, System.StringComparison.InvariantCultureIgnoreCase)) continue;

                return quest;
            }
            catch { continue; }
        }

        return null;
    }

    public Quest? GetQuest(string name)
    {
        int questCount = Quests.Length;
        for (int i = 0; i < questCount; i++)
        {
            try
            {
                Quest quest = Quests[i];
                string questName = quest.Name.ExtractText();

                if (!name.Equals(questName, System.StringComparison.InvariantCultureIgnoreCase)) continue;

                return quest;
            }
            catch { continue; }
        }

        return null;
    }

    public uint? GetWorldID(string worldName)
    {
        foreach (World world in worlds)
        {
            string name = world.InternalName.ExtractText();
            if (!name.Equals(worldName, System.StringComparison.InvariantCultureIgnoreCase)) continue;

            return world.RowId;
        }

        return null;
    }

    public string? GetWorldName(ushort worldID)
    {
        World? world = worlds.GetRow(worldID);
        if (world == null) return null;

        return world.Value.InternalName.ExtractText();
    }
}
