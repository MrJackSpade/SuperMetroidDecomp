using SuperMetroid.Core.Enemies.Platforms;

namespace SuperMetroid.Verification.Enemies.Platforms;

/// <summary>The kind and operands of one platform native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct PlatformNativeListAddressesInstruction(PlatformCueKind Kind, string Operands);
