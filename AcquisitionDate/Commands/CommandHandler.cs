using AcquisitionDate.AcquisitionDate.Commands.Commands;
using AcquisitionDate.AcquisitionDate.Commands.Interfaces;
using AcquisitionDate.Commands.Commands;
using AcquisitionDate.Windows;
using System.Collections.Generic;

namespace AcquisitionDate.AcquisitionDate.Commands;

internal class CommandHandler : ICommandHandler
{
    readonly WindowHandler WindowHandler;
    readonly Configuration Configuration;

    readonly List<ICommand> Commands = new List<ICommand>();

    public CommandHandler(Configuration configuration, WindowHandler windowHandler)
    {
        Configuration = configuration;
        WindowHandler = windowHandler;

        RegisterCommands();
    }

    void RegisterCommands()
    {
        RegisterCommand(new AcquisitionMainCommand          (WindowHandler));
        RegisterCommand(new AcquisitionMainSubCommand       (WindowHandler));
        RegisterCommand(new AcquisitionSettingsCommand      (WindowHandler));
        RegisterCommand(new AcquisitionSettingsSubCommand   (WindowHandler));
        RegisterCommand(new DevCommand                      (Configuration, WindowHandler));
    }

    void RegisterCommand(ICommand command)
    {
        Commands.Add(command);
    }

    public void Dispose()
    {
        foreach(ICommand command in Commands)
        {
            command?.Dispose();
        }   
        Commands.Clear();
    }
}
