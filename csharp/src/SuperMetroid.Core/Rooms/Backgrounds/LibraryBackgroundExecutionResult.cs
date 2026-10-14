namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Observable output of one bank-$82 library-background list, including PPU register
/// side effects which cannot be represented by VRAM writes alone.
/// </summary>
public readonly record struct LibraryBackgroundExecutionResult(
    ushort? Bg3CharacterBaseWord);
