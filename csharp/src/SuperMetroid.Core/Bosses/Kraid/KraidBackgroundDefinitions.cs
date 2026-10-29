namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The shapes of what Kraid shows behind the room (#1413, #1466): the plane his body layer covers and the room backdrop's
/// characters. The tilemaps his body sheet is composed from are the import's (<c>AssetExtraction/Bosses/KraidWorkingMapDefinitions</c>).
/// </summary>
public static class KraidBackgroundDefinitions
{
    /// <summary>The first of the room's tile characters the room backdrop binds over.</summary>
    public const int RoomBackgroundFirstCharacter = 1008;

    /// <summary>The room backdrop's characters, which its installed picture holds exactly.</summary>
    public const int RoomBackgroundCharacterCount = 16;

    /// <summary>The size of the background plane Kraid's body shows on: two screens wide and two screens tall.</summary>
    public static Extent16<TilemapCells> BodyPlaneSize { get; } = new(64, 64);
}
