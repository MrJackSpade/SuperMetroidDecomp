using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The draw of the hardcoded elevatube PLM spawned by Maridia's door setup.
/// </summary>
internal static class MaridiaElevatubePlmDefinitions
{
    /// <summary>The elevatube's one draw.</summary>
    internal enum Draw
    {
        /// <summary>Places the elevatube's solid block.</summary>
        Block,
    }

    /// <summary>The draw that places the elevatube's solid block.</summary>
    internal static PlmFrameDraw<Draw> BlockDraw { get; } = PlmDraw.Of(Draw.Block);

    /// <summary>The elevatube's block: a solid block showing block $180, the block Kraid's crumble draw also shows.</summary>
    internal static RoomTerrainBlock Terrain { get; } = RoomTerrainBlock.Of(384, BlockMirroring.None, RoomCollisionType.SolidBlock);

    /// <summary>Temporary artwork description for export; gameplay draws the single block directly.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList BlockDrawList =>
        new(BlockDraw,
            new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(PlmRunDirection.Horizontal, new[] { Terrain }, new(0, 0)),
            });

    /// <summary>Stable asset identity assigned to the elevatube's one-block artwork.</summary>
    internal const string VisualId = "elevatube-block";

    /// <summary>Exposes the sole elevatube draw list to artwork export.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> AllDraws => [BlockDrawList];
}
