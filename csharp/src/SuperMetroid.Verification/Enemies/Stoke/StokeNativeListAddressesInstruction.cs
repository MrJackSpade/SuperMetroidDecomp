using SuperMetroid.Core.Enemies.Stoke;

namespace SuperMetroid.Verification.Enemies.Stoke;

/// <summary>The kind and operands of one stoke native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct StokeNativeListAddressesInstruction(StokeCueKind Kind, string Operands);
