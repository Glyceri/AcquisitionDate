using AcquisitionDate.PetNicknames.Services.ServiceWrappers.Interfaces;

namespace AcquisitionDate.Services.Interfaces;

internal interface IAcquisitionServices
{
    IPetLog PetLog { get; }
    Configuration Configuration { get; }
    ISheets Sheets { get; }
}
