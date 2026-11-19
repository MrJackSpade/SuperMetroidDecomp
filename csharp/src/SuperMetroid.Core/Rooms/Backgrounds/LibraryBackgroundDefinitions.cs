namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The library backgrounds room states show (#1466), each named for what it shows, with the entry doors that show their
/// own pictures of it. The cleared background and the boss's body are one background each, whichever room shows them
/// (#2546). The drawing scripts are the import pipeline's: it runs each script of a background seen as its own picture
/// once and installs the pictures it leaves.
/// </summary>
/// <remarks>
/// The backgrounds' declaration is generated from the game's data with the import pipeline's readers; regenerate it
/// instead of editing the generated declaration.
/// </remarks>
public static partial class LibraryBackgroundDefinitions;
