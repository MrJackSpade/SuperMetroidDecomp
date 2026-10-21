using SuperMetroid.Core.Enemies.Alcoon;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>The program, step and shows of one alcoon instruction program definitions pose.</summary>
/// <param name="Program">The program.</param>
/// <param name="Step">The step.</param>
/// <param name="Shows">The shows.</param>
internal readonly record struct AlcoonInstructionProgramDefinitionsPose(AlcoonProgram Program, int Step, bool Shows);
