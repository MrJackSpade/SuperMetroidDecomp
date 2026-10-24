using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>One reserve label's equipment-page anchor and consecutive native BG tile references.</summary>
public sealed record PauseReserveLabelVisual
{
    /// <summary>First destination tile cell in the 32-by-32 equipment page; subsequent words continue in native linear tilemap order.</summary>
    public required PauseGridPoint Anchor { get; init; }
    /// <summary>Ordered glyph/attribute definitions: seven for Mode or ReserveTank, four for Manual or Auto.</summary>
    public required PauseBackdropCell[] Cells { get; init; }
}
