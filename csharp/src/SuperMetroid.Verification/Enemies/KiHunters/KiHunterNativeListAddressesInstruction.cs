using SuperMetroid.Core.Enemies.KiHunters;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>The kind and operands of one ki hunter native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct KiHunterNativeListAddressesInstruction(KiHunterCueKind Kind, string Operands);
