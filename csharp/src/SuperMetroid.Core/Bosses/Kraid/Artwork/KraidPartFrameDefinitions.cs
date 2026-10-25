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
        { new(Kind: KraidPartFrameKind.Arm, Index: 0), EnemyFrameId.KraidArm0 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 1), EnemyFrameId.KraidArm1 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 10), EnemyFrameId.KraidArm10 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 11), EnemyFrameId.KraidArm11 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 12), EnemyFrameId.KraidArm12 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 13), EnemyFrameId.KraidArm13 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 14), EnemyFrameId.KraidArm14 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 15), EnemyFrameId.KraidArm15 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 16), EnemyFrameId.KraidArm16 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 17), EnemyFrameId.KraidArm17 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 18), EnemyFrameId.KraidArm18 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 19), EnemyFrameId.KraidArm19 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 2), EnemyFrameId.KraidArm2 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 20), EnemyFrameId.KraidArm20 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 21), EnemyFrameId.KraidArm21 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 3), EnemyFrameId.KraidArm3 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 4), EnemyFrameId.KraidArm4 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 5), EnemyFrameId.KraidArm5 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 6), EnemyFrameId.KraidArm6 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 7), EnemyFrameId.KraidArm7 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 8), EnemyFrameId.KraidArm8 },
        { new(Kind: KraidPartFrameKind.Arm, Index: 9), EnemyFrameId.KraidArm9 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 0), EnemyFrameId.KraidFoot0 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 1), EnemyFrameId.KraidFoot1 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 10), EnemyFrameId.KraidFoot10 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 11), EnemyFrameId.KraidFoot11 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 12), EnemyFrameId.KraidFoot12 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 13), EnemyFrameId.KraidFoot13 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 14), EnemyFrameId.KraidFoot14 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 15), EnemyFrameId.KraidFoot15 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 16), EnemyFrameId.KraidFoot16 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 17), EnemyFrameId.KraidFoot17 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 18), EnemyFrameId.KraidFoot18 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 19), EnemyFrameId.KraidFoot19 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 2), EnemyFrameId.KraidFoot2 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 20), EnemyFrameId.KraidFoot20 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 21), EnemyFrameId.KraidFoot21 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 22), EnemyFrameId.KraidFoot22 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 23), EnemyFrameId.KraidFoot23 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 24), EnemyFrameId.KraidFoot24 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 25), EnemyFrameId.KraidFoot25 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 26), EnemyFrameId.KraidFoot26 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 27), EnemyFrameId.KraidFoot27 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 28), EnemyFrameId.KraidFoot28 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 29), EnemyFrameId.KraidFoot29 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 3), EnemyFrameId.KraidFoot3 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 30), EnemyFrameId.KraidFoot30 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 31), EnemyFrameId.KraidFoot31 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 32), EnemyFrameId.KraidFoot32 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 33), EnemyFrameId.KraidFoot33 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 4), EnemyFrameId.KraidFoot4 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 5), EnemyFrameId.KraidFoot5 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 6), EnemyFrameId.KraidFoot6 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 7), EnemyFrameId.KraidFoot7 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 8), EnemyFrameId.KraidFoot8 },
        { new(Kind: KraidPartFrameKind.Foot, Index: 9), EnemyFrameId.KraidFoot9 },
        { new(Kind: KraidPartFrameKind.FootStanding, Index: 0), EnemyFrameId.KraidFootStanding },
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
