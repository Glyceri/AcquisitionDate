using AcquisitionDate.Core.Handlers;
using Dalamud.Game;
using System.Collections.Generic;

namespace PetRenamer.PetNicknames.TranslatorSystem;

// Trying to stay away from statics, but in this case it just made MUCH more sense.
internal static class Translator
{
    static Dictionary<string, string> EnglishTranslations = new Dictionary<string, string>()
    {
        { "...", "..." },
        { "AchievedOn", "Acquired:" },
        { "NoDate", "Date unavailable." },
        { "NotAccurate", "May not be accurate." },
    };

    static Dictionary<string, string> GermanTranslations = new Dictionary<string, string>()
    {
        { "AchievedOn", "Erhalten:" },
        { "NoDate", "Datum nicht verfügbar." },
        { "NotAccurate", "Möglicherweise nicht genau." },
    };

    static Dictionary<string, string> FrenchTranslations = new Dictionary<string, string>()
    {
        { "AchievedOn", "Date d'obtention :" },
        { "NoDate", "Date indisponible." },
        { "NotAccurate", "Ce n'est peut-être pas exact." },
    };

    static Dictionary<string, string> JapaneseTranslations = new Dictionary<string, string>()
    {
        { "AchievedOn", "修得日:" },
        { "NoDate", "日付が利用できません。" },
        { "NotAccurate", "正確ではない可能性があります。" },
    };


    internal static string GetLine(string identifier)
    {
        ClientLanguage language = PluginHandlers.ClientState.ClientLanguage;

        AcquisitionDateLanguage Language = AcquisitionDateLanguage.Default;

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
