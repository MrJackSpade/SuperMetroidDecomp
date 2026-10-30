using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The Zebes reveal's editable tilemap and characters (#1467), which the import pictures the planet from; the game draws
/// that installed picture and reads neither.
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

    /// <summary>Loads the Zebes reveal's tilemap and validates its characters' size.</summary>
    /// <param name="mapJson">One-page BG tilemap JSON from <c>zebes-reveal-tilemap.json</c>, consumed from its current position and left open.</param>
    /// <param name="characters">The installed 4bpp characters of <c>zebes-reveal-characters.png</c>, $4000 bytes.</param>
    /// <exception cref="InvalidDataException">The tilemap is malformed or the characters are not $4000 bytes.</exception>
    public static ZebesRevealArtwork Load(Stream mapJson, RoomCharacterAtlas characters)
    {
        Ensure.NotNull(mapJson);
        Ensure.NotNull(characters);
        if (characters.Characters.Length != ZebesRevealArtworkFormat.CharacterByteCount)
            throw new InvalidDataException($"Zebes reveal characters hold {characters.Characters.Length} bytes; expected {ZebesRevealArtworkFormat.CharacterByteCount}.");
        return new ZebesRevealArtwork(RoomBackgroundTilemapAtlas.Load(mapJson, ZebesRevealArtworkFormat.MapByteCount), characters);
    }
}
