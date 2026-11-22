using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The colours the import pre-colours a fade's shades in (#2563): the opening's crossfade and the ending's screen fades are
/// installed as a shade at each level, each colour's steps (<see cref="FadeLevelColour"/>) scaled by the level and rounded
/// down, as the cartridge's fades show them.
/// </summary>
internal static partial class PreColouredFade
{
    /// <summary><paramref name="colour"/> at <paramref name="level"/> between black and itself.</summary>
    /// <param name="colour">The colour at its whole level.</param>
    /// <param name="level">How far up from black the colour shows.</param>
    /// <exception cref="ArgumentException">The colour lies off the game's colour steps.</exception>
    internal static RgbColor AtLevel(RgbColor colour, FadeLevel level) =>
        FadeLevelColour.Of(colour).Map((_, step) => step * level.Step / level.Steps).ToColour();
}
