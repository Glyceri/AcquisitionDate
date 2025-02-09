namespace AcquisitionDate.ImageDatabase.Structs;

internal readonly struct TextureRequestUser
{
    public readonly string Name;
    public readonly ushort Homeworld;

    public TextureRequestUser(string name, ushort homeworld)
    {
        Name = name;
        Homeworld = homeworld;
    }

    public bool Equals(TextureRequestUser other)
    {
        return other.Name == Name && other.Homeworld == Homeworld;
    }
}
