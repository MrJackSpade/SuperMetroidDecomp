using System.Collections.Frozen;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The shades the import pre-colours the Ridleys' sheets and rooms in (#1512): the look of each encounter's door,
/// container and Baby, the eye and body fades, health bands, the retreat, the getaway floor's zoom, Norfair Ridley's
/// blacked-out arrival and his arena's reveal, and the self-destruct warning's alarm, which only the warning's own
/// written pictures show (#1544).
/// </summary>
public static class RidleyShades
{
    /// <summary>
    /// The installed name of each of the Ceres Baby's shades (#2409), written out so renaming a shade's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly IdentityTable<CeresBabyShade, string> BabyKeys =
        new(new Dictionary<CeresBabyShade, string>
        {
            [CeresBabyShade.Resting] = "ridley-baby-Resting",
            [CeresBabyShade.HorizontalSquish] = "ridley-baby-HorizontalSquish",
            [CeresBabyShade.Round] = "ridley-baby-Round",
            [CeresBabyShade.VerticalSquish] = "ridley-baby-VerticalSquish",
        });

    /// <summary>The Ceres door, container and Baby as Ceres Ridley's fight starts.</summary>
    public static PictureShade CeresStart { get; } = new("ridley-ceres-start", 0);

    /// <summary>Ridley shown black before he appears in Ceres.</summary>
    public static PictureShade CeresHidden { get; } = new("ridley-ceres-hidden", 0);

    /// <summary>
    /// The colours <paramref name="image"/> of one of the encounters' authored runs shows (#2316): his eyes' or his
    /// body's fade in, his arena's reveal, the self-destruct alarm or the getaway floor's zoom.
    /// </summary>
    public static PictureShade Fade(RidleyFadeImage image) => new(Ensure.NotNull(image.Fade).Key, image.Step);

    /// <summary>Ridley's own colours in health colouring <paramref name="shade"/> (#2242), which both encounters show.</summary>
    public static PictureShade Health(RidleyHealthShade shade) => new($"ridley-health-{Ensure.NotNull(shade).Key}", 0);

    /// <summary>The colours his retreat gives the room's scenery that shares them.</summary>
    public static PictureShade RetreatShared { get; } = new("ridley-retreat-shared", 0);

    /// <summary>The room's colours as he retreats.</summary>
    public static PictureShade RetreatRoom { get; } = new("ridley-retreat-room", 0);

    /// <summary>The Baby's and its container's <paramref name="shade"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The shade has no installed name.</exception>
    public static PictureShade Baby(CeresBabyShade shade) => new(BabyKeys[shade], 0);

    /// <summary>The look of Norfair Ridley's arena as his fight starts.</summary>
    public static PictureShade NorfairStart { get; } = new("ridley-norfair-start", 0);

    /// <summary>Norfair Ridley's body and tail cleared to black as he arrives.</summary>
    public static PictureShade NorfairHidden { get; } = new("ridley-norfair-hidden", 0);
}
