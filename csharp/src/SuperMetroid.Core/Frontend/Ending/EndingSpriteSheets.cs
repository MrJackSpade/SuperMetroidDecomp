using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The sprite sheet each ending page draws its actors from (#1392). A sheet holds what a page shows over the characters
/// its scene's transfers leave there: the post-credits reward page holds the Samus the reward dresses, and the shooting
/// scene's page loses its characters to the logo's a quarter at a time, so it has a sheet for each quarter.
/// </summary>
internal static class EndingSpriteSheets
{
    /// <summary>The shooting scene's sheets, by how many of the logo's character transfers have replaced its own.</summary>
    internal static ImmutableArray<SpriteSheetId> ShootingSheets { get; } =
    [
        SpriteSheetId.EndingShootingSprites, SpriteSheetId.EndingShootingLogoQuarterSprites, SpriteSheetId.EndingShootingLogoHalfSprites,
        SpriteSheetId.EndingShootingLogoThreeQuartersSprites, SpriteSheetId.EndingLogoSprites,
    ];

    /// <summary>
    /// The sheet of <paramref name="objectSheet"/>'s page in an ending of <paramref name="reward"/>, once the post-shot
    /// has made <paramref name="logoTileTransfers"/> of the logo's character transfers.
    /// </summary>
    internal static SpriteSheetId For(EndingObjectSheet objectSheet, EndingReward reward, int logoTileTransfers) => objectSheet switch
    {
        EndingObjectSheet.Clouds => SpriteSheetId.EndingCloudSprites,
        EndingObjectSheet.Explosion => SpriteSheetId.EndingExplosionSprites,
        EndingObjectSheet.Reward => reward == EndingReward.Suitless
            ? SpriteSheetId.EndingSuitlessRewardSprites : SpriteSheetId.EndingSuitedRewardSprites,
        EndingObjectSheet.RewardFalling => ShootingSheets[logoTileTransfers],
        _ => throw new ArgumentOutOfRangeException(nameof(objectSheet), objectSheet, "Undefined ending object sheet."),
    };
}
