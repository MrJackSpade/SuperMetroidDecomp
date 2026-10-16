using SuperMetroid.Core.Samus;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// The native-snapshot boundary for Samus words whose port state is typed (#1323): decodes imported WRAM words and
/// encodes port state for word-level comparison. Undefined words are rejected rather than normalized.
/// </summary>
internal static class SamusSnapshotWords
{
    /// <summary>The only word native code writes to <c>$0A76</c> to enable the hyper beam.</summary>
    internal const ushort HyperBeamEnabledWord = 1 << 15;

    /// <summary>Decodes <c>$0A76</c>: zero or <see cref="HyperBeamEnabledWord"/>.</summary>
    internal static bool DecodeHyperBeam(ushort word) => word switch
    {
        0 => false,
        HyperBeamEnabledWord => true,
        _ => throw new InvalidDataException($"Hyper beam word ${word:X4} is neither clear nor ${HyperBeamEnabledWord:X4}."),
    };

    /// <summary>Encodes the hyper beam state as the word native code writes.</summary>
    internal static ushort EncodeHyperBeam(bool enabled) => enabled ? HyperBeamEnabledWord : (ushort)0;

    /// <summary>Decodes <c>$0DC0</c>: zero, one, or any word with bit 15 set.</summary>
    internal static ResumeChargingBeamSoundLatch DecodeResumeChargingBeamSound(ushort word) => word switch
    {
        0 => ResumeChargingBeamSoundLatch.Clear,
        1 => ResumeChargingBeamSoundLatch.ResumeNextPass,
        >= 1 << 15 => ResumeChargingBeamSoundLatch.SignedLatch,
        _ => throw new InvalidDataException($"Resume-charging-beam sound word ${word:X4} is not a native latch state."),
    };
}
