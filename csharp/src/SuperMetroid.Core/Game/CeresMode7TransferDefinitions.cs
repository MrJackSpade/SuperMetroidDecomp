using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>One $A6 Mode 7 low-byte tilemap DMA descriptor and its authored source bytes.</summary>
public readonly record struct CeresMode7Transfer(
    int SourceAddress, ushort DestinationWord, CeresMode7TransferDefinitions.TileSequence TileNumbers);

/// <summary>
/// The seven fixed transfer lists selected by the Ceres door/elevator and Ridley
/// animation routines. These are tilemap instructions, not character pixels: $2118
/// replaces only the low byte of each VRAM word and leaves its high byte untouched.
/// </summary>
public static class CeresMode7TransferDefinitions
{
    /// <summary>$A6:F904, Ceres elevator landing-platform light frame.</summary>
    public const ushort ElevatorLight = 0xf904;
    /// <summary>$A6:F90E, Ceres elevator landing-platform dark frame.</summary>
    public const ushort ElevatorDark = 0xf90e;
    /// <summary>$A6:ACE2, baby-capsule tilemap frame zero.</summary>
    public const ushort BabyFrame0 = 0xace2;
    /// <summary>$A6:ACF5, baby-capsule tilemap frame one (also frame three).</summary>
    public const ushort BabyFrame1 = 0xacf5;
    /// <summary>$A6:AD08, baby-capsule tilemap frame two.</summary>
    public const ushort BabyFrame2 = 0xad08;
    /// <summary>$A6:AD49, Ridley wing tilemap frame zero.</summary>
    public const ushort WingFrame0 = 0xad49;
    /// <summary>$A6:AD80, Ridley wing tilemap frame one.</summary>
    public const ushort WingFrame1 = 0xad80;

    /// <summary>
    /// $A6:ACDA-$ACE1 selects the three consecutive capsule transfer records
    /// in reflected phase order0,1,2,1. Each record has two nine-byte transfers
    /// and a one-byte terminator; phase must already be bounded to0..3.
    /// </summary>
    internal static ushort BabyFrameForPhase(int phase)
    {
        if ((uint)phase >= 4) throw new IndexOutOfRangeException();
        int reflectedPhase = Math.Min(phase, 4 - phase);
        return checked((ushort)(BabyFrame0 + reflectedPhase * (BabyFrame1 - BabyFrame0)));
    }

    /// <summary>Mode7 maps have128 tiles per row. Transfer destinations name low bytes of VRAM words.</summary>
    private const int MapColumns = 128;
    /// <summary>$A6:F918: paired four-byte platform frames following both ten-byte DMA lists.</summary>
    private const int PlatformPayload = 0xa6f918;
    /// <summary>$A6:AD1B: three baby frames, each two rows of two atlas tiles.</summary>
    private const int BabyPayload = 0xa6ad1b;
    /// <summary>$A6:ADB7: wing rows store frame0 then frame1, with identical extent per row.</summary>
    private const int WingPayload = 0xa6adb7;
    /// <summary>$A6:F904/F90E: chosen platform map position(14,12) remains REQUIRED layout input.</summary>
    private const int PlatformColumn = 14, PlatformRow = 12;
    /// <summary>$A6:ACE2/ACF5/AD08: chosen baby map position(4,10) remains REQUIRED layout input.</summary>
    private const int BabyColumn = 4, BabyRow = 10;
    private readonly record struct WingRegion(int Column, int Width);
    /// <summary>$A6:AD49/AD80: six chosen wing row extents remain REQUIRED under Wing0/Wing1. Only row stepping and packed paired-frame addresses derive from them.</summary>
    private static readonly WingRegion[] RequiredWingRegions =
    [ new(11,4), new(0,14), new(0,14), new(1,12), new(1,15), new(0,16) ];

