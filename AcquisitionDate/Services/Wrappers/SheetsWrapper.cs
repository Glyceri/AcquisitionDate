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

    public Quest[] AllQuests => quests.ToArray();
    public Item[] AllItems => items.ToArray();
    public Achievement[] AllAchievements => achievements.ToArray();
    public ContentFinderCondition[] AllContentFinderConditions => contentFinderConditions.ToArray();
    public FishParameter[] AllFishies => fishies.ToArray();
    public SpearfishingItem[] AllSpearFishies => spearFishies.ToArray();
    public ClassJob[] AllClassJobs => classJobs.ToArray();
    public Emote[] AllEmotes => emotes.ToArray();

    readonly ExcelSheet<World> worlds;
    readonly ExcelSheet<Quest> quests;
    readonly ExcelSheet<Achievement> achievements;
    readonly ExcelSheet<Item> items;
    readonly ExcelSheet<Companion> companions;
    readonly ExcelSheet<ContentFinderCondition> contentFinderConditions;
    readonly ExcelSheet<ClassJob> classJobs;
    readonly ExcelSheet<FishParameter> fishies;
    readonly ExcelSheet<SpearfishingItem> spearFishies;
    readonly ExcelSheet<Adventure> vistas;
    readonly ExcelSheet<Mount> mounts;
    readonly ExcelSheet<Glasses> glasses;
    readonly ExcelSheet<Ornament> ornaments;
    readonly ExcelSheet<Orchestrion> orchestrions;
    readonly ExcelSheet<Emote> emotes;

    public SheetsWrapper()
    {
        worlds                      = PluginHandlers.DataManager.GetExcelSheet<World>();
        achievements                = PluginHandlers.DataManager.GetExcelSheet<Achievement>();
        quests                      = PluginHandlers.DataManager.GetExcelSheet<Quest>();
        items                       = PluginHandlers.DataManager.GetExcelSheet<Item>();
        companions                  = PluginHandlers.DataManager.GetExcelSheet<Companion>();
        contentFinderConditions     = PluginHandlers.DataManager.GetExcelSheet<ContentFinderCondition>();
        classJobs                   = PluginHandlers.DataManager.GetExcelSheet<ClassJob>();
        fishies                     = PluginHandlers.DataManager.GetExcelSheet<FishParameter>();
        spearFishies                = PluginHandlers.DataManager.GetExcelSheet<SpearfishingItem>();
        vistas                      = PluginHandlers.DataManager.GetExcelSheet<Adventure>();
        mounts                      = PluginHandlers.DataManager.GetExcelSheet<Mount>();
        glasses                     = PluginHandlers.DataManager.GetExcelSheet<Glasses>();
        ornaments                   = PluginHandlers.DataManager.GetExcelSheet<Ornament>();
        orchestrions                = PluginHandlers.DataManager.GetExcelSheet<Orchestrion>();
        emotes                      = PluginHandlers.DataManager.GetExcelSheet<Emote>();

        SetupSheetDataCache();
    }

    void SetupSheetDataCache()
    {
        foreach (Glasses glass in glasses)
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

        foreach (Companion companion in companions)
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
        foreach (Adventure adventure in vistas)
        {
            curIndex++;
            if (curIndex != index) continue;

            return adventure;
        }

        return null;
    }

    public ClassJob? GetClassJob(uint id)
    {
        foreach (ClassJob classJob in classJobs)
        {
            if (classJob.RowId != id) continue;

            return classJob;
        }

        return null;
    }

    public ContentFinderCondition? GetContentFinderCondition(ushort id)
    {
        foreach (ContentFinderCondition contentFinderCondition in contentFinderConditions)
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

        foreach (Achievement achievement in achievements)
        {
            string achiName = achievement.Name.ExtractText();
            if (!string.Equals(achiName, betterName, System.StringComparison.InvariantCultureIgnoreCase)) continue;

            return achievement;
        }

        return null;
    }

    public Quest? GetQuest(string name, string journalGroup)
    {
        foreach (Quest quest in quests)
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
        foreach (Quest quest in quests)
        {
            if (quest.RowId != id) continue;

            return quest;
        }

        return null;
    }

    public Quest? GetQuest(string name)
    {
        foreach (Quest quest in quests)
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
        foreach (World world in worlds)
        {
            string name = world.InternalName.ExtractText();
            if (!name.Equals(worldName, System.StringComparison.InvariantCultureIgnoreCase)) continue;

            return world.RowId;
        }

        return null;
    }

    public string? GetWorldName(ushort worldID)
    {
        World? world = worlds.GetRow(worldID);
        if (world == null) return null;

        return world.Value.InternalName.ExtractText();
    }

    public Mount? GetMountByName(string name)
    {
        foreach (Mount mount in mounts)
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
        foreach (Ornament ornament in ornaments)
        {
            string mName = ornament.Singular.ExtractText();
            if (!mName.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)) continue;

            return ornament;
        }

        return null;
    }

    public Orchestrion? GetOrchestrionByName(string name)
    {
        foreach (Orchestrion orchestrion in orchestrions)
        {
            string mName = orchestrion.Name.ExtractText();
            if (!mName.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)) continue;

            return orchestrion;
        }

        return null;
    }

    public ContentFinderCondition? GetContentFinderConditionByName(string name)
    {
        foreach (ContentFinderCondition content in contentFinderConditions)
        {
            string mName = content.Name.ExtractText();
            if (!mName.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)) continue;

            return content;
        }

        return null;
    }

    public Achievement? GetAchievementByID(uint id)
    {
        foreach (Achievement achievement in achievements)
        {
            if (achievement.RowId != id) continue;

            return achievement;
        }

        return null;
    }

    public Mount? GetMountByID(uint id)
    {
        foreach (Mount mount in mounts)
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
        foreach (Ornament ornament in ornaments)
        {
            if (ornament.RowId != id) continue;

            return ornament;
        }

        return null;
    }

    public Orchestrion? GetOrchestrionByID(uint id)
    {
        foreach (Orchestrion orchestrion in orchestrions)
        {
            if (orchestrion.RowId != id) continue;

            return orchestrion;
        }

        return null;
    }

    public Emote? GetEmoteByID(uint id)
    {
        foreach (Emote emote in emotes)
        {
            if (emote.RowId != id) continue;

            return emote;
        }

        return null;
    }
}
