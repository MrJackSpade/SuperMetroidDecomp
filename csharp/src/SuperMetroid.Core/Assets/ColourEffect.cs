namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour effect a draw publishes as a shader parameter (#1512): a function of each colour its picture shows, after its
/// shades, such as a fade or a flash. An effect that moves colours between a picture's inks, such as a cycle, cannot be
/// one: it is a <see cref="PictureShade"/> the import pre-colours. A fade names how far it has come (<see cref="FadeLevel"/>)
/// and the backend blends it (<see cref="IColourBlend"/>, #2563).
/// </summary>
public abstract record ColourEffect
{
    /// <summary>The colour an ink showing <paramref name="shown"/> shows under the effect, any fade blended by <paramref name="blend"/>.</summary>
    /// <param name="shown">The colour the ink shows before the effect.</param>
    /// <param name="blend">The backend's blending of a fade's colours.</param>
    public abstract RgbColor Apply(RgbColor shown, IColourBlend blend);
}
