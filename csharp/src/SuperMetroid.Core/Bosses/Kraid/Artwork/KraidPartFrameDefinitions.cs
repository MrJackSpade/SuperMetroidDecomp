using SuperMetroid.Core.Enemies.Artwork;


namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The presentation boundary of Kraid's foot, arm, lints and fingernails (#641): the bank-$A7 extended spritemaps
/// their typed programs show. Gameplay animates <see cref="KraidPartFrame"/> values; only this catalog knows the
/// spritemaps.
/// </summary>
internal static class KraidPartFrameDefinitions
{

    /// <summary>The installed frame each <see cref="KraidPartFrame"/> shows.</summary>
    internal static EnemyFrameTable<KraidPartFrame> ShownFrames { get; } = new()
    {
        { new(Kind: KraidPartFrameKind.Arm, Index: 0), EnemyFrameId.KraidArmOam8F59 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 1), EnemyFrameId.KraidArmOam8F83 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 10), EnemyFrameId.KraidArmOam90FD },
        { new(Kind: KraidPartFrameKind.Arm, Index: 11), EnemyFrameId.KraidArmOam9127 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 12), EnemyFrameId.KraidArmOam9151 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 13), EnemyFrameId.KraidArmOam917B },
        { new(Kind: KraidPartFrameKind.Arm, Index: 14), EnemyFrameId.KraidArmOam91A5 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 15), EnemyFrameId.KraidArmOam91CF },
        { new(Kind: KraidPartFrameKind.Arm, Index: 16), EnemyFrameId.KraidArmOam91F9 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 17), EnemyFrameId.KraidArmOam9223 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 18), EnemyFrameId.KraidArmOam924D },
        { new(Kind: KraidPartFrameKind.Arm, Index: 19), EnemyFrameId.KraidArmOam9277 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 2), EnemyFrameId.KraidArmOam8FAD },
        { new(Kind: KraidPartFrameKind.Arm, Index: 20), EnemyFrameId.KraidArmOam92A1 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 21), EnemyFrameId.KraidArmOam92AB },
        { new(Kind: KraidPartFrameKind.Arm, Index: 3), EnemyFrameId.KraidArmOam8FD7 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 4), EnemyFrameId.KraidArmOam9001 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 5), EnemyFrameId.KraidArmOam902B },
        { new(Kind: KraidPartFrameKind.Arm, Index: 6), EnemyFrameId.KraidArmOam9055 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 7), EnemyFrameId.KraidArmOam907F },
        { new(Kind: KraidPartFrameKind.Arm, Index: 8), EnemyFrameId.KraidArmOam90A9 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 9), EnemyFrameId.KraidArmOam90D3 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 0), EnemyFrameId.KraidFootOam8CE3 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 1), EnemyFrameId.KraidFootOam8CF5 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 10), EnemyFrameId.KraidFootOam8D97 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 11), EnemyFrameId.KraidFootOam8DA9 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 12), EnemyFrameId.KraidFootOam8DBB },
        { new(Kind: KraidPartFrameKind.Foot, Index: 13), EnemyFrameId.KraidFootOam8DCD },
        { new(Kind: KraidPartFrameKind.Foot, Index: 14), EnemyFrameId.KraidFootOam8DDF },
        { new(Kind: KraidPartFrameKind.Foot, Index: 15), EnemyFrameId.KraidFootOam8DF1 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 16), EnemyFrameId.KraidFootOam8E03 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 17), EnemyFrameId.KraidFootOam8E15 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 18), EnemyFrameId.KraidFootOam8E27 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 19), EnemyFrameId.KraidFootOam8E39 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 2), EnemyFrameId.KraidFootOam8D07 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 20), EnemyFrameId.KraidFootOam8E4B },
        { new(Kind: KraidPartFrameKind.Foot, Index: 21), EnemyFrameId.KraidFootOam8E5D },
        { new(Kind: KraidPartFrameKind.Foot, Index: 22), EnemyFrameId.KraidFootOam8E6F },
        { new(Kind: KraidPartFrameKind.Foot, Index: 23), EnemyFrameId.KraidFootOam8E81 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 24), EnemyFrameId.KraidFootOam8E93 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 25), EnemyFrameId.KraidFootOam8EA5 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 26), EnemyFrameId.KraidFootOam8EB7 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 27), EnemyFrameId.KraidFootOam8EC9 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 28), EnemyFrameId.KraidFootOam8EDB },
        { new(Kind: KraidPartFrameKind.Foot, Index: 29), EnemyFrameId.KraidFootOam8EED },
        { new(Kind: KraidPartFrameKind.Foot, Index: 3), EnemyFrameId.KraidFootOam8D19 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 30), EnemyFrameId.KraidFootOam8EFF },
        { new(Kind: KraidPartFrameKind.Foot, Index: 31), EnemyFrameId.KraidFootOam8F11 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 32), EnemyFrameId.KraidFootOam8F23 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 33), EnemyFrameId.KraidFootOam8F47 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 4), EnemyFrameId.KraidFootOam8D2B },
        { new(Kind: KraidPartFrameKind.Foot, Index: 5), EnemyFrameId.KraidFootOam8D3D },
        { new(Kind: KraidPartFrameKind.Foot, Index: 6), EnemyFrameId.KraidFootOam8D4F },
        { new(Kind: KraidPartFrameKind.Foot, Index: 7), EnemyFrameId.KraidFootOam8D61 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 8), EnemyFrameId.KraidFootOam8D73 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 9), EnemyFrameId.KraidFootOam8D85 },
        { new(Kind: KraidPartFrameKind.FootStanding, Index: 0), EnemyFrameId.KraidFootOamA565 },
        { new(Kind: KraidPartFrameKind.Lint, Index: 0), EnemyFrameId.KraidLint00 },
        { new(Kind: KraidPartFrameKind.Lint, Index: 1), EnemyFrameId.KraidLint01 },
        { new(Kind: KraidPartFrameKind.Nail, Index: 0), EnemyFrameId.KraidNail0 },
        { new(Kind: KraidPartFrameKind.Nail, Index: 1), EnemyFrameId.KraidNail1 },
        { new(Kind: KraidPartFrameKind.Nail, Index: 2), EnemyFrameId.KraidNail2 },
        { new(Kind: KraidPartFrameKind.Nail, Index: 3), EnemyFrameId.KraidNail3 },
        { new(Kind: KraidPartFrameKind.Nail, Index: 4), EnemyFrameId.KraidNail4 },
        { new(Kind: KraidPartFrameKind.Nail, Index: 5), EnemyFrameId.KraidNail5 },
        { new(Kind: KraidPartFrameKind.Nail, Index: 6), EnemyFrameId.KraidNail6 },
        { new(Kind: KraidPartFrameKind.Nail, Index: 7), EnemyFrameId.KraidNail7 },
    };
}
