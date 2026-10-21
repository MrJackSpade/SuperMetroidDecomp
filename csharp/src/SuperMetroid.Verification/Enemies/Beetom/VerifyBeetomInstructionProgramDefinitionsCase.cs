using SuperMetroid.Core.Enemies.Beetom;

namespace SuperMetroid.Verification.Enemies.Beetom;

/// <summary>The left, drain and loop of one verify beetom instruction program definitions case.</summary>
/// <param name="Left">The left.</param>
/// <param name="Drain">The drain.</param>
/// <param name="Loop">The loop.</param>
internal readonly record struct VerifyBeetomInstructionProgramDefinitionsCase(bool Left, BeetomProgram Drain, BeetomProgram Loop);
