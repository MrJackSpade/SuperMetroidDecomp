using SuperMetroid.Core.Enemies.Beetom;

namespace SuperMetroid.Verification.Enemies.Beetom;

/// <summary>The kind and operands of one beetom native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct BeetomNativeListAddressesInstruction(BeetomCueKind Kind, string Operands);
