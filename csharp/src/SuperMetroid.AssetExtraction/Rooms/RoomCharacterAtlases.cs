using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed room background character sheets, which the import pipeline composes the rooms' pre-coloured pictures
/// from (#1392, #1466); gameplay reads none.
/// </summary>
public sealed class RoomCharacterAtlases
{
    /// <summary>Copied lookup by character sheet; the RoomCharacterAtlas instances are shared and immutable.</summary>
    internal readonly Dictionary<RoomCharacterSet, RoomCharacterAtlas> byRoomCharacterSet;

    /// <summary>Copies the lookup and requires an entry for every retail graphics set's character sheet.</summary>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="InvalidDataException">A graphics set's character sheet is missing or null.</exception>
    public RoomCharacterAtlases(RoomCharacterAtlas cre,
        IReadOnlyDictionary<RoomCharacterSet, RoomCharacterAtlas> sheets)
    {
        Cre = Ensure.NotNull(cre);
        Ensure.NotNull(sheets);
        for (int graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            RoomCharacterSet required = RoomTilesets.Get(graphicsSet).Characters;
            if (!sheets.TryGetValue(required, out RoomCharacterAtlas? value) || value is null)
                throw new InvalidDataException($"RoomCharacterAtlases lacks graphics set ${graphicsSet:X2}'s {required}.");
        }
        byRoomCharacterSet = new Dictionary<RoomCharacterSet, RoomCharacterAtlas>(sheets);
    }

    /// <summary>The common room elements, shared by every graphics set.</summary>
    public RoomCharacterAtlas Cre { get; }

    /// <summary>The installed RoomCharacterAtlas for <paramref name="set"/>.</summary>
    /// <exception cref="InvalidDataException">None is installed for <paramref name="set"/>.</exception>
    public RoomCharacterAtlas Get(RoomCharacterSet set) =>
        byRoomCharacterSet.TryGetValue(set, out RoomCharacterAtlas? value)
            ? value
            : throw new InvalidDataException($"No RoomCharacterAtlas is installed for {set}.");
}
