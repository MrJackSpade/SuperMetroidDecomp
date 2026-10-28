using static SuperMetroid.Core.Rooms.Plms.MaridiaElevatubePlmDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The artwork names of the Maridia elevatube's draws.</summary>
internal static class MaridiaElevatubeDraws
{
    /// <summary>Maps the native elevatube draw pointer to its asset identity and rejects unrelated pointers.</summary>
    internal static string DrawVisualId(PlmDraw request) => request == BlockDraw
        ? VisualId
        : throw new InvalidDataException($"Maridia elevatube draw {request} has no visual ID.");

    /// <summary>Resolves the extracted elevatube artwork by its stable asset identity.</summary>
    internal static bool TryGetDrawByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        ShotBlockPlmDraws.FindByVisualId([BlockDrawList], _ => VisualId, id, out draw);
}
