using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// The background characters gameplay's native VRAM words stand for (#1406): where the cartridge keeps the room's tile
/// characters and layer 3's, decoded once into the sets and characters Core binds by.
/// </summary>
[PpuLayoutBoundary("codec: gameplay's background character VRAM words as character sets and characters")]
internal static class BgCharacterWords
{
    /// <summary>Whether <paramref name="firstWord"/> lies among the room's tile characters.</summary>
    internal static bool AmongTiles(int firstWord) =>
        firstWord >= RoomGraphicsLayout.AreaCharacters.Offset && firstWord < RoomGraphicsLayout.AreaCharacters.Offset + RoomGraphicsLayout.AreaCharacters.Count;

    /// <summary>
    /// <paramref name="bytes"/> of <paramref name="identity"/>, which the cartridge copies to <paramref name="firstWord"/>,
    /// placed over the room's tile characters or the standard layer-3 characters there.
    /// </summary>
    /// <exception cref="InvalidDataException">The words hold neither, or not from a character's first word.</exception>
    internal static BgCharacterPlacement Place(BgArtworkIdentity identity, int firstWord, ReadOnlyMemory<byte> bytes)
    {
        bool amongHud = firstWord >= GameplayVramLayout.HudCharacters.Offset && firstWord < GameplayVramLayout.HudCharacters.Offset + GameplayVramLayout.HudCharacters.Count;
        if (!AmongTiles(firstWord) && !amongHud)
            throw new InvalidDataException($"{identity.Name} is copied to VRAM word ${firstWord:X4}, which holds no background characters.");
        BgCharacterSet set = AmongTiles(firstWord) ? BgCharacterSet.Tiles : BgCharacterSet.Layer3;
        int offset = firstWord - (set == BgCharacterSet.Tiles ? RoomGraphicsLayout.AreaCharacters.Offset : GameplayVramLayout.HudCharacters.Offset);
        int characterWords = set.CharacterBytes() / sizeof(ushort);
        return offset % characterWords == 0 ? new BgCharacterPlacement(identity, set, offset / characterWords, bytes)
            : throw new InvalidDataException($"{identity.Name} is copied to VRAM word ${firstWord:X4}, inside one of the {set} characters.");
    }
}
