using SuperMetroid.Core.Enemies.Magdollite;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>The cue one magdollite native list addresses instruction must become, and its operands.</summary>
/// <param name="Cue">The cue the routine becomes, or null for the sound routine, whose cue carries its decoded operand.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct MagdolliteNativeListAddressesInstruction(MagdolliteCue? Cue, string Operands);
