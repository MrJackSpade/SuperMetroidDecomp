using SuperMetroid.Core.Saves;

namespace SuperMetroid.Verification.Saves;

/// <summary>The field and out of range of one verify widened slot fields round trip case.</summary>
/// <param name="Field">The field.</param>
/// <param name="OutOfRange">The out of range.</param>
internal readonly record struct VerifyWidenedSlotFieldsRoundTripCase(string Field, SuperMetroidSaveSnapshot OutOfRange);
