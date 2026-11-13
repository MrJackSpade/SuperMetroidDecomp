using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The state of an Etecoon.</summary>
public sealed class EtecoonEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<EtecoonSegment> Playback { get; set; }

    /// <summary>
    /// Signed 16.16 velocity in room pixels per update.
    /// </summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The current behaviour's wrapping countdown, in updates.</summary>
    public WrappingCountdown ActionTimer { get; internal set; }

    /// <summary>
    /// Whether the Etecoon still lies dormant, waiting for Samus to come level with it (#1627): from its initialization
    /// until she does, when it starts flexing and <see cref="ActionTimer"/> counts the flex down.
    /// </summary>
    public bool AwaitingSamus { get; internal set; }

    /// <summary>The teaching hops counted before the count empties.</summary>
    internal const int TeachingHopsPerLesson = 4;

    /// <summary>The main-AI behaviour.</summary>
    public EtecoonAction? Action { get; internal set; }

    /// <summary>The authored route it takes after its first landing.</summary>
    public EtecoonRoute Route { get; internal set; }

    /// <summary>
    /// The updates an earthquake still suspends the Etecoon for: an earthquake starts
    /// <see cref="EtecoonQuakeDefinitions.QuakePauseFrames"/>, and every retail Etecoon's spawn settings start none.
    /// </summary>
    public int QuakePauseFrames
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, EtecoonQuakeDefinitions.QuakePauseFrames, value);
    }

    /// <summary>
    /// Whether the Etecoon is heading right, from its spawn settings until it first takes a heading of its own.
    /// </summary>
    /// <remarks>
    /// The game keeps the heading and <see cref="TeachingHops"/> together and tests them together, so a hop count
    /// would also read as heading right; but every state that tests the heading is reached only after the Etecoon has
    /// taken a heading (<see cref="Head"/>), which empties the count, and the teaching hops that count again always end
    /// in the return run, whose wall takes a heading before the next test. The heading is therefore read alone.
    /// </remarks>
    public bool HeadingRight { get; internal set; }

    /// <summary>
    /// Teaching hops taken since the count was last emptied, starting from the spawn settings (none in every retail
    /// Etecoon): the count empties on reaching <see cref="TeachingHopsPerLesson"/>.
    /// </summary>
    public int TeachingHops
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, EtecoonEnemyState.TeachingHopsPerLesson, value);
    }

    /// <summary>Takes a heading, which also empties the teaching-hop count.</summary>
    internal void Head(HorizontalDirection side)
    {
        HeadingRight = side == HorizontalDirection.Right;
        TeachingHops = 0;
    }

    /// <summary>Installs the authored route and heading from the spawn settings.</summary>
    internal void InstallSpawnSettings(EtecoonSpawnSettings settings)
    {
        HeadingRight = Ensure.NotNull(settings).Heading == HorizontalDirection.Right;
        TeachingHops = settings.TeachingHops;
        Route = settings.Route;
        QuakePauseFrames = settings.QuakePauseFrames;
    }
}
