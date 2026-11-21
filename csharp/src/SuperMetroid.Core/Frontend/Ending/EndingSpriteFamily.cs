using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The sprite families the ending draws (#2553): each of its actors' pictures lie on one of the ending's sheets
/// (<see cref="EndingSpriteSheets"/>), and each is the cinematic sprite family of the same name
/// (<see cref="EndingSpriteSheets.Cinematic"/>); the intro's and Ceres's families are none of them.
/// </summary>
internal enum EndingSpriteFamily
{
    /// <summary>The escape's clouds.</summary>
    EndingCloud,

    /// <summary>The planet explosion's actors.</summary>
    EndingExplosion,

    /// <summary>The result text and the clear time.</summary>
    EndingCompletionText,

    /// <summary>The post-credits Samus the reward dresses.</summary>
    EndingReward,

    /// <summary>The logo the falling Samus's shot reveals.</summary>
    EndingLogo,

    /// <summary>The animals' escape pod.</summary>
    EndingAnimalEscape,
}
