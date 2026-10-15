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
    internal static readonly (RidleyBodyProgram Program, ushort Entry)[] BodyEntries =
    [
        (RidleyBodyProgram.Initial, 0xe538),
        (RidleyBodyProgram.InitialFacingRight, 0xe542),
        (RidleyBodyProgram.CeresLunge, 0xe548),
        (RidleyBodyProgram.CeresLungeFacingRight, 0xe576),
        (RidleyBodyProgram.RetrieveBabyMetroid, 0xe658),
        (RidleyBodyProgram.RetrieveBabyMetroidFacingRight, 0xe676),
        (RidleyBodyProgram.OpeningRoar, 0xe690),
        (RidleyBodyProgram.OpeningRoarFacingRight, 0xe6ae),
        (RidleyBodyProgram.DeathRoar, 0xe6c8),
        (RidleyBodyProgram.DeathRoarFacingRight, 0xe6de),
        (RidleyBodyProgram.TurnFromLeftToRight, 0xe6f0),
        (RidleyBodyProgram.TurnFromRightToLeft, 0xe706),
        (RidleyBodyProgram.Fireballing, 0xe73a),
        (RidleyBodyProgram.FireballingEnd, 0xe7ac),
        (RidleyBodyProgram.FireballingFacingRight, 0xe7b4),
        (RidleyBodyProgram.FireballingEndFacingRight, 0xe820),
        (RidleyBodyProgram.TransitionToFlying, 0xe91d),
        (RidleyBodyProgram.TransitionToFlyingFacingRight, 0xe945),
    ];

    /// <summary>Each bank-$A6 Ridley instruction the body lists use: the cue it must become and its operand words.</summary>
    internal static readonly Dictionary<ushort, (RidleyBodyCueKind Kind, int Operands)> BodyInstructions = new()
    {
        [0xe4be] = (RidleyBodyCueKind.Roar, 0),
        [0xe4ca] = (RidleyBodyCueKind.ClearRoaringFlag, 0),
        [0xe4d2] = (RidleyBodyCueKind.GoToIfCeresAndSamusLowEnergy, 1),
        [0xe501] = (RidleyBodyCueKind.SetFeetDistance, 1),
        [0xe517] = (RidleyBodyCueKind.GoToUnlessFacingLeft, 1),
        [0xe51f] = (RidleyBodyCueKind.Move, 2),
        [0xe71c] = (RidleyBodyCueKind.FlipLeft, 0),
        [0xe727] = (RidleyBodyCueKind.FaceForward, 0),
        [0xe72f] = (RidleyBodyCueKind.FlipRight, 0),
        [0xe84d] = (RidleyBodyCueKind.AimFireball, 0),
        [0xe904] = (RidleyBodyCueKind.SpawnFireballWithAfterburn, 0),
        [0xe909] = (RidleyBodyCueKind.SpawnFireballWithoutAfterburn, 0),
        [0xe969] = (RidleyBodyCueKind.CeresStartLiftoff, 0),
        [0xe976] = (RidleyBodyCueKind.StartLiftoff, 0),
    };

    /// <summary>Each breakup program's native one-frame list in bank $A6 ($A6:CA47..CAEF), in authored order.</summary>
    internal static readonly (RidleyBreakupFrame Frame, ushort Entry)[] BreakupEntries =
    [
        (RidleyBreakupFrame.TailSegmentSmall, 0xca47),
        (RidleyBreakupFrame.TailSegmentMedium, 0xca4d),
        (RidleyBreakupFrame.TailSegmentLarge, 0xca53),
        (RidleyBreakupFrame.WingsFacingLeft, 0xca59),
        (RidleyBreakupFrame.WingsFacingRight, 0xca5f),
        (RidleyBreakupFrame.LegsFacingLeft, 0xca65),
        (RidleyBreakupFrame.LegsFacingRight, 0xca6b),
        (RidleyBreakupFrame.OpenHeadAndNeckFacingLeft, 0xca71),
        (RidleyBreakupFrame.OpenHeadAndNeckFacingRight, 0xca77),
        (RidleyBreakupFrame.TorsoFacingLeft, 0xca7d),
        (RidleyBreakupFrame.TorsoFacingRight, 0xca83),
        (RidleyBreakupFrame.ClawFacingLeft, 0xca89),
        (RidleyBreakupFrame.ClawFacingRight, 0xca8f),
        (RidleyBreakupFrame.TailTip0, 0xca95),
        (RidleyBreakupFrame.TailTip1, 0xca9b),
        (RidleyBreakupFrame.TailTip2, 0xcaa1),
        (RidleyBreakupFrame.TailTip3, 0xcaa7),
        (RidleyBreakupFrame.TailTip4, 0xcaad),
        (RidleyBreakupFrame.TailTip5, 0xcab3),
        (RidleyBreakupFrame.TailTip6, 0xcab9),
        (RidleyBreakupFrame.TailTip7, 0xcabf),
        (RidleyBreakupFrame.TailTip8, 0xcac5),
        (RidleyBreakupFrame.TailTip9, 0xcacb),
        (RidleyBreakupFrame.TailTip10, 0xcad1),
        (RidleyBreakupFrame.TailTip11, 0xcad7),
        (RidleyBreakupFrame.TailTip12, 0xcadd),
        (RidleyBreakupFrame.TailTip13, 0xcae3),
        (RidleyBreakupFrame.TailTip14, 0xcae9),
        (RidleyBreakupFrame.TailTip15, 0xcaef),
    ];

    /// <summary>The native list of breakup <paramref name="frame"/>.</summary>
    internal static ushort BreakupEntry(RidleyBreakupFrame frame) => BreakupEntries.Single(item => item.Frame == frame).Entry;

    /// <summary>Each typed fireball program's native entry point in bank $86.</summary>
    internal static readonly (RidleyFireballProgram Program, ushort Entry)[] FireballEntries =
    [
        (RidleyFireballProgram.Fireball, 0x9552),
        (RidleyFireballProgram.FireballLoop, 0x9560),
        (RidleyFireballProgram.AfterburnFinal, 0x9574),
        (RidleyFireballProgram.HorizontalCenter, 0x95a0),
        (RidleyFireballProgram.VerticalCenter, 0x95d3),
        (RidleyFireballProgram.DirectionalAfterburn, 0x9606),
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
