namespace SuperMetroid.Verification.Harness;

/// <summary>The definition pointer, program start, color byte index and branches to shared body of one verify stream3 nintendo fade entries case.</summary>
/// <param name="DefinitionPointer">The definition pointer.</param>
/// <param name="ProgramStart">The program start.</param>
/// <param name="ColorByteIndex">The color byte index.</param>
/// <param name="BranchesToSharedBody">The branches to shared body.</param>
internal readonly record struct VerifyStream3NintendoFadeEntriesCase(ushort DefinitionPointer, ushort ProgramStart, ushort ColorByteIndex, bool BranchesToSharedBody);
