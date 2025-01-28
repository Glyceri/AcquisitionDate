using AcquisitionDate.AcquisitionDate.Services.ServiceWrappers.Interfaces;

namespace AcquisitionDate.Services.Interfaces;

internal interface IAcquisitionServices
{
    IBackupService Backup { get; }
    IPetLog PetLog { get; }
    Configuration Configuration { get; }
    ISheets Sheets { get; }
}
