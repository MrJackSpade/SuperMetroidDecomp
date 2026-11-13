using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;

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
            ? new SamusTransitionShot(SuperMetroid.Core.Samus.Poses.SamusPoseRecordDecoding.DirectionOfAimByte(word & byte.MaxValue))
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

    /// <summary>
    /// Decodes <c>$0DC0</c>: zero or one, the only words the game writes there (#1682). A word with bit 15 set, which
    /// the post-draw check would rearm to one, is never written, and is rejected like any other undefined word.
    /// </summary>
    internal static bool DecodeResumeChargingBeamSound(ushort word) => word switch
    {
        0 => false,
        1 => true,
        _ => throw new InvalidDataException($"Resume-charging-beam sound word ${word:X4} is not a native latch state."),
    };

    /// <summary>The bit of <c>$0B18</c> that marks the Hyper Beam's glow; the glow's step count sits below it.</summary>
    private const int HyperBeamGlowMarker = 1 << 15;

    /// <summary>
    /// Decodes <c>$0B18</c> (#1682): zero for no glow, one through four for an ordinary charged shot's flash, and
    /// <see cref="HyperBeamGlowMarker"/> plus zero through twenty for the Hyper Beam's glow; other words are rejected.
    /// </summary>
    internal static ChargedShotGlow DecodeChargedShotGlow(ushort word) => word switch
    {
        0 => ChargedShotGlow.None,
        <= 4 => ChargedShotGlow.Of(ChargedShotGlowKind.ChargedShotFlash, word),
        >= HyperBeamGlowMarker and <= HyperBeamGlowMarker + 20 =>
            ChargedShotGlow.Of(ChargedShotGlowKind.HyperBeamGlow, word - HyperBeamGlowMarker),
        _ => throw new InvalidDataException($"Charged-shot glow word ${word:X4} is not a native glow state."),
    };

    /// <summary>Encodes a charged-shot glow as the <c>$0B18</c> word the game keeps it in.</summary>
    internal static ushort EncodeChargedShotGlow(ChargedShotGlow glow) => glow.Kind switch
    {
        ChargedShotGlowKind.None => 0,
        ChargedShotGlowKind.ChargedShotFlash => checked((ushort)glow.RemainingSteps),
        ChargedShotGlowKind.HyperBeamGlow => checked((ushort)(HyperBeamGlowMarker + glow.RemainingSteps)),
        _ => throw new InvalidOperationException($"Undefined {nameof(ChargedShotGlowKind)} {glow.Kind}."),
    };

    /// <summary>The <c>$0CD0</c>/<c>$0DC2</c> word the Hyper Beam's flare keeps in place of a charge count.</summary>
    private const ushort HyperBeamFlareWord = 1 << 15;

    /// <summary>
    /// Decodes a beam charge word, <c>$0CD0</c> or <c>$0DC2</c> (#1682): a count of zero through the special-attack clamp,
    /// or the Hyper Beam's flare; other words are rejected.
    /// </summary>
    internal static BeamChargeLevel DecodeBeamCharge(ushort word) => word switch
    {
        <= BeamChargeDefinitions.SpecialAttackCounter => BeamChargeLevel.Charged(word),
        HyperBeamFlareWord => BeamChargeLevel.HyperBeam,
        _ => throw new InvalidDataException($"Beam charge word ${word:X4} is neither a charge count nor the Hyper Beam's flare."),
    };

    /// <summary>Encodes a beam charge as the word the game keeps it in.</summary>
    internal static ushort EncodeBeamCharge(BeamChargeLevel charge) =>
        charge.HyperBeamFlare ? HyperBeamFlareWord : checked((ushort)charge.FlareCounter);

    /// <summary>The bit of <c>$0A46</c> that enables Samus's slope alignment after a horizontal move.</summary>
    private const int SlopeAlignmentBit = 1 << 1;

    /// <summary>
    /// Whether <c>$0A46</c> enables Samus's slope alignment after a horizontal move (#1682). The game writes only three,
    /// which enables it, outside a movement type no retail pose reaches, so the port has no state that disables it and
    /// a recorded word that does is not a state it can hold.
    /// </summary>
    internal static bool AlignsToSlopes(ushort word) => (word & SlopeAlignmentBit) != 0;

    /// <summary>The bits of a pose-history word's facing byte, below its movement byte.</summary>
    private const int HistoryFacingBits = 8;

    /// <summary>
    /// Decodes a pose-history word (<c>$0A22</c>/<c>$0A26</c>, #1473): the facing byte, then the movement byte, the
    /// movement type's dispatcher index. The port keeps only the movement; a movement byte the dispatcher lacks is rejected.
    /// </summary>
    internal static SamusMovementType DecodeHistoryMovement(int word)
    {
        int movement = (word >> HistoryFacingBits) & byte.MaxValue;
        return SamusMovementTypeNativeWords.Types.TryDecode(movement, out SamusMovementType type)
            ? type
            : throw new InvalidDataException($"Pose-history word ${word:X4} records movement byte ${movement:X2}, which no movement type has.");
    }
}
