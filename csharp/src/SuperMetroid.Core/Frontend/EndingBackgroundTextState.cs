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
    private readonly ISnesAddressSpace bus;
    private readonly ushort[] tilemap;
    private readonly EndingInventorySnapshot inventory;
    private readonly bool japaneseText;
    private ushort instructionPointer;
    private ushort instructionTimer = 1;
    private readonly ushort tilemapDestination;

    public EndingBackgroundTextState(
        ISnesAddressSpace bus,
        ushort[] tilemap,
        ushort instructionPointer,
        EndingInventorySnapshot inventory,
        bool japaneseText,
        ushort tilemapDestination = EndingCreditsRomData.Rendering.PostCreditsTilemapWord)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.tilemap = tilemap ?? throw new ArgumentNullException(nameof(tilemap));
        if (tilemap.Length != EndingCreditsRomData.Rendering.TilemapWords)
            throw new ArgumentException("Ending BG tilemap must contain exactly $400 words.", nameof(tilemap));
        this.instructionPointer = instructionPointer;
        this.inventory = inventory;
        this.japaneseText = japaneseText;
        this.tilemapDestination = tilemapDestination;
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
            if ((word & CinematicCodePointers.InstructionCommandBit) == 0)
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
                case CinematicCodePointers.CinematicBackgroundObject_Instruction_Delete:
                    instructionPointer = 0;
                    Upload(vram);
                    return;
                case CinematicCodePointers.CinematicBackgroundObject_Instruction_Goto:
                    cursor = ReadWord(Add(cursor, 2));
                    break;
                case CinematicCodePointers.Ending_Instruction_DrawItemPercentage:
                    DrawItemPercentage();
                    cursor = Add(cursor, 2);
                    break;
                case CinematicCodePointers.Ending_Instruction_DrawItemPercentageSubtitle:
                    if (japaneseText)
                        CopyWords(
                            EndingCreditsRomData.Instructions.JapaneseItemPercentageSubtitle,
                            EndingCreditsRomData.Text.JapaneseSubtitleDestination,
                            EndingCreditsRomData.Text.JapaneseSubtitleWords);
                    cursor = Add(cursor, 2);
                    break;
                case CinematicCodePointers.Ending_Instruction_ClearItemPercentageSubtitle:
                    Array.Fill(
                        tilemap,
                        EndingCreditsRomData.Rendering.BlankTile,
                        EndingCreditsRomData.Text.JapaneseSubtitleDestination,
                        EndingCreditsRomData.Text.JapaneseSubtitleWords);
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
        if (drawFunction == CinematicCodePointers.IndirectInstruction_DoNothing)
            return;
        if (drawFunction != CinematicCodePointers.IndirectInstruction_DrawToBackgroundTilemap)
        {
            throw new InvalidDataException(
                $"Ending BG indirect function $8B:{drawFunction:X4} at $8C:{dataPointer:X4} is invalid.");
        }

        byte width = bus.ReadByte((int)EndingCreditsRomData.Instructions.Bank.AddWithinBank(
            Add(dataPointer, 2)));
        byte height = bus.ReadByte((int)EndingCreditsRomData.Instructions.Bank.AddWithinBank(
            Add(dataPointer, 3)));
        int x = packedPosition & EndingCreditsRomData.Text.PackedPositionXMask;
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
        count += BitOperations.PopCount((uint)(
            inventory.CollectedItems & EndingCreditsRomData.Text.CollectibleItemMask));
        count += BitOperations.PopCount((uint)(
            inventory.CollectedBeams & EndingCreditsRomData.Text.CollectibleBeamMask));
        count = Math.Clamp(count, 0, 100);

        int hundreds = count / 100;
        int tens = count / 10 % 10;
        int units = count % 10;
        if (hundreds != 0)
            WriteDigit(EndingCreditsRomData.Text.PercentageHundredsTopIndex, hundreds);
        if (tens != 0 || hundreds != 0)
            WriteDigit(EndingCreditsRomData.Text.PercentageHundredsTopIndex + 1, tens);
        WriteDigit(EndingCreditsRomData.Text.PercentageHundredsTopIndex + 2, units);
        tilemap[EndingCreditsRomData.Text.PercentageHundredsTopIndex + 3] =
            EndingCreditsRomData.Text.PercentTopTile;
        tilemap[
            EndingCreditsRomData.Text.PercentageHundredsTopIndex + 3 +
            EndingCreditsRomData.Rendering.TilemapWidth] =
            EndingCreditsRomData.Text.PercentBottomTile;
    }

    private void WriteDigit(int topIndex, int digit)
    {
        tilemap[topIndex] = unchecked((ushort)(EndingCreditsRomData.Text.DigitTopTile + digit));
        tilemap[topIndex + EndingCreditsRomData.Rendering.TilemapWidth] =
            unchecked((ushort)(EndingCreditsRomData.Text.DigitBottomTile + digit));
    }

    private void CopyWords(ushort source, int destination, int count)
    {
        for (int index = 0; index < count; index++)
            tilemap[destination + index] = ReadWord(Add(source, index * 2));
    }

    private void Upload(SnesVram vram) =>
        vram.ExecuteWordTransfer(
            tilemap,
            tilemapDestination,
            wordIncrement: 1);

    private ushort ReadWord(ushort pointer) =>
        RomDataReader.ReadWordFixedBank(
            bus,
            EndingCreditsRomData.Instructions.Bank.AddWithinBank(pointer));

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
