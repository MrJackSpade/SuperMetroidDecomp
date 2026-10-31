namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where the Chainsaw beam's animation lands when its borrowed Power Bomb fuse speeds up (#1301, #1407, #1474). The
/// uncharged Chainsaw combination's callback runs the Power Bomb fuse over the shot. When the fuse reaches its fast stage,
/// the original game moves the animation as far on as a bomb's fast list lies from its slow one; on a beam that distance
/// lands in whichever animation the original game laid out there. The import pipeline decodes once, for every animation
/// position, the position it lands on.
/// </summary>
/// <remarks>
/// Consulted by the Chainsaw shot's pre-instruction during the projectile update, on the update its fuse reaches the
/// fast stage. Evidence: the original fuse routine and the projectile list layout, which the projectile program check
/// compares with this service's landings.
/// </remarks>
/// <typeparam name="TCursor">Where a shot's animation stands in its program.</typeparam>
[CompatibilityService("the Chainsaw beam's Power Bomb fuse moves its animation as far as a bomb's fast animation lies")]
public sealed class ChainsawFuseAnimationShift<TCursor>
    where TCursor : notnull
{
    /// <summary>Where each animation position lands.</summary>
    private readonly IReadOnlyDictionary<TCursor, TCursor> _landings;

    /// <summary>Keeps the decoded landing of every animation position that lands on one.</summary>
    /// <param name="landings">Where each animation position lands.</param>
    public ChainsawFuseAnimationShift(IReadOnlyDictionary<TCursor, TCursor> landings) => _landings = Ensure.NotNull(landings);

    /// <summary>Where an animation standing at <paramref name="cursor"/> lands when the fuse speeds up.</summary>
    /// <exception cref="InvalidDataException">The move lands on no animation position.</exception>
    public TCursor Shift(TCursor cursor) =>
        _landings.TryGetValue(cursor, out TCursor? landing)
            ? landing
            : throw new InvalidDataException($"The Chainsaw fuse moves the animation at {cursor} onto no animation position.");
}
