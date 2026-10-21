namespace SuperMetroid.DesktopVerification.Audio;

/// <summary>A command written to an APU port.</summary>
/// <param name="Port">The port.</param>
/// <param name="Command">The command.</param>
public readonly record struct ApuPortCommand(ApuPort Port, byte Command);
