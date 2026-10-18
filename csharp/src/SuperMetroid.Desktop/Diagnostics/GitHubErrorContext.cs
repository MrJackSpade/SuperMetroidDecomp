using SuperMetroid.Core.Input;
namespace SuperMetroid.Desktop.Diagnostics;

/// <summary>Debugger context captured at a recoverable desktop-host error boundary.</summary>
public sealed record GitHubErrorContext(
    string Boundary,
    ushort? FrameNumber = null,
    string? GameState = null,
    string? Phase = null,
    SnesButton? ControllerInput = null,
    string? Room = null,
    string? RoomState = null,
    string? Door = null,
    string? InputRecordingPath = null,
    string? FrameDiagnostics = null);
