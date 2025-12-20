using AcquisitionDate;
using AcquisitionDate.Core.Handlers;
using Dalamud.Game;
using System.Collections.Generic;

namespace AcquistionDate.PetNicknames.TranslatorSystem;

// Trying to stay away from statics, but in this case it just made MUCH more sense.
internal static class Translator
{
    static Configuration? Configuration;

    static Dictionary<string, string> EnglishTranslations = new Dictionary<string, string>()
    {
        { "...",                                "..." },

        { "AchievedOn",                         "Acquired:" },
        { "NoDate",                             "Date unavailable." },
        { "NotAccurate",                        "May not be accurate." },

        { "Day",                                "Day" },
        { "Month",                              "Month" },
        { "Year",                               "Year" },
        { "Config.DateFormat",                  "Date Format" },

        { "Default",                            "Default" },
        { "English",                            "English" },
        { "German",                             "German" },
        { "French",                             "French" },
        { "Japanese",                           "Japanese" },
        { "Config.PluginLanguage",              "Plugin Language" },

        { "Config.Title",                       "Acquisition Date Settings" },
        { "Config.Header.GeneralSettings",      "General Settings" },
        { "Config.Header.UISettings",           "UI Settings" },
        { "Config.Header.NativeSettings",       "Game-UI Settings" },
        { "Config.DrawDatesOnAchievements",     "Draw Dates for Achievements" },
        { "Config.DrawDatesOnLevelScreen",      "Draw Dates for Character Level" },
        { "Config.DrawDatesOnCutsceneReplay",   "Draw Dates on Cutscene Replay" },
        { "Config.DrawDatesOnEorzeaIncognita",  "Draw Dates on the Sightseeing Log" },
        { "Config.DrawDatesOnFishGuide",        "Draw Dates on the Fish Guide" },
        { "Config.DrawDatesOnGlassesSelect",    "Draw Dates for Glasses" },
        { "Config.DrawDatesOnMinionNotebook",   "Draw Dates for Minions" },
        { "Config.DrawDatesOnMountNotebook",    "Draw Dates for Mounts" },
        { "Config.DrawDatesOnOrchestrion",      "Draw Dates for Orchestrion Rolls" },
        { "Config.DrawDatesOnFashionSelect",    "Draw Dates for Fashion Items" },
        { "Config.DrawDatesOnQuestJournal",     "Draw Dates for Quests" },
        { "Config.DrawDatesOnDutyFinder",       "Draw Dates on the Duty Finder" },
        { "Config.ShowPlaceholderDates",        "Show Placeholder Dates" },
        { "Config.Toggle",                      "Quick Buttons toggle instead of open." },
        { "Config.Kofi",                        "Show Ko-fi Button." },
        { "Config.DrawAlts",                    "Show Dates for Alt-Accounts if they are available." },

        { "Kofi.Title",                         "Ko-fi" },
        { "Kofi.Line1",                         "This is about real life money." },
        { "Kofi.Line2",                         "It will be used to my cat toys!" },
        { "Kofi.TakeMe",                        "Take me" },

        { "Acquiry.Save",                       "Apply Session Token." },
        { "Acquiry.Clear",                      "Clear Session Token." },
        { "ClearButton.Label",                  "Hold \"Left Ctrl\" + \"Left Shift\" to clear the Session Token." },
        { "Acquiry.Start",                      "Download {0} dates from the Lodestone."},
        { "Acquiry.Stop",                       "Stop Lodestone Download." },
        { "Acquiry.CompPerc",                   "Completion Percentage: {0}%"},
        { "Acquiry.Title",                      "Acquisition"},

        { "DateTimeString.Before",              "Before {0}"},
    };

