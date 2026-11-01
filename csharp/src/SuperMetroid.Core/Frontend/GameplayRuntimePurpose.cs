namespace SuperMetroid.Core.Frontend;

/// <summary>Why the session creates a gameplay runtime, which decides whether host options reach it.</summary>
internal enum GameplayRuntimePurpose
{
    /// <summary>A new, saved or Space-Time-restarted game the player controls; the assists and menu choices apply.</summary>
    SelectedGame,

    /// <summary>An attract demo's private runtime, which keeps default controls and plays unassisted.</summary>
    AttractDemo,
}
