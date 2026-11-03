namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour effect a draw publishes as a shader parameter (#1512): a function of each colour its picture shows, after its
/// shades, such as a fade or a flash. An effect that moves colours between a picture's inks, such as a cycle, cannot be
/// one: it is a <see cref="PictureShade"/> the import pre-colours.
/// </summary>
public abstract record ColourEffect
{
    /// <summary>The colour an ink showing <paramref name="shown"/> shows under the effect.</summary>
    public abstract RgbColor Apply(RgbColor shown);
}
