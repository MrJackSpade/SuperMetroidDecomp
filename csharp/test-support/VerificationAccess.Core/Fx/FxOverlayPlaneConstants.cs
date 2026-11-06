namespace SuperMetroid.Core.Fx;

/// <summary>
/// The original game's effect overlay tilemap, as verification compares the port with the words the cartridge writes
/// (#1466, #1514): the display model shows the effect's page as a placed picture, and only these oracles lay it out cell
/// by cell.
/// </summary>
internal static class FxOverlayPlaneConstants
{
    /// <summary>Tile columns of the original game's overlay tilemap.</summary>
    public const int Columns = 32;

    /// <summary>Tile rows of the original game's overlay tilemap.</summary>
    public const int Rows = 64;

    /// <summary>The tilemap row the original game writes an effect's page from.</summary>
    public const int EffectPageFirstRow = 31;

    /// <summary>The rows at the top of the plane the HUD's images cover.</summary>
    public const int HudRows = 4;

    /// <summary>The tile word the cartridge clears the effect area to, which the overlay sheet's cleared picture pictures (#1466).</summary>
    public static SuperMetroid.AssetExtraction.Artwork.BackgroundTile ClearedCell => SuperMetroid.AssetExtraction.Fx.Layer3ClearedCellDefinitions.ClearedCell;
}
