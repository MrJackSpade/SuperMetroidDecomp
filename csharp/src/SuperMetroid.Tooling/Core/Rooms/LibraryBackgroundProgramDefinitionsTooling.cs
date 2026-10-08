namespace SuperMetroid.Core.Rooms;

/// <summary>Development-tool members of <see cref="LibraryBackgroundProgramDefinitions"/>; never linked by player hosts.</summary>
internal static class LibraryBackgroundProgramDefinitionsTooling
{
    /// <summary>Gets a pinned retail list or fails rather than reading an uncatalogued source.</summary>
    public static LibraryBackgroundProgram Get(ushort pointer) =>
        LibraryBackgroundProgramDefinitions.TryGet(pointer, out LibraryBackgroundProgram program)
            ? program
            : throw new InvalidDataException(
                $"No compiled library-background program for $8F:{pointer:X4}.");
}
