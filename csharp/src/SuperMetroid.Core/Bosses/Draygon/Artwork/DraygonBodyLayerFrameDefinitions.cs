using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>The 34 poses Draygon draws on his body layer behind the sprites (#1466), left-facing then right-facing; his other poses are sprite compositions.</summary>
internal static class DraygonBodyLayerFrameDefinitions
{
    /// <summary>The frames Draygon draws on his body layer, each a frame of his installed body sheet.</summary>
    internal static IReadOnlySet<EnemyFrameId> Frames { get; } = new HashSet<EnemyFrameId>
    {
        EnemyFrameId.DraygonFrame6,
        EnemyFrameId.DraygonFrame7,
        EnemyFrameId.DraygonFrame8,
        EnemyFrameId.DraygonFrame9,
        EnemyFrameId.DraygonFrame10,
        EnemyFrameId.DraygonFrame11,
        EnemyFrameId.DraygonFrame12,
        EnemyFrameId.DraygonFrame13,
        EnemyFrameId.DraygonFrame14,
        EnemyFrameId.DraygonFrame15,
        EnemyFrameId.DraygonFrame16,
        EnemyFrameId.DraygonFrame17,
        EnemyFrameId.DraygonFrame18,
        EnemyFrameId.DraygonFrame19,
        EnemyFrameId.DraygonFrame20,
        EnemyFrameId.DraygonFrame21,
        EnemyFrameId.DraygonFrame22,
        EnemyFrameId.DraygonFrame47,
        EnemyFrameId.DraygonFrame48,
        EnemyFrameId.DraygonFrame49,
        EnemyFrameId.DraygonFrame50,
        EnemyFrameId.DraygonFrame51,
        EnemyFrameId.DraygonFrame52,
        EnemyFrameId.DraygonFrame53,
        EnemyFrameId.DraygonFrame54,
        EnemyFrameId.DraygonFrame55,
        EnemyFrameId.DraygonFrame56,
        EnemyFrameId.DraygonFrame57,
        EnemyFrameId.DraygonFrame58,
        EnemyFrameId.DraygonFrame59,
        EnemyFrameId.DraygonFrame60,
        EnemyFrameId.DraygonFrame61,
        EnemyFrameId.DraygonFrame62,
        EnemyFrameId.DraygonFrame63,
    };
}
