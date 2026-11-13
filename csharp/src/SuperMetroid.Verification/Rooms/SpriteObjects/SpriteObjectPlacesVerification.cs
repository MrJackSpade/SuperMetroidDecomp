using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Rooms.SpriteObjects;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the room's sprite objects (#1591): they have no limit and run in spawn order, while the places they take,
/// which a dud shot on Kraid's mouth leaves as its pass's bound, follow the original's thirty-two places.
/// </summary>
internal static class SpriteObjectPlacesVerification
{
    /// <summary>
    /// Spawns take the places from the last down and none once all are taken, yet every spawn joins the live objects in
    /// spawn order; an ended object's place is the next one taken, and the dud bound reads twice the place or the shot's X.
    /// </summary>
    internal static void VerifySpriteObjectPlaces()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomSpriteObjects objects = enemies.SpriteObjects;
        var spawned = new List<RoomSpriteObject>();
        for (int spawn = 0; spawn < SpriteObjectPlaces.PlaceCount; spawn++)
        {
            RoomSpriteObject dust = objects.SpawnRoomSpriteObject(new(spawn, 0), RoomSpriteObjectKind.DustCloud, SpriteGraphics.SharedStandard);
            AssertEqual<int?>(SpriteObjectPlaces.PlaceCount - 1 - spawn, objects.PlaceOf(dust), $"spawn {spawn} takes the highest free place");
            spawned.Add(dust);
        }

        RoomSpriteObject beyond = objects.SpawnRoomSpriteObject(new(64, 0), RoomSpriteObjectKind.DustCloud, SpriteGraphics.SharedStandard);
        AssertEqual<int?>(null, objects.PlaceOf(beyond), "a spawn while every place is taken takes none");
        AssertTrue(beyond.IsActive, "a spawn that takes no place still runs");
        spawned.Add(beyond);
        AssertTrue(objects.Live.SequenceEqual(spawned), "every spawn is live, in spawn order");

        RoomSpriteObject ended = spawned[11];
        int endedPlace = objects.PlaceOf(ended) ?? throw new InvalidOperationException("The twelfth spawn took a place.");
        ended.End();
        spawned.Remove(ended);
        RoomSpriteObject refill = objects.SpawnRoomSpriteObject(new(65, 0), RoomSpriteObjectKind.DustCloud, SpriteGraphics.SharedStandard);
        AssertEqual<int?>(endedPlace, objects.PlaceOf(refill), "the next spawn takes the place an ended object freed");
        spawned.Add(refill);
        AssertTrue(objects.Live.SequenceEqual(spawned), "the ended object leaves and the new one runs last");

        var shot = new Position16<RoomPixels>(300, 200);
        KraidPassBounds placed = KraidPassBounds.Placed(left: 0, top: 0, bottom: 0);
        placed.LeaveAfterDud(endedPlace, shot);
        AssertEqual(endedPlace * 2, placed.Bottom, "a dud whose flash took a place leaves twice the place as the bottom");
        placed.LeaveAfterDud(null, shot);
        AssertEqual<int>(shot.X, placed.Bottom, "a dud whose flash took no place leaves the shot's X as the bottom");

        objects.EndAll();
        AssertTrue(!objects.Live.Any(), "ending every object leaves none live");
        RoomSpriteObject afterEnd = objects.SpawnRoomSpriteObject(new(66, 0), RoomSpriteObjectKind.DustCloud, SpriteGraphics.SharedStandard);
        AssertEqual<int?>(SpriteObjectPlaces.PlaceCount - 1, objects.PlaceOf(afterEnd), "once every object has ended the last place is free again");
        Console.WriteLine("Sprite object places: thirty-two places taken from the last down, unlimited live objects in spawn order, and the Kraid dud bound pass.");
    }

    /// <summary>Runs the focused sprite-object places check.</summary>
    [VerificationCommand("--sprite-object-places")]
    internal static int SpriteObjectPlacesCommand()
    {
        VerifySpriteObjectPlaces();
        return 0;
    }
}
