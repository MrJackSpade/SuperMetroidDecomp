using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Editable arrow placement and palette behavior corresponding to native reserve-arrow owner <c>$82:AD0A</c>.</summary>
public sealed record PauseReserveArrowVisual
{
    /// <summary>Exactly ten distinct equipment-page cell locations; stock layout is an eight-cell vertical stem followed by a two-cell horizontal arm.</summary>
    public required PauseGridPoint[] Cells { get; init; }
    /// <summary>BG palette selector 0-7 applied when the pause owner enables the arrow; stock uses six.</summary>
    public required int EnabledPalette { get; init; }
    /// <summary>BG palette selector 0-7 applied when the pause owner disables the arrow; stock uses seven.</summary>
    public required int DisabledPalette { get; init; }
    /// <summary>RGB5 bevel color for arrow slot six when pulse animation is disabled.</summary>
    public required PaletteRgb5 SolidColor6 { get; init; }
    /// <summary>RGB5 bevel color for arrow slot eleven when pulse animation is disabled.</summary>
    public required PaletteRgb5 SolidColor11 { get; init; }
    /// <summary>Exactly 32 ordered pulse frames, selected by the low five bits of the accepted NMI counter; native colors originate at $82:AD5D/$AD9D.</summary>
    public required PauseReserveArrowFrame[] Frames { get; init; }
}
