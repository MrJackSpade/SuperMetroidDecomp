namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Observable output of one bank-$82 library-background list beyond its VRAM writes: whether it moved the HUD's BG3
/// characters to Kraid's base.
/// </summary>
/// <param name="SelectsKraidHudCharacters">Whether the list ran Kraid's transfer, which moves the HUD characters to Kraid's base.</param>
public readonly record struct LibraryBackgroundExecutionResult(
    bool SelectsKraidHudCharacters);
