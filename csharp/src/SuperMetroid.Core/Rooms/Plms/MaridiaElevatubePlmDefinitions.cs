using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Bounded bank-$84 instruction and physical draw data for the hardcoded
/// elevatube PLM spawned by Maridia's door setup.
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

    /// <summary>The elevatube's level entry: a solid block showing visual block $180, the block Kraid's physical draw also shows.</summary>
    internal static RoomLevelWord PhysicalWord { get; } = RoomLevelWord.Create(0x180, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock);

    /// <summary>Temporary artwork description for export; gameplay draws the single physical word directly.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList BlockDrawList =>
        new(BlockDraw,
            new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(PlmRunDirection.Horizontal, new ushort[] { PhysicalWord.Raw }, new(0, 0)),
            });

    /// <summary>Stable asset identity assigned to the elevatube's one-block artwork.</summary>
    internal const string VisualId = "elevatube-block";

    /// <summary>Exposes the sole elevatube draw list to artwork export.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> AllDraws => [BlockDrawList];
}
