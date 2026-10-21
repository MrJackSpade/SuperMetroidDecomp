using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>The kind and operands of one yapping maw native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct YappingMawNativeListAddressesInstruction(YappingMawCueKind Kind, string Operands);
