using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Services.Interfaces;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System.Linq;
using System.Text.RegularExpressions;

namespace AcquisitionDate.Services.Wrappers;

internal class SheetsWrapper : ISheets
{
    public Quest[] AllQuests => Quests;
    public Item[] AllItems => items.ToArray();
    public Achievement[] AllAchievements => achievements.ToArray();
    public ContentFinderCondition[] AllContentFinderConditions => contentFinderConditions.ToArray();

    readonly Quest[] Quests;

    readonly ExcelSheet<World> worlds;
    readonly ExcelSheet<Achievement> achievements;
    readonly ExcelSheet<Item> items;
    readonly ExcelSheet<Companion> companions;
    readonly ExcelSheet<ContentFinderCondition> contentFinderConditions;

    public SheetsWrapper()
    {
        worlds = PluginHandlers.DataManager.GetExcelSheet<World>();
        achievements = PluginHandlers.DataManager.GetExcelSheet<Achievement>();
        Quests = [
            ..PluginHandlers.DataManager.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.English).ToArray(),
            //..PluginHandlers.DataManager.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.German).ToArray(),
            //..PluginHandlers.DataManager.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.French).ToArray(),
            //..PluginHandlers.DataManager.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.Japanese).ToArray(),
            ];
        items = PluginHandlers.DataManager.GetExcelSheet<Item>();
        companions = PluginHandlers.DataManager.GetExcelSheet<Companion>();
        contentFinderConditions = PluginHandlers.DataManager.GetExcelSheet<ContentFinderCondition>();
    }

    public ContentFinderCondition? GetContentFinderCondition(ushort id)
    {
        foreach (ContentFinderCondition contentFinderCondition in contentFinderConditions)
        {
            if (contentFinderCondition.RowId != id) continue;

            return contentFinderCondition;
        }

        return null;
    }

    public Companion? GetCompanion(ushort ID)
    {
        foreach (Companion companion in companions)
        {
            if (companion.RowId != ID) continue;

            return companion;
        }

        return null;
    }

    public Achievement? GetAchievement(string name)
    {
        string betterName = Regex.Replace(name, @"[\r\n]+", string.Empty);

        foreach (Achievement achievement in achievements)
        {
            string achiName = achievement.Name.ExtractText();
            if (!string.Equals(achiName, betterName, System.StringComparison.InvariantCultureIgnoreCase)) continue;

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
