﻿using AcquiryDate.PetNicknames.Services.ServiceWrappers.Interfaces;
using AcquisitionDate.Core.Handlers;
using Dalamud.Game;
using Dalamud.Utility;
using System.Collections.Generic;
using System.Linq;

internal struct PetSheetData : IPetSheetData
{
    public uint ID { get; private set; }
    public int Model { get; private set; }
    public uint Icon { get; private set; }
    public sbyte Pronoun { get; private set; }

    public string[] Singular { get; private set; }
    public string[] Plural { get; private set; }

    public string BaseSingular { get; private set; }
    public string BasePlural { get; private set; }

    public string ActionName { get; private set; } = "";
    public uint ActionID { get; private set; } = 0;

    public int LegacyModelID { get; private set; }

    public uint RaceID { get; private set; } = 0;
    public string? RaceName { get; private set; } = null;
    public string? BehaviourName { get; private set; } = null;
    public uint FootstepIcon { get; private set; } = 0;


    public PetSheetData(uint ID, int Model, int legacyModelID, uint Icon, string? raceName, uint raceID, string? behaviourName, sbyte Pronoun, string Singular, string Plural, string actionName, uint actionID)
        : this(ID, Model, legacyModelID, Icon, Pronoun, Singular, Plural, actionName, actionID)
    {
        this.RaceName = raceName;
        this.BehaviourName = behaviourName;
        this.RaceID = raceID;
    }

    public PetSheetData(uint ID, int Model, int legacyModelID, uint Icon, sbyte Pronoun, string Singular, string Plural, string actionName, uint actionID)
        : this(ID, Model, Icon, Pronoun, Singular, Plural)
    {
        ActionID = actionID;
        ActionName = actionName;
        LegacyModelID = legacyModelID;
    }

    public PetSheetData(uint ID, int model, uint icon, sbyte pronoun, string singular, string plural)
    {
        this.ID = ID;
        Model = model;
        Icon = icon;
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

        string[] starterList = GetList(clientLanguage);
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

    readonly string[] pronounList = ["er", "e", "es", "en"];
    readonly string[] englishStarters = ["the ", string.Empty];
    readonly string[] germanStarters = ["den ", "des ", "dem ", "die ", "der ", "das ", string.Empty];
    readonly string[] frenchStarters = ["le ", "la ", "l'", string.Empty];
    readonly string[] japaneseStarters = [string.Empty];

    readonly string[] GetList(ClientLanguage clientLanguage) => clientLanguage switch
    {
        ClientLanguage.English => englishStarters,
        ClientLanguage.German => germanStarters,
        ClientLanguage.French => frenchStarters,
        ClientLanguage.Japanese => japaneseStarters,
        _ => englishStarters
    };

    public readonly bool IsPet(string name)
    {
        if (name.IsNullOrWhitespace()) return false;
        return string.Equals(BaseSingular, name, System.StringComparison.InvariantCultureIgnoreCase);
    }

    public readonly bool IsAction(string action)
    {
        if (action.IsNullOrWhitespace()) return false;
        return string.Equals(ActionName, action, System.StringComparison.InvariantCultureIgnoreCase);
    }

    public readonly bool Contains(string line)
    {
        for (int i = 0; i < Singular.Length; i++)
            if (line.Contains(Singular[i], System.StringComparison.InvariantCultureIgnoreCase))
                return true;

        for (int i = 0; i < Plural.Length; i++)
            if (line.Contains(Plural[i], System.StringComparison.InvariantCultureIgnoreCase))
                return true;

        return false;
    }

    public readonly string LongestIdentifier()
    {
        string curIdentifier = BaseSingular;
        if (curIdentifier.Length < BasePlural.Length) curIdentifier = BasePlural;
        if (curIdentifier.Length < ActionName.Length) curIdentifier = ActionName;
        return curIdentifier;
    }

    public readonly bool IsAction(uint action) => ActionID == action;
}