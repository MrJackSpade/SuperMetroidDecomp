using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Diagnostics.Configuration;

/// <summary>
/// Everything a host reads from its options file: the game's own option values, and the host's settings for
/// presentation, audio output, recovery autosaves and error reporting.
/// </summary>
/// <remarks>
/// The game receives only <see cref="Game"/>. The other members configure the host around it and never reach the
/// simulation, recordings or debugger states.
/// </remarks>
public sealed record HostOptions
{
    /// <summary>The option values the game reads.</summary>
    public SuperMetroidGameOptions Game { get; init; } = new();

    /// <summary>Whether the host creates its audio output device and plays the game's audio frames.</summary>
    public bool AudioEnabled { get; init; } = true;

    /// <summary>Host PCM gain applied to the game's audio frames, from zero (mute) to 100.</summary>
    public int MasterVolumePercent { get; init; } = 100;

    /// <summary>Preserves a separate recovery state after each completed door transition.</summary>
    public bool DoorTransitionAutosave { get; init; } = true;

    /// <summary>Whether recoverable desktop-host errors are filed as GitHub issues.</summary>
    /// <remarks>
    /// This development convenience is deliberately disabled by default. When enabled, the
    /// desktop host prints the complete failure locally, queues a deduplicated issue through
    /// the authenticated GitHub CLI, and attempts the next frame. It is not part of the game's
    /// options and is therefore not serialized into deterministic input recordings.
    /// </remarks>
    public bool ReportErrorsToGitHub { get; init; }

    /// <summary>GitHub <c>owner/repository</c> receiving automatic private error reports.</summary>
    public string GitHubErrorRepository { get; init; } = "MrJackSpade/SuperMetroidDecomp";
}
