using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Serializiation;
using Dalamud.Configuration;
using AcquistionDate.PetNicknames.TranslatorSystem;
using System;

namespace AcquisitionDate;

[Serializable]
internal class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public int DateType = 0;
    public int AcquisitionLanuage = 1;
    public bool ShowPlaceholderDates = true;
    public int FetchDelaySeconds = 0;
    public bool ShowDatesFromAlts = true;

    // UI

    public bool quickButtonsToggle = true;
    public bool showKofiButton = true;

    // Game Elements

    public bool DrawDatesOnAchievements = true;
    public bool DrawDatesOnLevelScreen = true;
    public bool DrawDatesOnCutsceneReplay = true;
    public bool DrawDatesOnEorzeaIncognita = true;
    public bool DrawDatesOnFishGuide = true;
    public bool DrawDatesOnGlassesSelect = true;
    public bool DrawDatesOnMinionNotebook = true;
    public bool DrawDatesOnMountNotebook = true;
    public bool DrawDatesOnOrchestrion = true;
    public bool DrawDatesOnFashionSelect = true;
    public bool DrawDatesOnQuestJournal = true;
    public bool DrawDatesOnDutyFinder = true;

    // End Game Elements

    // ------------------------- Debug SETTINGS --------------------------
    public bool debugModeActive = false;
    public bool openDebugWindowOnStart = false;

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

    public float FetchDelayInSeconds =>
        Math.Clamp
        (
            UnclampedFetchDelayInSeconds,   // Get the fetch delay in seconds
            1.5f,                           // This ensure the minimal delay is always 1.5!
            10.0f                           // This ensures that the maximum delay is always 10 (not relevant, but more than 10 seems craaazy)
        ); 

    float UnclampedFetchDelayInSeconds =>
        FetchDelaySeconds * 0.5f + 1.5f;

    public string[] FetchDelay =>
    [
        $"1.5s",
        $"2s",
        $"2.5s",
        $"3s",
        $"3.5s",
        $"4s",
        $"4.5s",
        $"5s",
    ];
}
