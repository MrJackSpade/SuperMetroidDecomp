using SuperMetroid.Core.Enemies.Magdollite;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>The kind and operands of one magdollite native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct MagdolliteNativeListAddressesInstruction(MagdolliteCueKind Kind, string Operands);
