namespace SuperMetroid.Core.Rooms;

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
