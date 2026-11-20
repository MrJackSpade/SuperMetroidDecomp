using System.Collections.Frozen;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The sprite sheet each of the ending's actors draws its frames from (#1392, #1513, #1626): the escape's clouds, the
/// explosion's actors, the result text and the animals' pod, the post-credits Samus each reward dresses with the shooting
/// stars, and the falling Samus with the logo. Each actor's frames lie on its own sheet, whatever scene shows it.
/// </summary>
internal static class EndingSpriteSheets
{
    /// <summary>The sheet of the escape's clouds.</summary>
    private const SpriteSheetId Clouds = SpriteSheetId.EndingCloudSprites;

    /// <summary>The sheet of the planet explosion's actors, the result text, the clear time and the animals' escape pod.</summary>
    private const SpriteSheetId Explosion = SpriteSheetId.EndingExplosionSprites;

    /// <summary>The sheet of Samus falling back from above the screen, landing and shooting, and of the logo the shot reveals.</summary>
    internal const SpriteSheetId FallingSamusAndLogo = SpriteSheetId.EndingShootingSprites;

    /// <summary>The sheet of the post-credits Samus an ending of <paramref name="reward"/> shows, until she falls, and of the shooting stars.</summary>
    internal static SpriteSheetId Reward(EndingReward reward) => reward == EndingReward.Suitless
        ? SpriteSheetId.EndingSuitlessRewardSprites : SpriteSheetId.EndingSuitedRewardSprites;

    /// <summary>
    /// The sheet the frames of <paramref name="family"/> lie on in an ending of <paramref name="reward"/>; the reward's
    /// falling Samus draws from <see cref="FallingSamusAndLogo"/> instead.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="family"/> is not an ending family.</exception>
    internal static SpriteSheetId Of(CinematicSpriteFamily family, EndingReward reward) => family == CinematicSpriteFamily.EndingReward
        ? Reward(reward)
        : Ensure.Listed(FamilySheets, family);

    /// <summary>The sheet of each ending family other than the reward's, whose sheet depends on the reward.</summary>
    private static FrozenDictionary<CinematicSpriteFamily, SpriteSheetId> FamilySheets => field ??= new Dictionary<CinematicSpriteFamily, SpriteSheetId>
    {
        [CinematicSpriteFamily.EndingCloud] = Clouds,
        [CinematicSpriteFamily.EndingExplosion] = Explosion,
        [CinematicSpriteFamily.EndingCompletionText] = Explosion,
        [CinematicSpriteFamily.EndingAnimalEscape] = Explosion,
        [CinematicSpriteFamily.EndingLogo] = FallingSamusAndLogo,
    }.ToFrozenDictionary();
}
