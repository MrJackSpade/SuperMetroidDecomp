namespace SuperMetroid.Verification.Saves;

/// <summary>The marker and expected carry of one verify invalid slot and noncanonical marker case.</summary>
/// <param name="Marker">The marker.</param>
/// <param name="ExpectedCarry">The expected carry.</param>
internal readonly record struct VerifyInvalidSlotAndNoncanonicalMarkerCase(byte[] Marker, bool ExpectedCarry);
