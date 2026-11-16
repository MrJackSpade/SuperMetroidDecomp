using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>
/// The cue one escaping-Dachora native list instruction becomes: a branch of its kind, whose continuation its target
/// operand names, or the move it makes.
/// </summary>
/// <param name="Branch">The branch's kind; none for a move.</param>
/// <param name="Move">The move; none for a branch.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct EscapeDachoraNativeListAddressesInstruction(EscapeDachoraCueKind? Branch, EscapeDachoraCue.Move? Move, string Operands);
