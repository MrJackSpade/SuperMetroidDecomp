using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Development-tool table of the cartridge spritemaps Kraid's part frames are stored at; never linked by player hosts.</summary>
internal static class KraidPartFrameToolingDefinitions
{
    /// <summary>$A7:A565: the foot's standing pose.</summary>
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
    /// <summary>The installed extended spritemap of <paramref name="frame"/>, one of Kraid's part frames.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not one of Kraid's part frames.</exception>
    internal static ushort Spritemap(EnemyFrameId frame) => Spritemaps.TryGetValue(frame, out ushort spritemap)
        ? spritemap
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a Kraid part frame.");

    /// <summary>Each of Kraid's part frames by the extended spritemap the cartridge stores it at.</summary>
    private static readonly Dictionary<EnemyFrameId, ushort> Spritemaps = new()
    {
        [EnemyFrameId.KraidFootPlanted] = FootStanding,
        [EnemyFrameId.KraidFootPlantedTwin] = FootRoot(0),
        [EnemyFrameId.KraidFootPlantedOffset2By1] = FootRoot(1),
        [EnemyFrameId.KraidFootPlantedOffset4By2] = FootRoot(2),
        [EnemyFrameId.KraidFootPlantedOffset6By3] = FootRoot(3),
        [EnemyFrameId.KraidFootPlantedOffset8By4] = FootRoot(4),
        [EnemyFrameId.KraidFootLiftingOffset10By19] = FootRoot(5),
        [EnemyFrameId.KraidFootLiftingOffset12By20] = FootRoot(6),
        [EnemyFrameId.KraidFootLiftingOffset14By21] = FootRoot(7),
        [EnemyFrameId.KraidFootLiftingOffset16By22] = FootRoot(8),
        [EnemyFrameId.KraidFootLiftingOffset18By23] = FootRoot(9),
        [EnemyFrameId.KraidFootRaisedOffset20By26] = FootRoot(10),
        [EnemyFrameId.KraidFootSwingingOffset22By23] = FootRoot(11),
        [EnemyFrameId.KraidFootSwingingOffset24By21] = FootRoot(12),
        [EnemyFrameId.KraidFootSwingingOffset26By19] = FootRoot(13),
        [EnemyFrameId.KraidFootSwingingOffset28By17] = FootRoot(14),
        [EnemyFrameId.KraidFootSwingingOffset30By15] = FootRoot(15),
        [EnemyFrameId.KraidFootSwingingOffset32By13] = FootRoot(16),
        [EnemyFrameId.KraidFootSwingingOffset34By11] = FootRoot(17),
        [EnemyFrameId.KraidFootReachingOffset32By10] = FootRoot(18),
        [EnemyFrameId.KraidFootReachingOffset30By10] = FootRoot(19),
        [EnemyFrameId.KraidFootReachingOffset28By10] = FootRoot(20),
        [EnemyFrameId.KraidFootRaisedOffset26By15] = FootRoot(21),
        [EnemyFrameId.KraidFootRaisedOffset24By15] = FootRoot(22),
        [EnemyFrameId.KraidFootLiftingOffset22By14] = FootRoot(23),
        [EnemyFrameId.KraidFootLiftingOffset20By14] = FootRoot(24),
        [EnemyFrameId.KraidFootLiftingOffset18By14] = FootRoot(25),
        [EnemyFrameId.KraidFootLiftingOffset16By14] = FootRoot(26),
        [EnemyFrameId.KraidFootLiftingOffset14By14] = FootRoot(27),
        [EnemyFrameId.KraidFootLiftingOffset12By14] = FootRoot(28),
        [EnemyFrameId.KraidFootLiftingOffset10By14] = FootRoot(29),
        [EnemyFrameId.KraidFootLiftingOffset8By14] = FootRoot(30),
        [EnemyFrameId.KraidFootPlantedOffset6By0] = FootRoot(31),
        [EnemyFrameId.KraidFootPlantedOffset4By0] = FootRoot(32),
        [EnemyFrameId.KraidFootPlantedTwinB] = FootLast,
        [EnemyFrameId.KraidArmRaisedClawClosed] = ArmRoot(0),
        [EnemyFrameId.KraidArmRaisedClawOpen] = ArmRoot(1),
        [EnemyFrameId.KraidArmLoweringClawHooked] = ArmRoot(2),
        [EnemyFrameId.KraidArmLevelClawHooked] = ArmRoot(3),
        [EnemyFrameId.KraidArmLevelClawHangingDown] = ArmRoot(4),
        [EnemyFrameId.KraidArmLevelClawCurlingUnder] = ArmRoot(5),
        [EnemyFrameId.KraidArmLevelClawTucked] = ArmRoot(6),
        [EnemyFrameId.KraidArmLevelClawTuckedTight] = ArmRoot(7),
        [EnemyFrameId.KraidArmLevelClawFolded] = ArmRoot(8),
        [EnemyFrameId.KraidArmLevelClawFoldedFlat] = ArmRoot(9),
        [EnemyFrameId.KraidArmRaisedClawClosedTwin] = ArmRoot(10),
        [EnemyFrameId.KraidArmRaisedClawOpenTwin] = ArmRoot(11),
        [EnemyFrameId.KraidArmLoweringClawHookedTwin] = ArmRoot(12),
        [EnemyFrameId.KraidArmLevelClawHookedTwin] = ArmRoot(13),
        [EnemyFrameId.KraidArmLevelClawHangingDownTwin] = ArmRoot(14),
        [EnemyFrameId.KraidArmLevelClawCurlingUnderTwin] = ArmRoot(15),
        [EnemyFrameId.KraidArmLevelClawTuckedTwin] = ArmRoot(16),
        [EnemyFrameId.KraidArmLevelClawTuckedTightTwin] = ArmRoot(17),
        [EnemyFrameId.KraidArmLevelClawFoldedTwin] = ArmRoot(18),
        [EnemyFrameId.KraidArmLevelClawFoldedFlatTwin] = ArmRoot(19),
        [EnemyFrameId.KraidArmDroopingDiagonal] = ArmRoot(20),
        [EnemyFrameId.KraidArmOutstretched] = ArmLast,
        [EnemyFrameId.KraidLintBudding] = LintInitial,
        [EnemyFrameId.KraidLintGrown] = LintPostGrowth,
        [EnemyFrameId.KraidNailTurned0] = NailRoot(0),
        [EnemyFrameId.KraidNailTurned45] = NailRoot(1),
        [EnemyFrameId.KraidNailTurned90] = NailRoot(2),
        [EnemyFrameId.KraidNailTurned135] = NailRoot(3),
        [EnemyFrameId.KraidNailTurned180] = NailRoot(4),
        [EnemyFrameId.KraidNailTurned225] = NailRoot(5),
        [EnemyFrameId.KraidNailTurned270] = NailRoot(6),
        [EnemyFrameId.KraidNailTurned315] = NailRoot(7),
    };

    /// <summary>The foot pose stored <paramref name="index"/> eighteen-byte roots into the foot run.</summary>
    private static ushort FootRoot(int index) => unchecked((ushort)(FootStart + 18 * index));

    /// <summary>The arm pose stored <paramref name="index"/> forty-two-byte roots into the arm run.</summary>
    private static ushort ArmRoot(int index) => unchecked((ushort)(ArmStart + 42 * index));

    /// <summary>The fingernail pose <paramref name="index"/>: poses alternate twelve- and twenty-two-byte roots.</summary>
    private static ushort NailRoot(int index) => unchecked((ushort)(NailStart + 34 * (index / 2) + 12 * (index % 2)));
}
