using AcquisitionDate.Parser.Elements;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Parser;

internal class AcquisitionParser : IAcquisitionParser
{
    public ListPageCountParser              PageCountParser                 { get; }
    public AchievementListPageCountParser   AchievementListPageCountParser  { get; }
    public AchievementElementParser         AchievementElementParser        { get; }
    public AchievementListParser            AchievementListParser           { get; }
    public LodestoneIDParser                LodestoneIDParser               { get; }
    public LodestonePageLanguageParser      LodestonePageLanguageParser     { get; }
    public QuestListParser                  QuestListParser                 { get; }
    public QuestDataParser                  QuestDataParser                 { get; }
    public ItemPageListParser               ItemPageListParser              { get; }
    public ItemPageDataParser               ItemPageDataParser              { get; }

    public AcquisitionParser(ISheets sheets)
    {
        LodestonePageLanguageParser     = new LodestonePageLanguageParser();
        PageCountParser                 = new ListPageCountParser();
        AchievementListPageCountParser  = new AchievementListPageCountParser(PageCountParser);
        AchievementListParser           = new AchievementListParser();
        AchievementElementParser        = new AchievementElementParser();
        LodestoneIDParser               = new LodestoneIDParser(sheets);
        QuestListParser                 = new QuestListParser();
        QuestDataParser                 = new QuestDataParser(sheets);
        ItemPageListParser              = new ItemPageListParser();
        ItemPageDataParser              = new ItemPageDataParser();
    }

    public void Dispose()
    {
        QuestDataParser.Dispose();
    }
}
