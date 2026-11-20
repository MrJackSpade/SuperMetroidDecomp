using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's live fight keeps the camera from looking above his arena (#2243): he holds the room camera's floor at Y 464
/// from his initialization to his death, a defeated room holds none, and the camera raises its whole Y to the floor and
/// nothing else, leaving a camera already at or below it untouched.
/// </summary>
internal static class SporeSpawnCameraFloorVerification
{
    /// <summary>
    /// Initializes a live and a defeated Spore Spawn, kills the live one through his bomb reaction, and applies held and
    /// released floors to cameras above, at and below the floor.
    /// </summary>
    internal static void VerifySporeSpawnCameraFloor()
    {
        bool defeated = false;
        RoomEnemySystem live = NewSporeSpawnRoom(loadsDefeated: false, onDefeated: () => defeated = true);
        AssertEqual<int?>(464, live.CameraFloor.LowestY, "live Spore Spawn holds the camera at his arena's top");

        RoomEnemyPlace body = live.Places.InUpdateOrder[0];
        body.Health = 0;
        body.HurtFlash.End();
        EnemyBombContext killingBomb = KillingBomb(body);
        // His body's own stages around common bomb damage, for the bomb that selects his shot reaction.
        EnemyBombReaction reaction = (body.Entity as IEnemyBombReaction)?.CommonBombDamageReaction(killingBomb)
            ?? throw new InvalidOperationException("Spore Spawn's body runs no stages of its own around common bomb damage.");
        Action<EnemyBombHitContext> afterCommon = reaction.AfterCommon
            ?? throw new InvalidOperationException("Spore Spawn's bomb reaction resolves nothing after the common damage.");
        afterCommon(new EnemyBombHitContext(killingBomb, body.Health));
        AssertTrue(defeated, "the killing bomb defeats Spore Spawn");
        AssertEqual<int?>(null, live.CameraFloor.LowestY, "Spore Spawn's death frees the camera");

        RoomEnemySystem dead = NewSporeSpawnRoom(loadsDefeated: true, onDefeated: () => throw new InvalidOperationException("A defeated Spore Spawn is defeated again."));
        AssertEqual<int?>(null, dead.CameraFloor.LowestY, "a defeated Spore Spawn's room holds the camera nowhere");

        var floor = new RoomCameraFloor();
        floor.Hold(464);
        foreach ((ushort trackedY, ushort keptY) in new (ushort, ushort)[] { (400, 464), (463, 464), (464, 464), (465, 465), (600, 600) })
        {
            ScrollBoundaryCamera camera = CameraAt(trackedY);
            Position16<RoomPixels> ideal = camera.IdealPosition;
            camera.ApplyFloor(floor);
            AssertEqual(FixedPosition<RoomPixels>.FromStoredParts(0x0010, 0x1234, keptY, 0x5678), camera.Position,
                $"a camera tracked to Y {trackedY} keeps to the floor at Y {keptY}, its X and fractions unchanged");
            AssertEqual(ideal, camera.IdealPosition, $"the floor leaves the ideal position of a camera tracked to Y {trackedY}");
        }

        floor.Release();
        ScrollBoundaryCamera free = CameraAt(400);
        free.ApplyFloor(floor);
        AssertEqual(FixedPosition<RoomPixels>.FromStoredParts(0x0010, 0x1234, 400, 0x5678), free.Position,
            "a released floor leaves the camera where it tracked");
        Console.WriteLine("  Spore Spawn camera floor: his live fight holds the camera at Y 464 until he dies, and the camera keeps to it.");
    }

    /// <summary>A room whose Spore Spawn has initialized, live or already defeated.</summary>
    private static RoomEnemySystem NewSporeSpawnRoom(bool loadsDefeated, Action onDefeated)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().IsAreaMiniBossDefeated = () => loadsDefeated;
        enemies.VerificationServices().SetAreaMiniBossDefeated = onDefeated;
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.SporeSpawn, body.Definition);
        body.Position = body.Position.WithWhole(new(128, 624));
        body.Health = 960;
        enemies.SporeSpawnBehavior.InitializeSporeSpawn(body.HeldOccupant(), body);
        return enemies;
    }

    /// <summary>A bomb exploding on the hitbox of Spore Spawn's body that selects his shot reaction.</summary>
    private static EnemyBombContext KillingBomb(RoomEnemyPlace body)
    {
        var ordinaryProjectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads,
            new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        var bomb = new SamusBomb(0);
        bomb.Occupy(SamusProjectileType.Of(SamusProjectileFamily.Bomb));
        bomb.Position = FixedPosition<RoomPixels>.FromWhole(body.Position.Whole);
        return new EnemyBombContext(body.HeldOccupant(), bomb.HeldBomb(), ordinaryProjectiles, RoomEnemyFrameFixtures.StandaloneSamus(),
            SelectedShot: SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnReactions.Shot, UsesFrameHitboxes: false, new Position16<RoomPixels>(0, 0))
        {
            StruckOccupant = body.OccupantStates,
        };
    }

    /// <summary>A camera in a one-by-three-screen room tracked to <paramref name="y"/>, with fractions on both axes.</summary>
    private static ScrollBoundaryCamera CameraAt(ushort y)
    {
        var storage = new byte[RoomScrollGrid.KeptCellCount];
        Array.Fill(storage, (byte)1);
        var camera = global::SuperMetroid.Core.PrivateState.With<ScrollBoundaryCamera>(new ScrollBoundaryCamera(RoomScrollGrid.LoadNativeTable(storage, new Extent16<RoomScreens>(1, 3)))
        {
            }, "Position", FixedPosition<RoomPixels>.FromStoredParts(0x0010, 0x1234, y, 0x5678));
        return camera;
    }

    /// <summary>Runs the Spore Spawn camera floor check.</summary>
    [VerificationCommand("--spore-spawn-camera-floor")]
    internal static int SporeSpawnCameraFloorCommand()
    {
        Suite(nameof(VerifySporeSpawnCameraFloor), () => VerifySporeSpawnCameraFloor());
        return 0;
    }
}
