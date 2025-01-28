using AcquisitionDate.Parser.Elements;
using System;

namespace AcquisitionDate.Parser.Interfaces;

internal interface IAcquisitionParser : IDisposable
{
    LodestonePageLanguageParser LodestonePageLanguageParser { get; }
    ListPageCountParser PageCountParser { get; }
    AchievementListPageCountParser AchievementListPageCountParser { get; }
    AchievementElementParser AchievementElementParser { get; }
    AchievementListParser AchievementListParser { get; }
    LodestoneIDParser LodestoneIDParser { get; }
    QuestListParser QuestListParser { get; }
    QuestDataParser QuestDataParser { get; }
}
