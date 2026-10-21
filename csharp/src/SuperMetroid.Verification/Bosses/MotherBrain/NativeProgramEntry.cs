namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>A program and the native address its instruction list starts at.</summary>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
public readonly record struct NativeProgramEntry<TProgram>(TProgram Program, ushort Entry);
