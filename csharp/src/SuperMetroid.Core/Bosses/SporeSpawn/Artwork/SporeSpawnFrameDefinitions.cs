using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The presentation boundary of Spore Spawn's body (#641): the bank-$A5 extended spritemap each
/// <see cref="SporeSpawnFrame"/> shows. The opening and open poses are consecutive eighteen-byte roots.
/// </summary>
internal static class SporeSpawnFrameDefinitions
{
    /// <summary>$A5:EE65: the hardened husk.</summary>
    private const ushort Dead = 0xee65;
    /// <summary>$A5:EE6F: the closed head.</summary>
    private const ushort Closed = 0xee6f;
    /// <summary>$A5:EE79: the first of seven opening poses.</summary>
    private const ushort FirstOpening = 0xee79;
    /// <summary>$A5:EF3D: the first of three fully open poses.</summary>
    private const ushort FirstOpen = 0xef3d;
    /// <summary>The size of one opening or open root.</summary>
    private const int RootBytes = 18;
    /// <summary>The opening poses.</summary>
    internal const int OpeningPoseCount = 7;
    /// <summary>The fully open poses.</summary>
    internal const int OpenPoseCount = 3;

    /// <summary>The extended spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(SporeSpawnFrame frame) => frame switch
    {
        { Kind: SporeSpawnFrameKind.Dead, Pose: 0 } => Dead,
        { Kind: SporeSpawnFrameKind.Closed, Pose: 0 } => Closed,
        { Kind: SporeSpawnFrameKind.Opening, Pose: >= 0 and < OpeningPoseCount } => (ushort)(FirstOpening + RootBytes * frame.Pose),
        { Kind: SporeSpawnFrameKind.Open, Pose: >= 0 and < OpenPoseCount } => (ushort)(FirstOpen + RootBytes * frame.Pose),
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Spore Spawn frame."),
    };

    /// <summary>The installed frame each <see cref="SporeSpawnFrame"/> shows.</summary>
    internal static EnemyFrameTable<SporeSpawnFrame> ShownFrames { get; } = new()
    {
        { new(Kind: SporeSpawnFrameKind.Closed, Pose: 0), EnemyFrameId.SporeSpawnOamEE6F },
        { new(Kind: SporeSpawnFrameKind.Dead, Pose: 0), EnemyFrameId.SporeSpawnOamEE65 },
        { new(Kind: SporeSpawnFrameKind.Open, Pose: 0), EnemyFrameId.SporeSpawnOamEF3D },
        { new(Kind: SporeSpawnFrameKind.Open, Pose: 1), EnemyFrameId.SporeSpawnOamEF4F },
        { new(Kind: SporeSpawnFrameKind.Open, Pose: 2), EnemyFrameId.SporeSpawnOamEF61 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 0), EnemyFrameId.SporeSpawnOamEE79 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 1), EnemyFrameId.SporeSpawnOamEE8B },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 2), EnemyFrameId.SporeSpawnOamEE9D },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 3), EnemyFrameId.SporeSpawnOamEEAF },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 4), EnemyFrameId.SporeSpawnOamEEC1 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 5), EnemyFrameId.SporeSpawnOamEED3 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 6), EnemyFrameId.SporeSpawnOamEEE5 },
    };
}
