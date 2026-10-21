using SuperMetroid.Core.Enemies.Zoa;

namespace SuperMetroid.Verification.Enemies.Zoa;

/// <summary>The kind and operands of one zoa native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct ZoaNativeListAddressesInstruction(ZoaCueKind Kind, string Operands);
