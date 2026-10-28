using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Cartridge values and helpers of <see cref="EndingSpriteSheets"/> that only verification reads.</summary>
internal static class EndingSpriteSheetsConstants
{
    /// <summary>Every ending sheet.</summary>
    internal static IReadOnlyList<SpriteSheetId> Sheets { get; } =
    [
        SpriteSheetId.EndingCloudSprites, SpriteSheetId.EndingExplosionSprites, SpriteSheetId.EndingSuitedRewardSprites,
        SpriteSheetId.EndingSuitlessRewardSprites, .. EndingSpriteSheets.ShootingSheets,
    ];
}
