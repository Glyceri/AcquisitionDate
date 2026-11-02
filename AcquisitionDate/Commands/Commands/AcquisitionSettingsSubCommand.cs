using AcquisitionDate.Windows;

namespace AcquisitionDate.Commands.Commands;

internal class AcquisitionSettingsSubCommand : AcquisitionSettingsCommand
{
    public AcquisitionSettingsSubCommand(WindowHandler windowHandler) 
        : base(windowHandler) 
        { }
    
    public override string CommandCode 
        => "/acqdsettings";
}
