using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The rot step each region of the OBJ characters a rotting corpse uploads shows (#1392). A corpse rots by moving the pixel
/// rows of its staged characters down and uploading them after each step; its sheet holds the picture of each step, so the
/// room keeps only the step each uploaded row showed last, for its frames to name the picture they draw.
/// </summary>
[RoomLifetime]
internal sealed class CorpseRotUploads
{
    /// <summary>The rows uploaded, oldest first; a row a later one covers is dropped.</summary>
    private readonly List<CorpseRotRow> rows = [];

    /// <summary>Records the upload of <paramref name="byteCount"/> bytes of a corpse's characters after rot step <paramref name="step"/>.</summary>
    /// <param name="destination">The native destination the cartridge uploads the row to.</param>
    /// <param name="byteCount">The row's bytes.</param>
    /// <param name="step">The rot steps the corpse's characters had taken when uploaded.</param>
    [PpuLayoutBoundary("codec: a corpse row upload's native VRAM destination in the OBJ characters")]
    internal void Show(VramDmaDestination destination, int byteCount, int step)
    {
        int firstByte = ObjCharacterUploads.TableByte(destination);
        if (firstByte < 0 || byteCount <= 0 || firstByte + byteCount > ObjCharacterUploads.TableByteCount)
            throw new ArgumentOutOfRangeException(nameof(destination), destination.Word,
                $"{byteCount} corpse bytes at VRAM word ${destination.Word:X4} do not lie inside the OBJ characters.");
        Ensure.AtLeastZero(step);
        // A row wholly covered by a later upload no longer shows anywhere.
        rows.RemoveAll(row => row.FirstByte >= firstByte && row.FirstByte + row.ByteCount <= firstByte + byteCount);
        rows.Add(new CorpseRotRow(firstByte, byteCount, step));
    }

    /// <summary>The rot step <paramref name="region"/> shows, while a corpse upload shows over it.</summary>
    /// <exception cref="InvalidDataException">A corpse upload shows over only part of the region.</exception>
    internal bool TryShowing(CharacterRegion region, out int step)
    {
        Ensure.NotNull(region);
        for (int index = rows.Count - 1; index >= 0; index--)
        {
            CorpseRotRow row = rows[index];
            if (row.FirstByte >= region.FirstByte + region.ByteCount || region.FirstByte >= row.FirstByte + row.ByteCount)
                continue;
            if (region.FirstByte < row.FirstByte || region.FirstByte + region.ByteCount > row.FirstByte + row.ByteCount)
                throw new InvalidDataException(
                    $"A corpse row upload at table byte {row.FirstByte} shows over part of the region at {region.FirstByte}.");
            step = row.Step;
            return true;
        }
        step = 0;
        return false;
    }

    /// <summary>One uploaded corpse row.</summary>
    private sealed record CorpseRotRow(int FirstByte, int ByteCount, int Step);
}
