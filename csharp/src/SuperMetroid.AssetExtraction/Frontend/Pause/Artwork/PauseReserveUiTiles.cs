using SuperMetroid.AssetExtraction.Artwork;
using static SuperMetroid.AssetExtraction.Frontend.Pause.Artwork.PauseReserveUiDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// Draws the pause screen's reserve-tank labels (<see cref="PauseReserveUiDefinitions"/>).
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
    internal static int StockLabelWords(PauseReserveLabel label) => label switch
    {
        PauseReserveLabel.Mode or PauseReserveLabel.ReserveTank => LabelWords,
        PauseReserveLabel.Manual or PauseReserveLabel.Auto => ModeWords,
        _ => throw new ArgumentOutOfRangeException(nameof(label)),
    };

    /// <summary>$82:C068/C06A places the seven-word labels on consecutive rows; replacement modes follow MODE.</summary>
    internal static int StockLabelOffset(PauseReserveLabel label) => sizeof(ushort) * (label switch
    {
        PauseReserveLabel.Mode => ModeCell - ModePrefixWords,
        PauseReserveLabel.ReserveTank => ModeCell - ModePrefixWords + TilemapColumns,
        PauseReserveLabel.Manual or PauseReserveLabel.Auto => ModeCell,
        _ => throw new ArgumentOutOfRangeException(nameof(label)),
    });

    /// <summary>Compose reserve text from named consecutive glyph runs. Independent glyph pixels remain artwork.</summary>
    internal static BackgroundTile StockLabelTile(PauseReserveLabel label, int index)
    {
        if ((uint)index >= StockLabelWords(label)) throw new ArgumentOutOfRangeException(nameof(index));
        return label switch
        {
            PauseReserveLabel.Mode => index < ModePrefixWords ? Glyph(ModeGlyph + index, 1)
                : Glyph(ManualGlyph + index - ModePrefixWords, 7),
            PauseReserveLabel.ReserveTank => Glyph(ReserveTankGlyph + index, 7),
            PauseReserveLabel.Manual => Glyph(ManualGlyph + index, 7),
            PauseReserveLabel.Auto => Glyph(AutoGlyph + index, 7),
            _ => throw new ArgumentOutOfRangeException(nameof(label)),
        };
    }

    /// <summary>Places a reserve UI glyph in <paramref name="palette"/> with priority.</summary>
    private static BackgroundTile Glyph(int glyph, int palette) => new(glyph, palette, FlipHorizontally: false, FlipVertically: false, HighPriority: true);

    /// <summary>The tile of decimal digit <paramref name="digit"/>.</summary>
    public static BackgroundTile DigitTile(int digit) => DigitZeroTile with { Tile = DigitZeroTile.Tile + digit };
}
