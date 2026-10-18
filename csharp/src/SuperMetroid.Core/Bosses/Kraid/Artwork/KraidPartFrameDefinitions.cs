using SuperMetroid.Core.Enemies.Artwork;


namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The presentation boundary of Kraid's foot, arm, lints and fingernails (#641): the bank-$A7 extended spritemaps
/// their typed programs show. Gameplay animates <see cref="KraidPartFrame"/> values; only this catalog knows the
/// spritemaps.
/// </summary>
internal static class KraidPartFrameDefinitions
{
    /// <summary>$A7:A565: the foot's standing pose.</summary>
    private const ushort FootStanding = 0xa565;
    /// <summary>$A7:8CE3: the first of 33 eighteen-byte foot roots.</summary>
    private const ushort FootStart = 0x8ce3;
    /// <summary>$A7:8F47: the last foot pose, one unused root past the run.</summary>
    private const ushort FootLast = 0x8f47;
    /// <summary>The foot poses in the eighteen-byte run.</summary>
    private const int FootRunCount = 33;
    /// <summary>$A7:8F59: the first of 21 forty-two-byte arm roots.</summary>
    private const ushort ArmStart = 0x8f59;
    /// <summary>$A7:92AB: the last arm pose, outside the forty-two-byte run.</summary>
    private const ushort ArmLast = 0x92ab;
    /// <summary>The arm poses in the forty-two-byte run.</summary>
    private const int ArmRunCount = 21;
    /// <summary>$A7:A5DF: the initial belly-lint pose.</summary>
    private const ushort LintInitial = 0xa5df;
    /// <summary>$A7:8C6C: the belly-lint pose after Kraid grows.</summary>
    private const ushort LintPostGrowth = 0x8c6c;
    /// <summary>$A7:A617: the first fingernail pose; poses alternate twelve- and twenty-two-byte roots.</summary>
    private const ushort NailStart = 0xa617;
    /// <summary>The fingernail poses.</summary>
    private const int NailCount = 8;

