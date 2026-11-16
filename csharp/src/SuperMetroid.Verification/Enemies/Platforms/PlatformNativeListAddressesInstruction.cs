using SuperMetroid.Core.Enemies.Platforms;

namespace SuperMetroid.Verification.Enemies.Platforms;

/// <summary>The cue and operands of one platform native list addresses instruction.</summary>
/// <param name="Cue">The cue it becomes.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct PlatformNativeListAddressesInstruction(PlatformCue Cue, string Operands);
