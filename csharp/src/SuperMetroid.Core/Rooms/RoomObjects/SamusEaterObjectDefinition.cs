namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One Brinstar plant: its mounting and its authored chew (#1554).</summary>
/// <param name="Side">The side it sits on (#2560): up for a ceiling eater, down for a floor eater.</param>
/// <param name="Chews">How many chews the plant makes before it releases Samus.</param>
/// <param name="Chew">One chew's images in order; the chew repeats <paramref name="Chews"/> times.</param>
/// <param name="Released">What the plant shows for <see cref="SamusEaterObjectDefinitions.ReleaseDuration"/> updates once it releases Samus.</param>
/// <param name="Idle">What the plant shows for its last update.</param>
[IndependentFlag("whether the eater hangs from the ceiling, a fact of the plant's definition")]
internal sealed record SamusEaterObjectDefinition(
    VerticalDirection Side,
    int Chews,
    IReadOnlyList<SamusEaterChewFrame> Chew,
    SamusEaterDraw Released,
    SamusEaterDraw Idle);
