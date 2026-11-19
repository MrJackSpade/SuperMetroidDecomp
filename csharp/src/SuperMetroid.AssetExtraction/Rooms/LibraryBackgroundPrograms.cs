using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The command list that builds each library background the player sees as its own picture (#1466, #1649): the import
/// pipeline's knowledge of how it is built. Each such background has one list; the cleared background and the boss's body
/// are left by several lists the import never runs (#2546).
/// </summary>
public static class LibraryBackgroundPrograms
{
    /// <summary>Each pictured background's program.</summary>
    private static readonly Dictionary<LibraryBackground, LibraryBackgroundProgram> ByBackground =
        LibraryBackgroundProgramDefinitions.All
            .Where(program => program.Background.View == LibraryBackgroundView.Picture)
            .ToDictionary(program => program.Background);

    /// <summary>The command list that builds <paramref name="background"/>, which the player sees as its own picture.</summary>
    /// <exception cref="InvalidDataException">No program builds the background as its own picture.</exception>
    public static LibraryBackgroundProgram Of(LibraryBackground background) =>
        ByBackground.TryGetValue(background, out LibraryBackgroundProgram? program) ? program
            : throw new InvalidDataException($"No library-background program builds {background} as its own picture.");
}
