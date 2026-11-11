using SuperMetroid.AssetExtraction.Artwork;
using static SuperMetroid.AssetExtraction.Frontend.Pause.Artwork.PauseReserveUiDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The pause screen's reserve-tank labels' keys and widths and the digit tiles the import extracts
/// (<see cref="PauseReserveUiDefinitions"/>).
/// </summary>
public static class PauseReserveUiTiles
{
    /// <summary>The JSON key of a reserve label.</summary>
    public static string Name(PauseReserveLabel label) => label switch
    {
        PauseReserveLabel.Mode => "Mode",
        PauseReserveLabel.ReserveTank => "ReserveTank",
        PauseReserveLabel.Manual => "Manual",
        PauseReserveLabel.Auto => "Auto",
        _ => throw new ArgumentOutOfRangeException(nameof(label)),
    };

    /// <summary>MODE and RESERVE TANK occupy seven cells; MANUAL and AUTO occupy four replacement cells.</summary>
    internal static int LabelCellCount(PauseReserveLabel label) => label switch
    {
        PauseReserveLabel.Mode or PauseReserveLabel.ReserveTank => LabelWords,
        PauseReserveLabel.Manual or PauseReserveLabel.Auto => ModeWords,
        _ => throw new ArgumentOutOfRangeException(nameof(label)),
    };

    /// <summary>The tile of decimal digit <paramref name="digit"/>.</summary>
    public static BackgroundTile DigitTile(int digit) => DigitZeroTile with { Tile = DigitZeroTile.Tile + digit };
}
