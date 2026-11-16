using SuperMetroid.Core.Enemies.Hoppers;

namespace SuperMetroid.Verification.Enemies.Hoppers;

/// <summary>The cue one hopper native list addresses instruction must become, and its operands.</summary>
/// <param name="Cue">The cue the routine becomes, or null for the sound routine, whose cue carries its decoded operand.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct HopperNativeListAddressesInstruction(HopperCue? Cue, string Operands);
