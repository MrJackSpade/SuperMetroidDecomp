using SuperMetroid.Core.Enemies.Waver;

namespace SuperMetroid.Verification.Enemies.Waver;

/// <summary>The kind and operands of one waver native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct WaverNativeListAddressesInstruction(WaverCueKind Kind, string Operands);
