using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// Samus's heat shimmer's animations (#1298, #1598, #1739, #1982): one per suit, which loops the suit's heat pulse, one
/// frame per heat phase, and the place in that pulse the shimmer restarts at when the heated room's glow publishes a new
/// heat phase. The shimmer's frames hold each suit's pulse in turn (<see cref="ColourEffectAnimations.HeatPulseFirstFrames"/>).
/// </summary>
public static class ColourEffectHeatAnimations
{
    /// <summary>How the heat shimmer for <paramref name="suit"/> plays: its pulse, which it loops.</summary>
    internal static Animation<ColourEffectId> Shimmer(ColourEffectHeatSuit suit) =>
        ColourEffectAnimations.HeatShimmers.TryGetValue(suit, out Animation<ColourEffectId>? shimmer) ? shimmer
            : throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit.");

    /// <summary>
    /// Where the heat shimmer for <paramref name="suit"/> stands at heat phase <paramref name="phase"/>: its pulse's frame for
    /// that phase, which the shimmer plays on from.
    /// </summary>
    internal static AnimationPlace Phase(ColourEffectHeatSuit suit, HeatPhase phase) =>
        ColourEffectAnimations.Steps.PlaceOfFrame(Shimmer(suit), ColourEffectId.SamusInHeat,
            ColourEffectAnimations.HeatPulseFirstFrames[suit] + phase.Index);

    /// <summary>The suit whose heat shimmer <paramref name="equippedItems"/> selects: Gravity before Varia.</summary>
    public static ColourEffectHeatSuit SuitFor(SamusEquipmentFlags equippedItems) =>
        equippedItems.HasAny(SamusEquipmentFlags.GravitySuit) ? ColourEffectHeatSuit.Gravity
        : equippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? ColourEffectHeatSuit.Varia
        : ColourEffectHeatSuit.Power;
}
