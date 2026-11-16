namespace SuperMetroid.Verification.Harness;

/// <summary>The raw value, instruction pointer, spritemap pointer, x, y and shown record of one verify lookup stream1 projectile programs case.</summary>
/// <param name="RawValue">The raw value.</param>
/// <param name="InstructionPointer">The instruction pointer.</param>
/// <param name="SpritemapPointer">The spritemap pointer.</param>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="ShownRecord">The bank position of the timed record whose frame the projectile shows.</param>
internal readonly record struct VerifyLookupStream1ProjectileProgramsCase(ushort RawValue, int InstructionPointer, int SpritemapPointer, ushort X, ushort Y, int ShownRecord);
