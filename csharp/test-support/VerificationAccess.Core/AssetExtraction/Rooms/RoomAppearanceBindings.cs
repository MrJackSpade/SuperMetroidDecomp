using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The background character bindings the cartridge would hold for what a room shows (#1466), for verification that compares
/// a VRAM image: the room's load, and the collectibles and Kraid's backdrop gameplay showed over it, each where the import
/// pipeline places it and counted as <see cref="BgArtworkBytes"/> holds its characters.
/// </summary>
internal static class RoomAppearanceBindings
{
    /// <summary>The bindings a room of <paramref name="tileset"/>'s load makes: the common room elements, then the tileset's characters.</summary>
    internal static BgArtworkBindings Load(TilesetDefinition tileset, BgArtworkBytes artwork)
    {
        var bindings = new BgArtworkBindings();
        BindLoad(bindings, tileset, artwork);
        return bindings;
    }

    /// <summary>Binds a room of <paramref name="tileset"/>'s load into <paramref name="bindings"/>.</summary>
    internal static void BindLoad(BgArtworkBindings bindings, TilesetDefinition tileset, BgArtworkBytes artwork)
    {
        Bind(bindings, RoomBgArtwork.CommonRoomElements, RoomTileCharacters.CommonRoomElementsFirst, artwork);
        Bind(bindings, RoomBgArtwork.Tileset(tileset.Characters), RoomTileCharacters.TilesetFirst, artwork);
    }

    /// <summary>The bindings of what <paramref name="appearance"/> shows over a room's load.</summary>
    internal static BgArtworkBindings Shown(RoomAppearance appearance, BgArtworkBytes artwork)
    {
        var bindings = new BgArtworkBindings();
        BindShown(bindings, appearance, artwork);
        return bindings;
    }

    /// <summary>
    /// Binds what <paramref name="appearance"/> shows over a room's load into <paramref name="bindings"/>: each slot's
    /// collectible, then Kraid's backdrop, which his rise shows after the room's PLMs set their collectibles up.
    /// </summary>
    internal static void BindShown(BgArtworkBindings bindings, RoomAppearance appearance, BgArtworkBytes artwork)
    {
        Ensure.NotNull(appearance);
        for (int slot = 0; slot < CollectiblePlms.GraphicsSlots; slot++)
        {
            if (appearance.CollectibleIn(slot) is { } kind)
                Bind(bindings, RoomBgArtwork.Collectible(kind), RoomTileCharacters.CollectiblesFirst + slot * RoomTileCharacters.CollectibleSlotCharacters, artwork);
        }
        if (appearance.KraidBackdropShown)
            Bind(bindings, RoomBgArtwork.KraidBackdrop, RoomTileCharacters.KraidBackdropFirst, artwork);
    }

    /// <summary>Binds all of <paramref name="identity"/>'s characters over the tile characters from <paramref name="first"/>.</summary>
    private static void Bind(BgArtworkBindings bindings, BgArtworkIdentity identity, int first, BgArtworkBytes artwork) =>
        Ensure.NotNull(bindings).Bind(identity, BgCharacterSet.Tiles, first, BgCharacterSet.Tiles.CharactersIn(Ensure.NotNull(artwork).Of(identity).Length));
}
