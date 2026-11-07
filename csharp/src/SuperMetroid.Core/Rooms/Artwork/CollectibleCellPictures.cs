using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The pictures a room plane's cells name past the tileset's blocks (#1538): every frame of every collectible kind that
/// shows its own picture. A cell where such a collectible lies names its picture, so a room's block picture table holds
/// the pictures of the tileset's blocks followed by these, and the X-ray reveal shows such an item by its first frame.
/// </summary>
public static class CollectibleCellPictures
{
    /// <summary>The kinds that show their own picture, in kind order.</summary>
    private static readonly InWorldCollectibleKind[] Kinds = [.. Enum.GetValues<InWorldCollectibleKind>().Where(CollectibleObjects.ShowsOwnPicture)];

    /// <summary>The first definition past the tileset's blocks: the first kind's first frame.</summary>
    public static int First => RoomBlockAppearance.LastBlock + 1;

    /// <summary>The definition after the last kind's last frame: the number of pictures a room's block picture table holds.</summary>
    public static int End => First + Kinds.Length * CollectiblePicture.Frames;

    /// <summary>The definition a cell showing <paramref name="picture"/> names.</summary>
    public static int DefinitionOf(CollectiblePicture picture) =>
        First + Array.IndexOf(Kinds, picture.Kind) * CollectiblePicture.Frames + picture.Frame;

    /// <summary>The collectible picture <paramref name="definition"/> names; false for a block of the tileset.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The definition lies past every collectible picture.</exception>
    public static bool TryPictureOf(int definition, out CollectiblePicture picture)
    {
        picture = default;
        if (definition < First)
            return false;
        if (definition >= End)
            throw new ArgumentOutOfRangeException(nameof(definition), definition, "The definition names no block and no collectible picture.");
        int index = definition - First;
        picture = new CollectiblePicture(Kinds[index / CollectiblePicture.Frames], index % CollectiblePicture.Frames);
        return true;
    }
}
