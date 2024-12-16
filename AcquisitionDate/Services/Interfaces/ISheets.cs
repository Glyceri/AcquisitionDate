using Lumina.Excel.Sheets;

namespace AcquisitionDate.Services.Interfaces;

internal interface ISheets
{
    Quest[] AllQuests { get; }
    Item[] AllItems { get; }
    Achievement[] AllAchievements { get; }
    ContentFinderCondition[] AllContentFinderConditions { get; }

    string? GetWorldName(ushort worldID);
    uint? GetWorldID(string worldName);

    Quest? GetQuest(string name, string journalGroup);
    Quest? GetQuest(string name);
    Achievement? GetAchievement(string name);
    Companion? GetCompanion(ushort ID);
    ContentFinderCondition? GetContentFinderCondition(ushort id);
}
