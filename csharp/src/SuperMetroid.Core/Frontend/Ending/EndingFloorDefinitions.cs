using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Where the ending's turning floors are installed (#1467).</summary>
public static class EndingFloorDefinitions
{
    /// <summary>The name of the whole floor's picture in each ending floor sheet.</summary>
    public const string FrameName = "floor";

    /// <summary>The installed sheet of each shot's floor, coloured with the palette its scene loads.</summary>
    public static IReadOnlyDictionary<EndingFloorShot, SpriteSheetId> Sheets { get; } = new Dictionary<EndingFloorShot, SpriteSheetId>
    {
        [EndingFloorShot.SideCloudEscape] = SpriteSheetId.EndingSideCloudEscapeFloor,
        [EndingFloorShot.TopAndBottomCloudEscape] = SpriteSheetId.EndingTopAndBottomCloudEscapeFloor,
        [EndingFloorShot.PlanetExplosion] = SpriteSheetId.EndingPlanetExplosionFloor,
        [EndingFloorShot.Flyaway] = SpriteSheetId.EndingFlyawayFloor,
        [EndingFloorShot.RewardIcon] = SpriteSheetId.EndingRewardIconFloor,
    };

    /// <summary>The scenery each shot's floor is, which names the colour effects it shows (#1544).</summary>
    public static IReadOnlyDictionary<EndingFloorShot, EndingScenery> Scenery { get; } = new Dictionary<EndingFloorShot, EndingScenery>
    {
        [EndingFloorShot.SideCloudEscape] = EndingScenery.SideCloudEscapeFloor,
        [EndingFloorShot.TopAndBottomCloudEscape] = EndingScenery.TopAndBottomCloudEscapeFloor,
        [EndingFloorShot.PlanetExplosion] = EndingScenery.PlanetExplosionFloor,
        [EndingFloorShot.Flyaway] = EndingScenery.FlyawayFloor,
        [EndingFloorShot.RewardIcon] = EndingScenery.PostCreditsShotFloor,
    };
}
