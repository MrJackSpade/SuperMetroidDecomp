namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>
/// One colour of a cartridge colour table the import pre-colours Samus's shades from (#1512): the table's step (one
/// listed run of colours, such as a beam combination, a cycle frame or a death step) and the colour's place in that run.
/// </summary>
/// <param name="Step">The run of the table the colour belongs to.</param>
/// <param name="Colour">The colour's place within its run.</param>
public readonly record struct TableColourPlace(int Step, int Colour);
