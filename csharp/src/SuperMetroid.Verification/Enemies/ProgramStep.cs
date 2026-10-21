namespace SuperMetroid.Verification.Enemies;

/// <summary>A program and the position of one of its steps.</summary>
/// <param name="Program">The program.</param>
/// <param name="Position">The position.</param>
internal readonly record struct ProgramStep<TProgram>(TProgram Program, int Position);
