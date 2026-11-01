namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour's place in an authored colour table (#1405, #1488): the step of the animation or selection the table walks
/// through (a frame, health band, pose or selection) and the colour within that step. Catalogs key the colours a player
/// edited by cell, so a key names what it locates.
/// </summary>
/// <param name="Step">The step: the frame, band, pose or selection the colour belongs to.</param>
/// <param name="Colour">The colour within the step.</param>
public readonly record struct ColourCell(int Step, int Colour);
