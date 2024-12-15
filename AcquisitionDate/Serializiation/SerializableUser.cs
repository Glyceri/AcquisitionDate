using AcquisitionDate.Database.Interfaces;
using Newtonsoft.Json;
using System;

namespace AcquisitionDate.Serializiation;

[Serializable]
internal class SerializableUser
{
    public readonly ulong ContentID;
    public readonly ulong? LodestoneID;
    public readonly string Name;
    public readonly ushort Homeworld;

    public readonly SerializableList AchievementList;
    public readonly SerializableList QuestList;
    public readonly SerializableList MinionList;
    public readonly SerializableList MountList;
    public readonly SerializableList FacewearList;
    public readonly SerializableList OrchestrionList;
    public readonly SerializableList ClassLVLList;
    public readonly SerializableList CardList;
    public readonly SerializableList FashionList;
    public readonly SerializableList DutyList;
    public readonly SerializableList FishingList;
    public readonly SerializableList SightList;

    [JsonConstructor]
    public SerializableUser(
        ulong contentId, 
        ulong? lodestoneID, 
        string name, 
        ushort homeworld, 
        SerializableList achievementList, 
        SerializableList questList, 
        SerializableList minionList, 
        SerializableList mountList,
        SerializableList facewearList,
        SerializableList orchestrionList,
        SerializableList classLVLList,
        SerializableList cardList,
        SerializableList fashionList,
        SerializableList dutyList,
        SerializableList fishingList,
        SerializableList sightList)
    {
        ContentID = contentId;
        LodestoneID = lodestoneID;
        Name = name;
        Homeworld = homeworld;

        AchievementList = achievementList;
        QuestList = questList;
        MinionList = minionList;
        MountList = mountList;
        FacewearList = facewearList;
        OrchestrionList = orchestrionList;
        ClassLVLList = classLVLList;
        CardList = cardList;
        FashionList = fashionList;
        DutyList = dutyList;
        FishingList = fishingList;
        SightList = sightList;
    }

    public SerializableUser(IDatableData data)
    {
        ContentID = data.ContentID;
        LodestoneID = data.LodestoneID;
        Name = data.Name;
        Homeworld = data.Homeworld;

        AchievementList = new SerializableList(data.AchievementList);
        QuestList = new SerializableList(data.QuestList);
        MinionList = new SerializableList(data.MinionList);
        MountList = new SerializableList(data.MountList);

        FacewearList = new SerializableList(data.FacewearList);
        OrchestrionList = new SerializableList(data.OrchestrionList);
        ClassLVLList = new SerializableList(data.ClassLVLList);
        CardList = new SerializableList(data.CardList);
        FashionList = new SerializableList(data.FashionList);
        DutyList = new SerializableList(data.DutyList);
        FishingList = new SerializableList(data.FishingList);
        SightList = new SerializableList(data.SightList);
    }
}
