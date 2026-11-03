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

    /// <summary>The shade an installed key names.</summary>
    /// <exception cref="InvalidDataException">The key is not an effect name, a slash and a step.</exception>
    public static PictureShade FromKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        int slash = key.LastIndexOf('/');
        return slash > 0 && int.TryParse(key.AsSpan(slash + 1), System.Globalization.NumberStyles.None,
            System.Globalization.CultureInfo.InvariantCulture, out int step)
            ? new PictureShade(key[..slash], step)
            : throw new InvalidDataException($"Shade key {key} is not an effect name, a slash and a step.");
    }
}
