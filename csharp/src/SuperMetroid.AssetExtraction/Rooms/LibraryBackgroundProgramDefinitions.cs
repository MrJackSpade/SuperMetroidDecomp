namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Immutable bank-$8F room-background command programs, one for each library background retail room states show. The
/// import pipeline runs each once and installs the pictures it leaves (#1466); the simulation names only the backgrounds.
/// </summary>
/// <remarks>
/// The programs are generated from the pinned cartridge by tools/generate-library-background-programs.ps1, holding
/// fixed command operands only; regenerate them instead of editing the generated declaration.
/// </remarks>
public static partial class LibraryBackgroundProgramDefinitions;
