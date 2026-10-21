using SuperMetroid.Core.Enemies.Hibashi;

namespace SuperMetroid.Verification.Enemies.Hibashi;

/// <summary>The kind, operands and value of one hibashi native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
/// <param name="Value">The value.</param>
internal readonly record struct HibashiNativeListAddressesInstruction(HibashiCueKind Kind, string Operands, ushort Value);
