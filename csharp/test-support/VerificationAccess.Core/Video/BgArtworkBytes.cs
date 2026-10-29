using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// The bytes each background artwork name stands for (#1422): Core binds artwork by <see cref="BgArtworkIdentity"/> and
/// character count, and this resolves the names against the installed artwork and the import pipeline's own data, the one
/// place the names meet their bytes. Each source registers the names it holds; a name nothing registered fails.
/// </summary>
internal sealed class BgArtworkBytes
{
    /// <summary>Each registered name and how its bytes are read.</summary>
    private readonly Dictionary<BgArtworkIdentity, Func<ReadOnlyMemory<byte>>> sources = [];

    /// <summary>Whether a source registered <paramref name="identity"/>.</summary>
    internal bool Names(BgArtworkIdentity identity) => sources.ContainsKey(identity);

    /// <summary>The bytes <paramref name="identity"/> stands for, from its first character.</summary>
    /// <exception cref="InvalidDataException">No source registered the name.</exception>
    internal ReadOnlyMemory<byte> Of(BgArtworkIdentity identity) => sources.TryGetValue(identity, out Func<ReadOnlyMemory<byte>>? read)
        ? read()
        : throw new InvalidDataException($"No installed or imported artwork resolves background artwork {identity.Name}.");

    /// <summary>The bytes <paramref name="binding"/> shows: the first characters of its artwork that it counts.</summary>
    /// <exception cref="InvalidDataException">No source registered the artwork, or it holds fewer characters than bound.</exception>
    internal ReadOnlyMemory<byte> Of(BgArtworkBinding binding) => Shown(binding, Of(Ensure.NotNull(binding).Identity));

    /// <summary>The bytes <paramref name="binding"/> shows of <paramref name="artwork"/>: its first characters that the binding counts.</summary>
    /// <exception cref="InvalidDataException">The artwork holds fewer characters than bound.</exception>
    internal static ReadOnlyMemory<byte> Shown(BgArtworkBinding binding, ReadOnlyMemory<byte> artwork)
    {
        Ensure.NotNull(binding);
        int count = binding.CharacterCount * binding.Set.CharacterBytes();
        return artwork.Length >= count ? artwork[..count]
            : throw new InvalidDataException($"{binding.Identity.Name} holds {artwork.Length} bytes; its binding shows {count}.");
    }

    /// <summary>Registers <paramref name="identity"/> as <paramref name="bytes"/>.</summary>
    /// <exception cref="InvalidOperationException">A source already registered the name.</exception>
    internal BgArtworkBytes Add(BgArtworkIdentity identity, ReadOnlyMemory<byte> bytes) => Add(identity, () => bytes);

    /// <summary>Registers <paramref name="identity"/>, read by <paramref name="read"/> when composed.</summary>
    /// <exception cref="InvalidOperationException">A source already registered the name.</exception>
    private BgArtworkBytes Add(BgArtworkIdentity identity, Func<ReadOnlyMemory<byte>> read)
    {
        if (!sources.TryAdd(identity, read))
            throw new InvalidOperationException($"Background artwork {identity.Name} is registered twice.");
        return this;
    }

    /// <summary>The common room elements' characters and every tileset's, as <paramref name="characters"/> installs them.</summary>
    internal BgArtworkBytes WithRoomCharacters(RoomCharacterAtlases characters)
    {
        Ensure.NotNull(characters);
        Add(RoomBgArtwork.CommonRoomElements, () => characters.Cre.Characters);
        foreach (RoomCharacterSet set in Enum.GetValues<RoomCharacterSet>())
            Add(RoomBgArtwork.Tileset(set), () => characters.Get(set).Characters);
        return this;
    }

    /// <summary>
    /// The HUD's characters as <paramref name="hud"/> installs them: the standard HUD artwork and the copy Kraid's library
    /// background makes.
    /// </summary>
    internal BgArtworkBytes WithHud(HudTileAtlas hud)
    {
        Ensure.NotNull(hud);
        Add(BgArtworkIdentity.Asset(InstalledArtworkId.StandardHudTiles), () => hud.Bytes);
        Add(RoomBgArtwork.Library(LibraryBackgroundSource.HudCharacters), () => hud.Bytes);
        return this;
    }

    /// <summary>Kraid's room backdrop characters, as <paramref name="kraid"/> installs them.</summary>
    internal BgArtworkBytes WithKraidBackground(KraidBackgroundArtwork kraid)
    {
        Ensure.NotNull(kraid);
        return Add(KraidBehavior.KraidRoomBackgroundIdentity, () => kraid.RoomBackgroundTiles.Characters);
    }

    /// <summary>
    /// Every permanent item's cartridge characters, which the room block sheet's stock pictures of the item over its slot
    /// are composed from (#1453): gameplay binds the item by name and uploads nothing.
    /// </summary>
    internal BgArtworkBytes WithCollectibles()
    {
        foreach (InWorldCollectibleKind kind in RoomPlmDynamicCollectibleGraphicsDefinitions.Kinds)
        {
            InWorldCollectibleKind collectible = kind;
            Add(RoomBgArtwork.Collectible(collectible), () => DynamicCollectibleGraphics.Get(collectible).Tiles);
        }
        return this;
    }
}
