using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>The selected pause reserve-tank labels, digits, arrow placements and arrow colours a presentation is compiled from.</summary>
internal sealed record PauseReserveUiSelection
{
    /// <summary>Each label's first cell in the equipment tilemap.</summary>
    public required Dictionary<PauseReserveLabel, int> LabelCells { get; init; }

    /// <summary>Each label's tiles.</summary>
    public required Dictionary<PauseReserveLabel, BackgroundTile[]> LabelTiles { get; init; }

    /// <summary>The cell of the three-place reserve supply's first digit.</summary>
    public required int DigitCell { get; init; }

    /// <summary>The ten decimal glyphs' tiles.</summary>
    public required BackgroundTile[] Digits { get; init; }

    /// <summary>The ten distinct arrow cells.</summary>
    public required int[] ArrowCells { get; init; }

    /// <summary>Colour variant used when reserve control is enabled.</summary>
    public required int EnabledPalette { get; init; }

    /// <summary>Colour variant used when reserve control is unavailable.</summary>
    public required int DisabledPalette { get; init; }

    /// <summary>Static arrow bevel colour for palette slot six.</summary>
    public required RgbColor SolidColor6 { get; init; }

    /// <summary>Static arrow bevel colour for palette slot eleven.</summary>
    public required RgbColor SolidColor11 { get; init; }

    /// <summary>The two bevel colours of each of the 32 pulse phases.</summary>
    public required ReserveArrowColors[] ArrowFrames { get; init; }
}
