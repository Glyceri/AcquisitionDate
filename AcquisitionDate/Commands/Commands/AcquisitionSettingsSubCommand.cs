using AcquisitionDate.AcquisitionDate.Commands.Commands;
using AcquisitionDate.Windows;

namespace AcquisitionDate.Commands.Commands;

internal class AcquisitionSettingsSubCommand : AcquisitionSettingsCommand
{
    public override string CommandCode => "/acqdsettings";

    public AcquisitionSettingsSubCommand(WindowHandler windowHandler) : base(windowHandler) { }
}
