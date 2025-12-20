using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Services.Wrappers;

internal class BackupWrapper : IBackupService
{
    const int MaxAmountOfBackups = 10;

    public void DoBackup()
    {
        PluginHandlers.PluginLog.Fatal("BACKUP SERVICE IS NOT IMPLEMENTED YET!");
    }
}
