namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The shot places a shinespark crash's two echoes take (#2556): the first echo the one before Samus's last shot place,
/// the second the last, each over whatever shot holds it. Gameplay reads the places: a released echo replaces the shot
/// in its place, and a Spazer special attack's last trailing particle, which holds the one before the last, ends the echo
/// released onto its partner's place when it strikes something.
/// </summary>
/// <remarks>
/// Consulted by the crash as it releases its echoes. Evidence: the original crash's echo setup writes its two echoes
/// into the last two of Samus's projectile slots without searching for free ones, replacing whatever holds them.
/// </remarks>
[CompatibilityService("a shinespark crash's two echoes take Samus's last two shot places over whatever shots hold them")]
public static partial class ShinesparkCrashEchoPlaces
{
    /// <summary>The place the crash's first echo takes among <paramref name="placeCount"/> shot places: the one before the last.</summary>
    /// <param name="placeCount">The number of Samus's shot places, at least two.</param>
    public static int First(int placeCount) => Ensure.BetweenInclusive(2, int.MaxValue, placeCount) - 2;

    /// <summary>The place the crash's second echo takes among <paramref name="placeCount"/> shot places: the last.</summary>
    /// <param name="placeCount">The number of Samus's shot places, at least two.</param>
    public static int Second(int placeCount) => Ensure.BetweenInclusive(2, int.MaxValue, placeCount) - 1;
}
