using AcquiryDate.PetNicknames.Services.ServiceWrappers.Interfaces;
using Lumina.Excel.Sheets;

namespace AcquisitionDate.Services.Interfaces;

internal interface ISheets
{
    Quest[] AllQuests { get; }
    Item[] AllItems { get; }
    Achievement[] AllAchievements { get; }
    ContentFinderCondition[] AllContentFinderConditions { get; }
    FishParameter[] AllFishies { get; }
    SpearfishingItem[] AllSpearFishies { get; }
    ClassJob[] AllClassJobs { get; }

    string? GetWorldName(ushort worldID);
    uint? GetWorldID(string worldName);

    Quest? GetQuest(string name, string journalGroup);
    Quest? GetQuest(string name);
    Quest? GetQuest(uint id);
    Achievement? GetAchievement(string name);
    IPetSheetData? GetCompanion(ushort ID);
    IPetSheetData? GetCompanionByIcon(uint icon);
    IPetSheetData? GetCompanionByName(string name);
    Mount? GetMountByName(string name);
    IGlassesSheetData? GetGlassesByName(string name);
    ContentFinderCondition? GetContentFinderCondition(ushort id);
    ClassJob? GetClassJob(uint id);
    Adventure? GetAdventureByIndex(uint index);
}
