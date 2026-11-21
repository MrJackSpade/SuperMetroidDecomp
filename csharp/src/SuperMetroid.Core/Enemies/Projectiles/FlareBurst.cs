using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// One owner's pictures of the flare burst a projectile ends in (#2557): a spark that swells into a flare ball, then a cored
/// ring, a ring and the ring fading. Kraid's spat rock, Crocomire's shot, a Kago bug, a Torizo's swipe, explosions and sonic
/// boom, a Chozo orb and the Golden Torizo's eye beam all burst alike with their own pictures, so the burst's order is
/// stated once here and each owner names only its pictures and its pace.
/// </summary>
/// <param name="Spark">The spark the burst starts as.</param>
/// <param name="FlareBall">The flare ball the spark swells into.</param>
/// <param name="FlareRingCored">The ring still holding its core.</param>
/// <param name="FlareRing">The open ring.</param>
/// <param name="FlareRingFading">The ring fading away.</param>
internal sealed partial record FlareBurst(
    EnemyProjectileFrameId Spark,
    EnemyProjectileFrameId FlareBall,
    EnemyProjectileFrameId FlareRingCored,
    EnemyProjectileFrameId FlareRing,
    EnemyProjectileFrameId FlareRingFading)
{
    /// <summary><paramref name="pictures"/> burst with every picture shown for <paramref name="updates"/> updates.</summary>
    /// <typeparam name="TCue">The owner's cue type.</typeparam>
    /// <param name="pictures">The owner's burst pictures.</param>
    /// <param name="updates">The updates each picture shows for.</param>
    internal static AnimationBeat<EnemyProjectileFrameId, TCue>[] Steady<TCue>(FlareBurst pictures, int updates)
        where TCue : notnull =>
        AnimationBeats<EnemyProjectileFrameId, TCue>.Steady(updates, pictures.Spark, pictures.FlareBall,
            pictures.FlareRingCored, pictures.FlareRing, pictures.FlareRingFading);

    /// <summary>
    /// <paramref name="pictures"/> gathering: the burst played from its fading ring back to its spark, every picture shown
    /// for <paramref name="updates"/> updates.
    /// </summary>
    /// <typeparam name="TCue">The owner's cue type.</typeparam>
    /// <param name="pictures">The owner's burst pictures.</param>
    /// <param name="updates">The updates each picture shows for.</param>
    internal static AnimationBeat<EnemyProjectileFrameId, TCue>[] Gathering<TCue>(FlareBurst pictures, int updates)
        where TCue : notnull =>
        AnimationBeats<EnemyProjectileFrameId, TCue>.Steady(updates, pictures.FlareRingFading, pictures.FlareRing,
            pictures.FlareRingCored, pictures.FlareBall, pictures.Spark);

    /// <summary>
    /// <paramref name="pictures"/> burst with each picture shown for its own number of updates: the spark's, the flare
    /// ball's, the cored ring's, the ring's and the fading ring's.
    /// </summary>
    /// <typeparam name="TCue">The owner's cue type.</typeparam>
    /// <param name="pictures">The owner's burst pictures.</param>
    /// <param name="spark">The updates the spark shows for.</param>
    /// <param name="flareBall">The updates the flare ball shows for.</param>
    /// <param name="flareRingCored">The updates the cored ring shows for.</param>
    /// <param name="flareRing">The updates the ring shows for.</param>
    /// <param name="flareRingFading">The updates the fading ring shows for.</param>
    internal static AnimationBeat<EnemyProjectileFrameId, TCue>[] Paced<TCue>(FlareBurst pictures, int spark, int flareBall,
        int flareRingCored, int flareRing, int flareRingFading)
        where TCue : notnull =>
    [
        new AnimationBeat<EnemyProjectileFrameId, TCue>.Frame(pictures.Spark, spark),
        new AnimationBeat<EnemyProjectileFrameId, TCue>.Frame(pictures.FlareBall, flareBall),
        new AnimationBeat<EnemyProjectileFrameId, TCue>.Frame(pictures.FlareRingCored, flareRingCored),
        new AnimationBeat<EnemyProjectileFrameId, TCue>.Frame(pictures.FlareRing, flareRing),
        new AnimationBeat<EnemyProjectileFrameId, TCue>.Frame(pictures.FlareRingFading, flareRingFading),
    ];
}
