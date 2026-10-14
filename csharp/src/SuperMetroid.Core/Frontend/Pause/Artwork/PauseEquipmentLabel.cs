namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>One horizontal strip of pause BG tile artwork and its destination within the equipment page's 32x32 tilemap.</summary>
public sealed record PauseEquipmentLabel
{
    /// <summary>Zero-based destination tile column 0..31, not an artwork-atlas column; the displayed label must fit within the tilemap row.</summary>
    public required int Column { get; init; }
    /// <summary>Zero-based destination tile row 0..31 in the equipment page, with each tile occupying eight screen pixels vertically.</summary>
    public required int Row { get; init; }
    /// <summary>Left-to-right nonnull artwork cells: five for ordinary beams and nine for equipment, boots, and Beam.Hyper; Hyper displays only its first five cells.</summary>
    public required PauseBackdropCell[] Cells { get; init; }
}
