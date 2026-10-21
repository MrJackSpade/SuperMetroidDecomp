namespace SuperMetroid.Verification.Enemies.Platforms;

/// <summary>The pointer and name of one platform instruction program definitions expected.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Name">The name.</param>
internal readonly record struct PlatformInstructionProgramDefinitionsExpected(ushort Pointer, string Name);
