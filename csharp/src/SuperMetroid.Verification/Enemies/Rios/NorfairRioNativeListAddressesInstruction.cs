using SuperMetroid.Core.Enemies.Rios;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>The kind and operands of one norfair rio native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct NorfairRioNativeListAddressesInstruction(NorfairRioCueKind Kind, string Operands);
