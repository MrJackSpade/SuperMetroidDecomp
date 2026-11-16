using SuperMetroid.Core.Enemies.Alcoon;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>The cue and operands of one alcoon native list addresses instruction.</summary>
/// <param name="Cue">The cue it becomes.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct AlcoonNativeListAddressesInstruction(AlcoonCue Cue, string Operands);
