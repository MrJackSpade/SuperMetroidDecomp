using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="SpriteArtworkPlacements"/> members production does not use.</summary>
internal static class SpriteCharactersAccess
{
    /// <summary>Exposes <see cref="SpriteArtworkPlacements"/> state that only verification reads.</summary>
    extension(SpriteArtworkPlacements)
    {
        /// <summary>The table byte an upload to <paramref name="destination"/> starts at, which lies outside the table for one that misses it.</summary>
        internal static int TableByte(VramDmaDestination destination) =>
            (destination.Word - GameplayVramLayoutConstants.ObjectCharacters.Offset) * sizeof(ushort);
    }

    extension(SpriteArtworkPlacements self)
    {
        /// <summary>The uploads in order.</summary>
        public IReadOnlyList<SpriteArtworkPlacement> Uploads => self.placements;

        /// <summary>Shows <paramref name="placement"/>, as production's setup and encounters do.</summary>
        public void Record(SpriteArtworkPlacement placement) => self.Show(placement);

        /// <summary>
        /// Shows <paramref name="byteCount"/> bytes of <paramref name="source"/> from table byte <paramref name="firstByte"/>,
        /// <paramref name="sourceOffset"/> bytes into it, as the verification queue's transfers land.
        /// </summary>
        /// <exception cref="InvalidDataException">The bytes are not whole characters from a character's start.</exception>
        public void Record(int firstByte, int byteCount, SpriteArtworkSource source, int sourceOffset = 0)
        {
            if (firstByte % SpriteArtworkPlacements.CharacterByteCount != 0 || byteCount % SpriteArtworkPlacements.CharacterByteCount != 0)
                throw new InvalidDataException($"{byteCount} bytes of {source} at table byte {firstByte} are not whole sprite characters.");
            self.Show(new SpriteArtworkPlacement(sourceOffset == 0 ? source : new OffsetSpriteCharacters(source, sourceOffset),
                firstByte / SpriteArtworkPlacements.CharacterByteCount, byteCount / SpriteArtworkPlacements.CharacterByteCount));
        }
        /// <summary>
        /// Records the part of one VRAM upload that lands in the OBJ character table. Uploads that miss the table are
        /// ignored, as the table does not show them.
        /// </summary>
        /// <param name="destination">The upload's VRAM destination.</param>
        /// <param name="byteCount">Bytes uploaded.</param>
        /// <param name="source">The owned artwork or memory bytes uploaded.</param>
        /// <exception cref="InvalidDataException">The upload reaches the table with a non-linear word increment.</exception>
        public void RecordUpload(VramDmaDestination destination, int byteCount, SpriteArtworkSource source)
        {
            int uploadStart = TableByte(destination);
            int shownStart = Math.Max(uploadStart, 0);
            int shownEnd = Math.Min(uploadStart + byteCount, SpriteArtworkPlacements.CharacterCount * SpriteArtworkPlacements.CharacterByteCount);
            if (shownStart >= shownEnd)
                return;
            if (destination.WordIncrement != 1)
                throw new InvalidDataException(
                    $"A VRAM upload to word ${destination.Word:X4} reaches the OBJ characters with increment {destination.WordIncrement}; only linear uploads record characters.");
            self.Record(shownStart, shownEnd - shownStart, source, shownStart - uploadStart);
        }
    }
}
