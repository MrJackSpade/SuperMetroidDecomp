using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Cartridge values and helpers of <see cref="MotherBrainBodyVisualDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainBodyVisualDefinitionsConstants
{
    /// <summary>Recognizes a selected body root with a BG2 stream, excluding the OAM-only dummy.</summary>
    internal static bool HasBg2(ushort pointer) => pointer == MotherBrainBodyVisualDefinitionsConstants.Standing ||
        InRun(pointer, MotherBrainBodyVisualDefinitionsConstants.WalkingStart, 3, 0x52) || InRun(pointer, MotherBrainBodyVisualDefinitionsConstants.LaterWalkingStart, 6, 0x4a) ||
        InRun(pointer, MotherBrainBodyVisualDefinitionsConstants.Uncrouching, 2, 0x4a) || InRun(pointer, MotherBrainBodyVisualDefinitionsConstants.DeathBeamStart, 4, 0x4a);
    /// <summary>Recognizes a native record origin within a fixed-stride body-frame run.</summary>
    internal static bool InRun(ushort pointer, ushort start, int count, int stride)
    {
        int offset = pointer - start;
        return offset >= 0 && offset < count * stride && offset % stride == 0;
    }
    /// <summary>$A9:9FA0, standing frame with nine components.</summary>
    internal const ushort Standing = 0x9fa0;
    /// <summary>$A9:9FEA, first of three ten-component walking frames.</summary>
    internal const ushort WalkingStart = 0x9fea;
    /// <summary>$A9:A0E0, five nine-component walking frames followed by crouched.</summary>
    internal const ushort LaterWalkingStart = 0xa0e0;
    /// <summary>$A9:A28C, uncrouching and leaning-down nine-component frames.</summary>
    internal const ushort Uncrouching = 0xa28c;
    /// <summary>$A9:A384, first of four nine-component death-beam frames.</summary>
    internal const ushort DeathBeamStart = 0xa384;
    /// <summary>Every body frame in the order of its cartridge root.</summary>
    internal static readonly MotherBrainBodyFrame[] CartridgeOrder =
    [
        MotherBrainBodyFrame.Standing,
        MotherBrainBodyFrame.WalkingFirst,
        MotherBrainBodyFrame.WalkingSecond,
        MotherBrainBodyFrame.WalkingThird,
        MotherBrainBodyFrame.WalkingFourth,
        MotherBrainBodyFrame.WalkingFifth,
        MotherBrainBodyFrame.WalkingSixth,
        MotherBrainBodyFrame.WalkingSeventh,
        MotherBrainBodyFrame.WalkingEighth,
        MotherBrainBodyFrame.Crouched,
        MotherBrainBodyFrame.Uncrouching,
        MotherBrainBodyFrame.LeaningDown,
        MotherBrainBodyFrame.InitialDummy,
        MotherBrainBodyFrame.HandBeamSecond,
        MotherBrainBodyFrame.HandBeamThird,
        MotherBrainBodyFrame.HandBeamFourth,
        MotherBrainBodyFrame.HandBeamFifth,
    ];
    /// <summary>The published OAM identity of <paramref name="frame"/>.</summary>
    internal static NativeEnemySpritemapDefinition Frame(MotherBrainBodyFrame frame) => Frame(Array.IndexOf(CartridgeOrder, frame));
    /// <summary>Calculates the published OAM identity. Native record strides are
    /// two header bytes plus eight bytes per component; explicit run boundaries
    /// preserve the shorter crouched/dummy frames and skipped unselected roots.
    /// The name is the one the installed extended-frame document stores the frame under (#1623).</summary>
    internal static NativeEnemySpritemapDefinition Frame(int index)
    {
        if ((uint)index >= MotherBrainBodyFrameDefinitions.FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        ushort pointer = (ushort)(index == 0 ? Standing
            : index < 4 ? WalkingStart + 0x52 * (index - 1)
            : index < 10 ? LaterWalkingStart + 0x4a * (index - 4)
            : index < 13 ? Uncrouching + 0x4a * (index - 10)
            : DeathBeamStart + 0x4a * (index - 13));
        string name = EnemyFrameNameDefinitions.Names[MotherBrainBodyVisualDefinitions.ShownFrames.Of(CartridgeOrder[index])];
        return new(MotherBrainBodyVisualToolingDefinitions.Bank, pointer, name);
    }
}
