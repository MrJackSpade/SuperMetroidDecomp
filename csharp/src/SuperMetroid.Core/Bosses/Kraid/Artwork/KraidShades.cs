using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The shades the import pre-colours Kraid's pictures in (#1512): his health bands, his death colours and rocks, and what
/// his room's backdrop settles on; its fades between them are typed fades the encounter publishes (#2215).
/// </summary>
public static class KraidShades
{
    /// <summary>
    /// Kraid's colours for <paramref name="band"/>, a health band or the hurt flash: his body's on the background and his
    /// sprite parts' (#1512). The shade's step is the band's place among his health colours' groups.
    /// </summary>
    public static PictureShade Health(KraidColourBand band) => new("kraid-health", band.ColourGroup);

    /// <summary>His body's colours as he dies, with his arm drawn into them.</summary>
    public static PictureShade DeathArm { get; } = new("kraid-death-arm", 0);

    /// <summary>The colours his ceiling rocks and spat rocks draw in, which his initialization loads.</summary>
    public static PictureShade Rocks { get; } = new("kraid-rocks", 0);

    /// <summary>Kraid's backdrop colours, as his defeated room shows them.</summary>
    public static PictureShade Backdrop { get; } = new("kraid-backdrop", 0);

    /// <summary>The backdrop black, as his death fade leaves it.</summary>
    public static PictureShade BackdropBlack { get; } = new("kraid-backdrop-black", 0);
}
