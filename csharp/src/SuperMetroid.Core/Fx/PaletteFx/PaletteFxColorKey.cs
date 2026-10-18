namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One colour a palette-FX record writes: the record and the colour's position among the colours it writes.</summary>
/// <param name="Frame">The timed record.</param>
/// <param name="Color">The colour's position among the record's written colours, skipped CGRAM entries not counted.</param>
public readonly record struct PaletteFxColorKey(PaletteFxFrame Frame, int Color);
