namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether a room's liquid has begun moving (#1987). The liquid's first motion pass after the room loads only begins its
/// motion: it moves nothing and, for lava or acid, starts the ambient sound clock but leaves the shared random number as
/// it is. From the next pass on, every pass rises, falls or turns the tide of the liquid, and lava or acid swaps the halves
/// of the shared random number, so the liquid's first move and the first swap come one update after the room loads.
/// </summary>
/// <remarks>
/// Consulted once an update by the liquid's motion pass, before the main loop's random draw; the pass also runs during door
/// fades and pause entry. Evidence: the original's room liquid effect spends its first run installing the routine it then
/// runs on every later frame, so its first frame moves nothing and swaps nothing; the recorded lava-room snapshots begin
/// past that run, which the replay import restores as <see cref="Begun"/>.
/// </remarks>
[CompatibilityService("a room's liquid first moves, and lava or acid first swaps the shared random number's halves, on its second motion pass after the room loads")]
public readonly record struct LiquidMotionStart
{
    /// <summary>Whether the liquid's motion has begun: every later motion pass moves it.</summary>
    public bool HasBegun { get; private init; }

    /// <summary>A liquid whose motion has begun: each motion pass moves it.</summary>
    public static LiquidMotionStart Begun { get; } = new() { HasBegun = true };
}
