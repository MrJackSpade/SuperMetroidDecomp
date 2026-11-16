using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>
/// The cue one escaping-Dachora native list instruction becomes: a lap's end, whose taken branch's continuation its target
/// operand names, or the move it makes.
/// </summary>
/// <param name="Branch">The lap-end cue's type; none for a move.</param>
/// <param name="Move">The move; none for a branch.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct EscapeDachoraNativeListAddressesInstruction(Type? Branch, EscapeDachoraCue.Move? Move, string Operands);
