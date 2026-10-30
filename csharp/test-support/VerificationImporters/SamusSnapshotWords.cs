using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;

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

    /// <summary>The high byte the normal-jump initializer puts on <c>$0B5E</c> above the shot direction.</summary>
    internal const ushort NormalJumpShotTag = 1 << 15;

    /// <summary>The high byte the turn out of a moonwalk puts on <c>$0B5E</c> above the shot direction.</summary>
    internal const ushort MoonwalkTurnShotTag = 1 << 8;

    /// <summary>
    /// Decodes <c>$0B5E</c>: zero hands over no shot; a tagged word hands over the shot in its low byte's direction.
    /// The tag only marked the word as set; which pose initializer set it is not kept.
    /// </summary>
    internal static SamusTransitionShot? DecodeTransitionShot(ushort word)
    {
        if (word == 0)
            return null;
        int tag = word & ~byte.MaxValue;
        return tag is NormalJumpShotTag or MoonwalkTurnShotTag
            ? new SamusTransitionShot(word & byte.MaxValue)
            : throw new InvalidDataException($"Transition shot word ${word:X4} carries no native initializer's tag.");
    }

    /// <summary>The counter value of one boost stage in <c>$0B3E</c>: the stage is the high byte, the cadence the low.</summary>
    private const int SpeedBoostStageStep = 1 << 8;

    /// <summary>Decodes <c>$0B3E</c>; a stage past full boost is rejected by the counter.</summary>
    internal static SamusSpeedBoostCounter DecodeSpeedBoostCounter(ushort word) =>
        new(word / SpeedBoostStageStep, word % SpeedBoostStageStep);

    /// <summary>Encodes the Speed Booster counter as the word native code holds.</summary>
    internal static int EncodeSpeedBoostCounter(SamusSpeedBoostCounter counter) =>
        counter.Stage * SpeedBoostStageStep + counter.Cadence;

    /// <summary>The high byte the Spring Ball landing puts on <c>$0B20</c> above the rebound count.</summary>
    internal const ushort SpringBallBounceFamily = 6 << 8;

    /// <summary>Decodes <c>$0B20</c>: the rebound count in the low byte, with the Spring Ball family's high byte or none.</summary>
    internal static SamusBallBounceState DecodeBallBounce(ushort word) => word switch
    {
        0 => SamusBallBounceState.None,
        1 => SamusBallBounceState.FirstRebound,
        2 => SamusBallBounceState.SecondRebound,
        SpringBallBounceFamily + 1 => SamusBallBounceState.SpringBallFirstRebound,
        SpringBallBounceFamily + 2 => SamusBallBounceState.SpringBallSecondRebound,
        _ => throw new InvalidDataException($"Ball bounce word ${word:X4} is not a native bounce state."),
    };

    /// <summary>Encodes a bounce state as the word native code writes.</summary>
    internal static int EncodeBallBounce(SamusBallBounceState state) => state switch
    {
        SamusBallBounceState.None => 0,
        SamusBallBounceState.FirstRebound => 1,
        SamusBallBounceState.SecondRebound => 2,
        SamusBallBounceState.SpringBallFirstRebound => SpringBallBounceFamily + 1,
        SamusBallBounceState.SpringBallSecondRebound => SpringBallBounceFamily + 2,
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusBallBounceState)} {state}."),
    };

    /// <summary>Decodes <c>$0DC0</c>: zero, one, or any word with bit 15 set.</summary>
    internal static ResumeChargingBeamSoundLatch DecodeResumeChargingBeamSound(ushort word) => word switch
    {
        0 => ResumeChargingBeamSoundLatch.Clear,
        1 => ResumeChargingBeamSoundLatch.ResumeNextPass,
        >= 1 << 15 => ResumeChargingBeamSoundLatch.SignedLatch,
        _ => throw new InvalidDataException($"Resume-charging-beam sound word ${word:X4} is not a native latch state."),
    };
}
