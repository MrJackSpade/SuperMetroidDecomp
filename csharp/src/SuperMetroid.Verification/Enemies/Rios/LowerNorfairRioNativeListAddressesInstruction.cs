using SuperMetroid.Core.Enemies.Rios;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>The kind and operands of one lower norfair rio native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct LowerNorfairRioNativeListAddressesInstruction(LowerNorfairRioCueKind Kind, string Operands);
