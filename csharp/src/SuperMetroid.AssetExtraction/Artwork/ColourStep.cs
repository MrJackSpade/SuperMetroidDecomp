namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A colour's place in an authored colour table the import reads (#1512): the step of the effect the table walks through
/// (a frame or a selection) and the colour within that step. Catalogs key the colours a player edited by it.
/// </summary>
/// <param name="Step">The step: the frame or selection the colour belongs to.</param>
/// <param name="Colour">The colour within the step.</param>
internal readonly record struct ColourStep(int Step, int Colour);
