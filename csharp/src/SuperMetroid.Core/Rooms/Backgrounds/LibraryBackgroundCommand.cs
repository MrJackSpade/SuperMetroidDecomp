namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The operations a room's library-background list performs. Values are arbitrary identities (#1414); extraction
/// translates the cartridge's command words, and a list's end is the end of its instructions.
/// </summary>
public enum LibraryBackgroundCommand
{
    /// <summary>Copy a source to its destination.</summary>
    Transfer = 1,
    /// <summary>Decompress a background tilemap into the tilemap staging.</summary>
    Decompress,
    /// <summary>Fill and copy the shared layer-3 effect tilemap.</summary>
    ClearFxTilemap,
    /// <summary>Copy a source to its destination and move the HUD's characters to Kraid's base.</summary>
    TransferForKraid,
    /// <summary>Clear the background layer's two tilemap pages.</summary>
    ClearBg2,
    /// <summary>Clear the background layer's pages and Kraid's additional pages.</summary>
    ClearBg2ForKraid,
    /// <summary>Copy a source to its destination only when the room was entered through the instruction's door.</summary>
    TransferForDoor,
}
