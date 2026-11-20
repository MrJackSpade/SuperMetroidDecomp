using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// The state of one Atomic: its spinning animation. It chases Samus at its spawn settings' speed, which it reads where it
/// moves (#2476).
/// </summary>
public sealed class AtomicEnemyState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal AtomicSpawnSettings Settings { get; }

    /// <summary>Creates the state of an enemy whose entry told its species <paramref name="settings"/>.</summary>
    /// <param name="settings">What the enemy's population entry told its species.</param>
    internal AtomicEnemyState(AtomicSpawnSettings settings) => Settings = Ensure.NotNull(settings);

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<AtomicSegment> Playback { get; set; }
}
