using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The shades the import pre-colours the Ridleys' sheets and rooms in (#1512): the colours each encounter loads for its
/// door, container and Baby, the eye and body fades, health bands, the retreat, the getaway floor's zoom, Norfair Ridley's
/// blacked-out arrival and his arena's reveal, and the self-destruct warning's alarm, which only the warning's own written
/// pictures show (#1544).
/// </summary>
public static class RidleyShades
{
    /// <summary>The Ceres door's, container's and Baby's colours Ceres Ridley's initialization loads.</summary>
    public static PictureShade CeresStart { get; } = new("ridley-ceres-start", 0);

    /// <summary>Ridley's own colours cleared to black as Ceres Ridley's initialization loads them.</summary>
    public static PictureShade CeresHidden { get; } = new("ridley-ceres-hidden", 0);

    /// <summary>Ceres Ridley's eyes at fade shade <paramref name="shade"/>, zero through fifteen.</summary>
    public static PictureShade EyeFade(int shade) => new("ridley-eye-fade", shade);

    /// <summary>Ceres Ridley's body at fade shade <paramref name="shade"/>, zero through fifteen.</summary>
    public static PictureShade BodyFade(int shade) => new("ridley-body-fade", shade);

    /// <summary>Ceres Ridley's colours in health band <paramref name="band"/>, zero through two.</summary>
    public static PictureShade Health(int band) => new("ridley-health", band);

    /// <summary>The colours his retreat gives the room's scenery that shares them.</summary>
    public static PictureShade RetreatShared { get; } = new("ridley-retreat-shared", 0);

    /// <summary>The room's colours as he retreats.</summary>
    public static PictureShade RetreatRoom { get; } = new("ridley-retreat-room", 0);

    /// <summary>The Baby's and its container's <paramref name="shade"/>.</summary>
    public static PictureShade Baby(CeresBabyShade shade) => new($"ridley-baby-{shade}", 0);

    /// <summary>The warning's EMERGENCY title at self-destruct alarm step <paramref name="step"/>, zero through fifteen.</summary>
    public static PictureShade Alarm(int step) => new("ridley-alarm", step);

    /// <summary>The getaway floor at zoom step <paramref name="step"/>, zero through eight.</summary>
    public static PictureShade FloorZoom(int step) => new("ridley-floor-zoom", step);

    /// <summary>The colours Norfair Ridley's initialization loads.</summary>
    public static PictureShade NorfairStart { get; } = new("ridley-norfair-start", 0);

    /// <summary>Norfair Ridley's body and tail cleared to black as he arrives.</summary>
    public static PictureShade NorfairHidden { get; } = new("ridley-norfair-hidden", 0);

    /// <summary>His arena at reveal step <paramref name="step"/>.</summary>
    public static PictureShade NorfairReveal(int step) => new("ridley-norfair-reveal", step);
}
