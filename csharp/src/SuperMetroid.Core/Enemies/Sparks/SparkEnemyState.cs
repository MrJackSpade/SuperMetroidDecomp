using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// The state of one Spark. The function, base time and countdown are values the cartridge keeps in the enemy's
/// common record, so they are emptied with that record when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class SparkEnemyState
{
    /// <summary>Where the typed animation (#641) resumes; none when the selected list word is not an animation.</summary>
    internal AnimationProgramCursor<SparkProgram>? Program { get; set; }

    /// <summary>The always-active, intermittent or falling-spark behaviour; none once a kill has emptied the record.</summary>
    public SparkEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// Population parameter two (0 through 65535): the base delay, or, when its word's sign bit is set, a request for a
    /// randomized delay. The delay it gives wraps at the word boundary when an extra delay is added.
    /// </summary>
    public int BaseFunctionTime { get; internal set; }

    /// <summary>The countdown decremented with the cartridge's <c>DEC; BEQ</c>: reaching zero expires it, and one that starts from zero wraps instead.</summary>
    public WrappingCountdown FunctionTimer { get; internal set; }

    /// <summary>Empties every value, when a kill empties the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        BaseFunctionTime = 0;
        FunctionTimer = WrappingCountdown.Start(0);
    }
}
