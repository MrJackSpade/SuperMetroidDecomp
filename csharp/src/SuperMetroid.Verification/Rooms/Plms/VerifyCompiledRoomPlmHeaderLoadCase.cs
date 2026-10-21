namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The first and running program of one verify compiled room plm header load case.</summary>
/// <param name="First">The first.</param>
/// <param name="RunningProgram">The running program.</param>
internal readonly record struct VerifyCompiledRoomPlmHeaderLoadCase(Type? First, Enum? RunningProgram);
