using System.Buffers.Binary;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// The work memory a rotting corpse stages its characters in (#1392): the room's enemy BG2 buffer, which other rooms leave
/// other data in, and the dead Torizo's sand strips in the rotting heap. A corpse reads and uploads only bytes it wrote,
/// which this checks, so its pictures never depend on what an earlier room left.
/// </summary>
internal sealed class CorpseStaging
{
    private readonly byte[] tiles = new byte[EnemyStagingDefinitions.Bg2ByteCount];

    private readonly bool[] written = new bool[EnemyStagingDefinitions.Bg2ByteCount];

    /// <summary>The sand strips, zero as the session's rotting heap holds them until the dead Torizo's sand falls.</summary>
    private readonly byte[] sand = new byte[EnemyStagingDefinitions.DeadTorizoSandByteCount];

    /// <summary>The Sidehopper corpse's staged characters for graphics variant <paramref name="graphicsVariant"/>, copied from <paramref name="atlas"/>.</summary>
    internal static CorpseStaging Sidehopper(ReadOnlySpan<byte> atlas, ushort graphicsVariant)
    {
        var staged = new CorpseStaging();
        for (int row = 0; row < 5; row++)
            staged.Copy(atlas, DeadMonsterRottingDefinitions.SidehopperInitialCopy(graphicsVariant, row));
        return staged;
    }

    /// <summary>A dead Zoomer's, Ripper's or Skree's staged characters for its variant <paramref name="layout"/>, copied from <paramref name="atlas"/>.</summary>
    internal static CorpseStaging Tourian(ReadOnlySpan<byte> atlas, DeadTourianBehavior.DeadTourianCorpseVariant layout)
    {
        var staged = new CorpseStaging();
        foreach (CorpseGraphicsCopy copy in Ensure.NotNull(layout).InitialGraphicsCopies)
            staged.Copy(atlas, copy);
        return staged;
    }

    /// <summary>The dead Torizo's staged characters: every byte cleared, then its rows copied from <paramref name="atlas"/>.</summary>
    internal static CorpseStaging Torizo(ReadOnlySpan<byte> atlas)
    {
        var staged = new CorpseStaging();
        Array.Fill(staged.written, true);
        for (int row = 0; row < DeadTorizoGeometryDefinitions.Rows; row++)
            staged.Copy(atlas, DeadTorizoGeometryDefinitions.InitialCopy(row));
        return staged;
    }

    /// <summary>Copies a run of installed corpse characters into the staged characters.</summary>
    internal void Copy(ReadOnlySpan<byte> atlas, CorpseGraphicsCopy copy)
    {
        atlas.Slice(copy.SourceOffset, copy.Length).CopyTo(tiles.AsSpan(copy.DestinationOffset, copy.Length));
        Array.Fill(written, true, copy.DestinationOffset, copy.Length);
    }

    /// <summary>
    /// Copies pixel row <paramref name="y"/> of each of the corpse's character columns down to the next, as the cartridge's
    /// unrolled movers do, and clears it for a move.
    /// </summary>
    internal void MoveRow(CorpseRowGeometry geometry, ushort y, CorpseRowOperation operation)
    {
        Ensure.NotNull(geometry);
        int source = DeadMonsterRottingDefinitions.RotationOffset(geometry.Table, y) + (y & 7) * 2;
        int destination = (y & 7) >= 6 ? geometry.Wrap + source : source;
        for (int column = 0; column < geometry.ColumnWords.Length; column++)
        {
            if (y < geometry.ColumnMinimumY[column])
                continue;
            int sourceWord = source / 2 + geometry.ColumnWords[column];
            int destinationWord = destination / 2 + geometry.ColumnWords[column] + 1;
            if (y < geometry.CopyLimit)
            {
                WriteWord(destinationWord, ReadWord(sourceWord));
                WriteWord(destinationWord + 8, ReadWord(sourceWord + 8));
            }
            if (operation == CorpseRowOperation.Move)
            {
                WriteWord(sourceWord, 0);
                WriteWord(sourceWord + 8, 0);
            }
        }
    }

    /// <summary>Copies the first bitplane word of eighteen spaced rows of sand line <paramref name="line"/> into the sand strips.</summary>
    internal void CopySandLine(ReadOnlySpan<byte> atlas, int line)
    {
        int destination = DeadMonsterRottingDefinitions.SandDestination(line);
        int source = DeadMonsterRottingDefinitions.SandSource(line);
        // Eighteen tile rows are sixteen bytes apart in both the source sheet and the strips; only their first word changes.
        for (int row = 0; row < 18; row++)
            BinaryPrimitives.WriteUInt16LittleEndian(sand.AsSpan(destination + row * 16), BinaryPrimitives.ReadUInt16LittleEndian(atlas[(source + row * 16)..]));
    }

    /// <summary>The bytes the cartridge uploads for <paramref name="row"/> now.</summary>
    /// <exception cref="InvalidDataException">The row uploads bytes the corpse never wrote.</exception>
    [CpuBusBoundary("codec: a corpse row transfer's native work-memory source")]
    internal byte[] Upload(CorpseRotRow row)
    {
        Ensure.NotNull(row);
        int tileOffset = row.SourceAddress - EnemyStagingDefinitions.Bg2NativeAddress;
        if (tileOffset >= 0 && tileOffset + row.ByteCount <= tiles.Length)
        {
            if (Array.IndexOf(written, false, tileOffset, row.ByteCount) is int unwritten and >= 0)
                throw new InvalidDataException($"A corpse row uploads staged byte {unwritten}, which no corpse wrote.");
            return tiles.AsSpan(tileOffset, row.ByteCount).ToArray();
        }
        int sandOffset = row.SourceAddress - EnemyStagingDefinitions.RottingHeapNativeAddress - EnemyStagingDefinitions.DeadTorizoSandOffset;
        if (sandOffset >= 0 && sandOffset + row.ByteCount <= sand.Length)
            return sand.AsSpan(sandOffset, row.ByteCount).ToArray();
        throw new InvalidDataException($"A corpse row uploads ${row.SourceAddress:X6}+{row.ByteCount}, outside its staged characters and sand.");
    }

    private ushort ReadWord(int word)
    {
        if (!written[word * 2] || !written[word * 2 + 1])
            throw new InvalidDataException($"A corpse row reads staged word {word}, which no corpse wrote.");
        return BinaryPrimitives.ReadUInt16LittleEndian(tiles.AsSpan(word * 2));
    }

    private void WriteWord(int word, ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(tiles.AsSpan(word * 2), value);
        written[word * 2] = written[word * 2 + 1] = true;
    }
}
