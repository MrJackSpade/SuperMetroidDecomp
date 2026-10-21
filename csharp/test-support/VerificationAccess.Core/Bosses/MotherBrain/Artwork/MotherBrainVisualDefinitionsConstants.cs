using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Cartridge values and helpers of <see cref="MotherBrainVisualDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainVisualDefinitionsConstants
{
    /// <summary>Enumerates all eighteen installed Mother Brain compositions in catalog order.</summary>
    internal static IEnumerable<NativeEnemySpritemapDefinition> Frames()
    {
        for (int index = 0; index < MotherBrainVisualDefinitionsConstants.FrameCount; index++) yield return MotherBrainVisualDefinitionsConstants.Frame(index);
    }
    /// <summary>Head, mouth, damage, and falling-tube compositions in the installed catalog.</summary>
    internal const int FrameCount = 18;
    /// <summary>$A9:A586, Spritemaps_MotherBrain_0; eleven-character head frames.</summary>
    internal const ushort HeadStart = 0xa586;
    /// <summary>$A9:A5F8, Spritemaps_MotherBrain_2; ten-character mouth frames then the neck joint.</summary>
    internal const ushort MouthStart = 0xa5f8;
    /// <summary>$A9:A69B, Spritemaps_MotherBrain_6; twelve-character damaged-head frames.</summary>
    internal const ushort DamagedHeadStart = 0xa69b;
    /// <summary>$A9:A717, Spritemaps_MotherBrain_8; eleven-character damaged mouth frames.</summary>
    internal const ushort DamagedMouthStart = 0xa717;
    /// <summary>$A9:AD3E, Spritemaps_MotherBrain_18; the nine-character high-tile head followed by map 19.</summary>
    internal const ushort HighTileHeadStart = 0xad3e;
    /// <summary>$A9:ADA1, Spritemaps_MotherBrainTubes_0; ten-character tube records.</summary>
    internal const ushort TubesStart = 0xada1;
    /// <summary>$A9:AE33, Spritemaps_MotherBrainTubes_3; eight-character tube followed by the main tube.</summary>
    internal const ushort UpperTubesStart = 0xae33;
    /// <summary>Each ordinary OAM record has a two-byte count and five bytes per character.</summary>
    internal static int RecordBytes(int characters) => 2 + characters * 5;
    /// <summary>Calculates a bounded native OAM pointer from each composition family's record size.</summary>
    internal static NativeEnemySpritemapDefinition Frame(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        ushort pointer = (ushort)(index < 2 ? HeadStart + RecordBytes(11) * index
            : index < 6 ? MouthStart + RecordBytes(10) * (index - 2)
            : index < 8 ? DamagedHeadStart + RecordBytes(12) * (index - 6)
            : index < 11 ? DamagedMouthStart + RecordBytes(11) * (index - 8)
            : index < 13 ? HighTileHeadStart + RecordBytes(9) * (index - 11)
            : index < 16 ? TubesStart + RecordBytes(10) * (index - 13)
            : UpperTubesStart + RecordBytes(8) * (index - 16));
        return new NativeEnemySpritemapDefinition(MotherBrainVisualToolingDefinitions.Bank, pointer, $"mother_brain_a9_{pointer:x4}");
    }
}
