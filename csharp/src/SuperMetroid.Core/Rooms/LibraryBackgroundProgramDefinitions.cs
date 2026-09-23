namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Immutable bank-$8F room-background command programs for every retail room state.
/// These are engine-owned transfer/decompression instructions, not replaceable art;
/// their referenced character sheets and tilemaps are installed separately.
/// </summary>
public static partial class LibraryBackgroundProgramDefinitions
{
    /// <summary>Distinct high-bank command lists selected by the 323 retail states.</summary>
    public const int RetailProgramCount = 68;

    /// <summary>All pinned program definitions, ordered by native list pointer.</summary>
    public static IReadOnlyList<LibraryBackgroundProgram> All { get; }

    static LibraryBackgroundProgramDefinitions()
    {
        All = Array.AsReadOnly(programs);
        ushort[] selected = RoomStateDefinitions.All
            .Select(state => state.BackgroundDataPointer)
            .Where(pointer => unchecked((short)pointer) < 0)
            .Distinct()
            .Order()
            .ToArray();
        if (programs.Length != RetailProgramCount ||
            !programs.Select(program => program.Pointer).SequenceEqual(selected) ||
            programs.Any(program => program.NativeByteCount < sizeof(ushort) ||
                program.Instructions.Count >= RoomAssetRomData.LibraryBackground.MaximumCommandsPerList))
        {
            throw new InvalidDataException(
                "Compiled library-background programs do not cover the retail room states.");
        }
    }

    /// <summary>Finds a compiled retail command list; synthetic fixture lists remain interpretable from RAM/ROM.</summary>
    public static bool TryGet(ushort pointer, out LibraryBackgroundProgram program)
    {
        int low = 0;
        int high = programs.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            int comparison = programs[middle].Pointer.CompareTo(pointer);
            if (comparison == 0)
            {
                program = programs[middle];
                return true;
            }
            if (comparison < 0) low = middle + 1;
            else high = middle - 1;
        }
        program = null!;
        return false;
    }

    /// <summary>Gets a pinned retail list or fails rather than reading an uncatalogued source.</summary>
    public static LibraryBackgroundProgram Get(ushort pointer) =>
        TryGet(pointer, out LibraryBackgroundProgram program)
            ? program
            : throw new InvalidDataException(
                $"No compiled library-background program for $8F:{pointer:X4}.");

    /// <summary>Finds a command-E transfer for one door in a compiled program.</summary>
    public static LibraryBackgroundInstruction GetDoorTransfer(ushort listPointer,
        ushort doorPointer)
    {
        foreach (LibraryBackgroundInstruction instruction in Get(listPointer).Instructions)
        {
            if (instruction.Command == LibraryBackgroundCommand.TransferForDoor &&
                instruction.DoorPointer == doorPointer)
                return instruction;
        }
        throw new InvalidDataException(
            $"Library background $8F:{listPointer:X4} has no command-E record " +
            $"for door $83:{doorPointer:X4}.");
    }
}

/// <summary>One ordered, fixed native list; the terminator is implicit in the command count.</summary>
public sealed class LibraryBackgroundProgram(
    ushort pointer, LibraryBackgroundInstruction[] instructions, ushort nativeByteCount)
{
    public ushort Pointer { get; } = pointer;
    public IReadOnlyList<LibraryBackgroundInstruction> Instructions { get; } =
        Array.AsReadOnly(instructions);
    public ushort NativeByteCount { get; } = nativeByteCount;
}

/// <summary>
/// One fixed transfer/control command. Destination is a VRAM word for transfers
/// or a work-RAM byte offset for decompression; unused operands are zero.
/// </summary>
public readonly record struct LibraryBackgroundInstruction(
    LibraryBackgroundCommand Command,
    int SourceAddress,
    ushort Destination,
    ushort ByteCount,
    ushort DoorPointer);
