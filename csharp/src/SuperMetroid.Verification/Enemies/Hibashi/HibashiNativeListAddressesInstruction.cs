using SuperMetroid.Core.Enemies.Hibashi;

namespace SuperMetroid.Verification.Enemies.Hibashi;

/// <summary>The cue one hibashi native list addresses instruction must become, and its operands.</summary>
/// <param name="Cue">The cue the routine becomes: a rising routine's cue carries the eruption step its place names.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct HibashiNativeListAddressesInstruction(HibashiCue Cue, string Operands);
