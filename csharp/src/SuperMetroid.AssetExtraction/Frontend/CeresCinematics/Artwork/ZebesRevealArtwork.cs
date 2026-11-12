using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The Zebes reveal's tilemap and characters (#1467), which the import pictures the planet from; the game draws that
/// installed picture and reads neither. The characters are import data the installation never holds (#1636).
/// </summary>
public sealed class ZebesRevealArtwork
{
    /// <summary>Retains the validated tilemap and characters.</summary>
    private ZebesRevealArtwork(RoomBackgroundTilemapAtlas map, RoomCharacterAtlas characters)
    {
        Map = map;
        Characters = characters;
    }

    /// <summary>One compiled 32-by-32 ordinary BG tilemap page for the Zebes reveal, replacing the $0800-byte map imported from $97:8ADB and transferred by $8B:C6F1–$C70E.</summary>
    public RoomBackgroundTilemapAtlas Map { get; }
    /// <summary>Compiled $4000-byte 4bpp character sheet for the Zebes reveal, replacing artwork imported from $96:EC76 and loaded at VRAM byte $C000.</summary>
    public RoomCharacterAtlas Characters { get; }

    /// <summary>The Zebes reveal of the installed tilemap <paramref name="map"/> and the import's <paramref name="characters"/>.</summary>
    /// <exception cref="InvalidDataException">The characters are not $4000 bytes.</exception>
    public static ZebesRevealArtwork Of(RoomBackgroundTilemapAtlas map, RoomCharacterAtlas characters)
    {
        Ensure.NotNull(map);
        Ensure.NotNull(characters);
        if (characters.Characters.Length != ZebesRevealArtworkFormat.CharacterByteCount)
            throw new InvalidDataException($"Zebes reveal characters hold {characters.Characters.Length} bytes; expected {ZebesRevealArtworkFormat.CharacterByteCount}.");
        return new ZebesRevealArtwork(map, characters);
    }
}
