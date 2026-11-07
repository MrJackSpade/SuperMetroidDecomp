namespace SuperMetroid.Core.Assets;

/// <summary>
/// One of the colourings the import pre-coloured an installed picture in besides its standard one (#1512): a step of a
/// colour effect the picture's owner shows, such as a glow's cycle frame, a suit's charge shade or an enemy's palette
/// variant. A draw names the shade; which of the picture's colours it changes, and to what, is the installation's.
/// </summary>
/// <param name="Effect">The colour effect, named for what it shows in the game.</param>
/// <param name="Step">The effect's step, from zero; a one-step effect is step zero.</param>
public readonly record struct PictureShade(string Effect, int Step)
{
    /// <summary>The shade's installed key, which the sheet manifest stores.</summary>
    public string Key => $"{Effect}/{Step}";
}