    /// <summary>Resolves a native list pointer without reading cartridge memory or constructing a transfer table.</summary>
    public static TransferSequence Get(ushort pointer) => pointer switch
    {
        ElevatorLight or ElevatorDark => new(pointer, 1),
        BabyFrame0 or BabyFrame1 or BabyFrame2 => new(pointer, 2),
        WingFrame0 or WingFrame1 => new(pointer, RequiredWingRegions.Length),
        _ => throw new InvalidDataException($"Unknown Ceres Mode7 transfer list $A6:{pointer:X4}."),
    };
    public readonly struct TransferSequence(ushort pointer, int count) : IReadOnlyList<CeresMode7Transfer>
    {
        public int Count => count;
        public int Length => Count;
        public CeresMode7Transfer this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                if (pointer is ElevatorLight or ElevatorDark)
                    return new(PlatformPayload + (pointer == ElevatorDark ? 4 : 0),
                        (ushort)(PlatformRow * MapColumns + PlatformColumn), new(pointer, index, 4));
                if (pointer is BabyFrame0 or BabyFrame1 or BabyFrame2)
                {
                    int frame = (pointer - BabyFrame0) / (2 * 9 + 1);
                    return new(BabyPayload + frame * 4 + index * 2,
                        (ushort)((BabyRow + index) * MapColumns + BabyColumn), new(pointer, index, 2));
                }
                WingRegion region = RequiredWingRegions[index];
                int offset = 0;
                for (int row = 0; row < index; row++) offset += 2 * RequiredWingRegions[row].Width;
                if (pointer == WingFrame1) offset += region.Width;
                return new(WingPayload + offset, (ushort)(index * MapColumns + region.Column),
                    new(pointer, index, region.Width));
            }
        }
        public IEnumerator<CeresMode7Transfer> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public readonly struct TileSequence(ushort pointer, int row, int count) : IReadOnlyList<byte>
    {
        public int Count => count;
        public int Length => Count;
        public byte this[int index] => (uint)index < Count ? TileAt(pointer, row, index) : throw new IndexOutOfRangeException();
        public IEnumerator<byte> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$A6:F918-F91F,AD1B-AD26: independent platform boundary/middle glyphs and six baby row origins remain REQUIRED; duplicate middle and adjacent right tiles calculate.</summary>
    private static byte TileAt(ushort pointer, int row, int column)
    {
        if (pointer is ElevatorLight or ElevatorDark)
        {
            bool dark = pointer == ElevatorDark;
            return (byte)(column == 0 ? (dark ? 0x8d : 0x68) : column == 3 ? (dark ? 0x79 : 0x78) : (dark ? 0x8e : 0x69));
        }
        if (pointer is BabyFrame0 or BabyFrame1 or BabyFrame2)
        {
            int origin = (pointer, row) switch
            {
                (BabyFrame0, 0) => 0x59, (BabyFrame0, 1) => 0x69,
                (BabyFrame1, 0) => 0x8a, (BabyFrame1, 1) => 0x8c,
                (BabyFrame2, 0) => 0x8e, (BabyFrame2, 1) => 0x9d,
                _ => throw new InvalidOperationException("Unknown baby tilemap row."),
            };
            return (byte)(origin + column);
        }
        return WingTile(pointer == WingFrame1, row, column);
    }
    /// <summary>$A6:ADB7-AE4C: contiguous atlas runs and shared cells derive. Every selected run origin/extent and isolated glyph below remains REQUIRED artwork placement; no artwork exemption is claimed.</summary>
    private static byte WingTile(bool secondFrame, int row, int column)
    {
        const byte transparent = 0xff;
        if (!secondFrame)
        {
            return row switch
            {
                0 => (byte)column,
                1 => column is 6 or 7 ? transparent : (byte)(column < 6 ? 4 + column : 0x0a + column - 8),
                2 => (byte)(column == 13 ? 0xa8 : 0x10 + column),
                3 => (byte)(0x21 + column),
                4 => column is >= 2 and <= 4 ? (byte)(0x1d + column - 2)
                    : column is >= 5 and <= 9 ? (byte)(0x30 + column - 5) : transparent,
                5 => column is 4 or 5 ? (byte)(0x2e + column - 4)
                    : column is >= 6 and <= 10 ? (byte)(0x40 + column - 6) : transparent,
                _ => throw new InvalidOperationException("Unknown wing row."),
            };
        }
        if (row < 2) return transparent;
        if (row == 2) return column switch { 6 => 0x20, 7 => WingTile(false,row,column), 8 => 0xaa, _ => transparent };
        if (row == 3) return column is >= 5 and <= 7 ? WingTile(false,row,column) : transparent;
        if (row == 4)
            return column < 5 ? (byte)(0x91 + column)
                : column < 8 ? WingTile(false,row,column)
                : column < 13 ? (byte)(0x96 + column - 8)
                : column == 13 ? (byte)0x98 : (byte)0x9c;
        if (row == 5)
            return column == 0 ? (byte)0x90
                : column < 6 ? (byte)(0x9f + column - 1)
                : column < 9 ? WingTile(false,row,column)
                : column < 13 ? (byte)(0xa4 + column - 9)
                : column == 13 ? (byte)0x7d : column == 14 ? (byte)0x83 : (byte)0x2d;
        throw new InvalidOperationException("Unknown wing row.");
    }
    /// <summary>Replays direct calculated tiles as native $2118 low-byte writes, preserving high bytes and list order.</summary>
    public static void ApplyTo(SnesVram vram, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(vram);
        foreach (CeresMode7Transfer transfer in Get(pointer))
            for (int index = 0; index < transfer.TileNumbers.Count; index++)
                vram.FillMode7MapBytes(transfer.TileNumbers[index], 1, (ushort)(transfer.DestinationWord + index));
    }
}
