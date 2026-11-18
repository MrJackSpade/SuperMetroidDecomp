using SuperMetroid.Core.Enemies.Cacatac;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>The kind and operands of one cacatac native list addresses instruction.</summary>
/// <param name="Kind">The signal the command raises, or none for the spike's spawn, whose direction is its operand.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct CacatacNativeListAddressesInstruction(CacatacCueKind? Kind, string Operands);
