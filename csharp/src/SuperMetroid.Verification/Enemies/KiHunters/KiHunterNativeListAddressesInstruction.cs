using SuperMetroid.Core.Enemies.KiHunters;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>The cue and operands of one ki hunter native list addresses instruction.</summary>
/// <param name="Cue">The cue it becomes.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct KiHunterNativeListAddressesInstruction(KiHunterCue Cue, string Operands);
