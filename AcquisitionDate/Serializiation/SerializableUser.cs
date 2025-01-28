using AcquisitionDate.Database.Interfaces;
using Newtonsoft.Json;
using System;

namespace AcquisitionDate.Serializiation;

[Serializable]
internal class SerializableUser
{
    [JsonProperty] public ulong ContentID                       { get; set; }
    [JsonProperty] public ulong? LodestoneID                    { get; set; }
    [JsonProperty] public string Name                           { get; set; }
    [JsonProperty] public ushort Homeworld                      { get; set; }

    [JsonProperty] public SerializableList AchievementList      { get; set; }
    [JsonProperty] public SerializableList QuestList            { get; set; }
    [JsonProperty] public SerializableList MinionList           { get; set; }
    [JsonProperty] public SerializableList MountList            { get; set; }
    [JsonProperty] public SerializableList FacewearList         { get; set; }
    [JsonProperty] public SerializableList OrchestrionList      { get; set; }
    [JsonProperty] public SerializableList ClassLVLList         { get; set; }
    [JsonProperty] public SerializableList CardList             { get; set; }
    [JsonProperty] public SerializableList FashionList          { get; set; }
    [JsonProperty] public SerializableList DutyList             { get; set; }
    [JsonProperty] public SerializableList FishingList          { get; set; }
    [JsonProperty] public SerializableList SightList            { get; set; }
    [JsonProperty] public SerializableList FramersList          { get; set; }
    [JsonProperty] public SerializableList SecretRecipeBookList { get; set; } //Idk what this is tbf, but im tracking it c:
    [JsonProperty] public SerializableList BuddyEquipList       { get; set; }
    [JsonProperty] public SerializableList UnlockLinkList       { get; set; } // Riding maps, blu totems, emotes/dances, hairstyles
    [JsonProperty] public SerializableList FolkloreTomeList     { get; set; }
    [JsonProperty] public SerializableList EmoteList            { get; set; }

    public SerializableUser()
    {
        ContentID               = 0;
        LodestoneID             = null;
        Name                    = string.Empty;
        Homeworld               = 0;

        AchievementList         = SerializableList.CreateEmpty();
        QuestList               = SerializableList.CreateEmpty();
        MinionList              = SerializableList.CreateEmpty();
        MountList               = SerializableList.CreateEmpty();
        FacewearList            = SerializableList.CreateEmpty();
        OrchestrionList         = SerializableList.CreateEmpty();
        ClassLVLList            = SerializableList.CreateEmpty();
        CardList                = SerializableList.CreateEmpty();
        FashionList             = SerializableList.CreateEmpty();
        DutyList                = SerializableList.CreateEmpty();
        FishingList             = SerializableList.CreateEmpty();
        SightList               = SerializableList.CreateEmpty();
        FramersList             = SerializableList.CreateEmpty();
        SecretRecipeBookList    = SerializableList.CreateEmpty();
        BuddyEquipList          = SerializableList.CreateEmpty();
        UnlockLinkList          = SerializableList.CreateEmpty();
        FolkloreTomeList        = SerializableList.CreateEmpty();
        EmoteList               = SerializableList.CreateEmpty();
    }

    public SerializableUser(IDatableData data)
    {
        ContentID               = data.ContentID;
        LodestoneID             = data.LodestoneID;
        Name                    = data.Name;
        Homeworld               = data.Homeworld;

        AchievementList         = data.AchievementList      .Serialise();
        QuestList               = data.QuestList            .Serialise();
        MinionList              = data.MinionList           .Serialise();
        MountList               = data.MountList            .Serialise();

        FacewearList            = data.FacewearList         .Serialise();
        OrchestrionList         = data.OrchestrionList      .Serialise();
        ClassLVLList            = data.ClassLVLList         .Serialise();
        CardList                = data.CardList             .Serialise();
        FashionList             = data.FashionList          .Serialise();
        DutyList                = data.DutyList             .Serialise();
        FishingList             = data.FishingList          .Serialise();
        SightList               = data.SightList            .Serialise();
        FramersList             = data.FramersList          .Serialise();
        SecretRecipeBookList    = data.SecretRecipeBookList .Serialise();
        BuddyEquipList          = data.BuddyEquipList       .Serialise();
        UnlockLinkList          = data.UnlockLinkList       .Serialise();
        FolkloreTomeList        = data.FolkloreTomeList     .Serialise();
        EmoteList               = data.EmoteList            .Serialise();
    }
}
