namespace SuperMetroid.Core.Assets;

/// <summary>One 32-by-32 BG page containing exactly 1024 ordered tile-reference cells.</summary>
public sealed record RoomBackgroundTilemapPage
{
    /// <summary>Caller-owned mutable cell array in row-major destination order: index <c>y * 32 + x</c> selects a page cell, not a character-sheet position.</summary>
    public required RoomBackgroundTilemapCell[] Cells { get; init; }
}
