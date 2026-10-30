using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

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

    /// <summary>The arrow's solid and pulse colours.</summary>
    public required PauseReserveArrowColours ArrowColours { get; init; }
}
