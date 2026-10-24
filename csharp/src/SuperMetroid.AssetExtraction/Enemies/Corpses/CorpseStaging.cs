using System.Buffers.Binary;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// The work memory a rotting corpse stages its characters in (#1392): the room's enemy BG2 buffer, which other rooms leave
/// other data in, and the rotting heap, which holds Mother Brain's corpse and the dead Torizo's sand strips. A corpse reads
/// and uploads only bytes it wrote, which this checks, so its pictures never depend on what an earlier room left.
/// </summary>
internal sealed class CorpseStaging
{
    /// <summary>The enemy BG2 buffer the Tourian corpses and the dead Torizo stage in.</summary>
    private readonly StagedBytes bg2 = new(EnemyStagingDefinitions.Bg2NativeAddress, EnemyStagingDefinitions.Bg2ByteCount);

    /// <summary>The rotting heap Mother Brain's corpse stages in and the dead Torizo's sand falls into.</summary>
    private readonly StagedBytes heap = new(EnemyStagingDefinitions.RottingHeapNativeAddress, EnemyStagingDefinitions.RottingHeapByteCount);

    /// <summary>Bytes in one character.</summary>
    private const int TileBytes = 32;

    /// <summary>The Sidehopper corpse's staged characters for graphics variant <paramref name="graphicsVariant"/>, copied from <paramref name="atlas"/>.</summary>
    internal static CorpseStaging Sidehopper(ReadOnlySpan<byte> atlas, ushort graphicsVariant)
    {
        var staged = new CorpseStaging();
        for (int row = 0; row < 5; row++)
            staged.Copy(atlas, CorpseStagingGeometryDefinitions.SidehopperInitialCopy(graphicsVariant, row));
        return staged;
    }

    /// <summary>A dead Zoomer's, Ripper's or Skree's staged characters for its variant <paramref name="layout"/>, copied from <paramref name="atlas"/>.</summary>
    internal static CorpseStaging Tourian(ReadOnlySpan<byte> atlas, DeadTourianCorpseVariant layout)
    {
        var staged = new CorpseStaging();
        foreach (CorpseGraphicsCopy copy in Ensure.NotNull(layout).InitialGraphicsCopies)
            staged.Copy(atlas, copy);
        return staged;
    }

    /// <summary>
    /// The dead Torizo's staged characters: every byte cleared, then its rows copied from <paramref name="atlas"/>; its sand
    /// strips zero, as the session's rotting heap holds them until its sand falls.
    /// </summary>
    internal static CorpseStaging Torizo(ReadOnlySpan<byte> atlas)
    {
        var staged = new CorpseStaging();
        staged.bg2.Clear(0, EnemyStagingDefinitions.Bg2ByteCount);
        staged.heap.Clear(EnemyStagingDefinitions.DeadTorizoSandOffset, EnemyStagingDefinitions.DeadTorizoSandByteCount);
        for (int row = 0; row < DeadTorizoGeometryDefinitions.Rows; row++)
            staged.Copy(atlas, DeadTorizoGeometryDefinitions.InitialCopy(row));
        return staged;
    }

    /// <summary>Mother Brain's staged corpse characters: its rows copied from the installed corpse <paramref name="atlas"/> into the rotting heap.</summary>
    internal static CorpseStaging MotherBrain(ReadOnlySpan<byte> atlas)
    {
        var staged = new CorpseStaging();
        for (int row = 0; row < MotherBrainCorpseArtworkDefinitions.RowCount; row++)
            staged.heap.Copy(atlas, MotherBrainCorpseArtworkDefinitions.InitialCopySource(row) - MotherBrainCorpseArtworkDefinitions.SourceAddress,
                row * MotherBrainCorpseArtworkDefinitions.RowBytes, MotherBrainCorpseArtworkDefinitions.InitialCopyLength(row));
        return staged;
    }

    /// <summary>Copies a run of installed corpse characters into the enemy BG2 buffer.</summary>
    internal void Copy(ReadOnlySpan<byte> atlas, CorpseGraphicsCopy copy) => bg2.Copy(atlas, copy.SourceOffset, copy.DestinationOffset, copy.Length);

