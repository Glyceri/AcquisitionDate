using AcquisitionDate.Parser.Elements;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Parser;

internal class AcquisitionParser : IAcquisitionParser
{
    public ListPageCountParser PageCountParser { get; init; }
    public AchievementListPageCountParser AchievementListPageCountParser { get; init; }
    public AchievementElementParser AchievementElementParser { get; init; }
    public AchievementListParser AchievementListParser { get; init; }
    public LodestoneIDParser LodestoneIDParser { get; init; }
    public LodestonePageLanguageParser LodestonePageLanguageParser { get; init; }
    public QuestListParser QuestListParser { get; init; }
    public QuestDataParser QuestDataParser { get; init; }
    public ItemPageListParser ItemPageListParser { get; init; }
    public ItemPageDataParser ItemPageDataParser { get; init; }

    public AcquisitionParser(ISheets sheets)
    {
        LodestonePageLanguageParser = new LodestonePageLanguageParser();
        PageCountParser = new ListPageCountParser();
        AchievementListPageCountParser = new AchievementListPageCountParser(PageCountParser);
        AchievementListParser = new AchievementListParser();
        AchievementElementParser = new AchievementElementParser();
        LodestoneIDParser = new LodestoneIDParser(sheets);
        QuestListParser = new QuestListParser();
        QuestDataParser = new QuestDataParser(sheets);
        ItemPageListParser = new ItemPageListParser();
        ItemPageDataParser = new ItemPageDataParser();
    }

    public void Dispose()
    {
        QuestDataParser.Dispose();
    }
}
