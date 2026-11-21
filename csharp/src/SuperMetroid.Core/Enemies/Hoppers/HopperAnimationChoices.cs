namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Chooses a Sidehopper's or Dessgeega's animation for its kind, surface and movement phase.</summary>
internal static class HopperAnimationChoices
{
    /// <summary>
    /// Each kind's animation on each surface in each phase, read once from <see cref="HopperAnimationDefinitions.Animations"/>
    /// when it is built, so a combination the definitions leave out fails there and no lookup can miss (#2553).
    /// </summary>
    private static readonly IdentityTable<HopperKind, IdentityTable<HopperSurface, IdentityTable<HopperPhase, HopperSegment>>> Animations =
        new(kind => new(surface => new(phase => HopperAnimationDefinitions.Animations[new(kind, surface, phase)])));

    /// <summary>The animation of a <paramref name="kind"/> hopper on <paramref name="surface"/> in <paramref name="phase"/>.</summary>
    internal static HopperSegment Animation(HopperKind kind, HopperSurface surface, HopperPhase phase) => Animations[kind][surface][phase];
}
