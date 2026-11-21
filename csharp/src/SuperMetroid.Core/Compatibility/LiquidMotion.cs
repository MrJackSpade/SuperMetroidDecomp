namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where a room's liquid's motion stands (#1987, #2560): awaiting its first motion pass, moving, or stopped for good. The
/// liquid's first motion pass after the room loads only begins its motion: it moves nothing and, for lava or acid, starts
/// the ambient sound clock but leaves the shared random number as it is. From the next pass on, every pass rises, falls or
/// turns the tide of the liquid, and lava or acid swaps the halves of the shared random number, so the liquid's first move
/// and the first swap come one update after the room loads. An enemy can end the motion directly, which stops its passes
/// until the next room load, begun or not.
/// </summary>
/// <remarks>
/// Consulted once an update by the liquid's motion pass, before the main loop's random draw; the pass also runs during door
/// fades and pause entry. Evidence: the original's room liquid effect spends its first run installing the routine it then
/// runs on every later frame, so its first frame moves nothing and swaps nothing; the recorded lava-room snapshots begin
/// past that run, which the replay import restores as <see cref="Moving"/> or <see cref="Stopped"/>.
/// </remarks>
[CompatibilityService("a room's liquid first moves, and lava or acid first swaps the shared random number's halves, on its second motion pass after the room loads")]
public readonly record struct LiquidMotion
{
    /// <summary>The motion's stage.</summary>
    private readonly LiquidMotionStage stage;

    /// <summary>A motion at <paramref name="stage"/>.</summary>
    private LiquidMotion(LiquidMotionStage stage) => this.stage = stage;

    /// <summary>The liquid of a room just loaded: its next motion pass only begins its motion.</summary>
    public static LiquidMotion AwaitingFirstPass { get; } = new(LiquidMotionStage.AwaitingFirstPass);

    /// <summary>A liquid whose motion has begun: each motion pass moves it.</summary>
    public static LiquidMotion Moving { get; } = new(LiquidMotionStage.Moving);

    /// <summary>A liquid an enemy has stopped: no motion pass moves it again until the next room load.</summary>
    public static LiquidMotion Stopped { get; } = new(LiquidMotionStage.Stopped);

    /// <summary>Whether the motion has begun and runs: every motion pass moves the liquid.</summary>
    public bool IsMoving => stage is LiquidMotionStage.Moving;

    /// <summary>Whether an enemy has stopped the motion.</summary>
    public bool IsStopped => stage is LiquidMotionStage.Stopped;

    /// <summary>The stages of a room liquid's motion.</summary>
    private enum LiquidMotionStage
    {
        /// <summary>The next motion pass only begins the motion.</summary>
        AwaitingFirstPass,

        /// <summary>Each motion pass moves the liquid.</summary>
        Moving,

        /// <summary>No motion pass moves the liquid again until the next room load.</summary>
        Stopped,
    }
}
