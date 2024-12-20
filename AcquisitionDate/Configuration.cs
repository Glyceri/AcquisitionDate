using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Serializiation;
using Dalamud.Configuration;
using PetRenamer.PetNicknames.TranslatorSystem;
using System;

namespace AcquisitionDate;

[Serializable]
internal class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public int DateType = 0;
    public int AcquisitionLanuage = 0;
    public bool ShowPlaceholderDates = true;

    public SerializableUser[]? SerializableUsers { get; set; } = null;

    IDatabase? Database;
    bool hasSetUp = false;

    public void Initialise(IDatabase database)
    {
        Database = database;
        hasSetUp = true;

        CurrentInitialise();
    }

    void CurrentInitialise()
    {
        SerializableUsers ??= [];
    }

    public void Save()
    {
        if (!hasSetUp) return;

        SerializableUsers = Database!.SerializeDatabase();

        PluginHandlers.PluginInterface.SavePluginConfig(this);
    }

    public string DateParseString() => DateType switch
    {
        1 => "MM/dd/yyyy",      // Month / Day   / Year
        2 => "yyyy/MM/dd",      // Year  / Month / Day
        3 => "yyyy/dd/MM",      // Year  / Day   / Month
        _ => "dd/MM/yyyy"       // Day   / Month / Year
    };

    public AcquisitionDateLanguage GetLanguage => (AcquisitionDateLanguage)AcquisitionLanuage;

    public string[] DateFormatString =>
    [
        $"{Translator.GetLine("Day")}/{Translator.GetLine("Month")}/{Translator.GetLine("Year")}",
        $"{Translator.GetLine("Month")}/{Translator.GetLine("Day")}/{Translator.GetLine("Year")}",
        $"{Translator.GetLine("Year")}/{Translator.GetLine("Month")}/{Translator.GetLine("Day")}",
        $"{Translator.GetLine("Year")}/{Translator.GetLine("Day")}/{Translator.GetLine("Month")}",
    ];

    public string[] Languages =>
    [
        $"{Translator.GetLine("Default")}",
        $"{Translator.GetLine("English")}",
        $"{Translator.GetLine("German")}",
        $"{Translator.GetLine("French")}",
        $"{Translator.GetLine("Japanese")}",
    ];
}
