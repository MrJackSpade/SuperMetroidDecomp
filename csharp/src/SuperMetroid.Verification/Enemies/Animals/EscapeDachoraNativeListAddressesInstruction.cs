using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>The kind and operands of one escape dachora native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct EscapeDachoraNativeListAddressesInstruction(EscapeDachoraCueKind Kind, string Operands);
