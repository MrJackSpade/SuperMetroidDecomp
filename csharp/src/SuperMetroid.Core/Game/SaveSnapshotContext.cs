using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>The save location, timer, bindings and options a save snapshot records with Samus and system state (#633).</summary>
public sealed record SaveSnapshotContext
{
    /// <summary>Retail area index to resume in.</summary>
    public required ushort Area { get; init; }

    /// <summary>Zero-based load-station index within <see cref="Area"/>.</summary>
    public required ushort SaveStation { get; init; }

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
