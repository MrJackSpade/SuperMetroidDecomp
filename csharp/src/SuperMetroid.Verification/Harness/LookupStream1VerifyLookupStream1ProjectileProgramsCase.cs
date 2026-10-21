namespace SuperMetroid.Verification.Harness;

/// <summary>The raw value, instruction pointer, spritemap pointer, x and y of one lookup stream1 verify lookup stream1 projectile programs case.</summary>
/// <param name="RawValue">The raw value.</param>
/// <param name="InstructionPointer">The instruction pointer.</param>
/// <param name="SpritemapPointer">The spritemap pointer.</param>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
internal readonly record struct LookupStream1VerifyLookupStream1ProjectileProgramsCase(ushort RawValue, int InstructionPointer, int SpritemapPointer, ushort X, ushort Y);
