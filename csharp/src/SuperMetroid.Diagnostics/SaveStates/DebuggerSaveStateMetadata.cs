using SuperMetroid.Core.Progression;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>Capture timestamp, frame and room identity of one debugger slot.</summary>
internal readonly record struct DebuggerSaveStateMetadata(
    DateTimeOffset SavedUtc,
    ushort FrameNumber,
    SuperMetroidGameState GameState,
    string? Room,
    string? RoomState,
    string Path);
