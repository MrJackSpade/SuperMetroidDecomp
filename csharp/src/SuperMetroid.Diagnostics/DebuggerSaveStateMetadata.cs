using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Diagnostics;

/// <summary>Capture timestamp, frame and room identity of one debugger slot.</summary>
internal readonly record struct DebuggerSaveStateMetadata(
    DateTimeOffset SavedUtc,
    ushort FrameNumber,
    SuperMetroidGameState GameState,
    ushort? RoomPointer,
    ushort? RoomStatePointer,
    string Path);
