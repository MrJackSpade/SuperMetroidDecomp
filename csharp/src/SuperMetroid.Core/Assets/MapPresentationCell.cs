namespace SuperMetroid.Core.Assets;

/// <summary>Editable artwork and display attributes for one map cell; these fields compile to a SNES BG word but do not define exploration or map-station rules.</summary>
public sealed record MapPresentationCell
{
    /// <summary>Zero-based 8-pixel character column from 0 through 31 in <c>map-tiles.png</c>; combined with the atlas row to form the character index.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based 8-pixel character row from 0 through 7 in the 256-by-64-pixel <c>map-tiles.png</c> atlas.</summary>
    public required int TileRow { get; init; }
    /// <summary>SNES BG palette index from 0 through 7, encoded in bits 10–12; screen projection may adjust exploration or HUD palette attributes.</summary>
    public required int Palette { get; init; }
    /// <summary>Whether to set the BG tile priority bit $2000 in the compiled word; this is a display attribute, not a reveal flag.</summary>
    public required bool Priority { get; init; }
    /// <summary>Whether to mirror the selected character horizontally, setting packed-word bit $4000.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether to mirror the selected character vertically, setting packed-word bit $8000.</summary>
    public required bool FlipY { get; init; }
}
