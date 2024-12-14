using AcquisitionDate.Core.Handlers;
using AcquisitionDate.PetNicknames.Services.ServiceWrappers;
using AcquisitionDate.PetNicknames.Services.ServiceWrappers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.Services.Wrappers;

namespace AcquisitionDate.Services;

internal class AcquisitionSevices : IAcquisitionServices
{
    public IPetLog PetLog { get; init; }
    public Configuration Configuration { get; init; }
    public ISheets Sheets { get; init; }

    public AcquisitionSevices()
    {
        PetLog = new PetLogWrapper(PluginHandlers.PluginLog);
        Configuration = PluginHandlers.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Sheets = new SheetsWrapper();
    }
}
