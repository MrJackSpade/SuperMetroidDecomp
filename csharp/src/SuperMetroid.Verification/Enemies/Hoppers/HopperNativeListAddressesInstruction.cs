using SuperMetroid.Core.Enemies.Hoppers;

namespace SuperMetroid.Verification.Enemies.Hoppers;

/// <summary>The kind and operands of one hopper native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct HopperNativeListAddressesInstruction(HopperCueKind Kind, string Operands);
