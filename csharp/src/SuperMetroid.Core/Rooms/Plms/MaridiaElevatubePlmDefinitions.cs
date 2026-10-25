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
                new(1, new ushort[] { PhysicalWord.Raw }, new(0, 0)),
            });

    /// <summary>Stable asset identity assigned to the elevatube's one-block artwork.</summary>
    internal const string VisualId = "elevatube-block";

    /// <summary>Exposes the sole elevatube draw list to artwork export.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> AllDraws
    {
        get { yield return BlockDrawList; }
    }

    /// <summary>Maps the native elevatube draw pointer to its asset identity and rejects unrelated pointers.</summary>
    internal static string DrawVisualId(PlmDraw request) => request == BlockDraw
        ? VisualId
        : throw new InvalidDataException($"Maridia elevatube draw {request} has no visual ID.");

    /// <summary>Resolves the extracted elevatube artwork by its stable asset identity.</summary>
    internal static bool TryGetDrawByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId([BlockDrawList], _ => VisualId, id, out draw);
}
