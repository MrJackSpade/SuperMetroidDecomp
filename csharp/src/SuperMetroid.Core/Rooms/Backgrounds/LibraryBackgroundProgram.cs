namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>One ordered, fixed native list; the terminator is implicit in the command count.</summary>
/// <param name="name">The program's name, the disassembly's label for its command list without its prefix, or a fixture's description.</param>
/// <param name="instructions">Ordered nonterminating commands. Wrapped without copying, so the caller must not modify the array after construction.</param>
/// <param name="nativeByteCount">Original encoded byte length including the two-byte zero terminator; retained as supplied, not calculated or validated from the commands.</param>
public sealed class LibraryBackgroundProgram(
    string name, LibraryBackgroundInstruction[] instructions, int nativeByteCount)
{
    /// <summary>The program's name, used in diagnostics.</summary>
    public string Name { get; } = Ensure.NotNull(name);
    /// <summary>Read-only wrapper over the supplied array in execution order, with no explicit End command; shares that array rather than owning a copy.</summary>
    public IReadOnlyList<LibraryBackgroundInstruction> Instructions { get; } =
        Array.AsReadOnly(instructions);
    /// <summary>Native encoded list size in bytes, including mixed-width operands and the implicit terminator; not a command count or transfer size.</summary>
    /// <remarks>
    /// A byte count rather than a native word: the generated catalog and the import oracle supply a sixteen-bit
    /// span (under a hundred bytes in retail), nothing modifies it, and it is only compared.
    /// </remarks>
    public int NativeByteCount { get; } = nativeByteCount;
}
