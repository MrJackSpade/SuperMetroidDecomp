using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The sprite sheet each of the ending's actors draws its frames from (#1392, #1513, #1626): the escape's clouds, the
/// explosion's actors, the result text and the animals' pod, the post-credits Samus each reward dresses with the shooting
/// stars, and the falling Samus with the logo. Each actor's frames lie on its own sheet, whatever scene shows it.
/// </summary>
internal static partial class EndingSpriteSheets
{
    /// <summary>The sheet of the escape's clouds.</summary>
    internal const SpriteSheetId Clouds = SpriteSheetId.EndingCloudSprites;

    /// <summary>The sheet of the planet explosion's actors, the result text, the clear time and the animals' escape pod.</summary>
    internal const SpriteSheetId Explosion = SpriteSheetId.EndingExplosionSprites;

    /// <summary>The sheet of Samus falling back from above the screen, landing and shooting, and of the logo the shot reveals.</summary>
    internal const SpriteSheetId FallingSamusAndLogo = SpriteSheetId.EndingShootingSprites;

    /// <summary>The sheet of the post-credits Samus an ending of <paramref name="reward"/> shows, until she falls, and of the shooting stars.</summary>
    internal static SpriteSheetId Reward(EndingReward reward) => EndingRewardDefinitions.Rewards[reward].Sheet;

    /// <summary>
    /// The sheet the frames of <paramref name="family"/> lie on in an ending of <paramref name="reward"/>; the reward's
    /// falling Samus draws from <see cref="FallingSamusAndLogo"/> instead.
    /// </summary>
    internal static SpriteSheetId Of(EndingSpriteFamily family, EndingReward reward) => FamilySheets[family](reward);

    /// <summary>The sheet of each ending family in an ending of each reward: only the reward's own depends on the reward.</summary>
    private static IdentityTable<EndingSpriteFamily, Func<EndingReward, SpriteSheetId>> FamilySheets => field ??= new(new Dictionary<EndingSpriteFamily, Func<EndingReward, SpriteSheetId>>
    {
        [EndingSpriteFamily.EndingCloud] = static _ => Clouds,
        [EndingSpriteFamily.EndingExplosion] = static _ => Explosion,
        [EndingSpriteFamily.EndingCompletionText] = static _ => Explosion,
        [EndingSpriteFamily.EndingReward] = Reward,
        [EndingSpriteFamily.EndingAnimalEscape] = static _ => Explosion,
        [EndingSpriteFamily.EndingLogo] = static _ => FallingSamusAndLogo,
    });

    /// <summary>The cinematic sprite family <paramref name="family"/> is, whose installed pictures it draws.</summary>
    internal static CinematicSpriteFamily Cinematic(EndingSpriteFamily family) => CinematicFamilies[family];

    /// <summary>The cinematic sprite family each ending family is.</summary>
    private static IdentityTable<EndingSpriteFamily, CinematicSpriteFamily> CinematicFamilies => field ??= new(new Dictionary<EndingSpriteFamily, CinematicSpriteFamily>
    {
        [EndingSpriteFamily.EndingCloud] = CinematicSpriteFamily.EndingCloud,
        [EndingSpriteFamily.EndingExplosion] = CinematicSpriteFamily.EndingExplosion,
        [EndingSpriteFamily.EndingCompletionText] = CinematicSpriteFamily.EndingCompletionText,
        [EndingSpriteFamily.EndingReward] = CinematicSpriteFamily.EndingReward,
        [EndingSpriteFamily.EndingAnimalEscape] = CinematicSpriteFamily.EndingAnimalEscape,
        [EndingSpriteFamily.EndingLogo] = CinematicSpriteFamily.EndingLogo,
    });
}
