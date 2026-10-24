namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="ObjCharacterUploads"/> members production does not use.</summary>
internal static class ObjCharacterUploadsAccess
{
    /// <summary>Exposes <see cref="ObjCharacterUploads"/> state that only verification reads.</summary>
    extension(ObjCharacterUploads self)
    {
        /// <summary>The uploads in order.</summary>
        public IReadOnlyList<ObjCharacterUpload> Uploads => self.uploads;
        /// <summary>
        /// Records the part of one VRAM upload that lands in the OBJ character table. Uploads that miss the table are
        /// ignored, as the table does not show them.
        /// </summary>
        /// <param name="destination">The upload's VRAM destination.</param>
        /// <param name="byteCount">Bytes uploaded.</param>
        /// <param name="source">The owned artwork or memory bytes uploaded.</param>
        /// <exception cref="InvalidDataException">The upload reaches the table with a non-linear word increment.</exception>
        public void RecordUpload(VramDmaDestination destination, int byteCount, ObjCharacterSource source)
        {
            int uploadStart = ObjCharacterUploads.TableByte(destination);
            int shownStart = Math.Max(uploadStart, 0);
            int shownEnd = Math.Min(uploadStart + byteCount, ObjCharacterUploads.TableByteCount);
            if (shownStart >= shownEnd)
                return;
            if (destination.WordIncrement != 1)
                throw new InvalidDataException(
                    $"A VRAM upload to word ${destination.Word:X4} reaches the OBJ characters with increment {destination.WordIncrement}; only linear uploads record characters.");
            self.Record(shownStart, shownEnd - shownStart, source, shownStart - uploadStart);
        }
    }
}
