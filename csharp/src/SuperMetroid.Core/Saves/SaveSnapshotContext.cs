using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Saves;

/// <summary>The save location, timer, bindings and options a save snapshot records with Samus and system state (#633).</summary>
public sealed record SaveSnapshotContext
{
    /// <summary>The load station the save resumes at.</summary>
    public required LoadStation Checkpoint { get; init; }

    /// <summary>The reading of the gameplay clock the save keeps.</summary>
    public required GameClockReading GameTime { get; init; }

    /// <summary>The buttons of the seven configurable actions.</summary>
    public required ControllerBindings ControllerBindings { get; init; }

    /// <summary>Whether to persist the moonwalk option.</summary>
    public required bool MoonwalkEnabled { get; init; }

    /// <summary>Whether to persist automatic HUD-item cancellation at doors.</summary>
    public required bool IconCancelEnabled { get; init; }

    /// <summary>Whether to persist the alternate Japanese text setting.</summary>
    public required TextLanguage Language { get; init; }
}
