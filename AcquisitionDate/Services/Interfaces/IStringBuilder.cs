using AcquisitionDate.Database.Structs;
using AcquisitionDate.Hooking.Hooks;

namespace AcquisitionDate.Services.Interfaces;

internal interface IStringBuilder
{
    bool CreateDateString(TypedDatabaseEntry[] entries, uint entryId, DrawTextFlags drawTextFlags, out string? dateString, out string? nameString, out string? homeworldString);
}