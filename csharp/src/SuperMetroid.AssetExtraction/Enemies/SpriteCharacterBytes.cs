using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The sprite characters' bytes as the cartridge's OBJ character table holds them (#1406): what extraction composes each
/// set's pictures over, one character after another.
/// </summary>
internal static class SpriteCharacterBytes
{
    /// <summary>The bytes of the whole table.</summary>
    extension(SpriteArtworkPlacements)
    {
        /// <summary>The bytes every sprite character holds together.</summary>
        internal static int TableByteCount => SpriteArtworkPlacements.CharacterCount * SpriteArtworkPlacements.CharacterByteCount;
    }

    /// <summary>The table bytes a placement shows over.</summary>
    extension(SpriteArtworkPlacement self)
    {
        /// <summary>The table byte the placement shows from.</summary>
        internal int FirstByte => self.FirstCharacter * SpriteArtworkPlacements.CharacterByteCount;

        /// <summary>The table bytes the placement shows.</summary>
        internal int ByteCount => self.CharacterCount * SpriteArtworkPlacements.CharacterByteCount;
    }
}
