using SuperMetroid.Core.Enemies.Sparks;

namespace SuperMetroid.Verification.Enemies.Sparks;

/// <summary>The kind and operands of one spark native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct SparkNativeListAddressesInstruction(SparkCueKind Kind, string Operands);
