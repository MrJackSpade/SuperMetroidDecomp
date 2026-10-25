namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>The selected pause reserve-tank labels, digits, arrow placements and arrow colours a presentation is compiled from.</summary>
internal sealed record PauseReserveUiSelection
{
    /// <summary>Each label's byte anchor in the equipment tilemap.</summary>
    public required Dictionary<PauseReserveLabel, int> LabelOffsets { get; init; }

    /// <summary>Each label's little-endian tile words.</summary>
    public required Dictionary<PauseReserveLabel, byte[]> LabelWords { get; init; }

    /// <summary>Byte anchor of the three-place reserve supply.</summary>
    public required int DigitOffset { get; init; }

    /// <summary>Ten decimal glyph words.</summary>
    public required byte[][] Digits { get; init; }

    /// <summary>Ten distinct arrow-cell byte positions.</summary>
    public required int[] ArrowOffsets { get; init; }

    /// <summary>BG palette selector used when reserve control is enabled.</summary>
    public required int EnabledPalette { get; init; }

    /// <summary>BG palette selector used when reserve control is unavailable.</summary>
    public required int DisabledPalette { get; init; }

    /// <summary>Static arrow bevel colour for palette slot six.</summary>
    public required RgbColor SolidColor6 { get; init; }

    /// <summary>Static arrow bevel colour for palette slot eleven.</summary>
    public required RgbColor SolidColor11 { get; init; }

    /// <summary>The two bevel colours of each of the 32 pulse phases.</summary>
    public required ReserveArrowColors[] ArrowFrames { get; init; }
}
