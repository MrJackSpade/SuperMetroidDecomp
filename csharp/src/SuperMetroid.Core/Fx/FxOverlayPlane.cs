namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room effect the effect overlay shows (#1348, #1466, #1514, #1601): the effect whose artwork it shows, or none, in
/// which case the overlay shows nothing. A liquid shows its surface strip on its surface line and its body below; an
/// atmosphere shows its picture repeating, scrolled (#1706). The HUD is not on the overlay: it is its own display layer.
/// </summary>
public sealed class FxOverlayPlane
{
    /// <summary>The overlay of a room whose effect shows nothing on it.</summary>
    public static FxOverlayPlane Cleared { get; } = new(null);

    /// <summary>The overlay showing each effect, shared so a frame showing the same effect reuses its resolved picture.</summary>
    private static readonly Dictionary<RoomFxType, FxOverlayPlane> ShowingEach =
        Enum.GetValues<RoomFxType>().ToDictionary(type => type, type => new FxOverlayPlane(type));

    /// <summary>Takes the effect whose pictures the overlay shows.</summary>
    private FxOverlayPlane(RoomFxType? effect) => Effect = effect;

    /// <summary>The overlay showing the pictures of <paramref name="effect"/>, or <see cref="Cleared"/> for none.</summary>
    /// <param name="effect">The effect whose pictures the overlay shows, or null.</param>
    public static FxOverlayPlane Of(RoomFxType? effect) => effect is { } type ? ShowingEach[type] : Cleared;

    /// <summary>The effect whose pictures the overlay shows, its liquid or its atmosphere; null when it shows none.</summary>
    public RoomFxType? Effect { get; }
}
