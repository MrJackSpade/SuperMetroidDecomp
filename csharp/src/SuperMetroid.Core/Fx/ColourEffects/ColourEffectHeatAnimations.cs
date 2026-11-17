using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// Samus's heat shimmer (#1298, #1598, #1739, #1982, #2054, #2366): her suit's own heat pulse
/// (<see cref="ColourEffectAnimations.HeatPulses"/>), whose frames are the heat phases' looks, played as that pulse plays,
/// and the place in that pulse the shimmer restarts at when the heated room's glow publishes a new heat phase.
/// </summary>
public static class ColourEffectHeatAnimations
{
    /// <summary>The heat pulse the shimmer for <paramref name="suit"/> plays, in whose frames it shows.</summary>
    internal static ColourEffectId Pulse(ColourEffectHeatSuit suit) =>
        ColourEffectAnimations.HeatPulses.TryGetValue(suit, out ColourEffectId pulse) ? pulse
            : throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit.");

    /// <summary>How the heat shimmer for <paramref name="suit"/> plays: its pulse's animation, which loops.</summary>
    internal static Animation<ColourEffectSegment> Shimmer(ColourEffectHeatSuit suit) => ColourEffectAnimations.Definition(Pulse(suit)).Animation;

    /// <summary>
    /// Where the heat shimmer for <paramref name="suit"/> stands at heat phase <paramref name="phase"/>: its pulse's look of
    /// that phase, which the shimmer plays on from.
    /// </summary>
    internal static AnimationPlace Phase(ColourEffectHeatSuit suit, HeatPhase phase)
    {
        Animation<ColourEffectSegment> shimmer = Shimmer(suit);
        return ColourEffectAnimations.Segments.PlaceOfFrame(shimmer, shimmer.Phases.Single().Segment, ColourEffectAnimations.HeatLook(phase));
    }

    /// <summary>The suit whose heat shimmer <paramref name="equippedItems"/> selects: Gravity before Varia.</summary>
    public static ColourEffectHeatSuit SuitFor(SamusEquipmentFlags equippedItems) =>
        equippedItems.HasAny(SamusEquipmentFlags.GravitySuit) ? ColourEffectHeatSuit.Gravity
        : equippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? ColourEffectHeatSuit.Varia
        : ColourEffectHeatSuit.Power;
}
