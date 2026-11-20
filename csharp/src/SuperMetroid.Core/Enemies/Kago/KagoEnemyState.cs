using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>The state of the Kago.</summary>
public sealed class KagoEnemyState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal KagoSpawnSettings Settings { get; }

    /// <summary>Creates the state of an enemy whose entry told its species <paramref name="settings"/>.</summary>
    /// <param name="settings">What the enemy's population entry told its species.</param>
    internal KagoEnemyState(KagoSpawnSettings settings) => Settings = Ensure.NotNull(settings);

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<KagoSegment> Playback { get; set; }

    /// <summary>
    /// The shots the Kago has taken (#2340): it withstands as many as its placement's hit count and dies of the next. The
    /// shell shows its slow loop until its first hit, and its fast loop, the same poses held more briefly, from then on
    /// (#2473).
    /// </summary>
    public int HitsTaken { get; internal set; }
}
