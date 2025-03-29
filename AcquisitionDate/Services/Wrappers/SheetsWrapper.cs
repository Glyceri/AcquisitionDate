using AcquiryDate.PetNicknames.Services.ServiceWrappers.Interfaces;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.Services.Structs;
using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AcquisitionDate.Services.Wrappers;

internal class SheetsWrapper : ISheets
{
    readonly List<IPetSheetData> petSheetCache = new List<IPetSheetData>();
    readonly List<IGlassesSheetData> glassesSheetCache = new List<IGlassesSheetData>();

    public Quest[] AllQuests => Quests.ToArray();
    public Item[] AllItems => Items.ToArray();
    public Achievement[] AllAchievements => Achievements.ToArray();
    public ContentFinderCondition[] AllContentFinderConditions => ContentFinderConditions.ToArray();
    public FishParameter[] AllFishies => Fishies.ToArray();
    public SpearfishingItem[] AllSpearFishies => SpearFishies.ToArray();
    public ClassJob[] AllClassJobs => ClassJobs.ToArray();
    public Emote[] AllEmotes => Emotes.ToArray();

    readonly ExcelSheet<World>                      Worlds;
    readonly ExcelSheet<Quest>                      Quests;
    readonly ExcelSheet<Achievement>                Achievements;
    readonly ExcelSheet<Item>                       Items;
    readonly ExcelSheet<Companion>                  Companions;
    readonly ExcelSheet<ContentFinderCondition>     ContentFinderConditions;
    readonly ExcelSheet<ClassJob>                   ClassJobs;
    readonly ExcelSheet<FishParameter>              Fishies;
    readonly ExcelSheet<SpearfishingItem>           SpearFishies;
    readonly ExcelSheet<Adventure>                  Vistas;
    readonly ExcelSheet<Mount>                      Mounts;
    readonly ExcelSheet<Glasses>                    Glasses;
    readonly ExcelSheet<Ornament>                   Ornaments;
    readonly ExcelSheet<Orchestrion>                Orchestrions;
    readonly ExcelSheet<Emote>                      Emotes;

    public SheetsWrapper()
    {
        Worlds                      = PluginHandlers.DataManager.GetExcelSheet<World>();
        Achievements                = PluginHandlers.DataManager.GetExcelSheet<Achievement>();
        Quests                      = PluginHandlers.DataManager.GetExcelSheet<Quest>();
        Items                       = PluginHandlers.DataManager.GetExcelSheet<Item>();
        Companions                  = PluginHandlers.DataManager.GetExcelSheet<Companion>();
        ContentFinderConditions     = PluginHandlers.DataManager.GetExcelSheet<ContentFinderCondition>();
        ClassJobs                   = PluginHandlers.DataManager.GetExcelSheet<ClassJob>();
        Fishies                     = PluginHandlers.DataManager.GetExcelSheet<FishParameter>();
        SpearFishies                = PluginHandlers.DataManager.GetExcelSheet<SpearfishingItem>();
        Vistas                      = PluginHandlers.DataManager.GetExcelSheet<Adventure>();
        Mounts                      = PluginHandlers.DataManager.GetExcelSheet<Mount>();
        Glasses                     = PluginHandlers.DataManager.GetExcelSheet<Glasses>();
        Ornaments                   = PluginHandlers.DataManager.GetExcelSheet<Ornament>();
        Orchestrions                = PluginHandlers.DataManager.GetExcelSheet<Orchestrion>();
        Emotes                      = PluginHandlers.DataManager.GetExcelSheet<Emote>();

        SetupSheetDataCache();
    }

    void SetupSheetDataCache()
    {
        foreach (Glasses glass in Glasses)
        {
            if(glass.RowId == 0) continue;

            uint rowId = glass.RowId;

            string singular = glass.Singular.ExtractText();

            if (singular.IsNullOrWhitespace()) continue;

            singular = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(singular);

            string plural = glass.Plural.ExtractText();

            sbyte pronoun = glass.Unknown_70_5;

            glassesSheetCache.Add(new GlassesSheetData((int)rowId, pronoun, singular, plural));
        }

        foreach (Companion companion in Companions)
        {
            if (!companion.Model.IsValid) continue;

            ModelChara? model = companion.Model.ValueNullable;
            if (model == null) continue;

            uint companionIndex = companion.RowId;
            int modelID = (int)model.Value.RowId;
            int legacyModelID = (int)model.Value.Model;

            if (legacyModelID == 0) continue;

            string singular = companion.Singular.ExtractText();

            if (singular.IsNullOrWhitespace()) continue;

            singular = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(singular);

            string plural = companion.Plural.ExtractText();
            uint icon = companion.Icon;

            sbyte pronoun = companion.Pronoun;

            uint raceID = companion.MinionRace.ValueNullable?.RowId ?? 0;
            string raceName = companion.MinionRace.ValueNullable?.Name.ExtractText() ?? string.Empty;
            string behaviourName = companion.Behavior.ValueNullable?.Name.ExtractText() ?? string.Empty;

            petSheetCache.Add(new PetSheetData(companionIndex, modelID, legacyModelID, icon, raceName, raceID, behaviourName, pronoun, singular, plural, singular, companionIndex));
        }
    }

    public Adventure? GetAdventureByIndex(uint index)
    {
        int curIndex = -1;
        foreach (Adventure adventure in Vistas)
        {
            curIndex++;
            if (curIndex != index) continue;

            return adventure;
        }

        return null;
    }

    public ClassJob? GetClassJob(uint id)
    {
        foreach (ClassJob classJob in ClassJobs)
        {
            if (classJob.RowId != id) continue;

            return classJob;
        }

        return null;
    }

