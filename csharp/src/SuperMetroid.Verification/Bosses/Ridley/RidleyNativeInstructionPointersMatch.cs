using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>The key, step and position of one ridley native instruction pointers match.</summary>
/// <param name="Key">The key.</param>
/// <param name="Step">The step.</param>
/// <param name="Position">The position.</param>
internal readonly record struct RidleyNativeInstructionPointersMatch(RidleyBodyProgram Key, ushort Step, int Position);
