using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>The slow, fast and fastest of one verify dachora instruction program definitions case.</summary>
/// <param name="Slow">The slow.</param>
/// <param name="Fast">The fast.</param>
/// <param name="Fastest">The fastest.</param>
internal readonly record struct VerifyDachoraInstructionProgramDefinitionsCase(DachoraProgram Slow, DachoraProgram Fast, DachoraProgram Fastest);
