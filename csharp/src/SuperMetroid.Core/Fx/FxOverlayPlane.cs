namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room effect the effect overlay shows (#1348, #1466, #1514, #1601): the effect whose artwork it shows, or none, in
/// which case the overlay shows nothing. A liquid shows its surface strip on its surface line and its body below; an
/// atmosphere shows its picture repeating, scrolled (#1706). The HUD is not on the overlay: it is its own display layer.
/// </summary>
public sealed class FxOverlayPlane
{
    /// <summary>The overlay of a room whose effect shows no page: nothing.</summary>
    public static FxOverlayPlane Cleared { get; } = new(null);

    /// <summary>The overlay showing each effect's page, shared so a frame showing the same page reuses its resolved picture.</summary>
    private static readonly Dictionary<RoomFxType, FxOverlayPlane> Pages =
        Enum.GetValues<RoomFxType>().ToDictionary(type => type, type => new FxOverlayPlane(type));

    /// <summary>Takes the effect whose page the overlay shows.</summary>
    private FxOverlayPlane(RoomFxType? effectPage) => EffectPage = effectPage;

    /// <summary>The overlay showing the page of <paramref name="effectPage"/>, or <see cref="Cleared"/> for none.</summary>
    /// <param name="effectPage">The effect whose page the overlay shows, or null.</param>
    public static FxOverlayPlane Of(RoomFxType? effectPage) => effectPage is { } type ? Pages[type] : Cleared;

    /// <summary>The effect whose page the overlay shows; null when it shows none.</summary>
    public RoomFxType? EffectPage { get; }
}
