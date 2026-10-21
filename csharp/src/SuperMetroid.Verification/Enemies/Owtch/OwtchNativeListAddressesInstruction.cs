using SuperMetroid.Core.Enemies.Owtch;

namespace SuperMetroid.Verification.Enemies.Owtch;

/// <summary>The kind and operands of one owtch native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct OwtchNativeListAddressesInstruction(OwtchCueKind Kind, string Operands);
