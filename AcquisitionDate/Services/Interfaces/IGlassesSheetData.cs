namespace AcquisitionDate.Services.Interfaces;

internal interface IGlassesSheetData
{
    int Model { get; }
    sbyte Pronoun { get; }

    string[] Singular { get; }
    public string[] Plural { get; }

    string BaseSingular { get; }
    string BasePlural { get; }

    bool IsGlass(string name);
}
