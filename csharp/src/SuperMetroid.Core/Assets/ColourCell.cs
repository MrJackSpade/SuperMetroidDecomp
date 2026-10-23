namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour's place in a colour catalog's own table (#1405): its row (a frame, band, pose or selection) and its colour
/// within the row. Catalogs key the colours a player edited by cell, so a key names what it locates.
/// </summary>
/// <param name="Row">The table row.</param>
/// <param name="Colour">The colour within the row.</param>
public readonly record struct ColourCell(int Row, int Colour);
