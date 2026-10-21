using SuperMetroid.Core.Enemies.Evir;

namespace SuperMetroid.Verification.Enemies.Evir;

/// <summary>The kind and operands of one evir native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct EvirNativeListAddressesInstruction(EvirCueKind Kind, string Operands);