    /// <summary>
    /// Copies pixel row <paramref name="y"/> of each of the corpse's character columns down to the next, as the cartridge's
    /// unrolled movers do, and clears it for a move.
    /// </summary>
    internal void MoveRow(CorpseRowGeometry geometry, ushort y, CorpseRowOperation operation)
    {
        Ensure.NotNull(geometry);
        StagedBytes tiles = geometry.InHeap ? heap : bg2;
        int row = y >> 3;
        if (row >= geometry.Rows)
            throw new ArgumentOutOfRangeException(nameof(y), y, $"The corpse has {geometry.Rows} rows of characters.");
        int source = row * geometry.TilesPerRow * TileBytes + (y & 7) * 2;
        int destination = (y & 7) >= 6 ? geometry.Wrap + source : source;
        for (int column = 0; column < geometry.ColumnWords.Length; column++)
        {
            if (y < geometry.ColumnMinimumY[column])
                continue;
            int sourceWord = source / 2 + geometry.ColumnWords[column];
            int destinationWord = destination / 2 + geometry.ColumnWords[column] + 1;
            if (y < geometry.CopyLimit)
            {
                tiles.WriteWord(destinationWord, tiles.ReadWord(sourceWord));
                tiles.WriteWord(destinationWord + 8, tiles.ReadWord(sourceWord + 8));
            }
            if (operation == CorpseRowOperation.Move)
            {
                tiles.WriteWord(sourceWord, 0);
                tiles.WriteWord(sourceWord + 8, 0);
            }
        }
    }

    /// <summary>Copies the first bitplane word of eighteen spaced rows of sand line <paramref name="line"/> into the sand strips.</summary>
    internal void CopySandLine(ReadOnlySpan<byte> atlas, int line)
    {
        int destination = (EnemyStagingDefinitions.DeadTorizoSandOffset + CorpseStagingGeometryDefinitions.SandDestination(line)) / 2;
        int source = CorpseStagingGeometryDefinitions.SandSource(line);
        // Eighteen tile rows are sixteen bytes apart in both the source sheet and the strips; only their first word changes.
        for (int row = 0; row < 18; row++)
            heap.WriteWord(destination + row * 8, BinaryPrimitives.ReadUInt16LittleEndian(atlas[(source + row * 16)..]));
    }

    /// <summary>The bytes the cartridge uploads for <paramref name="row"/> now.</summary>
    /// <exception cref="InvalidDataException">The row uploads bytes the corpse never wrote, or lies outside its work memory.</exception>
    [CpuBusBoundary("codec: a corpse row transfer's native work-memory source")]
    internal byte[] Upload(CorpseRotRow row)
    {
        Ensure.NotNull(row);
        if (bg2.TryRead(row.SourceAddress, row.ByteCount, out byte[]? bytes) || heap.TryRead(row.SourceAddress, row.ByteCount, out bytes))
            return bytes;
        throw new InvalidDataException($"A corpse row uploads ${row.SourceAddress:X6}+{row.ByteCount}, outside its work memory.");
    }

    /// <summary>One buffer of work memory, with which of its bytes a corpse has written.</summary>
    /// <param name="nativeAddress">The buffer's first native work-memory address.</param>
    /// <param name="byteCount">Its bytes.</param>
    private sealed class StagedBytes(int nativeAddress, int byteCount)
    {
        /// <summary>The buffer's bytes.</summary>
        private readonly byte[] bytes = new byte[byteCount];

        /// <summary>Which bytes a corpse has written.</summary>
        private readonly bool[] written = new bool[byteCount];

        /// <summary>Zeroes <paramref name="count"/> bytes from <paramref name="offset"/>.</summary>
        internal void Clear(int offset, int count)
        {
            Array.Clear(bytes, offset, count);
            Array.Fill(written, true, offset, count);
        }

        /// <summary>Copies <paramref name="count"/> bytes of <paramref name="atlas"/> from <paramref name="source"/> to <paramref name="destination"/>.</summary>
        internal void Copy(ReadOnlySpan<byte> atlas, int source, int destination, int count)
        {
            atlas.Slice(source, count).CopyTo(bytes.AsSpan(destination, count));
            Array.Fill(written, true, destination, count);
        }

        /// <summary>The word at <paramref name="word"/>, which a corpse must have written.</summary>
        /// <exception cref="InvalidDataException">No corpse wrote it.</exception>
        internal ushort ReadWord(int word)
        {
            if (!written[word * 2] || !written[word * 2 + 1])
                throw new InvalidDataException($"A corpse row reads staged word {word} of ${nativeAddress:X6}, which no corpse wrote.");
            return BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(word * 2));
        }

        /// <summary>Writes <paramref name="value"/> at word <paramref name="word"/>.</summary>
        internal void WriteWord(int word, ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(word * 2), value);
            written[word * 2] = written[word * 2 + 1] = true;
        }

        /// <summary>The <paramref name="count"/> bytes from native <paramref name="address"/>, when the buffer holds them.</summary>
        /// <exception cref="InvalidDataException">A corpse never wrote one of them.</exception>
        internal bool TryRead(int address, int count, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out byte[]? read)
        {
            int offset = address - nativeAddress;
            if (offset < 0 || offset + count > bytes.Length)
            {
                read = null;
                return false;
            }
            if (Array.IndexOf(written, false, offset, count) is int unwritten and >= 0)
                throw new InvalidDataException($"A corpse row uploads staged byte {unwritten} of ${nativeAddress:X6}, which no corpse wrote.");
            read = bytes.AsSpan(offset, count).ToArray();
            return true;
        }
    }
}
