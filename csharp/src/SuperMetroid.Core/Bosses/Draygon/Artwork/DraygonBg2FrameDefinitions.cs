using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// The 34 bank-$A5 Draygon extended frames whose sole zero-offset component
/// points to a $FFFE BG2 tilemap stream. The remaining 48 selected frames are
/// ordinary OAM components in <see cref="EnemyExtendedFrameDefinitions"/>.
/// Hitbox pointers and instruction timing are not part of this visual catalog.
/// </summary>
internal static class DraygonBg2FrameDefinitions
{
    /// <summary>Schema revision accepted by the installed Draygon BG2 frame document.</summary>
    internal const int Version = 1;
    /// <summary>Asset document containing the 34 body frames with BG2 tilemap streams.</summary>
    internal const string FileName = "draygon-bg2-frames.json";
    /// <summary>Each selected Draygon BG2 frame has one zero-offset stream.</summary>
    internal const int MaximumComponents = 1;

    /// <summary>The 34 BG2 frames, left-facing then right-facing.</summary>
    internal static ImmutableArray<EnemySpritemapDefinition> Frames { get; } =
    [
        new(EnemyFrameId.DraygonBg2A31B, "draygon_bg2_A31B"),
        new(EnemyFrameId.DraygonBg2A325, "draygon_bg2_A325"),
        new(EnemyFrameId.DraygonBg2A32F, "draygon_bg2_A32F"),
        new(EnemyFrameId.DraygonBg2A339, "draygon_bg2_A339"),
        new(EnemyFrameId.DraygonBg2A343, "draygon_bg2_A343"),
        new(EnemyFrameId.DraygonBg2A34D, "draygon_bg2_A34D"),
        new(EnemyFrameId.DraygonBg2A357, "draygon_bg2_A357"),
        new(EnemyFrameId.DraygonBg2A361, "draygon_bg2_A361"),
        new(EnemyFrameId.DraygonBg2A36B, "draygon_bg2_A36B"),
        new(EnemyFrameId.DraygonBg2A375, "draygon_bg2_A375"),
        new(EnemyFrameId.DraygonBg2A37F, "draygon_bg2_A37F"),
        new(EnemyFrameId.DraygonBg2A389, "draygon_bg2_A389"),
        new(EnemyFrameId.DraygonBg2A393, "draygon_bg2_A393"),
        new(EnemyFrameId.DraygonBg2A39D, "draygon_bg2_A39D"),
        new(EnemyFrameId.DraygonBg2A3A7, "draygon_bg2_A3A7"),
        new(EnemyFrameId.DraygonBg2A3B1, "draygon_bg2_A3B1"),
        new(EnemyFrameId.DraygonBg2A3BB, "draygon_bg2_A3BB"),
        new(EnemyFrameId.DraygonBg2A643, "draygon_bg2_A643"),
        new(EnemyFrameId.DraygonBg2A64D, "draygon_bg2_A64D"),
        new(EnemyFrameId.DraygonBg2A657, "draygon_bg2_A657"),
        new(EnemyFrameId.DraygonBg2A661, "draygon_bg2_A661"),
        new(EnemyFrameId.DraygonBg2A66B, "draygon_bg2_A66B"),
        new(EnemyFrameId.DraygonBg2A675, "draygon_bg2_A675"),
        new(EnemyFrameId.DraygonBg2A67F, "draygon_bg2_A67F"),
        new(EnemyFrameId.DraygonBg2A689, "draygon_bg2_A689"),
        new(EnemyFrameId.DraygonBg2A693, "draygon_bg2_A693"),
        new(EnemyFrameId.DraygonBg2A69D, "draygon_bg2_A69D"),
        new(EnemyFrameId.DraygonBg2A6A7, "draygon_bg2_A6A7"),
        new(EnemyFrameId.DraygonBg2A6B1, "draygon_bg2_A6B1"),
        new(EnemyFrameId.DraygonBg2A6BB, "draygon_bg2_A6BB"),
        new(EnemyFrameId.DraygonBg2A6C5, "draygon_bg2_A6C5"),
        new(EnemyFrameId.DraygonBg2A6CF, "draygon_bg2_A6CF"),
        new(EnemyFrameId.DraygonBg2A6D9, "draygon_bg2_A6D9"),
        new(EnemyFrameId.DraygonBg2A6E3, "draygon_bg2_A6E3"),
    ];

    /// <summary>Whether <paramref name="frame"/> is one of Draygon's BG2 frames.</summary>
    internal static bool IsFrame(EnemyFrameId frame) => Frames.Any(definition => definition.Frame == frame);
}
