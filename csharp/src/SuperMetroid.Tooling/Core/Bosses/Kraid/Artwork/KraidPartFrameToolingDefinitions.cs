using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Spatial;
using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// Development-tool table of the cartridge spritemaps Kraid's part poses are stored at; never linked by player hosts. Many
/// records show one installed picture (#2210): the foot's walk steps each of its five pictures through a run of records,
/// and the arm's swing while Kraid rises or sinks draws the normal swing's pictures from records of its own.
/// </summary>
internal static class KraidPartFrameToolingDefinitions
{
    /// <summary>$A7:A565: the foot's standing pose before Kraid's rise ends.</summary>
    internal const ushort FootStanding = 0xa565;
    /// <summary>$A7:8CE3: the first of 33 eighteen-byte foot roots.</summary>
    internal const ushort FootStart = 0x8ce3;
    /// <summary>$A7:8F47: the last foot pose, one unused root past the run.</summary>
    internal const ushort FootLast = 0x8f47;
    /// <summary>$A7:8F59: the first of 21 forty-two-byte arm roots.</summary>
    internal const ushort ArmStart = 0x8f59;
    /// <summary>$A7:92AB: the last arm pose, outside the forty-two-byte run.</summary>
    internal const ushort ArmLast = 0x92ab;
    /// <summary>$A7:A5DF: the initial belly-lint pose.</summary>
    internal const ushort LintInitial = 0xa5df;
    /// <summary>$A7:8C6C: the belly-lint pose after Kraid grows.</summary>
    internal const ushort LintPostGrowth = 0x8c6c;
    /// <summary>$A7:A617: the first fingernail pose; poses alternate twelve- and twenty-two-byte roots.</summary>
    internal const ushort NailStart = 0xa617;

    /// <summary>
    /// The first extended spritemap the cartridge stores <paramref name="pose"/> at: the foot's resting pose is stored twice,
    /// at the start and at the end of its walk's run, and reads as the first.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is not one of Kraid's part poses.</exception>
    internal static ushort Spritemap(KraidPartPose pose)
    {
        foreach ((ushort spritemap, KraidPartPose stored) in Records)
        {
            if (stored == pose)
                return spritemap;
        }
        throw new ArgumentOutOfRangeException(nameof(pose), pose, "Not a Kraid part pose.");
    }

    /// <summary>The pose the extended spritemap <paramref name="spritemap"/> stores, or null when it stores none of Kraid's.</summary>
    internal static KraidPartPose? PoseAt(ushort spritemap)
    {
        foreach ((ushort stored, KraidPartPose pose) in Records)
        {
            if (stored == spritemap)
                return pose;
        }
        return null;
    }

    /// <summary>The extended spritemaps of the foot's and arm's poses (#2210): every record of theirs the animations show.</summary>
    internal static IEnumerable<ushort> LimbSpritemaps => Records.Select(record => record.Spritemap)
        .Where(spritemap => spritemap is >= FootStart and <= ArmLast or FootStanding);

