using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The state of an Etecoon. The values the cartridge keeps in the enemy's common record are emptied with it when the
/// enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class EtecoonEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<EtecoonProgram> Program { get; set; }

    /// <summary>
    /// Signed 16.16 velocity in room pixels per update.
    /// </summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The current behaviour's wrapping countdown, in updates.</summary>
    public WrappingCountdown FunctionTimer { get; internal set; }

    /// <summary>The main-AI behaviour; none once a kill has emptied the record.</summary>
    public EtecoonAiFunction? Function { get; internal set; }

    /// <summary>
    /// The authored route, parameter two's low byte: zero runs left and one right to the teaching start, two starts
    /// teaching at once and loops its hops; route two also allows the long run.
    /// </summary>
    public int Route
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>The updates an earthquake still suspends the Etecoon for, 0 through 255 (parameter two's high byte).</summary>
    public int QuakePauseFrames
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>
    /// Whether the Etecoon is heading right, from its spawn settings until it first takes a heading of its own.
    /// </summary>
    /// <remarks>
    /// The cartridge keeps the heading and <see cref="TeachingHops"/> together and tests them together, so a hop count
    /// would also read as heading right; but every state that tests the heading is reached only after the Etecoon has
    /// taken a heading (<see cref="Head"/>), which empties the count, and the teaching hops that count again always end
    /// in the return run, whose wall takes a heading before the next test. The heading is therefore read alone.
    /// </remarks>
    public bool HeadingRight { get; internal set; }

    /// <summary>Teaching hops taken since the count was last emptied, 0 through 255, starting from the spawn settings.</summary>
    public int TeachingHops
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
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

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Velocity = FixedVector<RoomPixels>.Zero;
        FunctionTimer = WrappingCountdown.Start(0);
        Function = null;
        Route = 0;
        QuakePauseFrames = 0;
        HeadingRight = false;
        TeachingHops = 0;
    }
}
