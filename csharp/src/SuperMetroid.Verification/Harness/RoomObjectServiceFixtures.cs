using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// The room services a room-object check builds its room objects with (#2015). Every room has each service; a check
/// starts from <see cref="Uninstalled"/> and installs, with a <c>with</c> expression, the ones its room objects use.
/// </summary>
internal static class RoomObjectServiceFixtures
{
    /// <summary>Services that each fail at the exact dependency a check reaches without installing it.</summary>
    internal static RoomObjectServices Uninstalled { get; } = new()
    {
        Samus = FixtureRoomSamus.Uninstalled,
        Bosses = new FixtureAreaBossFlags(),
        Events = new FixtureEventFlags(),
        TourianStatue = new FixtureTourianStatue(),
        RoomFx = new FixtureRoomObjectFx(),
        Earthquake = new FixtureRoomEarthquake(),
        Enemies = new FixtureObjectEnemyEffects(),
    };
}
