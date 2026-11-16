using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>The cue one yapping maw native list addresses instruction must become, and its operands.</summary>
/// <param name="Cue">The cue the routine becomes: a hold routine's cue carries the aim whose mouth offset it holds Samus at (#2071).</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct YappingMawNativeListAddressesInstruction(YappingMawCue Cue, string Operands);
