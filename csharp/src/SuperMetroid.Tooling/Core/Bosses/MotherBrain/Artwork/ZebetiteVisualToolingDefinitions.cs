using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Development-tool members of <see cref="ZebetiteVisualDefinitions"/>; never linked by player hosts.</summary>
internal static class ZebetiteVisualToolingDefinitions
{
    /// <summary>Zebetite ordinary OAM bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>Ten barrier health-tier compositions at $A6:FE08..FEB0.</summary>
    internal const int FrameCount = 10;
    /// <summary>The health poses in the order of their cartridge compositions: the large tiers, then the split tiers.</summary>
    internal static readonly ZebetiteFrame[] CartridgeOrder =
    [
        ZebetiteFrame.LargeAtLeast800,
        ZebetiteFrame.LargeAtLeast600,
        ZebetiteFrame.LargeAtLeast400,
        ZebetiteFrame.LargeAtLeast200,
        ZebetiteFrame.LargeBelow200,
        ZebetiteFrame.SplitAtLeast800,
        ZebetiteFrame.SplitAtLeast600,
        ZebetiteFrame.SplitAtLeast400,
        ZebetiteFrame.SplitAtLeast200,
        ZebetiteFrame.SplitBelow200,
    ];
    /// <summary>The spritemap of health pose <paramref name="frame"/>.</summary>
    internal static ushort Spritemap(ZebetiteFrame frame) => Spritemap(Array.IndexOf(CartridgeOrder, frame));
    /// <summary>The health pose spritemap at position <paramref name="index"/>: large tiers 0 to 4, split tiers 5 to 9.</summary>
    internal static ushort Spritemap(int index) => index switch
    {
        0 => 0xfe08,
        1 => 0xfe19,
        2 => 0xfe2a,
        3 => 0xfe4a,
        4 => 0xfe6a,
        5 => 0xfe8a,
        6 => 0xfe91,
        7 => 0xfe98,
        8 => 0xfea4,
        9 => 0xfeb0,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Zebetites have ten health poses."),
    };
    /// <summary>The spritemaps the typed health programs show, in program order.</summary>
    internal static ushort[] Spritemaps() => [.. Enum.GetValues<ZebetiteSegment>()
        .SelectMany(program => ZebetiteAnimations.Segments.Beats(program))
        .OfType<AnimationBeat<ZebetiteFrame, NoAnimationCue>.Frame>()
        .Select(beat => ZebetiteVisualToolingDefinitions.Spritemap(beat.Shown))];
}
