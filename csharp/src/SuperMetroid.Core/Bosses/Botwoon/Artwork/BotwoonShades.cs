using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>The shades the import pre-colours Botwoon's head and body sheets in (#1512): each of his eight health bands' colours.</summary>
public static class BotwoonShades
{
    /// <summary>His colours in health band <paramref name="band"/>, zero through seven.</summary>
    public static PictureShade Health(int band) => new("botwoon-health", band);
}
