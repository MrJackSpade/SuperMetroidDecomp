using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The kind and operands of one fake kraid native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct FakeKraidNativeListAddressesInstruction(FakeKraidCueKind Kind, string Operands);
