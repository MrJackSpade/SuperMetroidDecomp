namespace SuperMetroid.Core.Runtime;

/// <summary>
/// What a host asks of one runtime frame beyond controller input (#633, #2542): the game state that owns the update, whose
/// permissions the update runs under, and host work immediately after the frame boundary. Nothing here is retained after
/// the frame.
/// </summary>
/// <remarks>A default request is a runtime-only host's update (<see cref="GameplayUpdateOwner.RuntimeOnlyHost"/>) with no boundary work.</remarks>
public sealed record RuntimeFrameRequest
{
    /// <summary>The game state that owns this update.</summary>
    public GameplayUpdateOwner Owner { get; init; } = GameplayUpdateOwner.RuntimeOnlyHost;

    /// <summary>
    /// Host work immediately after the ordinary frame boundary and before the liquid wave, statue descent and the update's
    /// owners, given <see cref="Owner"/> and returning the game state that owns the rest of the update: the work can move
    /// the game to another state before the update's gameplay runs, as the reserve refill's completion returns it to main
    /// gameplay (#2526). The suspended message-box frame path does not run it.
    /// </summary>
    public Func<GameplayUpdateOwner, GameplayUpdateOwner> AfterFrameBoundary { get; init; } = static owner => owner;
}
