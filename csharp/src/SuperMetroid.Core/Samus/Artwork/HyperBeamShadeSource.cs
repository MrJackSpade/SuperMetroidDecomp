namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The ink and brightness a hyper beam shade is derived from.</summary>
/// <param name="Ink">The ink.</param>
/// <param name="Brightness">The brightness.</param>
public readonly record struct HyperBeamShadeSource(int Ink, int Brightness);
