using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
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
    internal const int Version = 2;
    /// <summary>Asset document containing the 34 body frames with BG2 tilemap streams.</summary>
    internal const string FileName = "draygon-bg2-frames.json";
    /// <summary>Each selected Draygon BG2 frame has one zero-offset stream.</summary>
    internal const int MaximumComponents = 1;

    /// <summary>The 34 BG2 frames, left-facing then right-facing.</summary>
    internal static ImmutableArray<EnemySpritemapDefinition> Frames { get; } =
    [
        new(EnemyFrameId.DraygonFrame6, "draygon_bg2_A31B"),
        new(EnemyFrameId.DraygonFrame7, "draygon_bg2_A325"),
        new(EnemyFrameId.DraygonFrame8, "draygon_bg2_A32F"),
        new(EnemyFrameId.DraygonFrame9, "draygon_bg2_A339"),
        new(EnemyFrameId.DraygonFrame10, "draygon_bg2_A343"),
        new(EnemyFrameId.DraygonFrame11, "draygon_bg2_A34D"),
        new(EnemyFrameId.DraygonFrame12, "draygon_bg2_A357"),
        new(EnemyFrameId.DraygonFrame13, "draygon_bg2_A361"),
        new(EnemyFrameId.DraygonFrame14, "draygon_bg2_A36B"),
        new(EnemyFrameId.DraygonFrame15, "draygon_bg2_A375"),
        new(EnemyFrameId.DraygonFrame16, "draygon_bg2_A37F"),
        new(EnemyFrameId.DraygonFrame17, "draygon_bg2_A389"),
        new(EnemyFrameId.DraygonFrame18, "draygon_bg2_A393"),
        new(EnemyFrameId.DraygonFrame19, "draygon_bg2_A39D"),
        new(EnemyFrameId.DraygonFrame20, "draygon_bg2_A3A7"),
        new(EnemyFrameId.DraygonFrame21, "draygon_bg2_A3B1"),
        new(EnemyFrameId.DraygonFrame22, "draygon_bg2_A3BB"),
        new(EnemyFrameId.DraygonFrame47, "draygon_bg2_A643"),
        new(EnemyFrameId.DraygonFrame48, "draygon_bg2_A64D"),
        new(EnemyFrameId.DraygonFrame49, "draygon_bg2_A657"),
        new(EnemyFrameId.DraygonFrame50, "draygon_bg2_A661"),
        new(EnemyFrameId.DraygonFrame51, "draygon_bg2_A66B"),
        new(EnemyFrameId.DraygonFrame52, "draygon_bg2_A675"),
        new(EnemyFrameId.DraygonFrame53, "draygon_bg2_A67F"),
        new(EnemyFrameId.DraygonFrame54, "draygon_bg2_A689"),
        new(EnemyFrameId.DraygonFrame55, "draygon_bg2_A693"),
        new(EnemyFrameId.DraygonFrame56, "draygon_bg2_A69D"),
        new(EnemyFrameId.DraygonFrame57, "draygon_bg2_A6A7"),
        new(EnemyFrameId.DraygonFrame58, "draygon_bg2_A6B1"),
        new(EnemyFrameId.DraygonFrame59, "draygon_bg2_A6BB"),
        new(EnemyFrameId.DraygonFrame60, "draygon_bg2_A6C5"),
        new(EnemyFrameId.DraygonFrame61, "draygon_bg2_A6CF"),
        new(EnemyFrameId.DraygonFrame62, "draygon_bg2_A6D9"),
        new(EnemyFrameId.DraygonFrame63, "draygon_bg2_A6E3"),
    ];

    /// <summary>Whether <paramref name="frame"/> is one of Draygon's BG2 frames.</summary>
    internal static bool IsFrame(EnemyFrameId frame) => Frames.Any(definition => definition.Frame == frame);
}
