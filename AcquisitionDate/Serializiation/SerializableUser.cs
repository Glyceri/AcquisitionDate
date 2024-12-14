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

    [JsonConstructor]
    public SerializableUser(ulong contentId, ulong? lodestoneID, string name, ushort homeworld, SerializableList achievementList, SerializableList questList)
    {
        ContentID = contentId;
        LodestoneID = lodestoneID;
        Name = name;
        Homeworld = homeworld;

        AchievementList = achievementList;
        QuestList = questList;
    }

    public SerializableUser(IDatableData data)
    {
        ContentID = data.ContentID;
        LodestoneID = data.LodestoneID;
        Name = data.Name;
        Homeworld = data.Homeworld;

        AchievementList = new SerializableList(data.AchievementList);
        QuestList = new SerializableList(data.QuestList);
    }
}