    public ContentFinderCondition? GetContentFinderCondition(ushort id)
    {
        foreach (ContentFinderCondition contentFinderCondition in ContentFinderConditions)
        {
            if (contentFinderCondition.RowId != id) continue;

            return contentFinderCondition;
        }

        return null;
    }

    public IPetSheetData? GetCompanion(ushort ID)
    {
        foreach (IPetSheetData companion in petSheetCache)
        {
            if (companion.ID != ID) continue;

            return companion;
        }

        return null;
    }

    public IPetSheetData? GetCompanionByIcon(uint icon)
    {
        foreach (IPetSheetData companion in petSheetCache)
        {
            if (companion.Icon != icon) continue;

            return companion;
        }

        return null;
    }

    public IPetSheetData? GetCompanionByName(string name)
    {
        int sheetCount = petSheetCache.Count;
        for (int i = 0; i < sheetCount; i++)
        {
            IPetSheetData pet = petSheetCache[i];
            if (!pet.IsPet(name)) continue;
            return pet;
        }

        return null;
    }

    public Achievement? GetAchievement(string name)
    {
        string betterName = Regex.Replace(name, @"[\r\n]+", string.Empty);

        foreach (Achievement achievement in Achievements)
        {
            string achiName = achievement.Name.ExtractText();
            if (!string.Equals(achiName, betterName, System.StringComparison.InvariantCultureIgnoreCase)) continue;

            return achievement;
        }

        return null;
    }

    public Quest? GetQuest(string name, string journalGroup)
    {
        foreach (Quest quest in Quests)
        {
            try
            {
                string journalGenre = quest.JournalGenre.Value.Name.ExtractText();
                string questName = quest.Name.ExtractText();

                if (!name.Equals(questName, System.StringComparison.InvariantCultureIgnoreCase)) continue;
                if (!journalGroup.Equals(journalGenre, System.StringComparison.InvariantCultureIgnoreCase)) continue;

                return quest;
            }
            catch { continue; }
        }

        return null;
    }

    public Quest? GetQuest(uint id)
    {
        foreach (Quest quest in Quests)
        {
            if (quest.RowId != id) continue;

            return quest;
        }

        return null;
    }

    public Quest? GetQuest(string name)
    {
        foreach (Quest quest in Quests)
        {
            try
            {
                string questName = quest.Name.ExtractText();

                if (!name.Equals(questName, System.StringComparison.InvariantCultureIgnoreCase)) continue;

                return quest;
            }
            catch { continue; }
        }

        return null;
    }

    public uint? GetWorldID(string worldName)
    {
        foreach (World world in Worlds)
        {
            string name = world.InternalName.ExtractText();
            if (!name.Equals(worldName, System.StringComparison.InvariantCultureIgnoreCase)) continue;

            return world.RowId;
        }

        return null;
    }

    public string? GetWorldName(ushort worldID)
    {
        World? world = Worlds.GetRow(worldID);
        if (world == null) return null;

        return world.Value.InternalName.ExtractText();
    }

    public Mount? GetMountByName(string name)
    {
        foreach (Mount mount in Mounts)
        {
            string mName = mount.Singular.ExtractText();
            if (!mName.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)) continue;

            return mount;
        }

        return null;
    }

    public IGlassesSheetData? GetGlassesByName(string name)
    {
        foreach (IGlassesSheetData glass in glassesSheetCache)
        {
            if (!glass.IsGlass(name)) continue;

            return glass;
        }

        return null;
    }

    public Ornament? GetOrnamentByName(string name)
    {
        foreach (Ornament ornament in Ornaments)
        {
            string mName = ornament.Singular.ExtractText();
            if (!mName.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)) continue;

            return ornament;
        }

        return null;
    }

    public Orchestrion? GetOrchestrionByName(string name)
    {
        foreach (Orchestrion orchestrion in Orchestrions)
        {
            string mName = orchestrion.Name.ExtractText();
            if (!mName.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)) continue;

            return orchestrion;
        }

        return null;
    }

    public ContentFinderCondition? GetContentFinderConditionByName(string name)
    {
        foreach (ContentFinderCondition content in ContentFinderConditions)
        {
            string mName = content.Name.ExtractText();
            if (!mName.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)) continue;

            return content;
        }

        return null;
    }

    public Achievement? GetAchievementByID(uint id)
    {
        foreach (Achievement achievement in Achievements)
        {
            if (achievement.RowId != id) continue;

            return achievement;
        }

        return null;
    }

    public Mount? GetMountByID(uint id)
    {
        foreach (Mount mount in Mounts)
        {
            if (mount.RowId != id) continue;

            return mount;
        }

        return null;
    }

    public IGlassesSheetData? GetGlassesByID(uint id)
    {
        foreach (IGlassesSheetData glasses in glassesSheetCache)
        {
            if (glasses.Model != id) continue;

            return glasses;
        }

        return null;
    }

    public Ornament? GetOrnamentByID(uint id)
    {
        foreach (Ornament ornament in Ornaments)
        {
            if (ornament.RowId != id) continue;

            return ornament;
        }

        return null;
    }

    public Orchestrion? GetOrchestrionByID(uint id)
    {
        foreach (Orchestrion orchestrion in Orchestrions)
        {
            if (orchestrion.RowId != id) continue;

            return orchestrion;
        }

        return null;
    }

    public Emote? GetEmoteByID(uint id)
    {
        foreach (Emote emote in Emotes)
        {
            if (emote.RowId != id) continue;

            return emote;
        }

        return null;
    }
}
