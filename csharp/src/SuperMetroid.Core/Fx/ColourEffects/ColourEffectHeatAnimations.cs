using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// Samus's heat shimmer's animations (#1298, #1598, #1739): one per suit, which names her suit's colours and then loops the
/// suit's heat pulse, one frame per heat phase, and the place in that pulse the shimmer restarts at when the heated room's
/// glow publishes a new heat phase.
/// </summary>
public static class ColourEffectHeatAnimations
{
    /// <summary>The heat shimmer's animation for <paramref name="suit"/>.</summary>
    public static ColourEffectAnimation Start(ColourEffectHeatSuit suit) => suit switch
    {
        ColourEffectHeatSuit.Power => ColourEffectAnimation.SamusInHeatPowerSuit,
        ColourEffectHeatSuit.Varia => ColourEffectAnimation.SamusInHeatVariaSuit,
        ColourEffectHeatSuit.Gravity => ColourEffectAnimation.SamusInHeatGravitySuit,
        _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
    };

    /// <summary>How the heat shimmer for <paramref name="suit"/> plays: its setup, then its pulse, which it loops.</summary>
    internal static Animation<ColourEffectAnimation> Shimmer(ColourEffectHeatSuit suit) => ColourEffectAnimations.Of(Start(suit));

    /// <summary>
    /// Where the heat shimmer for <paramref name="suit"/> stands at heat phase <paramref name="phase"/>: its pulse's frame for
    /// that phase, its frame of that number, which the shimmer plays on from.
    /// </summary>
    internal static AnimationPlace Phase(ColourEffectHeatSuit suit, HeatPhase phase) =>
        ColourEffectAnimations.Steps.PlaceOfFrame(Shimmer(suit), Start(suit), phase.Index);

    /// <summary>The suit whose heat palette <paramref name="equippedItems"/> selects: Gravity before Varia.</summary>
    public static ColourEffectHeatSuit SuitFor(SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems) =>
        equippedItems.HasAny(SamusEquipmentFlags.GravitySuit) ? ColourEffectHeatSuit.Gravity
        : equippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? ColourEffectHeatSuit.Varia
        : ColourEffectHeatSuit.Power;
}
