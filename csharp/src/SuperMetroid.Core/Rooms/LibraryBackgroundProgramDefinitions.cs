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

    /// <summary>Finds a compiled retail command list. Constructed programs can be supplied explicitly, without byte decoding.</summary>
    /// <param name="pointer">16-bit bank-$8F list identity; values absent from the pinned retail catalog simply fail lookup.</param>
    /// <param name="program">Shared compiled definition on success, or null on failure; lookup does not execute transfers or load artwork.</param>
    /// <returns>True when the exact pointer is cataloged; false otherwise.</returns>
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
}

/// <summary>One ordered, fixed native list; the terminator is implicit in the command count.</summary>
/// <param name="pointer">Bank-$8F list identity, normally in its high half; construction does not validate its address.</param>
/// <param name="instructions">Ordered nonterminating commands. Wrapped without copying, so the caller must not modify the array after construction.</param>
/// <param name="nativeByteCount">Original encoded byte length including the two-byte zero terminator; retained as supplied, not calculated or validated from the commands.</param>
public sealed class LibraryBackgroundProgram(
    ushort pointer, LibraryBackgroundInstruction[] instructions, ushort nativeByteCount)
{
    /// <summary>16-bit bank-$8F command-list identity used for retail lookup and diagnostics, not a full source-data bus address.</summary>
    public ushort Pointer { get; } = pointer;
    /// <summary>Read-only wrapper over the supplied array in execution order, with no explicit End command; shares that array rather than owning a copy.</summary>
    public IReadOnlyList<LibraryBackgroundInstruction> Instructions { get; } =
        Array.AsReadOnly(instructions);
    /// <summary>Native encoded list size in bytes, including mixed-width operands and the implicit terminator; not a command count or transfer size.</summary>
    public ushort NativeByteCount { get; } = nativeByteCount;
}

/// <summary>
/// One fixed transfer/control command. Destination is a VRAM word for transfers
/// or a work-RAM byte offset for decompression; unused operands are zero.
/// </summary>
/// <param name="Command">Bank-$82 dispatcher operation. Program arrays contain nonterminating commands; End is represented by the array's end instead.</param>
/// <param name="SourceAddress">Full 24-bit source bus identity for transfers or compressed artwork; transfers may source live WRAM. Zero for operand-free clear commands.</param>
/// <param name="Destination">VRAM word address for transfers, or byte offset within bank $7E for decompression; zero when unused.</param>
/// <param name="ByteCount">Transfer payload length in bytes, required to be nonzero by the execution consumer; zero for decompression and clear operations whose sizes are determined elsewhere.</param>
/// <param name="DoorPointer">16-bit door identity compared with the current door for TransferForDoor, or zero for unconditional commands.</param>
public readonly record struct LibraryBackgroundInstruction(
    LibraryBackgroundCommand Command,
    int SourceAddress,
    ushort Destination,
    ushort ByteCount,
    ushort DoorPointer);
