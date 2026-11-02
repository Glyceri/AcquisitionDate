using System.Collections.Generic;
using AcquisitionDate.Commands.Commands;
using AcquisitionDate.Commands.Interfaces;
using AcquisitionDate.Windows;

namespace AcquisitionDate.Commands;

internal class CommandHandler : ICommandHandler
{
    private readonly WindowHandler  WindowHandler;
    private readonly List<ICommand> _commands = [];

    public CommandHandler(WindowHandler windowHandler)
    {
        WindowHandler = windowHandler;

        RegisterCommands();
    }
    
    public void Dispose()
    {
        foreach(ICommand command in _commands)
        {
            command.Dispose();
        }   
        _commands.Clear();
    }

    private void RegisterCommands()
    {
        RegisterCommand(new AcquisitionMainCommand       (WindowHandler));
        RegisterCommand(new AcquisitionMainSubCommand    (WindowHandler));
        RegisterCommand(new AcquisitionSettingsCommand   (WindowHandler));
        RegisterCommand(new AcquisitionSettingsSubCommand(WindowHandler));
        RegisterCommand(new DevCommand                   (WindowHandler));
    }

    private void RegisterCommand(ICommand command)
    {
        _commands.Add(command);
    }
}
