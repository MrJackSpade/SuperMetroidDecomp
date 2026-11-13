using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Samus's heat shimmer's animations (#1298, #1598): a start per suit, which falls into the suit's heat pulse, and the place
/// in that pulse the shimmer restarts at when the heated room's glow publishes a new heat phase.
/// </summary>
public static class PaletteFxHeatAnimations
{
    /// <summary>The heat shimmer's start for <paramref name="suit"/>.</summary>
    public static PaletteFxSegment Start(PaletteFxHeatSuit suit) => suit switch
    {
        PaletteFxHeatSuit.Power => PaletteFxSegment.SamusInHeatPowerSuit,
        PaletteFxHeatSuit.Varia => PaletteFxSegment.SamusInHeatVariaSuit,
        PaletteFxHeatSuit.Gravity => PaletteFxSegment.SamusInHeatGravitySuit,
        _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
    };

    /// <summary>The heat pulse of <paramref name="suit"/>: one frame per heat phase, in phase order.</summary>
    public static PaletteFxSegment Pulse(PaletteFxHeatSuit suit) => suit switch
    {
        PaletteFxHeatSuit.Power => PaletteFxSegment.SamusInHeatPowerSuitPulse,
        PaletteFxHeatSuit.Varia => PaletteFxSegment.SamusInHeatVariaSuitPulse,
        PaletteFxHeatSuit.Gravity => PaletteFxSegment.SamusInHeatGravitySuitPulse,
        _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
    };

    /// <summary>The heat shimmer's animation for <paramref name="suit"/>: its start, then its pulse, which it loops.</summary>
    internal static Animation<PaletteFxSegment> Shimmer(PaletteFxHeatSuit suit) => PaletteFxAnimations.StartingWith(Start(suit));

    /// <summary>
    /// Where the heat shimmer for <paramref name="suit"/> stands at heat phase <paramref name="phase"/>: its pulse's frame for
    /// that phase, which the shimmer plays on from.
    /// </summary>
    internal static AnimationPlace Phase(PaletteFxHeatSuit suit, HeatPhase phase) =>
        AnimationSegments<PaletteFxSegment, int, PaletteFxCue>.PlaceOf(Shimmer(suit), Pulse(suit), phase.Index);

    /// <summary>The suit whose heat palette <paramref name="equippedItems"/> selects: Gravity before Varia.</summary>
    public static PaletteFxHeatSuit SuitFor(SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems) =>
        equippedItems.HasAny(SamusEquipmentFlags.GravitySuit) ? PaletteFxHeatSuit.Gravity
        : equippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? PaletteFxHeatSuit.Varia
        : PaletteFxHeatSuit.Power;
}
