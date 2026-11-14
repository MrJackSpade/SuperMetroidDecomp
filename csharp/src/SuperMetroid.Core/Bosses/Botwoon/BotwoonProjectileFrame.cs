namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// A drawn Botwoon projectile frame (#1869): one of a body link's four frames by facing, the tail cap by facing, the
/// hidden segment's one frame, or one of the spit's five frames. Only these can be made.
/// </summary>
internal readonly record struct BotwoonProjectileFrame
{
    /// <summary>Keeps a frame the factories below have checked.</summary>
    private BotwoonProjectileFrame(BotwoonProjectileFrameKind kind, CompassDirection? facing, int phase)
    {
        Kind = kind;
        Facing = facing;
        Phase = phase;
    }

    /// <summary>What the frame depicts.</summary>
    internal BotwoonProjectileFrameKind Kind { get; }

    /// <summary>The direction a body link or tail cap faces; the hidden segment and the spit face none.</summary>
    internal CompassDirection? Facing { get; }

    /// <summary>The frame's place in a body link's or the spit's cycle; zero for the tail cap and the hidden segment.</summary>
    internal int Phase { get; }

    /// <summary>The hidden segment's empty frame.</summary>
    internal static BotwoonProjectileFrame Hidden { get; } = new(BotwoonProjectileFrameKind.Hidden, null, 0);

    /// <summary>Frame <paramref name="phase"/> of a body link facing <paramref name="facing"/>.</summary>
    internal static BotwoonProjectileFrame Body(CompassDirection facing, int phase) =>
        new(BotwoonProjectileFrameKind.Body, facing, Ensure.BetweenInclusive(0, BotwoonAnimations.BodyPhaseCount - 1, phase));

    /// <summary>The tail cap facing <paramref name="facing"/>.</summary>
    internal static BotwoonProjectileFrame Tail(CompassDirection facing) => new(BotwoonProjectileFrameKind.Tail, facing, 0);

    /// <summary>Frame <paramref name="phase"/> of the spit's cycle.</summary>
    internal static BotwoonProjectileFrame Spit(int phase) =>
        new(BotwoonProjectileFrameKind.Spit, null, Ensure.BetweenInclusive(0, BotwoonAnimations.SpitPhaseCount - 1, phase));
}
