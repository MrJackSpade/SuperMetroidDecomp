using SuperMetroid.Core.Enemies.Skree;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>The kind and operands of one skree metaree native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct SkreeMetareeNativeListAddressesInstruction(SkreeMetareeCueKind Kind, string Operands);
