using SuperMetroid.Core.Enemies.Choot;

namespace SuperMetroid.Verification.Enemies.Choot;

/// <summary>The kind and operands of one choot native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct ChootNativeListAddressesInstruction(ChootCueKind Kind, string Operands);