    /// <summary>Each record Kraid's part animations show, with the pose it stores, in the cartridge's order.</summary>
    private static readonly (ushort Spritemap, KraidPartPose Pose)[] Records =
    [
        (FootRoot(0), Foot(F.KraidFootPlanted, new(0, 0))),
        (FootRoot(1), Foot(F.KraidFootPlanted, new(-2, -1))),
        (FootRoot(2), Foot(F.KraidFootPlanted, new(-4, -2))),
        (FootRoot(3), Foot(F.KraidFootPlanted, new(-6, -3))),
        (FootRoot(4), Foot(F.KraidFootPlanted, new(-8, -4))),
        (FootRoot(5), Foot(F.KraidFootLifting, new(0, 0))),
        (FootRoot(6), Foot(F.KraidFootLifting, new(-2, -1))),
        (FootRoot(7), Foot(F.KraidFootLifting, new(-4, -2))),
        (FootRoot(8), Foot(F.KraidFootLifting, new(-6, -3))),
        (FootRoot(9), Foot(F.KraidFootLifting, new(-8, -4))),
        (FootRoot(10), Foot(F.KraidFootRaised, new(0, 0))),
        (FootRoot(11), Foot(F.KraidFootSwinging, new(0, 0))),
        (FootRoot(12), Foot(F.KraidFootSwinging, new(-2, 2))),
        (FootRoot(13), Foot(F.KraidFootSwinging, new(-4, 4))),
        (FootRoot(14), Foot(F.KraidFootSwinging, new(-6, 6))),
        (FootRoot(15), Foot(F.KraidFootSwinging, new(-8, 8))),
        (FootRoot(16), Foot(F.KraidFootSwinging, new(-10, 10))),
        (FootRoot(17), Foot(F.KraidFootSwinging, new(-12, 12))),
        (FootRoot(18), Foot(F.KraidFootReaching, new(0, 0))),
        (FootRoot(19), Foot(F.KraidFootReaching, new(2, 0))),
        (FootRoot(20), Foot(F.KraidFootReaching, new(4, 0))),
        (FootRoot(21), Foot(F.KraidFootRaised, new(-6, 11))),
        (FootRoot(22), Foot(F.KraidFootRaised, new(-4, 11))),
        (FootRoot(23), Foot(F.KraidFootLifting, new(-12, 5))),
        (FootRoot(24), Foot(F.KraidFootLifting, new(-10, 5))),
        (FootRoot(25), Foot(F.KraidFootLifting, new(-8, 5))),
        (FootRoot(26), Foot(F.KraidFootLifting, new(-6, 5))),
        (FootRoot(27), Foot(F.KraidFootLifting, new(-4, 5))),
        (FootRoot(28), Foot(F.KraidFootLifting, new(-2, 5))),
        (FootRoot(29), Foot(F.KraidFootLifting, new(0, 5))),
        (FootRoot(30), Foot(F.KraidFootLifting, new(2, 5))),
        (FootRoot(31), Foot(F.KraidFootPlanted, new(-6, 0))),
        (FootRoot(32), Foot(F.KraidFootPlanted, new(-4, 0))),
        (FootLast, Foot(F.KraidFootPlanted, new(0, 0))),
        (FootStanding, Sunk(F.KraidFootPlanted)),
        (ArmRoot(0), Shown(F.KraidArmRaisedClawClosed)),
        (ArmRoot(1), Shown(F.KraidArmRaisedClawOpen)),
        (ArmRoot(2), Shown(F.KraidArmLoweringClawHooked)),
        (ArmRoot(3), Shown(F.KraidArmLevelClawHooked)),
        (ArmRoot(4), Shown(F.KraidArmLevelClawHangingDown)),
        (ArmRoot(5), Shown(F.KraidArmLevelClawCurlingUnder)),
        (ArmRoot(6), Shown(F.KraidArmLevelClawTucked)),
        (ArmRoot(7), Shown(F.KraidArmLevelClawTuckedTight)),
        (ArmRoot(8), Shown(F.KraidArmLevelClawFolded)),
        (ArmRoot(9), Shown(F.KraidArmLevelClawFoldedFlat)),
        (ArmRoot(10), Sunk(F.KraidArmRaisedClawClosed)),
        (ArmRoot(11), Sunk(F.KraidArmRaisedClawOpen)),
        (ArmRoot(12), Sunk(F.KraidArmLoweringClawHooked)),
        (ArmRoot(13), Sunk(F.KraidArmLevelClawHooked)),
        (ArmRoot(14), Sunk(F.KraidArmLevelClawHangingDown)),
        (ArmRoot(15), Sunk(F.KraidArmLevelClawCurlingUnder)),
        (ArmRoot(16), Sunk(F.KraidArmLevelClawTucked)),
        (ArmRoot(17), Sunk(F.KraidArmLevelClawTuckedTight)),
        (ArmRoot(18), Sunk(F.KraidArmLevelClawFolded)),
        (ArmRoot(19), Sunk(F.KraidArmLevelClawFoldedFlat)),
        (ArmRoot(20), Shown(F.KraidArmDroopingDiagonal)),
        (ArmLast, Shown(F.KraidArmOutstretched)),
        (LintInitial, Shown(F.KraidLintBudding)),
        (LintPostGrowth, Shown(F.KraidLintGrown)),
        (NailRoot(0), Shown(F.KraidNailTurned0)),
        (NailRoot(1), Shown(F.KraidNailTurned45)),
        (NailRoot(2), Shown(F.KraidNailTurned90)),
        (NailRoot(3), Shown(F.KraidNailTurned135)),
        (NailRoot(4), Shown(F.KraidNailTurned180)),
        (NailRoot(5), Shown(F.KraidNailTurned225)),
        (NailRoot(6), Shown(F.KraidNailTurned270)),
        (NailRoot(7), Shown(F.KraidNailTurned315)),
    ];

    /// <summary>The pose showing <paramref name="picture"/> where the part stands.</summary>
    private static KraidPartPose Shown(EnemyFrameId picture) => new(picture);

    /// <summary>The foot's <paramref name="picture"/> moved by <paramref name="step"/>.</summary>
    private static KraidPartPose Foot(EnemyFrameId picture, Offset16<RoomPixels> step) => new(picture) { Placement = step };

    /// <summary>The pose showing <paramref name="picture"/> behind the room's front tiles.</summary>
    private static KraidPartPose Sunk(EnemyFrameId picture) => new(picture) { Depth = ActorDepth.BehindFrontParts };

    /// <summary>The foot pose stored <paramref name="index"/> eighteen-byte roots into the foot run.</summary>
    private static ushort FootRoot(int index) => unchecked((ushort)(FootStart + 18 * index));

    /// <summary>The arm pose stored <paramref name="index"/> forty-two-byte roots into the arm run.</summary>
    private static ushort ArmRoot(int index) => unchecked((ushort)(ArmStart + 42 * index));

    /// <summary>The fingernail pose <paramref name="index"/>: poses alternate twelve- and twenty-two-byte roots.</summary>
    private static ushort NailRoot(int index) => unchecked((ushort)(NailStart + 34 * (index / 2) + 12 * (index % 2)));
}
