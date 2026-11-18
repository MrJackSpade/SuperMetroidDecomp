using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$A6 and bank-$86 lists the Ridley ROM oracle (#1287) walks: the body, breakup and fireball list
/// entry points and the instructions their lists use. Only verification reads them; the port runs typed programs.
/// </summary>
internal static class RidleyNativeListAddresses
{
    /// <summary>Ridley's instruction bank, $A6.</summary>
    internal const byte BodyBankNumber = 0xa6;

    /// <summary>Each typed body program's native entry point in bank $A6, in authored order.</summary>
    internal static readonly BodyEntry[] BodyEntries =
    [
        new BodyEntry(RidleyBodySegment.StandingFacingLeft, 0xe538),
        new BodyEntry(RidleyBodySegment.StandingFacingRight, 0xe542),
        new BodyEntry(RidleyBodySegment.CeresLungeFacingLeft, 0xe548),
        new BodyEntry(RidleyBodySegment.CeresLungeFacingRight, 0xe576),
        new BodyEntry(RidleyBodySegment.RetrieveBabyMetroidFacingLeft, 0xe658),
        new BodyEntry(RidleyBodySegment.RetrieveBabyMetroidFacingRight, 0xe676),
        new BodyEntry(RidleyBodySegment.OpeningRoarFacingLeft, 0xe690),
        new BodyEntry(RidleyBodySegment.OpeningRoarFacingRight, 0xe6ae),
        new BodyEntry(RidleyBodySegment.DeathRoarFacingLeft, 0xe6c8),
        new BodyEntry(RidleyBodySegment.DeathRoarFacingRight, 0xe6de),
        new BodyEntry(RidleyBodySegment.TurnFromLeftToRight, 0xe6f0),
        new BodyEntry(RidleyBodySegment.TurnFromRightToLeft, 0xe706),
        new BodyEntry(RidleyBodySegment.FireballingFacingLeft, 0xe73a),
        new BodyEntry(RidleyBodySegment.FireballingEndFacingLeft, 0xe7ac),
        new BodyEntry(RidleyBodySegment.FireballingFacingRight, 0xe7b4),
        new BodyEntry(RidleyBodySegment.FireballingEndFacingRight, 0xe820),
        new BodyEntry(RidleyBodySegment.TransitionToFlyingFacingLeft, 0xe91d),
        new BodyEntry(RidleyBodySegment.TransitionToFlyingFacingRight, 0xe945),
    ];

    /// <summary>
    /// The bank-$A6 instruction that goes on to its operand's list unless Ridley faces left (#2175). A left-facing list opens
    /// with it, naming its right-facing counterpart; Ridley's owner starts the counterpart for his facing instead, so the
    /// instruction is no beat of either.
    /// </summary>
    internal const ushort GoToUnlessFacingLeft = 0xe517;

    /// <summary>Each bank-$A6 Ridley instruction the body lists use: the cue it must become and its operand words.</summary>
    internal static readonly Dictionary<ushort, BodyInstruction> BodyInstructions = new()
    {
        [0xe4be] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Signal(RidleyBodyCueKind.Roar)), 0),
        [0xe4ca] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Signal(RidleyBodyCueKind.StopRoaring)), 0),
        [0xe4d2] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Signal(RidleyBodyCueKind.MayCutFireballingShort)), 1),
        [0xe501] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Of<RidleyBodyCue.SetClawHeight>(""), 1),
        [0xe51f] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Of<RidleyBodyCue.Move>(""), 2),
        [0xe71c] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Flip(HorizontalDirection.Left)), 0),
        [0xe727] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Signal(RidleyBodyCueKind.FaceForward)), 0),
        [0xe72f] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Flip(HorizontalDirection.Right)), 0),
        [0xe84d] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Signal(RidleyBodyCueKind.AimFireball)), 0),
        [0xe904] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Signal(RidleyBodyCueKind.SpawnFireballWithAfterburn)), 0),
        [0xe909] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Signal(RidleyBodyCueKind.SpawnFireballWithoutAfterburn)), 0),
        // The cartridge has one lift-off routine per encounter; both set the same rise, and the port's one lift-off cue
        // leaves the rest to the encounter that owns the body (#2230).
        [0xe969] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Signal(RidleyBodyCueKind.LiftOff)), 0),
        [0xe976] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Signal(RidleyBodyCueKind.LiftOff)), 0),
    };

    /// <summary>Each breakup program's native one-frame list in bank $A6 ($A6:CA47..CAEF), in authored order.</summary>
    internal static readonly BreakupEntryValue[] BreakupEntries =
    [
        new BreakupEntryValue(EnemyFrameId.RidleyTailBaseLink, 0xca47),
        new BreakupEntryValue(EnemyFrameId.RidleyTailMiddleLink, 0xca4d),
        new BreakupEntryValue(EnemyFrameId.RidleyTailOuterLink, 0xca53),
        new BreakupEntryValue(EnemyFrameId.RidleyWingFacingLeftRaised, 0xca59),
        new BreakupEntryValue(EnemyFrameId.RidleyWingFacingRightRaised, 0xca5f),
        new BreakupEntryValue(EnemyFrameId.RidleyBreakupLegsLeft, 0xca65),
        new BreakupEntryValue(EnemyFrameId.RidleyBreakupLegsRight, 0xca6b),
        new BreakupEntryValue(EnemyFrameId.RidleyBreakupOpenHeadAndNeckLeft, 0xca71),
        new BreakupEntryValue(EnemyFrameId.RidleyBreakupOpenHeadAndNeckRight, 0xca77),
        new BreakupEntryValue(EnemyFrameId.RidleyBreakupTorsoLeft, 0xca7d),
        new BreakupEntryValue(EnemyFrameId.RidleyBreakupTorsoRight, 0xca83),
        new BreakupEntryValue(EnemyFrameId.RidleyBreakupClawLeft, 0xca89),
        new BreakupEntryValue(EnemyFrameId.RidleyBreakupClawRight, 0xca8f),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward180, 0xca95),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward157, 0xca9b),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward135, 0xcaa1),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward112, 0xcaa7),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward90, 0xcaad),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward67, 0xcab3),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward45, 0xcab9),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward22, 0xcabf),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward0, 0xcac5),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward337, 0xcacb),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward315, 0xcad1),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward292, 0xcad7),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward270, 0xcadd),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward247, 0xcae3),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward225, 0xcae9),
        new BreakupEntryValue(EnemyFrameId.RidleyTailTipPointingToward202, 0xcaef),
    ];

    /// <summary>The native list of breakup <paramref name="frame"/>.</summary>
    internal static ushort BreakupEntry(EnemyFrameId frame) => BreakupEntries.Single(item => item.Frame == frame).Entry;

    /// <summary>Each typed fireball program's native entry point in bank $86.</summary>
    internal static readonly FireballEntry[] FireballEntries =
    [
        new FireballEntry(RidleyFireballSegment.Fireball, 0x9552),
        // The flight is the part of the fireball's list its goto loops back to (#2235): after the launch's two poses.
        new FireballEntry(RidleyFireballSegment.FireballFlight, 0x9560),
        new FireballEntry(RidleyFireballSegment.AfterburnFinal, 0x9574),
        new FireballEntry(RidleyFireballSegment.HorizontalCenter, 0x95a0),
        new FireballEntry(RidleyFireballSegment.VerticalCenter, 0x95d3),
        new FireballEntry(RidleyFireballSegment.DirectionalAfterburn, 0x9606),
    ];

    /// <summary>The bank-$86 callbacks the fireball lists use and the cue each must become.</summary>
    internal static readonly Dictionary<ushort, RidleyFireballCue> FireballCallbacks = new()
    {
        [0x95ba] = RidleyFireballCue.SpawnHorizontalAfterburns,
        [0x95ed] = RidleyFireballCue.SpawnVerticalAfterburns,
        [0x9620] = RidleyFireballCue.SpawnNextAfterburn,
    };

    /// <summary>The body program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static RidleyBodySegment? BodyProgramAt(ushort address) =>
        BodyEntries.Where(item => item.Entry == address).Select(item => (RidleyBodySegment?)item.Program).SingleOrDefault();

    /// <summary>The fireball program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static RidleyFireballSegment? FireballProgramAt(ushort address) =>
        FireballEntries.Where(item => item.Entry == address).Select(item => (RidleyFireballSegment?)item.Program).SingleOrDefault();
}