    static Dictionary<string, string> GermanTranslations = new Dictionary<string, string>()
    {
        { "AchievedOn",                         "Erhalten:" },
        { "NoDate",                             "Datum nicht verfügbar." },
        { "NotAccurate",                        "Möglicherweise nicht genau." },
        { "Day",                                "Tag" },
        { "Month",                              "Monat" },
        { "Year",                               "Jahr" },
        { "Config.DateFormat",                  "Datumsformat" },
        { "Default",                            "Standard" },
        { "English",                            "Englisch" },
        { "German",                             "Deutsch" },
        { "French",                             "Französisch" },
        { "Japanese",                           "Japanisch" },
        { "Config.PluginLanguage",              "Plugin-Sprache" },
        { "Config.Header.GeneralSettings",      "Allgemeine Einstellungen" },
        { "Config.Header.UISettings",           "UI-Einstellungen" },
        { "Config.Header.NativeSettings",       "Game-UI-Einstellungen" },
        { "Config.DrawDatesOnAchievements",     "Erwerbsdatum für Errungenschaften anzeigen" },
        { "Config.DrawDatesOnLevelScreen",      "Erwerbsdatum für Character Level anzeigen" },
        { "Config.DrawDatesOnCutsceneReplay",   "Erwerbsdatum im Cutscene Replay anzeigen" },
        { "Config.DrawDatesOnEorzeaIncognita",  "Erwerbsdatum im Eorzea Incognita anzeigen" },
        { "Config.DrawDatesOnFishGuide",        "Erwerbsdatum im Fischverzeichnis anzeigen" },
        { "Config.DrawDatesOnGlassesSelect",    "Erwerbsdatum für Gesichtsaccesoires anzeigen" },
        { "Config.DrawDatesOnMinionNotebook",   "Erwerbsdatum für Begleiter anzeigen" },
        { "Config.DrawDatesOnMountNotebook",    "Erwerbsdatum für Reittiere anzeigen" },
        { "Config.DrawDatesOnOrchestrion",      "Erwerbsdatum für Orchestrion-Rolle anzeigen" },
        { "Config.DrawDatesOnFashionSelect",    "Erwerbsdatum für Modeaccessoires anzeigen" },
        { "Config.DrawDatesOnQuestJournal",     "Erwerbsdatum für Aufträge anzeigen" },
        { "Config.DrawDatesOnDutyFinder",       "Erwerbsdatum im Inhaltssuche anzeigen" },
        { "Config.ShowPlaceholderDates",        "Platzhalter für Erwerbsdaten anzeigen" },
        { "Config.Toggle",                      "Schnellschaltflächen werden umgeschaltet statt geöffnet." },
        { "Config.Kofi",                        "Ko-Fi-Button anzeigen." },
        { "Config.DrawAlts",                    "Erwerbsdatum für Alt-Accounts anzeigen wann sie verfügbar sind." },

        { "Kofi.Title",                         "Ko-fi" },
        { "Kofi.Line1",                         "Hier geht es um echtes Geld." },
        { "Kofi.Line2",                         "Es wird für Cat-Toys verwendet!" },
        { "Kofi.TakeMe",                        "Los geht's" },

        { "Acquiry.Save",                       "Session Token anwenden." },
        { "Acquiry.Clear",                      "Session Token löschen." },
        { "ClearButton.Label",                  "Halten Sie \"Linke Strg\" + \"Linke Umschalt\" gedrückt, um das Session Token zu löschen." },
        { "Acquiry.Start",                      "Laden {0} Daten vom Lodestone."},
        { "Acquiry.Stop",                       "Lodestone-Download Stoppen." },
        { "Acquiry.CompPerc",                   "Abschlussprozentsatz: {0}%"},

        { "DateTimeString.Before",              "Vor dem {0}"},
    };

    static Dictionary<string, string> FrenchTranslations = new Dictionary<string, string>()
    {
        { "AchievedOn", "Date d'obtention :" },
        { "NoDate", "Date indisponible." },
        { "NotAccurate", "Ce n'est peut-être pas exact." },
        { "Day", "Jour" },
        { "Month", "Mois" },
        { "Year", "Année" },        
        { "Config.DateFormat", "Format de date" },
        { "Default", "Défaut" },
        { "English", "Anglais" },
        { "German", "Allemande" },
        { "French", "Français" },
        { "Japanese", "Japonaise" },
        { "Config.PluginLanguage", "Langue du plug-in" },
        { "Config.Header.GeneralSettings", "Paramètres généraux" },
        { "Config.Header.UISettings", "Paramètres d'interface utilisateur" },
        { "Config.Header.NativeSettings", "Paramètres de l'interface du jeu" },
    };

    static Dictionary<string, string> JapaneseTranslations = new Dictionary<string, string>()
    {
        { "AchievedOn", "修得日:" },
        { "NoDate", "日付が利用できません。" },
        { "NotAccurate", "正確ではない可能性があります。" },
        { "Day", "日" },
        { "Month", "月" },
        { "Year", "年" },        
        { "Config.DateFormat", "日付形式" },
        { "Default", "既定" },
        { "English", "英語" },
        { "German", "ドイツ語" },
        { "French", "フランス語" },
        { "Japanese", "日本語" },
        { "Config.PluginLanguage", "プラグイン言語" },
        { "Config.Header.GeneralSettings", "一般設定" },
        { "Config.Header.UISettings", "UI設定" },
        { "Config.Header.NativeSettings", "ゲームUI設定" },
    };

    public static void Initialise(Configuration configuration)
    {
        Configuration = configuration;
    }

    internal static string GetLine(string identifier)
    {
        ClientLanguage language = PluginHandlers.ClientState.ClientLanguage;

        AcquisitionDateLanguage Language = Configuration?.GetLanguage ?? AcquisitionDateLanguage.Default;

        if (Language != AcquisitionDateLanguage.Default)
        {
            if (Language == AcquisitionDateLanguage.English) language = ClientLanguage.English;
            else if (Language == AcquisitionDateLanguage.German) language = ClientLanguage.German;
            else if (Language == AcquisitionDateLanguage.French) language = ClientLanguage.French;
            else if (Language == AcquisitionDateLanguage.Japanese) language = ClientLanguage.Japanese;
        }

        if (language == ClientLanguage.German) return GetTranslation(ref GermanTranslations, identifier);
        if (language == ClientLanguage.French) return GetTranslation(ref FrenchTranslations, identifier);
        if (language == ClientLanguage.Japanese) return GetTranslation(ref JapaneseTranslations, identifier);

        return GetTranslation(ref EnglishTranslations, identifier);
    }

    static string GetTranslation(ref Dictionary<string, string> translationDictionary, string identifier)
    {
        if (translationDictionary.TryGetValue(identifier, out string? translation)) return translation;
        if (EnglishTranslations.TryGetValue(identifier, out string? englishTranslations)) return englishTranslations;

        return identifier;
    }
}

internal enum AcquisitionDateLanguage
{
    Default,
    English,
    German,
    French,
    Japanese,
}
