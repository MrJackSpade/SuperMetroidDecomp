using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The command list that builds each library background (#1466): the import pipeline's knowledge of how it is built.</summary>
public static class LibraryBackgroundPrograms
{
    /// <summary>Each background's program.</summary>
    private static readonly Dictionary<LibraryBackground, LibraryBackgroundProgram> ByBackground =
        LibraryBackgroundProgramDefinitions.All.ToDictionary(program => program.Background);

    /// <summary>The command list that builds <paramref name="background"/>.</summary>
    /// <exception cref="InvalidDataException">No program builds the background.</exception>
    public static LibraryBackgroundProgram Of(LibraryBackground background) =>
        ByBackground.TryGetValue(background, out LibraryBackgroundProgram? program) ? program
            : throw new InvalidDataException($"No library-background program builds {background}.");
}
