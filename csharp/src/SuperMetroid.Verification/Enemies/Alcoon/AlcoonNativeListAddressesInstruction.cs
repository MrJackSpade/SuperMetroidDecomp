using SuperMetroid.Core.Enemies.Alcoon;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>The kind and operands of one alcoon native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct AlcoonNativeListAddressesInstruction(AlcoonCueKind Kind, string Operands);
