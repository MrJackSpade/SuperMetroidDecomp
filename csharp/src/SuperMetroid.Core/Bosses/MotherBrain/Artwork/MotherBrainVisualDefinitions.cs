using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Ordinary bank-$A9 OAM layouts selected by Mother Brain's head hook and
/// falling-tube programs. These identities name artwork, not encounter timing.
/// </summary>
internal static class MotherBrainVisualDefinitions
{
    /// <summary>$A9, the native bank containing Mother Brain's OAM maps.</summary>
    internal const byte Bank = 0xa9;

    /// <summary>Head, mouth, damage, and falling-tube compositions in the installed catalog.</summary>
    internal const int FrameCount = 18;

    /// <summary>$A9:A586, Spritemaps_MotherBrain_0; eleven-character head frames.</summary>
    private const ushort HeadStart = 0xa586;
    /// <summary>$A9:A5F8, Spritemaps_MotherBrain_2; ten-character mouth frames then the neck joint.</summary>
    private const ushort MouthStart = 0xa5f8;
    /// <summary>$A9:A69B, Spritemaps_MotherBrain_6; twelve-character damaged-head frames.</summary>
    private const ushort DamagedHeadStart = 0xa69b;
    /// <summary>$A9:A717, Spritemaps_MotherBrain_8; eleven-character damaged mouth frames.</summary>
    private const ushort DamagedMouthStart = 0xa717;
    /// <summary>$A9:AD3E, Spritemaps_MotherBrain_18; the nine-character high-tile head followed by map 19.</summary>
    private const ushort HighTileHeadStart = 0xad3e;
    /// <summary>$A9:ADA1, Spritemaps_MotherBrainTubes_0; ten-character tube records.</summary>
    private const ushort TubesStart = 0xada1;
    /// <summary>$A9:AE33, Spritemaps_MotherBrainTubes_3; eight-character tube followed by the main tube.</summary>
    private const ushort UpperTubesStart = 0xae33;

    /// <summary>Each ordinary OAM record has a two-byte count and five bytes per character.</summary>
    private static int RecordBytes(int characters) => 2 + characters * 5;

    /// <summary>Calculates a bounded native OAM pointer from each composition family's record size.</summary>
    internal static EnemySpritemapDefinition Frame(int index)
    {
        if ((uint)index >= FrameCount) throw new IndexOutOfRangeException();
        ushort pointer = (ushort)(index < 2 ? HeadStart + RecordBytes(11) * index
            : index < 6 ? MouthStart + RecordBytes(10) * (index - 2)
            : index < 8 ? DamagedHeadStart + RecordBytes(12) * (index - 6)
            : index < 11 ? DamagedMouthStart + RecordBytes(11) * (index - 8)
            : index < 13 ? HighTileHeadStart + RecordBytes(9) * (index - 11)
            : index < 16 ? TubesStart + RecordBytes(10) * (index - 13)
            : UpperTubesStart + RecordBytes(8) * (index - 16));
        return new EnemySpritemapDefinition(Bank, pointer, $"mother_brain_a9_{pointer:x4}");
    }
    /// <summary>Enumerates all eighteen installed Mother Brain compositions in catalog order.</summary>
    internal static IEnumerable<EnemySpritemapDefinition> Frames()
    {
        for (int index = 0; index < FrameCount; index++) yield return Frame(index);
    }

    /// <summary>The installed frame each <see cref="MotherBrainHeadFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainHeadFrame> HeadFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.MotherBrainA9A586 },
        { new(Index: 1), EnemyFrameId.MotherBrainA9A5bf },
        { new(Index: 10), EnemyFrameId.MotherBrainA9A789 },
        { new(Index: 11), EnemyFrameId.MotherBrainA9Ad3e },
        { new(Index: 12), EnemyFrameId.MotherBrainA9Ad6d },
        { new(Index: 2), EnemyFrameId.MotherBrainA9A5f8 },
        { new(Index: 3), EnemyFrameId.MotherBrainA9A62c },
        { new(Index: 4), EnemyFrameId.MotherBrainA9A660 },
        { new(Index: 6), EnemyFrameId.MotherBrainA9A69b },
        { new(Index: 7), EnemyFrameId.MotherBrainA9A6d9 },
        { new(Index: 8), EnemyFrameId.MotherBrainA9A717 },
        { new(Index: 9), EnemyFrameId.MotherBrainA9A750 },
    };

    /// <summary>The neck joint each neck segment shows.</summary>
    internal const EnemyFrameId NeckJointFrame = EnemyFrameId.MotherBrainA9A694;

    /// <summary>The installed frame each <see cref="MotherBrainTubeFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainTubeFrame> TubeFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.MotherBrainA9Ada1 },
        { new(Index: 1), EnemyFrameId.MotherBrainA9Add5 },
        { new(Index: 2), EnemyFrameId.MotherBrainA9Ae09 },
        { new(Index: 3), EnemyFrameId.MotherBrainA9Ae33 },
        { new(Index: 4), EnemyFrameId.MotherBrainA9Ae5d },
    };
}
