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
    public readonly SerializableList FramersList;
    public readonly SerializableList SecretRecipeBookList; //Idk what this is tbf, but im tracking it c:
    public readonly SerializableList BuddyEquipList;
    public readonly SerializableList UnlockLinkList; // Riding maps, blu totems, emotes/dances, hairstyles
    public readonly SerializableList FolkloreTomeList;

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
        SerializableList sightList,
        SerializableList framersList,
        SerializableList secretRecipeBookList,
        SerializableList buddyEquipList,
        SerializableList unlockLinkList,
        SerializableList folkloreTomeList)
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
        FramersList = framersList;
        SecretRecipeBookList = secretRecipeBookList;
        BuddyEquipList = buddyEquipList;
        UnlockLinkList = unlockLinkList;
        FolkloreTomeList = folkloreTomeList;
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
        FramersList = new SerializableList(data.FramersList);
        SecretRecipeBookList = new SerializableList(data.SecretRecipeBookList);
        BuddyEquipList = new SerializableList(data.BuddyEquipList);
        UnlockLinkList = new SerializableList(data.UnlockLinkList);
        FolkloreTomeList = new SerializableList(data.FolkloreTomeList);
    }
}
