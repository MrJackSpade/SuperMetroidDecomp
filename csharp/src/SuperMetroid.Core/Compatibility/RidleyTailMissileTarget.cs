namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which missile Ridley's tail attack swings at instead of Samus (#1592): the one in Samus's lowest shot place among the
/// missiles and Super Missiles within reach of the tail's tip, found only while her firing count says she has shots out.
/// A shot place the count does not cover is still searched, and a live missile is not found while the count reads none.
/// </summary>
/// <remarks>
/// Consulted once per tail attack aim, during Ridley's update, before the aim is turned into the tail's target angle.
/// Evidence: the original tail-attack check tests the shot count for zero, then walks the five shot places from the
/// first up and takes the first missile or Super Missile within reach.
/// </remarks>
[CompatibilityService("Ridley's tail swings at the missile in Samus's lowest shot place within reach, searched only while her shot count is nonzero")]
public static class RidleyTailMissileTarget
{
    /// <summary>The missile the tail swings at, or null when it swings at Samus.</summary>
    /// <typeparam name="TShot">One of Samus's shot places' projectiles.</typeparam>
    /// <param name="shotCount">How many shots Samus has out by her firing count.</param>
    /// <param name="shotPlaces">Samus's shot places' projectiles, in place order.</param>
    /// <param name="isMissileInReach">Whether a place's projectile is a missile or Super Missile within reach of the tail's tip.</param>
    public static TShot? Target<TShot>(int shotCount, IReadOnlyList<TShot> shotPlaces, Func<TShot, bool> isMissileInReach)
        where TShot : class
    {
        Ensure.NotNull(shotPlaces);
        Ensure.NotNull(isMissileInReach);
        return Ensure.AtLeastZero(shotCount) == 0 ? null : shotPlaces.FirstOrDefault(isMissileInReach);
    }
}
