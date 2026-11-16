using SuperMetroid.Core.Enemies.Owtch;

namespace SuperMetroid.Verification.Enemies.Owtch;

/// <summary>The cue and operands of one owtch native list addresses instruction.</summary>
/// <param name="Cue">The cue it becomes.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct OwtchNativeListAddressesInstruction(OwtchCue Cue, string Operands);
