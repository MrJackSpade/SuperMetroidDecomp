namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The first and running program of one verify compiled room roomObject header load case.</summary>
/// <param name="First">The first.</param>
/// <param name="RunningProgram">The running program.</param>
internal readonly record struct VerifyCompiledRoomObjectHeaderLoadCase(Type? First, Enum? RunningProgram);
