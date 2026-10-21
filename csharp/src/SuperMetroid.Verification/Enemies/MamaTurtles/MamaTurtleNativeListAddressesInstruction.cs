using SuperMetroid.Core.Enemies.MamaTurtles;

namespace SuperMetroid.Verification.Enemies.MamaTurtles;

/// <summary>The kind and operands of one mama turtle native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct MamaTurtleNativeListAddressesInstruction(MamaTurtleCueKind Kind, string Operands);
