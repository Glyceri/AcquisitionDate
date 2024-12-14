using Lumina.Excel.Sheets;

namespace AcquisitionDate.Services.Interfaces;

internal interface ISheets
{
    string? GetWorldName(ushort worldID);
    uint? GetWorldID(string worldName);

    Quest? GetQuest(string name, string journalGroup);
    Quest[] AllQuests { get; }
}
