using SuperMetroid.Core.Enemies.MamaTurtles;

namespace SuperMetroid.Verification.Enemies.MamaTurtles;

/// <summary>The cue and operands of one native instruction the baby turtles' lists use.</summary>
/// <param name="Cue">The cue it becomes.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct BabyTurtleNativeListAddressesInstruction(BabyTurtleCue Cue, string Operands);
