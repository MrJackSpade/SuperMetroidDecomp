using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Where the ending's turning floors are installed (#1467) and how many loads the flyaway's floor takes.</summary>
public static class EndingFloorDefinitions
{
    /// <summary>The name of the whole floor's picture in each ending floor sheet.</summary>
    public const string FrameName = "floor";

    /// <summary>The loads that bring the flyaway's floor in: its characters, then its map, eight each.</summary>
    public const int FlyawayChunkCount = 16;

    /// <summary>The installed sheet of each shot's floor, coloured with the palette its scene loads.</summary>
    public static IReadOnlyDictionary<EndingFloorShot, SpriteSheetId> Sheets { get; } = new Dictionary<EndingFloorShot, SpriteSheetId>
    {
        [EndingFloorShot.EscapeA] = SpriteSheetId.EndingEscapeAFloor,
        [EndingFloorShot.EscapeB] = SpriteSheetId.EndingEscapeBFloor,
        [EndingFloorShot.PlanetExplosion] = SpriteSheetId.EndingPlanetExplosionFloor,
        [EndingFloorShot.Flyaway] = SpriteSheetId.EndingFlyawayFloor,
        [EndingFloorShot.RewardIcon] = SpriteSheetId.EndingRewardIconFloor,
    };
}
