namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Immutable bank-$8F room-background command programs for every retail room state.
/// These are engine-owned transfer/decompression instructions, not replaceable art;
/// their referenced character sheets and tilemaps are installed separately.
/// </summary>
/// <remarks>
/// The programs are generated from the pinned cartridge by tools/generate-library-background-programs.ps1, holding
/// fixed command operands only; regenerate them instead of editing the generated declaration.
/// </remarks>
public static partial class LibraryBackgroundProgramDefinitions;
