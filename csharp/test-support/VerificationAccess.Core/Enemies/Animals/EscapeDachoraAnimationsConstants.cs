using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Animals.EscapeDachoraSegment;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Cartridge values and helpers of <see cref="EscapeDachoraAnimations"/> that only verification reads.</summary>
internal static class EscapeDachoraAnimationsConstants
{
    /// <summary>The animation that starts at each segment an owner can start or restart at: the run for the escape's.</summary>
    internal static IReadOnlyDictionary<EscapeDachoraSegment, Animation<EscapeDachoraSegment>> StartingAt { get; } = new Dictionary<EscapeDachoraSegment, Animation<EscapeDachoraSegment>>
    {
        [P.RunningForEscape] = EscapeDachoraAnimations.RunningForEscape,
        [P.RunningForEscapeAccelerating] = EscapeDachoraAnimations.RunningForEscapeAccelerating,
        [P.RunningForEscapeMaximumSpeed] = new([new(P.RunningForEscapeMaximumSpeed, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
    };
    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EscapeDachoraSegment> StartingWith(EscapeDachoraSegment segment) => StartingAt[segment];
}
