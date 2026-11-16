using SuperMetroid.Core.Enemies.MamaTurtles;

namespace SuperMetroid.Verification.Enemies.MamaTurtles;

/// <summary>The cue and operands of one mama turtle native list addresses instruction.</summary>
/// <param name="Cue">The cue it becomes.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct MamaTurtleNativeListAddressesInstruction(MamaTurtleCue Cue, string Operands);
