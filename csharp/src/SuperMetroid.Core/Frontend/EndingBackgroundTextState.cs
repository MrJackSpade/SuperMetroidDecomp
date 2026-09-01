using System.Numerics;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One bank-$8C post-credit cinematic BG object, including the item-percentage opcodes.
/// </summary>
/// <remarks>
/// The final messages are two-tile-high glyph rectangles, not an installed desktop font.
/// Interpreting their ROM records preserves localization, spacing, palette bits, and the
/// four-frame typewriter cadence while keeping the staging tilemap debugger-visible.
/// </remarks>
internal sealed class EndingBackgroundTextState
{
    private const ushort DeleteInstruction = 0x9698;
    private const ushort GotoInstruction = 0x971e;
    private const ushort DrawItemPercentageInstruction = 0xe627;
    private const ushort DrawItemPercentageSubtitleInstruction = 0xe769;
    private const ushort ClearItemPercentageSubtitleInstruction = 0xe780;
    private const ushort DrawNothing = 0x8849;
    private const ushort DrawTextToTilemap = 0x88b7;

    private readonly ISnesAddressSpace bus;
    private readonly ushort[] tilemap;
    private readonly EndingInventorySnapshot inventory;
    private readonly bool japaneseText;
    private ushort instructionPointer;
    private ushort instructionTimer = 1;

    public EndingBackgroundTextState(
        ISnesAddressSpace bus,
        ushort[] tilemap,
        ushort instructionPointer,
        EndingInventorySnapshot inventory,
        bool japaneseText)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.tilemap = tilemap ?? throw new ArgumentNullException(nameof(tilemap));
        if (tilemap.Length != 0x400)
            throw new ArgumentException("Ending BG tilemap must contain exactly $400 words.", nameof(tilemap));
        this.instructionPointer = instructionPointer;
        this.inventory = inventory;
        this.japaneseText = japaneseText;
    }

    public bool Completed => instructionPointer == 0;
    public bool RequestedItemPercentageScroll { get; private set; }

    public void Step(SnesVram vram)
    {
        ArgumentNullException.ThrowIfNull(vram);
        if (instructionPointer == 0 || instructionTimer-- != 1)
            return;

        ushort cursor = instructionPointer;
        while (true)
        {
            ushort word = ReadWord(cursor);
            if ((word & 0x8000) == 0)
            {
                instructionTimer = word;
                ushort packedPosition = ReadWord(Add(cursor, 2));
                ushort dataPointer = ReadWord(Add(cursor, 4));
                DrawRecord(packedPosition, dataPointer);
                instructionPointer = Add(cursor, 6);
                Upload(vram);
                return;
            }

            switch (word)
            {
                case DeleteInstruction:
                    instructionPointer = 0;
                    Upload(vram);
                    return;
                case GotoInstruction:
                    cursor = ReadWord(Add(cursor, 2));
                    break;
                case DrawItemPercentageInstruction:
                    DrawItemPercentage();
                    cursor = Add(cursor, 2);
                    break;
                case DrawItemPercentageSubtitleInstruction:
                    if (japaneseText)
                        CopyWords(0xdf5b, destination: 736, count: 64);
                    cursor = Add(cursor, 2);
                    break;
                case ClearItemPercentageSubtitleInstruction:
                    Array.Fill(tilemap, (ushort)0x007f, startIndex: 736, count: 64);
                    RequestedItemPercentageScroll = true;
                    cursor = Add(cursor, 2);
                    break;
                default:
                    throw new InvalidDataException(
                        $"Ending BG opcode $8B:{word:X4} at $8C:{cursor:X4} is untranslated.");
            }
        }
    }

    private void DrawRecord(ushort packedPosition, ushort dataPointer)
    {
        ushort drawFunction = ReadWord(dataPointer);
        if (drawFunction == DrawNothing)
            return;
        if (drawFunction != DrawTextToTilemap)
        {
            throw new InvalidDataException(
                $"Ending BG indirect function $8B:{drawFunction:X4} at $8C:{dataPointer:X4} is invalid.");
        }

        byte width = bus.ReadByte(0x8c0000 | Add(dataPointer, 2));
        byte height = bus.ReadByte(0x8c0000 | Add(dataPointer, 3));
        int x = packedPosition & 0xff;
        int y = packedPosition >> 8;
        if (width == 0 || height == 0 || x + width > 32 || y + height > 32)
            throw new InvalidDataException($"Ending BG rectangle ({x},{y}) {width}x{height} is invalid.");

        ushort source = Add(dataPointer, 4);
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                tilemap[(y + row) * 32 + x + column] = ReadWord(source);
                source = Add(source, 2);
            }
        }
    }

    private void DrawItemPercentage()
    {
        int count = inventory.MaxHealth / 100
                  + inventory.MaxReserveEnergy / 100
                  + inventory.MaxMissiles / 5
                  + inventory.MaxSuperMissiles / 5
                  + inventory.MaxPowerBombs / 5;
        count += BitOperations.PopCount((uint)(inventory.CollectedItems & 0xf32f));
        count += BitOperations.PopCount((uint)(inventory.CollectedBeams & 0x100f));
        count = Math.Clamp(count, 0, 100);

        int hundreds = count / 100;
        int tens = count / 10 % 10;
        int units = count % 10;
        if (hundreds != 0)
            WriteDigit(462, hundreds);
        if (tens != 0 || hundreds != 0)
            WriteDigit(463, tens);
        WriteDigit(464, units);
        tilemap[465] = 0x386a;
        tilemap[497] = 0x387a;
    }

    private void WriteDigit(int topIndex, int digit)
    {
        tilemap[topIndex] = unchecked((ushort)(0x3860 + digit));
        tilemap[topIndex + 32] = unchecked((ushort)(0x3870 + digit));
    }

    private void CopyWords(ushort source, int destination, int count)
    {
        for (int index = 0; index < count; index++)
            tilemap[destination + index] = ReadWord(Add(source, index * 2));
    }

    private void Upload(SnesVram vram) =>
        vram.ExecuteWordTransfer(tilemap, destinationWord: 0x4c00, wordIncrement: 1);

    private ushort ReadWord(ushort pointer) =>
        RomDataReader.ReadWordFixedBank(bus, 0x8c0000 | pointer);

    private static ushort Add(ushort pointer, int bytes) =>
        unchecked((ushort)(pointer + bytes));
}

internal readonly record struct EndingInventorySnapshot(
    ushort MaxHealth,
    ushort MaxReserveEnergy,
    ushort MaxMissiles,
    ushort MaxSuperMissiles,
    ushort MaxPowerBombs,
    ushort CollectedItems,
    ushort CollectedBeams);
