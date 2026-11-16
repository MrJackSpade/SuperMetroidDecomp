using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Saves;

/// <summary>The save location, timer, bindings and options a save snapshot records with Samus and system state (#633).</summary>
public sealed record SaveSnapshotContext
{
    /// <summary>The load station the save resumes at.</summary>
    public required LoadStation Checkpoint { get; init; }

    /// <summary>Optional gameplay clock; omitted values encode as zero.</summary>
    public GameTimeState? GameTime { get; init; } = null;

    /// <summary>Optional seven-action permutation; omitted values use retail defaults.</summary>
    public ControllerBindings? ControllerBindings { get; init; } = null;

    /// <summary>Whether to persist the moonwalk option.</summary>
    public bool MoonwalkEnabled { get; init; } = false;

    /// <summary>Whether to persist automatic HUD-item cancellation at doors.</summary>
    public bool IconCancelEnabled { get; init; } = false;

    /// <summary>Whether to persist the alternate Japanese text setting.</summary>
    public bool JapaneseText { get; init; } = false;
}
