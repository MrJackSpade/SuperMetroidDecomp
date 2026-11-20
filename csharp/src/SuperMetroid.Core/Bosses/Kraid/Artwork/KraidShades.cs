using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The shades the import pre-colours Kraid's pictures in (#1512): his health bands, his death colours and rocks, and what
/// his room's backdrop settles on; its fades between them are typed fades the encounter publishes (#2215).
/// </summary>
public static class KraidShades
{
    /// <summary>
    /// Kraid's colours for health band <paramref name="band"/>: his body's on the background and his sprite parts' (#1512),
    /// named by the band (#2297).
    /// </summary>
    public static PictureShade Health(KraidHealthBand band) => new($"kraid-health-{Ensure.NotNull(band).Key}", 0);

    /// <summary>His hurt flash's colours, on his body and his sprite parts, which cover his band's (#2297).</summary>
    public static PictureShade HurtFlash { get; } = new("kraid-hurt-flash", 0);

    /// <summary>His body's colours as he dies, with his arm drawn into them.</summary>
    public static PictureShade DeathArm { get; } = new("kraid-death-arm", 0);

    /// <summary>The shade his ceiling rocks and spat rocks are drawn in.</summary>
    public static PictureShade Rocks { get; } = new("kraid-rocks", 0);

    /// <summary>Kraid's backdrop colours, as his defeated room shows them.</summary>
    public static PictureShade Backdrop { get; } = new("kraid-backdrop", 0);

    /// <summary>The backdrop black, as his death fade leaves it.</summary>
    public static PictureShade BackdropBlack { get; } = new("kraid-backdrop-black", 0);
}
