using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// The colouring the opening draws each of its sprite families in (#1513), as the opening's draw sites name it: every draw
/// from a family names one colouring, so the intro sheet pictures each family's poses in that colouring's sprite set.
/// </summary>
internal static class IntroFamilyColourings
{
    /// <summary>
    /// Each family's colouring: the discovery's egg and baby, its egg effects and its Rinkas in the discovery actors'; the
    /// laboratory's delivered and examined baby and the narration caret in the cross-fade objects'; the flashback's Mother
    /// Brain in hers; her explosions in theirs.
    /// </summary>
    private static readonly Dictionary<CinematicSpriteFamily, IntroSpriteColouring> Colourings = new()
    {
        [CinematicSpriteFamily.IntroDiscoveryActor] = IntroSpriteColouring.DiscoveryActors,
        [CinematicSpriteFamily.IntroEggEffect] = IntroSpriteColouring.DiscoveryActors,
        [CinematicSpriteFamily.IntroRinka] = IntroSpriteColouring.DiscoveryActors,
        [CinematicSpriteFamily.IntroScientist] = IntroSpriteColouring.CrossFadeObjects,
        [CinematicSpriteFamily.IntroMotherBrain] = IntroSpriteColouring.MotherBrain,
        [CinematicSpriteFamily.IntroMotherBrainExplosion] = IntroSpriteColouring.Explosions,
        [CinematicSpriteFamily.IntroCaret] = IntroSpriteColouring.CrossFadeObjects,
    };

    /// <summary>The colouring the opening draws <paramref name="family"/> in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The family is not one the opening draws.</exception>
    internal static IntroSpriteColouring Of(CinematicSpriteFamily family) => Colourings.TryGetValue(family, out IntroSpriteColouring colouring)
        ? colouring
        : throw new ArgumentOutOfRangeException(nameof(family), family, "Not a family the opening draws.");
}
