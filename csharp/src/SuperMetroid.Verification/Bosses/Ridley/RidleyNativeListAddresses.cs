using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.Verification.Bosses.Ridley;

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
        new BodyEntry(RidleyBodyProgram.Initial, 0xe538),
        new BodyEntry(RidleyBodyProgram.InitialFacingRight, 0xe542),
        new BodyEntry(RidleyBodyProgram.CeresLunge, 0xe548),
        new BodyEntry(RidleyBodyProgram.CeresLungeFacingRight, 0xe576),
        new BodyEntry(RidleyBodyProgram.RetrieveBabyMetroid, 0xe658),
        new BodyEntry(RidleyBodyProgram.RetrieveBabyMetroidFacingRight, 0xe676),
        new BodyEntry(RidleyBodyProgram.OpeningRoar, 0xe690),
        new BodyEntry(RidleyBodyProgram.OpeningRoarFacingRight, 0xe6ae),
        new BodyEntry(RidleyBodyProgram.DeathRoar, 0xe6c8),
        new BodyEntry(RidleyBodyProgram.DeathRoarFacingRight, 0xe6de),
        new BodyEntry(RidleyBodyProgram.TurnFromLeftToRight, 0xe6f0),
        new BodyEntry(RidleyBodyProgram.TurnFromRightToLeft, 0xe706),
        new BodyEntry(RidleyBodyProgram.Fireballing, 0xe73a),
        new BodyEntry(RidleyBodyProgram.FireballingEnd, 0xe7ac),
        new BodyEntry(RidleyBodyProgram.FireballingFacingRight, 0xe7b4),
        new BodyEntry(RidleyBodyProgram.FireballingEndFacingRight, 0xe820),
        new BodyEntry(RidleyBodyProgram.TransitionToFlying, 0xe91d),
        new BodyEntry(RidleyBodyProgram.TransitionToFlyingFacingRight, 0xe945),
    ];

    /// <summary>Each bank-$A6 Ridley instruction the body lists use: the cue it must become and its operand words.</summary>
    internal static readonly Dictionary<ushort, BodyInstruction> BodyInstructions = new()
    {
        [0xe4be] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Command(RidleyBodyCueKind.Roar)), 0),
        [0xe4ca] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Command(RidleyBodyCueKind.ClearRoaringFlag)), 0),
        [0xe4d2] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Of<RidleyBodyCue.GoToIfCeresAndSamusLowEnergy>(""), 1),
        [0xe501] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Of<RidleyBodyCue.SetFeetDistance>(""), 1),
        [0xe517] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Of<RidleyBodyCue.GoToUnlessFacingLeft>(""), 1),
        [0xe51f] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Of<RidleyBodyCue.Move>(""), 2),
        [0xe71c] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Command(RidleyBodyCueKind.FlipLeft)), 0),
        [0xe727] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Command(RidleyBodyCueKind.FaceForward)), 0),
        [0xe72f] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Command(RidleyBodyCueKind.FlipRight)), 0),
        [0xe84d] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Command(RidleyBodyCueKind.AimFireball)), 0),
        [0xe904] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Command(RidleyBodyCueKind.SpawnFireballWithAfterburn)), 0),
        [0xe909] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Command(RidleyBodyCueKind.SpawnFireballWithoutAfterburn)), 0),
        [0xe969] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Command(RidleyBodyCueKind.CeresStartLiftoff)), 0),
        [0xe976] = new BodyInstruction(NativeCueShape<RidleyBodyCue>.Exactly(new RidleyBodyCue.Command(RidleyBodyCueKind.StartLiftoff)), 0),
    };

    /// <summary>Each breakup program's native one-frame list in bank $A6 ($A6:CA47..CAEF), in authored order.</summary>
    internal static readonly BreakupEntryValue[] BreakupEntries =
    [
        new BreakupEntryValue(RidleyBreakupFrame.TailSegmentSmall, 0xca47),
        new BreakupEntryValue(RidleyBreakupFrame.TailSegmentMedium, 0xca4d),
        new BreakupEntryValue(RidleyBreakupFrame.TailSegmentLarge, 0xca53),
        new BreakupEntryValue(RidleyBreakupFrame.WingsFacingLeft, 0xca59),
        new BreakupEntryValue(RidleyBreakupFrame.WingsFacingRight, 0xca5f),
        new BreakupEntryValue(RidleyBreakupFrame.LegsFacingLeft, 0xca65),
        new BreakupEntryValue(RidleyBreakupFrame.LegsFacingRight, 0xca6b),
        new BreakupEntryValue(RidleyBreakupFrame.OpenHeadAndNeckFacingLeft, 0xca71),
        new BreakupEntryValue(RidleyBreakupFrame.OpenHeadAndNeckFacingRight, 0xca77),
        new BreakupEntryValue(RidleyBreakupFrame.TorsoFacingLeft, 0xca7d),
        new BreakupEntryValue(RidleyBreakupFrame.TorsoFacingRight, 0xca83),
        new BreakupEntryValue(RidleyBreakupFrame.ClawFacingLeft, 0xca89),
        new BreakupEntryValue(RidleyBreakupFrame.ClawFacingRight, 0xca8f),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip0, 0xca95),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip1, 0xca9b),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip2, 0xcaa1),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip3, 0xcaa7),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip4, 0xcaad),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip5, 0xcab3),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip6, 0xcab9),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip7, 0xcabf),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip8, 0xcac5),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip9, 0xcacb),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip10, 0xcad1),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip11, 0xcad7),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip12, 0xcadd),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip13, 0xcae3),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip14, 0xcae9),
        new BreakupEntryValue(RidleyBreakupFrame.TailTip15, 0xcaef),
    ];

    /// <summary>The native list of breakup <paramref name="frame"/>.</summary>
    internal static ushort BreakupEntry(RidleyBreakupFrame frame) => BreakupEntries.Single(item => item.Frame == frame).Entry;

    /// <summary>Each typed fireball program's native entry point in bank $86.</summary>
    internal static readonly FireballEntry[] FireballEntries =
    [
        new FireballEntry(RidleyFireballProgram.Fireball, 0x9552),
        new FireballEntry(RidleyFireballProgram.FireballLoop, 0x9560),
        new FireballEntry(RidleyFireballProgram.AfterburnFinal, 0x9574),
        new FireballEntry(RidleyFireballProgram.HorizontalCenter, 0x95a0),
        new FireballEntry(RidleyFireballProgram.VerticalCenter, 0x95d3),
        new FireballEntry(RidleyFireballProgram.DirectionalAfterburn, 0x9606),
    ];

    /// <summary>The bank-$86 callbacks the fireball lists use and the cue each must become.</summary>
    internal static readonly Dictionary<ushort, RidleyFireballCue> FireballCallbacks = new()
    {
        [0x95ba] = RidleyFireballCue.SpawnHorizontalAfterburns,
        [0x95ed] = RidleyFireballCue.SpawnVerticalAfterburns,
        [0x9620] = RidleyFireballCue.SpawnNextAfterburn,
    };

    /// <summary>The body program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static RidleyBodyProgram? BodyProgramAt(ushort address) =>
        BodyEntries.Where(item => item.Entry == address).Select(item => (RidleyBodyProgram?)item.Program).SingleOrDefault();

    /// <summary>The fireball program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static RidleyFireballProgram? FireballProgramAt(ushort address) =>
        FireballEntries.Where(item => item.Entry == address).Select(item => (RidleyFireballProgram?)item.Program).SingleOrDefault();
}
