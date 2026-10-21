using SuperMetroid.Core.Enemies.Skultera;

namespace SuperMetroid.Verification.Enemies.Skultera;

/// <summary>The kind and operands of one skultera native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct SkulteraNativeListAddressesInstruction(SkulteraCueKind Kind, string Operands);
