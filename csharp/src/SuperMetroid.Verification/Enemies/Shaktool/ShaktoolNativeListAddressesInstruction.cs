using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>The kind and operands of one shaktool native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct ShaktoolNativeListAddressesInstruction(ShaktoolCueKind Kind, string Operands);
