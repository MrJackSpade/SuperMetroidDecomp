namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Immutable bank-$8F room-background command programs for every retail room state.
/// These are engine-owned transfer/decompression instructions, not replaceable art;
/// their referenced character sheets and tilemaps are installed separately.
/// </summary>
public static partial class LibraryBackgroundProgramDefinitions
{

    /// <summary>All pinned program definitions, in the cartridge's list order.</summary>
    public static IReadOnlyList<LibraryBackgroundProgram> All => programs;
}
