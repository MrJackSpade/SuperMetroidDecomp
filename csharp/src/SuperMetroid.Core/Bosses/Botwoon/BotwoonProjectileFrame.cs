namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// A drawn Botwoon projectile frame (#1869): one of a body link's four frames by facing, the tail cap by facing, the
/// hidden segment's one frame, or one of the spit's five frames. Only these can be made.
/// </summary>
/// <param name="Picture">What the frame depicts.</param>
internal readonly record struct BotwoonProjectileFrame(BotwoonProjectilePicture Picture)
{
    /// <summary>The hidden segment's empty frame.</summary>
    internal static BotwoonProjectileFrame Hidden { get; } = new(BotwoonProjectilePicture.HiddenSegment.Instance);

    /// <summary>Frame <paramref name="phase"/> of a body link facing <paramref name="facing"/>.</summary>
    internal static BotwoonProjectileFrame Body(CompassDirection facing, int phase) =>
        new(new BotwoonProjectilePicture.BodyLink(facing, Ensure.BetweenInclusive(0, BotwoonAnimations.BodyPhaseCount - 1, phase)));

    /// <summary>The tail cap facing <paramref name="facing"/>.</summary>
    internal static BotwoonProjectileFrame Tail(CompassDirection facing) => new(new BotwoonProjectilePicture.TailCap(facing));

    /// <summary>Frame <paramref name="phase"/> of the spit's cycle.</summary>
    internal static BotwoonProjectileFrame Spit(int phase) =>
        new(new BotwoonProjectilePicture.SpitFrame(Ensure.BetweenInclusive(0, BotwoonAnimations.SpitPhaseCount - 1, phase)));
}
