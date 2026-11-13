namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// One axis of a seeking boss's velocity while it accelerates toward a point (#1620, was in Bosses, unmarked): a run of
/// steps, each raising or lowering the velocity, in which a step that raises the velocity from below zero to zero or
/// above, or lowers it from zero or above to below zero, makes the run's next step one 256th of a pixel an update
/// larger. The run's first step is one unit larger when the target lies the other way round from the coordinates' plain
/// order: the distance across the wrapping coordinate space puts it ahead while the coordinates' order puts it behind, or
/// the reverse, as for a target above the room's top seen from inside the room. A step that starts a fresh run adds no
/// unit for a crossing before it. Ceres Ridley, Norfair Ridley and the Shitroid seek this way, so every reversal of
/// their flight, and every seek toward a point past the room's top, moves them by those extra units; combining a run's
/// steps into one sum would change their trajectories.
/// </summary>
/// <remarks>
/// Consulted on every update a seeking boss accelerates toward its target, once per axis, before the velocity is clamped
/// to the boss's speed limit and before it moves. Evidence: Ceres Ridley's acceleration (<c>$A6:D62F</c>,
/// <c>$A6:D6A6</c>) adds or subtracts its distance step with the carry its position-less-target comparison left, and
/// chains each further step's carry without clearing it; Ridley's (<c>$A6:D526</c>, <c>$A6:D5A9</c>) clears it only
/// before the extra deceleration and the reversal constant, and the Shitroid's gradual acceleration (<c>$A9:F46B</c>,
/// <c>$A9:F4E6</c>) only before its reversal constant, its off-screen push adding with the set carry its on-screen test
/// leaves. Ceres Ridley's retreat seeks a point 128 pixels above the room's top. The Ridley inertia check and the seeking
/// velocity check compare the run with that chained arithmetic, and the Shitroid gradual acceleration check reproduces the
/// recorded first rise after the Shitroid drains the Sidehopper.
/// </remarks>
[CompatibilityService("a seeking boss's acceleration step that takes its velocity across zero makes the next step one 256th of a pixel larger, and a target past the coordinate space's end lengthens the first")]
internal struct SeekingVelocityRun
{
    /// <summary>The units a velocity counts through, in 256ths of a pixel an update, before it continues from the other end.</summary>
    private const int VelocityUnits = 65536;

    /// <summary>Whether the run's next step adds one unit: after a step that crossed zero, or first toward a target the wrap reverses.</summary>
    private bool nextStepLengthened;

    /// <summary>Starts a run from <paramref name="start"/> toward <paramref name="target"/> from <paramref name="position"/>.</summary>
    /// <param name="start">The axis velocity before the run.</param>
    /// <param name="position">The boss's whole coordinate on the axis, as its position holds it.</param>
    /// <param name="target">The target's whole coordinate on the axis, as its position holds it.</param>
    internal SeekingVelocityRun(SignedEightEight start, int position, int target)
    {
        Velocity = start;
        bool targetAhead = CoordinateAxis.Difference(position, target) < 0;
        bool orderedAhead = position < target;
        nextStepLengthened = targetAhead != orderedAhead;
    }

    /// <summary>The velocity the run has reached.</summary>
    internal SignedEightEight Velocity { get; private set; }

    /// <summary>Starts a fresh run: the next step adds no unit for a crossing before it.</summary>
    internal void StartFresh() => nextStepLengthened = false;

    /// <summary>
    /// Raises the velocity by <paramref name="amount"/> 256ths of a pixel an update, never negative, plus the unit a
    /// crossing left; the raise lengthens the next step when it takes the velocity from below zero to zero or above.
    /// </summary>
    internal void Raise(int amount)
    {
        int raised = Velocity.UnsignedTwoHundredFiftySixthsOfPixel + Ensure.AtLeastZero(amount) + (nextStepLengthened ? 1 : 0);
        nextStepLengthened = raised >= VelocityUnits;
        Velocity = SignedEightEight.Wrapped(raised);
    }

    /// <summary>
    /// Lowers the velocity by <paramref name="amount"/> 256ths of a pixel an update, never negative, plus the unit a
    /// crossing left; the lowering lengthens the next step when it takes the velocity from zero or above to below zero.
    /// </summary>
    internal void Lower(int amount)
    {
        int lowered = Velocity.UnsignedTwoHundredFiftySixthsOfPixel - Ensure.AtLeastZero(amount) - (nextStepLengthened ? 1 : 0);
        nextStepLengthened = lowered < 0;
        Velocity = SignedEightEight.Wrapped(lowered);
    }
}
