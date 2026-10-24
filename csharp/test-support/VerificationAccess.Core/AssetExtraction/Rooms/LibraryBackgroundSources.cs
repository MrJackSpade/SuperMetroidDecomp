using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The import pipeline's map between the bank-$8F command-list pointers room states store and the compiled
/// library-background programs they address; the simulation knows only the named programs.
/// </summary>
public static class LibraryBackgroundSources
{
    /// <summary>The compiled program whose command list is at <paramref name="pointer"/>.</summary>
    public static LibraryBackgroundProgram Program(ushort pointer) =>
        LibraryBackgroundSourceDefinitions.ByPointer.TryGetValue(pointer, out LibraryBackgroundProgram? found)
            ? found
            : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not one of the 68 retail library-background programs.");
}
