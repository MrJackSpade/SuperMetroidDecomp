using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The sprite sheet each ending scene draws its actors from (#1392, #1513): the escape clouds, the explosion, the
/// post-credits Samus each reward dresses, and the shooting scene, whose sheet also holds the logo the scene ends on.
/// </summary>
internal static class EndingSpriteSheets
{
    /// <summary>The sheet of <paramref name="objectSheet"/>'s scene in an ending of <paramref name="reward"/>.</summary>
    internal static SpriteSheetId For(EndingObjectSheet objectSheet, EndingReward reward) => objectSheet switch
    {
        EndingObjectSheet.Clouds => SpriteSheetId.EndingCloudSprites,
        EndingObjectSheet.Explosion => SpriteSheetId.EndingExplosionSprites,
        EndingObjectSheet.Reward => reward == EndingReward.Suitless
            ? SpriteSheetId.EndingSuitlessRewardSprites : SpriteSheetId.EndingSuitedRewardSprites,
        EndingObjectSheet.RewardFalling => SpriteSheetId.EndingShootingSprites,
        _ => throw new ArgumentOutOfRangeException(nameof(objectSheet), objectSheet, "Undefined ending object sheet."),
    };
}
