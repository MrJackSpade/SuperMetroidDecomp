namespace SuperMetroid.Core.Assets;

/// <summary>JSON wrapper for an independently editable SNES BG tilemap cell, rather than a character in a compiled label.</summary>
public sealed record EndingTextCell
{
    /// <summary>Packed 16-bit SNES BG tilemap word: ten-bit tile index, three-bit palette selection, priority, and horizontal/vertical flip bits.</summary>
    public required ushort Raw { get; init; }
}
