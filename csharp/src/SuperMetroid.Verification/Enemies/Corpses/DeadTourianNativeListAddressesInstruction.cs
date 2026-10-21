using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>The kind and operands of one dead tourian native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct DeadTourianNativeListAddressesInstruction(DeadTourianCueKind Kind, string Operands);
