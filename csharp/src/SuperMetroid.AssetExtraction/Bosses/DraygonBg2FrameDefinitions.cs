using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The 34 Draygon poses drawn only on the background layer behind the sprites, each as one unshifted tilemap picture; its
/// other 48 poses are sprite compositions among the extended frames. Hitboxes and animation timing are not part of this
/// visual catalog.
/// </summary>
internal static class DraygonBg2FrameDefinitions
{
    /// <summary>Schema revision accepted by the installed Draygon BG2 frame document; revision 3 names each frame for its pose (#1465).</summary>
    internal const int Version = 3;
    /// <summary>Asset document containing the 34 body frames with BG2 tilemap streams.</summary>
    internal const string FileName = "draygon-bg2-frames.json";
    /// <summary>Each selected Draygon BG2 frame has one zero-offset stream.</summary>
    internal const int MaximumComponents = 1;

    /// <summary>The 34 BG2 frames, left-facing then right-facing.</summary>
    internal static ImmutableArray<EnemySpritemapDefinition> Frames { get; } =
    [
        new(EnemyFrameId.DraygonFrame6, "draygon_frame_6"),
        new(EnemyFrameId.DraygonFrame7, "draygon_frame_7"),
        new(EnemyFrameId.DraygonFrame8, "draygon_frame_8"),
        new(EnemyFrameId.DraygonFrame9, "draygon_frame_9"),
        new(EnemyFrameId.DraygonFrame10, "draygon_frame_10"),
        new(EnemyFrameId.DraygonFrame11, "draygon_frame_11"),
        new(EnemyFrameId.DraygonFrame12, "draygon_frame_12"),
        new(EnemyFrameId.DraygonFrame13, "draygon_frame_13"),
        new(EnemyFrameId.DraygonFrame14, "draygon_frame_14"),
        new(EnemyFrameId.DraygonFrame15, "draygon_frame_15"),
        new(EnemyFrameId.DraygonFrame16, "draygon_frame_16"),
        new(EnemyFrameId.DraygonFrame17, "draygon_frame_17"),
        new(EnemyFrameId.DraygonFrame18, "draygon_frame_18"),
        new(EnemyFrameId.DraygonFrame19, "draygon_frame_19"),
        new(EnemyFrameId.DraygonFrame20, "draygon_frame_20"),
        new(EnemyFrameId.DraygonFrame21, "draygon_frame_21"),
        new(EnemyFrameId.DraygonFrame22, "draygon_frame_22"),
        new(EnemyFrameId.DraygonFrame47, "draygon_frame_47"),
        new(EnemyFrameId.DraygonFrame48, "draygon_frame_48"),
        new(EnemyFrameId.DraygonFrame49, "draygon_frame_49"),
        new(EnemyFrameId.DraygonFrame50, "draygon_frame_50"),
        new(EnemyFrameId.DraygonFrame51, "draygon_frame_51"),
        new(EnemyFrameId.DraygonFrame52, "draygon_frame_52"),
        new(EnemyFrameId.DraygonFrame53, "draygon_frame_53"),
        new(EnemyFrameId.DraygonFrame54, "draygon_frame_54"),
        new(EnemyFrameId.DraygonFrame55, "draygon_frame_55"),
        new(EnemyFrameId.DraygonFrame56, "draygon_frame_56"),
        new(EnemyFrameId.DraygonFrame57, "draygon_frame_57"),
        new(EnemyFrameId.DraygonFrame58, "draygon_frame_58"),
        new(EnemyFrameId.DraygonFrame59, "draygon_frame_59"),
        new(EnemyFrameId.DraygonFrame60, "draygon_frame_60"),
        new(EnemyFrameId.DraygonFrame61, "draygon_frame_61"),
        new(EnemyFrameId.DraygonFrame62, "draygon_frame_62"),
        new(EnemyFrameId.DraygonFrame63, "draygon_frame_63"),
    ];
}
