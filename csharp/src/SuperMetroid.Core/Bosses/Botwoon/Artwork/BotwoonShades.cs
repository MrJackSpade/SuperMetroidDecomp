using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>The shades the import pre-colours Botwoon's head and body sheets in (#1512): each of his eight health bands' colours.</summary>
public static partial class BotwoonShades
{
    /// <summary>His colours in <paramref name="band"/> (#2266).</summary>
    public static PictureShade Health(BotwoonHealthBand band) => new($"botwoon-health-{Ensure.NotNull(band).Key}", 0);
}
