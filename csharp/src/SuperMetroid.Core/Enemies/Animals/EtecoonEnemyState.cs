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

    /// <summary>Wrapped countdown.</summary>
    public WrappingCountdown FunctionTimer { get; internal set; }

    /// <summary>Bank-$A7 function address; none once a kill has emptied the record.</summary>
    public EtecoonAiFunction? Function { get; internal set; }

    /// <summary>
    /// The authored route, parameter two's low byte: zero runs left and one right to the teaching start, two starts
    /// teaching at once and loops its hops; bit one also allows the long run.
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

    /// <summary>Whether the Etecoon is heading right (parameter one's low byte is nonzero).</summary>
    public bool HeadingRight { get; internal set; }

    /// <summary>Teaching hops taken since the counter was last cleared, 0 through 255 (parameter one's high byte).</summary>
    public int TeachingHops
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>
    /// Whether parameter one's word is nonzero, the cartridge's test for heading right: a hop count left in the high
    /// byte also passes it.
    /// </summary>
    internal bool HeadsRightOrCountsHops => HeadingRight || TeachingHops != 0;

    /// <summary>Stores a heading the way the cartridge stores the whole parameter word, which also clears the hop count.</summary>
    internal void Head(HorizontalDirection side)
    {
        HeadingRight = side == HorizontalDirection.Right;
        TeachingHops = 0;
    }

    /// <summary>Installs the authored route and heading from the spawn parameters.</summary>
    internal void InstallSpawnParameters(RoomEnemyPopulationRecord population)
    {
        PopulationParameterSettings heading = population.Parameter1Settings;
        PopulationParameterSettings route = population.Parameter2Settings;
        HeadingRight = heading.First != 0;
        TeachingHops = heading.Second;
        Route = route.First;
        QuakePauseFrames = route.Second;
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
