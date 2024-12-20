using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Game;
using Dalamud.Utility;
using System.Collections.Generic;
using System.Linq;

namespace AcquisitionDate.Services.Structs;

internal struct GlassesSheetData : IGlassesSheetData
{
    public int Model { get; }
    public sbyte Pronoun { get; }
    public string[] Singular { get; }
    public string[] Plural { get; }
    public string BaseSingular { get; }
    public string BasePlural { get; }

    public GlassesSheetData(int model, sbyte pronoun, string singular, string plural)
    {
        Model = model;
        Pronoun = pronoun;

        ClientLanguage clientLanguage = PluginHandlers.ClientState.ClientLanguage;

        if (clientLanguage == ClientLanguage.German)
        {
            BaseSingular = GermanReplace(singular, Pronoun);
            BasePlural = GermanReplace(plural, Pronoun);
        }
        else
        {
            BaseSingular = singular;
            BasePlural = plural;
        }

        string[] starterList = [string.Empty];
        if (clientLanguage == ClientLanguage.German)
        {
            string[] sArr = GetGermanNamedList(starterList, singular);
            string[] pArr = GetGermanNamedList(starterList, plural);

            string[] tempSArr = sArr;
            string[] tempPArr = pArr;

            tempSArr = AddSuffixToArray(tempSArr, "s");
            tempPArr = AddSuffixToArray(tempPArr, "s");

            Singular = [.. sArr, .. tempSArr];
            Plural = [.. pArr, .. tempPArr];
        }
        else
        {
            Singular = AddSuffixToArray(starterList, singular);
            Plural = AddSuffixToArray(starterList, plural);
        }
    }

    readonly string[] GetGermanNamedList(string[] starterList, string suffix)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            list.AddRange(AddSuffixToArray(starterList, GermanReplace(suffix, (sbyte)i)));
        }
        return list.ToArray();
    }

    readonly string[] AddSuffixToArray(string[] baseArray, string suffix)
    {
        string[] newBaseArray = baseArray.ToArray();
        for (int i = 0; i < newBaseArray.Length; i++)
        {
            newBaseArray[i] = newBaseArray[i] + suffix;
        }
        return newBaseArray;
    }

    readonly string GermanReplace(string baseString, sbyte pronoun)
    {
        try
        {
            baseString = baseString.Replace("[p]", "", System.StringComparison.InvariantCultureIgnoreCase);
            baseString = baseString.Replace("[a]", checked(pronounList[pronoun]), System.StringComparison.InvariantCultureIgnoreCase);
        }
        catch { }

        return baseString;
    }

    public bool IsGlass(string name)
    {
        if (name.IsNullOrWhitespace()) return false;
        return string.Equals(BaseSingular, name, System.StringComparison.InvariantCultureIgnoreCase);
    }

    readonly string[] pronounList = ["er", "e", "es", "en"];
}
