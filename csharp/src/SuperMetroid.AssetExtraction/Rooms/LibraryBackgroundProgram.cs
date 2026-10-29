using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The command list that builds one library background (#1466); the terminator is implicit in the command count. Only the
/// import pipeline runs it, once, and installs the picture it leaves under the background's name.
/// </summary>
/// <param name="background">The background the list builds, or a fixture's stand-in for one.</param>
/// <param name="instructions">Ordered nonterminating commands. Wrapped without copying, so the caller must not modify the array after construction.</param>
public sealed class LibraryBackgroundProgram(
    LibraryBackground background, LibraryBackgroundInstruction[] instructions)
{
    /// <summary>The background the list builds.</summary>
    public LibraryBackground Background { get; } = Ensure.NotNull(background);

    /// <summary>The program's name, the background's, used in diagnostics.</summary>
    public string Name => Background.Name;
    /// <summary>Read-only wrapper over the supplied array in execution order, with no explicit End command; shares that array rather than owning a copy.</summary>
    public IReadOnlyList<LibraryBackgroundInstruction> Instructions { get; } =
        Array.AsReadOnly(instructions);
}
