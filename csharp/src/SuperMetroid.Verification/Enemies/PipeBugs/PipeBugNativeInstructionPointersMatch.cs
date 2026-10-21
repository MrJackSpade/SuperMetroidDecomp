using SuperMetroid.Core.Enemies.PipeBugs;

namespace SuperMetroid.Verification.Enemies.PipeBugs;

/// <summary>The key, step and position of one pipe bug native instruction pointers match.</summary>
/// <param name="Key">The key.</param>
/// <param name="Step">The step.</param>
/// <param name="Position">The position.</param>
internal readonly record struct PipeBugNativeInstructionPointersMatch(PipeBugProgram Key, ushort Step, int Position);
