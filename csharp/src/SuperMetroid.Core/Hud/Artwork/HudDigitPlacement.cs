namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>One digit of a HUD counter and the HUD cell it sits in (#1466).</summary>
/// <param name="Numeral">The numeral the digit shows, zero through nine.</param>
/// <param name="Cell">The HUD cell, on the HUD's four rows.</param>
internal readonly record struct HudDigitPlacement(int Numeral, Position16<TilemapCells> Cell);
