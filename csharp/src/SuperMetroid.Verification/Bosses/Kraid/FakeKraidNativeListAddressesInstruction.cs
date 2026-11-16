using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The kind and operands of one fake kraid native list addresses instruction.</summary>
/// <param name="Cue">The cue it becomes.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct FakeKraidNativeListAddressesInstruction(FakeKraidCue Cue, string Operands);
