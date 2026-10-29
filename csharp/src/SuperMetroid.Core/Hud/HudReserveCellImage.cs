using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Core.Hud;

/// <summary>One of the AUTO reserve indicator's six cells as the indicator shows (#1466).</summary>
/// <param name="Indicator">What the indicator shows; a cleared indicator shows the blank cell.</param>
/// <param name="Cell">The cell, left to right and then top to bottom.</param>
public sealed record HudReserveCellImage(HudReserveIndicator Indicator, int Cell) : HudImage
{
    /// <summary>The cell, left to right and then top to bottom.</summary>
    public int Cell { get; } = Ensure.BetweenInclusive(0, GameplayHudDefinitions.AutoReserveCellCount - 1, Cell);

    /// <inheritdoc/>
    public override string FrameName => $"Hud.Auto.{Indicator}.{Cell}";
}
