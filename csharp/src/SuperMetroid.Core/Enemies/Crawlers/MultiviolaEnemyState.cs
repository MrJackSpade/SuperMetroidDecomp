using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The state of the Multiviola. Each update it moves by the speeds its heading and its spawn settings' speed give that
/// update (#2531), and turning a speed left or up is not that speed's exact opposite (<see cref="MultiviolaLeftOrUpSpeed"/>).
/// </summary>
public sealed class MultiviolaEnemyState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal MultiviolaSpawnSettings Settings { get; }

    /// <summary>Creates the state of an enemy whose entry told its species <paramref name="settings"/>.</summary>
    /// <param name="settings">What the enemy's population entry told its species.</param>
    internal MultiviolaEnemyState(MultiviolaSpawnSettings settings) => Settings = Ensure.NotNull(settings);

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MultiviolaSegment> Playback { get; set; }

    /// <summary>
    /// The flight heading, from its spawn settings, bounced into the neighbouring quarter turn at each wall or floor
    /// (<see cref="MultiviolaQuarterBounce"/>); each update's speeds come from it.
    /// </summary>
    public TrigonometryAngle Heading { get; internal set; }
}
