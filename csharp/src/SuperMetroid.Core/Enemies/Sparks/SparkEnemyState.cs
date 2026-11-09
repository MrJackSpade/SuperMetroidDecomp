using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>The state of one Spark.</summary>
public sealed class SparkEnemyState
{
    /// <summary>Where the typed animation (#641) resumes; none until the Spark is initialized.</summary>
    internal AnimationProgramCursor<SparkProgram>? Program { get; set; }

    /// <summary>The always-active, intermittent or falling-spark behaviour.</summary>
    public SparkEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// Population parameter two (0 through 65535): the base delay, or, when its word's sign bit is set, a request for a
    /// randomized delay.
    /// </summary>
    public int BaseFunctionTime { get; internal set; }

    /// <summary>The base delays from which <see cref="BaseFunctionTime"/> requests a randomized delay instead.</summary>
    private const int RandomDelayRequestFrom = 32768;

    /// <summary>Whether <see cref="BaseFunctionTime"/> requests a randomized delay rather than naming one.</summary>
    internal bool RequestsRandomDelay => BaseFunctionTime >= RandomDelayRequestFrom;

    /// <summary>The countdown, decremented and then tested for zero as the game does: reaching zero expires it, and one that starts from zero wraps instead.</summary>
    public int FunctionTimer { get; internal set; }
}