    /// <summary>The installed extended spritemap of <paramref name="frame"/>.</summary>
    internal static ushort Spritemap(KraidPartFrame frame) => frame switch
    {
        { Kind: KraidPartFrameKind.FootStanding, Index: 0 } => FootStanding,
        { Kind: KraidPartFrameKind.Foot, Index: >= 0 and < FootRunCount } => unchecked((ushort)(FootStart + 18 * frame.Index)),
        { Kind: KraidPartFrameKind.Foot, Index: FootRunCount } => FootLast,
        { Kind: KraidPartFrameKind.Arm, Index: >= 0 and < ArmRunCount } => unchecked((ushort)(ArmStart + 42 * frame.Index)),
        { Kind: KraidPartFrameKind.Arm, Index: ArmRunCount } => ArmLast,
        { Kind: KraidPartFrameKind.Lint, Index: 0 } => LintInitial,
        { Kind: KraidPartFrameKind.Lint, Index: 1 } => LintPostGrowth,
        { Kind: KraidPartFrameKind.Nail, Index: >= 0 and < NailCount } =>
            unchecked((ushort)(NailStart + 34 * (frame.Index / 2) + 12 * (frame.Index % 2))),
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Kraid part frame."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(KraidPartFrame frame) => frame switch
    {
        { Kind: KraidPartFrameKind.Arm, Index: 0 } => EnemyFrameId.KraidArmOam8F59,
        { Kind: KraidPartFrameKind.Arm, Index: 1 } => EnemyFrameId.KraidArmOam8F83,
        { Kind: KraidPartFrameKind.Arm, Index: 10 } => EnemyFrameId.KraidArmOam90FD,
        { Kind: KraidPartFrameKind.Arm, Index: 11 } => EnemyFrameId.KraidArmOam9127,
        { Kind: KraidPartFrameKind.Arm, Index: 12 } => EnemyFrameId.KraidArmOam9151,
        { Kind: KraidPartFrameKind.Arm, Index: 13 } => EnemyFrameId.KraidArmOam917B,
        { Kind: KraidPartFrameKind.Arm, Index: 14 } => EnemyFrameId.KraidArmOam91A5,
        { Kind: KraidPartFrameKind.Arm, Index: 15 } => EnemyFrameId.KraidArmOam91CF,
        { Kind: KraidPartFrameKind.Arm, Index: 16 } => EnemyFrameId.KraidArmOam91F9,
        { Kind: KraidPartFrameKind.Arm, Index: 17 } => EnemyFrameId.KraidArmOam9223,
        { Kind: KraidPartFrameKind.Arm, Index: 18 } => EnemyFrameId.KraidArmOam924D,
        { Kind: KraidPartFrameKind.Arm, Index: 19 } => EnemyFrameId.KraidArmOam9277,
        { Kind: KraidPartFrameKind.Arm, Index: 2 } => EnemyFrameId.KraidArmOam8FAD,
        { Kind: KraidPartFrameKind.Arm, Index: 20 } => EnemyFrameId.KraidArmOam92A1,
        { Kind: KraidPartFrameKind.Arm, Index: 21 } => EnemyFrameId.KraidArmOam92AB,
        { Kind: KraidPartFrameKind.Arm, Index: 3 } => EnemyFrameId.KraidArmOam8FD7,
        { Kind: KraidPartFrameKind.Arm, Index: 4 } => EnemyFrameId.KraidArmOam9001,
        { Kind: KraidPartFrameKind.Arm, Index: 5 } => EnemyFrameId.KraidArmOam902B,
        { Kind: KraidPartFrameKind.Arm, Index: 6 } => EnemyFrameId.KraidArmOam9055,
        { Kind: KraidPartFrameKind.Arm, Index: 7 } => EnemyFrameId.KraidArmOam907F,
        { Kind: KraidPartFrameKind.Arm, Index: 8 } => EnemyFrameId.KraidArmOam90A9,
        { Kind: KraidPartFrameKind.Arm, Index: 9 } => EnemyFrameId.KraidArmOam90D3,
        { Kind: KraidPartFrameKind.Foot, Index: 0 } => EnemyFrameId.KraidFootOam8CE3,
        { Kind: KraidPartFrameKind.Foot, Index: 1 } => EnemyFrameId.KraidFootOam8CF5,
        { Kind: KraidPartFrameKind.Foot, Index: 10 } => EnemyFrameId.KraidFootOam8D97,
        { Kind: KraidPartFrameKind.Foot, Index: 11 } => EnemyFrameId.KraidFootOam8DA9,
        { Kind: KraidPartFrameKind.Foot, Index: 12 } => EnemyFrameId.KraidFootOam8DBB,
        { Kind: KraidPartFrameKind.Foot, Index: 13 } => EnemyFrameId.KraidFootOam8DCD,
        { Kind: KraidPartFrameKind.Foot, Index: 14 } => EnemyFrameId.KraidFootOam8DDF,
        { Kind: KraidPartFrameKind.Foot, Index: 15 } => EnemyFrameId.KraidFootOam8DF1,
        { Kind: KraidPartFrameKind.Foot, Index: 16 } => EnemyFrameId.KraidFootOam8E03,
        { Kind: KraidPartFrameKind.Foot, Index: 17 } => EnemyFrameId.KraidFootOam8E15,
        { Kind: KraidPartFrameKind.Foot, Index: 18 } => EnemyFrameId.KraidFootOam8E27,
        { Kind: KraidPartFrameKind.Foot, Index: 19 } => EnemyFrameId.KraidFootOam8E39,
        { Kind: KraidPartFrameKind.Foot, Index: 2 } => EnemyFrameId.KraidFootOam8D07,
        { Kind: KraidPartFrameKind.Foot, Index: 20 } => EnemyFrameId.KraidFootOam8E4B,
        { Kind: KraidPartFrameKind.Foot, Index: 21 } => EnemyFrameId.KraidFootOam8E5D,
        { Kind: KraidPartFrameKind.Foot, Index: 22 } => EnemyFrameId.KraidFootOam8E6F,
        { Kind: KraidPartFrameKind.Foot, Index: 23 } => EnemyFrameId.KraidFootOam8E81,
        { Kind: KraidPartFrameKind.Foot, Index: 24 } => EnemyFrameId.KraidFootOam8E93,
        { Kind: KraidPartFrameKind.Foot, Index: 25 } => EnemyFrameId.KraidFootOam8EA5,
        { Kind: KraidPartFrameKind.Foot, Index: 26 } => EnemyFrameId.KraidFootOam8EB7,
        { Kind: KraidPartFrameKind.Foot, Index: 27 } => EnemyFrameId.KraidFootOam8EC9,
        { Kind: KraidPartFrameKind.Foot, Index: 28 } => EnemyFrameId.KraidFootOam8EDB,
        { Kind: KraidPartFrameKind.Foot, Index: 29 } => EnemyFrameId.KraidFootOam8EED,
        { Kind: KraidPartFrameKind.Foot, Index: 3 } => EnemyFrameId.KraidFootOam8D19,
        { Kind: KraidPartFrameKind.Foot, Index: 30 } => EnemyFrameId.KraidFootOam8EFF,
        { Kind: KraidPartFrameKind.Foot, Index: 31 } => EnemyFrameId.KraidFootOam8F11,
        { Kind: KraidPartFrameKind.Foot, Index: 32 } => EnemyFrameId.KraidFootOam8F23,
        { Kind: KraidPartFrameKind.Foot, Index: 33 } => EnemyFrameId.KraidFootOam8F47,
        { Kind: KraidPartFrameKind.Foot, Index: 4 } => EnemyFrameId.KraidFootOam8D2B,
        { Kind: KraidPartFrameKind.Foot, Index: 5 } => EnemyFrameId.KraidFootOam8D3D,
        { Kind: KraidPartFrameKind.Foot, Index: 6 } => EnemyFrameId.KraidFootOam8D4F,
        { Kind: KraidPartFrameKind.Foot, Index: 7 } => EnemyFrameId.KraidFootOam8D61,
        { Kind: KraidPartFrameKind.Foot, Index: 8 } => EnemyFrameId.KraidFootOam8D73,
        { Kind: KraidPartFrameKind.Foot, Index: 9 } => EnemyFrameId.KraidFootOam8D85,
        { Kind: KraidPartFrameKind.FootStanding, Index: 0 } => EnemyFrameId.KraidFootOamA565,
        { Kind: KraidPartFrameKind.Lint, Index: 0 } => EnemyFrameId.KraidLint00,
        { Kind: KraidPartFrameKind.Lint, Index: 1 } => EnemyFrameId.KraidLint01,
        { Kind: KraidPartFrameKind.Nail, Index: 0 } => EnemyFrameId.KraidNail0,
        { Kind: KraidPartFrameKind.Nail, Index: 1 } => EnemyFrameId.KraidNail1,
        { Kind: KraidPartFrameKind.Nail, Index: 2 } => EnemyFrameId.KraidNail2,
        { Kind: KraidPartFrameKind.Nail, Index: 3 } => EnemyFrameId.KraidNail3,
        { Kind: KraidPartFrameKind.Nail, Index: 4 } => EnemyFrameId.KraidNail4,
        { Kind: KraidPartFrameKind.Nail, Index: 5 } => EnemyFrameId.KraidNail5,
        { Kind: KraidPartFrameKind.Nail, Index: 6 } => EnemyFrameId.KraidNail6,
        { Kind: KraidPartFrameKind.Nail, Index: 7 } => EnemyFrameId.KraidNail7,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the KraidPartFrame programs show."),
    };
}
