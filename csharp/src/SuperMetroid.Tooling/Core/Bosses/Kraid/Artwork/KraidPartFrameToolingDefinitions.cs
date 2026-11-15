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
        [EnemyFrameId.KraidFootStanding] = FootStanding,
        [EnemyFrameId.KraidFootNeutral] = FootRoot(0),
        [EnemyFrameId.KraidFootWalkingForwardSecond] = FootRoot(1),
        [EnemyFrameId.KraidFootWalkingForwardThird] = FootRoot(2),
        [EnemyFrameId.KraidFootWalkingForwardFourth] = FootRoot(3),
        [EnemyFrameId.KraidFootWalkingForwardFifth] = FootRoot(4),
        [EnemyFrameId.KraidFootWalkingForwardSixth] = FootRoot(5),
        [EnemyFrameId.KraidFootWalkingForwardSeventh] = FootRoot(6),
        [EnemyFrameId.KraidFootWalkingForwardEighth] = FootRoot(7),
        [EnemyFrameId.KraidFootWalkingForwardNinth] = FootRoot(8),
        [EnemyFrameId.KraidFootWalkingForwardTenth] = FootRoot(9),
        [EnemyFrameId.KraidFootWalkingForwardEleventh] = FootRoot(10),
        [EnemyFrameId.KraidFootWalkingForwardTwelfth] = FootRoot(11),
        [EnemyFrameId.KraidFootWalkingForwardThirteenth] = FootRoot(12),
        [EnemyFrameId.KraidFootWalkingForwardFourteenth] = FootRoot(13),
        [EnemyFrameId.KraidFootWalkingForwardFifteenth] = FootRoot(14),
        [EnemyFrameId.KraidFootWalkingForwardSixteenth] = FootRoot(15),
        [EnemyFrameId.KraidFootWalkingForwardSeventeenth] = FootRoot(16),
        [EnemyFrameId.KraidFootWalkingForwardEighteenth] = FootRoot(17),
        [EnemyFrameId.KraidFootWalkingForwardNineteenth] = FootRoot(18),
        [EnemyFrameId.KraidFootWalkingForwardTwentieth] = FootRoot(19),
        [EnemyFrameId.KraidFootWalkingBackwardThirteenth] = FootRoot(20),
        [EnemyFrameId.KraidFootWalkingForwardTwentyFirst] = FootRoot(21),
        [EnemyFrameId.KraidFootWalkingForwardTwentySecond] = FootRoot(22),
        [EnemyFrameId.KraidFootWalkingForwardTwentyThird] = FootRoot(23),
        [EnemyFrameId.KraidFootWalkingForwardTwentyFourth] = FootRoot(24),
        [EnemyFrameId.KraidFootWalkingForwardTwentyFifth] = FootRoot(25),
        [EnemyFrameId.KraidFootWalkingForwardTwentySixth] = FootRoot(26),
        [EnemyFrameId.KraidFootWalkingForwardTwentySeventh] = FootRoot(27),
        [EnemyFrameId.KraidFootWalkingForwardTwentyEighth] = FootRoot(28),
        [EnemyFrameId.KraidFootWalkingForwardTwentyNinth] = FootRoot(29),
        [EnemyFrameId.KraidFootWalkingForwardThirtieth] = FootRoot(30),
        [EnemyFrameId.KraidFootWalkingForwardThirtyFirst] = FootRoot(31),
        [EnemyFrameId.KraidFootWalkingBackwardSecond] = FootRoot(32),
        [EnemyFrameId.KraidFootWalkingBackwardFirst] = FootLast,
        [EnemyFrameId.KraidArmNormalFirst] = ArmRoot(0),
        [EnemyFrameId.KraidArmNormalSecond] = ArmRoot(1),
        [EnemyFrameId.KraidArmNormalThird] = ArmRoot(2),
        [EnemyFrameId.KraidArmNormalFourth] = ArmRoot(3),
        [EnemyFrameId.KraidArmNormalFifth] = ArmRoot(4),
        [EnemyFrameId.KraidArmNormalSixth] = ArmRoot(5),
        [EnemyFrameId.KraidArmNormalSeventh] = ArmRoot(6),
        [EnemyFrameId.KraidArmNormalEighth] = ArmRoot(7),
        [EnemyFrameId.KraidArmNormalNinth] = ArmRoot(8),
        [EnemyFrameId.KraidArmNormalTenth] = ArmRoot(9),
        [EnemyFrameId.KraidArmRisingOrSinkingFirst] = ArmRoot(10),
        [EnemyFrameId.KraidArmRisingOrSinkingSecond] = ArmRoot(11),
        [EnemyFrameId.KraidArmRisingOrSinkingThird] = ArmRoot(12),
        [EnemyFrameId.KraidArmRisingOrSinkingFourth] = ArmRoot(13),
        [EnemyFrameId.KraidArmRisingOrSinkingFifth] = ArmRoot(14),
        [EnemyFrameId.KraidArmRisingOrSinkingSixth] = ArmRoot(15),
        [EnemyFrameId.KraidArmRisingOrSinkingSeventh] = ArmRoot(16),
        [EnemyFrameId.KraidArmRisingOrSinkingEighth] = ArmRoot(17),
        [EnemyFrameId.KraidArmRisingOrSinkingNinth] = ArmRoot(18),
        [EnemyFrameId.KraidArmRisingOrSinkingTenth] = ArmRoot(19),
        [EnemyFrameId.KraidArmDyingOrPreparingToLungeSecond] = ArmRoot(20),
        [EnemyFrameId.KraidArmDyingOrPreparingToLungeThird] = ArmLast,
        [EnemyFrameId.KraidLintInitial] = LintInitial,
        [EnemyFrameId.KraidLintPostGrowth] = LintPostGrowth,
        [EnemyFrameId.KraidNailLoopFirst] = NailRoot(0),
        [EnemyFrameId.KraidNailLoopSecond] = NailRoot(1),
        [EnemyFrameId.KraidNailLoopThird] = NailRoot(2),
        [EnemyFrameId.KraidNailLoopFourth] = NailRoot(3),
        [EnemyFrameId.KraidNailLoopFifth] = NailRoot(4),
        [EnemyFrameId.KraidNailLoopSixth] = NailRoot(5),
        [EnemyFrameId.KraidNailLoopSeventh] = NailRoot(6),
        [EnemyFrameId.KraidNailLoopEighth] = NailRoot(7),
    };

    /// <summary>The foot pose stored <paramref name="index"/> eighteen-byte roots into the foot run.</summary>
    private static ushort FootRoot(int index) => unchecked((ushort)(FootStart + 18 * index));

    /// <summary>The arm pose stored <paramref name="index"/> forty-two-byte roots into the arm run.</summary>
    private static ushort ArmRoot(int index) => unchecked((ushort)(ArmStart + 42 * index));

    /// <summary>The fingernail pose <paramref name="index"/>: poses alternate twelve- and twenty-two-byte roots.</summary>
    private static ushort NailRoot(int index) => unchecked((ushort)(NailStart + 34 * (index / 2) + 12 * (index % 2)));
}
