namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>The cue one magdollite native list addresses instruction must become, and its operands.</summary>
/// <typeparam name="TCue">The cues of the part whose lists use the instruction.</typeparam>
/// <param name="Cue">The cue the routine becomes, or null for the sound routine, whose cue carries its decoded operand.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct MagdolliteNativeListAddressesInstruction<TCue>(TCue? Cue, string Operands)
    where TCue : class;
