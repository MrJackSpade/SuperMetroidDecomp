namespace SuperMetroid.Core.Hud;

/// <summary>The AUTO reserve indicator as it shows one state (#1466, #1908): one picture, placed at the indicator's corner.</summary>
/// <param name="Indicator">What the indicator shows; a cleared indicator shows the blank band beneath it.</param>
public sealed record HudReserveIndicatorImage(HudReserveIndicator Indicator) : HudImage
{
    /// <summary>What the indicator shows.</summary>
    public HudReserveIndicator Indicator { get; } = Enum.IsDefined(Indicator)
        ? Indicator
        : throw new ArgumentOutOfRangeException(nameof(Indicator), Indicator, "Undefined reserve indicator.");

    /// <inheritdoc/>
    public override string FrameName => $"Hud.Auto.{HudReserveIndicatorKeys.Installed.Of(Indicator)}";
}
