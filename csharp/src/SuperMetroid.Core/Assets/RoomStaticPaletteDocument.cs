namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON representation of one room graphics-set palette source.</summary>
public sealed record RoomStaticPaletteDocument
{
    /// <summary>Gets the room-palette schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the exactly 128 RGB5 colors in native CGRAM order.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}
