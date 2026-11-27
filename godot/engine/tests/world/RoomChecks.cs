using Engine.Actors;
using Engine.Collision;
using Engine.Input;
using Engine.Shared;
using Engine.Tests.Actors;
using Engine.World;
using Godot;

namespace Engine.Tests.World;

/// <summary>Checks of the room and its parts: exits, triggers, zones, conditions, versions, liquid, room effects and sequences.</summary>
public sealed class RoomChecks
{
    /// <summary>An open room two screens wide: a zone of the view placed past the first screen lies on its second.</summary>
    private static readonly string[] TwoScreens =
    [
        new('.', 32), new('.', 32), new('.', 32), new('.', 32), new('.', 32), new('#', 32),
    ];

    private static readonly string[] OpenRoom =
    [
        "................", "................", "................", "................", "................", "################",
    ];

    [Check]
    public void EnteringAnExitIsReportedOnce()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        var exit = world.Place(new RoomExit { Name = "East", Size = new Vector2I(16, 80), LeadsToRoom = "next.tscn", LeadsToExit = "West" }, 120, 0);
        world.Setup();
        Expect.True(world.Room.ExitEntered is null);
        world.Step(60, GameAction.Right);
        Expect.True(ReferenceEquals(exit, world.Room.ExitEntered));
    }

    [Check]
    public void ThePlayerArrivesWhereAnExitSays()
    {
        using var world = new StandInWorld(OpenRoom);
        world.Place(new RoomExit { Name = "West", Size = new Vector2I(16, 80), ArrivalOffset = new Vector2I(40, 80) }, 0, 0);
        world.Room.Setup(world.Session, world.Player, new RoomEntry("West", PixelVelocity.Zero, Facing.Right));
        Expect.Equal(PixelPosition.FromWhole(40, 80), world.Player.Position);
    }

    [Check]
    public void ATriggerTellsItsTargetOnceWhenThePlayerEnters()
    {
        using var world = new StandInWorld(TwoScreens).StartAt(40, 80);
        var zone = world.Place(new CameraZone { Name = "Zone" }, 300, 0);
        world.Place(new Trigger { Name = "Trigger", Size = new Vector2I(16, 80), Target = new NodePath("../Zone"), What = "open" }, 100, 0);
        world.Setup();
        Expect.False(zone.IsOpen);

        world.Step(60, GameAction.Right);
        Expect.True(zone.IsOpen);

        zone.Triggered("close");
        world.Step(30, GameAction.Left);
        world.Step(60, GameAction.Right);
        Expect.False(zone.IsOpen);
    }

    [Check]
    public void ATriggerWithAConditionWaitsForIt()
    {
        using var world = new StandInWorld(TwoScreens).StartAt(40, 80);
        var zone = world.Place(new CameraZone { Name = "Zone" }, 300, 0);
        world.Place(new Trigger
        {
            Name = "Trigger", Size = new Vector2I(16, 80), Target = new NodePath("../Zone"), What = "open",
            OnlyWhen = Condition.Of(ConditionKind.FactRecorded, "power on"),
        }, 100, 0);
        world.Setup();

        world.Step(60, GameAction.Right);
        Expect.False(zone.IsOpen);

        world.Session.Progress.Record("power on");
        world.Step(40, GameAction.Left);
        world.Step(60, GameAction.Right);
        Expect.True(zone.IsOpen);
    }

    [Check]
    public void APlacedThingIsPresentOnlyWhenItsConditionHolds()
    {
        using (var absent = new StandInWorld(OpenRoom))
        {
            absent.Place(new StandInEnemy { Name = "Guard", PresentWhen = Condition.Of(ConditionKind.FactRecorded, "alarm raised") }, 200, 72);
            absent.Setup();
            Expect.Equal(0, absent.Room.Enemies.Count);
        }

        using var present = new StandInWorld(OpenRoom);
        present.Session.Progress.Record("alarm raised");
        present.Place(new StandInEnemy { Name = "Guard", PresentWhen = Condition.Of(ConditionKind.FactRecorded, "alarm raised") }, 200, 72);
        present.Setup();
        Expect.Equal(1, present.Room.Enemies.Count);
    }

    [Check]
    public void ConditionsCombine()
    {
        using var world = new StandInWorld(OpenRoom);
        var enemy = world.Place(new StandInEnemy { Name = "Guard" }, 200, 72);
        world.Setup();
        world.Session.Progress.Record("first");
        Condition first = Condition.Of(ConditionKind.FactRecorded, "first");
        Condition second = Condition.Of(ConditionKind.FactRecorded, "second");

        Expect.False(Condition.Of(ConditionKind.All, first, second).IsTrue(world.Room));
        Expect.True(Condition.Of(ConditionKind.Any, first, second).IsTrue(world.Room));
        Expect.False(Condition.Of(ConditionKind.None, first, second).IsTrue(world.Room));
        Expect.False(Condition.Of(ConditionKind.RoomHasNoEnemies).IsTrue(world.Room));
        Expect.False(Condition.Of(ConditionKind.PlacedThingGone, "Guard").IsTrue(world.Room));
        Expect.False(Condition.Of(ConditionKind.PlayerOwns, "boots").IsTrue(world.Room));

        enemy.Die();
        world.Player.Inventory.Grant("boots");
        Expect.True(Condition.Of(ConditionKind.RoomHasNoEnemies).IsTrue(world.Room));
        Expect.True(Condition.Of(ConditionKind.PlacedThingGone, "Guard").IsTrue(world.Room));
        Expect.True(Condition.Of(ConditionKind.PlayerOwns, "boots").IsTrue(world.Room));
    }

    [Check]
    public void ARoomUsesTheFirstVersionWhoseConditionHolds()
    {
        RoomVersion[] Versions() =>
        [
            new RoomVersion { When = Condition.Of(ConditionKind.FactRecorded, "boss beaten"), Music = "quiet", Patches = ["top left opened"] },
            new RoomVersion { Music = "battle" },
        ];

        using (var before = new StandInWorld("#...", "....", "####"))
        {
            before.Room.Versions = [.. Versions()];
            before.Setup();
            Expect.Equal("battle", before.Room.Version?.Music ?? "");
            Expect.False(before.Room.Terrain.IsPatchApplied("top left opened"));
        }

        using var after = new StandInWorld("#...", "....", "####");
        after.Session.Progress.Record("boss beaten");
        after.Room.Versions = [.. Versions()];
        after.Setup();
        Expect.Equal("quiet", after.Room.Version?.Music ?? "");
        Expect.True(after.Room.Terrain.IsPatchApplied("top left opened"));
    }

    [Check]
    public void LiquidSlowsThePlayerUnlessTheirEquipmentCancelsIt()
    {
        Pixels DistanceRun(bool suited)
        {
            using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
            world.Assets.With("water.json", """
                { "name": "water", "speed_times": 1, "speed_over": 2, "slowing_cancelled_by": ["diving suit"] }
                """);
            world.Place(new Liquid { Name = "Water", TypeFile = "water.json", LowSurface = 20, HighSurface = 20 }, 0, 0);
            world.Setup();
            if (suited)
                world.Player.Inventory.Grant("diving suit");
            world.Step(40, GameAction.Right);
            return world.Player.Position.X - Pixels.FromWhole(40);
        }

        Pixels slowed = DistanceRun(suited: false), free = DistanceRun(suited: true);
        Expect.True(slowed < free);
        Expect.True(slowed > Pixels.Zero);
    }

    [Check]
    public void ALiquidRisesWhenToldAndSubmergesWhatItCovers()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        world.Assets.With("water.json", """{ "name": "water" }""");
        var water = world.Place(new Liquid { Name = "Water", TypeFile = "water.json", LowSurface = 90, HighSurface = 30, MoveSpeed = 2 }, 0, 0);
        world.Setup();
        Expect.False(water.IsSubmerged(world.Player.Position));

        water.Triggered("rise");
        world.Step(10);
        Expect.Equal(Pixels.FromWhole(70), water.Surface);
        Expect.True(water.IsSubmerged(world.Player.Position));
        world.Step(40);
        Expect.Equal(Pixels.FromWhole(30), water.Surface);
    }

    [Check]
    public void OngoingDamageIsTakenALittleEachUpdateAndCancelledByEquipment()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        world.Assets.With("player.json", """
            { "half_width": 5, "standing_height": 40, "crouching_height": 28, "crawling_height": 14,
              "run_acceleration": 0.25, "run_speed": 2, "friction": 0.5, "air_acceleration": 0.25, "crawl_speed": 1.5,
              "jump_speed": 4.5, "jump_cut_speed": 1, "crawl_jump_speed": 3.5, "gravity": 0.125, "fall_speed_limit": 5,
              "wall_jump_up_speed": 4,
              "knockback_across_speed": 1.5, "knockback_up_speed": 2, "knockback_updates": 12, "invincible_updates": 60,
              "turn_pictures": [], "climb_per_pixel": 2, "follow_ground_down": 3, "starting_energy": 99,
              "can_crouch": true, "can_crawl": false, "can_jump_while_crawling": false,
              "crawl_needs": "", "jump_while_crawling_needs": "",
              "ongoing_damage_cancelled_by": { "heat": ["heat suit"] } }
            """);
        world.Player.DataFile = "player.json";
        world.Place(new StandInHeat { Name = "Heat", EffectName = "heat", DamageHundredths = 25 }, 0, 0);
        world.Setup();

        world.Step(3);
        Expect.Equal(99, world.Player.Inventory.Amount(Inventory.Energy));
        world.Step(1);
        Expect.Equal(98, world.Player.Inventory.Amount(Inventory.Energy));

        world.Player.Inventory.Grant("heat suit");
        world.Step(40);
        Expect.Equal(98, world.Player.Inventory.Amount(Inventory.Energy));
    }

    [Check]
    public void ASequenceRunsWhenToldAndRecordsThatItFinished()
    {
        using var world = new StandInWorld("#...", "....", "####").StartAt(40, 32);
        var sequence = new StandInSequence { Name = "Sequence", Length = 3, Patch = "top left opened", RememberedAs = "wall opened" };
        world.Room.AddChild(sequence);
        world.Setup();

        world.Step(5);
        Expect.False(world.Room.Terrain.IsPatchApplied("top left opened"));

        sequence.Triggered("start");
        world.Step(2);
        Expect.True(sequence.IsRunning);
        world.Step(1);
        Expect.False(sequence.IsRunning);
        Expect.True(world.Room.Terrain.IsPatchApplied("top left opened"));
        Expect.True(world.Session.Progress.Has("wall opened"));
    }

    [Check]
    public void AnEnemyFarOutsideTheViewDoesNotUpdate()
    {
        string wide = new('.', 64);
        using var world = new StandInWorld(wide, wide, wide, wide, wide, new string('#', 64)).StartAt(40, 80);
        var near = world.Place(new StandInEnemy { Name = "Near" }, 200, 40);
        var far = world.Place(new StandInEnemy { Name = "Far" }, 900, 40);
        world.Setup();
        world.Step(10);
        Expect.Equal(10, near.UpdatesBehaved);
        Expect.Equal(0, far.UpdatesBehaved);
    }

    [Check]
    public void TheViewFollowsThePlayerWithinTheRoomAndKeepsOutOfClosedZones()
    {
        string wide = new('.', 64);
        using var world = new StandInWorld(wide, wide, wide, wide, wide, new string('#', 64)).StartAt(40, 80);
        var zone = world.Place(new CameraZone { Name = "Secret" }, 512, 0);
        world.Setup();
        Expect.Equal(0, world.Session.Camera.Left);

        world.Player.Position = PixelPosition.FromWhole(480, 80);
        world.Step(1);
        Expect.Equal(256, world.Session.Camera.Left);

        zone.Triggered("open");
        world.Step(1);
        Expect.Equal(480 - 128, world.Session.Camera.Left);
    }

    [Check]
    public void TheMapCellThePlayerIsInIsMarkedExplored()
    {
        string wide = new('.', 64);
        using var world = new StandInWorld(wide, wide, wide, wide, wide, new string('#', 64)).StartAt(40, 80);
        world.Room.Area = "caves";
        world.Room.MapCell = new Vector2I(4, 7);
        world.Setup();
        world.Step(1);
        Expect.True(world.Session.Progress.IsExplored("caves", 4, 7));
        Expect.False(world.Session.Progress.IsExplored("caves", 5, 7));

        world.Player.Position = PixelPosition.FromWhole(300, 80);
        world.Step(1);
        Expect.True(world.Session.Progress.IsExplored("caves", 5, 7));
    }

    [Check]
    public void StoppedGroupsDoNotRunWhileTheRestDo()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        var enemy = world.Place(new StandInEnemy { Name = "Enemy" }, 200, 40);
        world.Setup();
        world.Room.Step(new GameInput([GameAction.Right], []), Stopped.Enemies);
        world.Room.Step(new GameInput([GameAction.Right], []), Stopped.Enemies);
        Expect.Equal(0, enemy.UpdatesBehaved);
        Expect.True(world.Player.Position.X > Pixels.FromWhole(40));

        Pixels held = world.Player.Position.X;
        world.Room.Step(new GameInput([GameAction.Right], []), Stopped.Player);
        Expect.Equal(held, world.Player.Position.X);
        Expect.Equal(1, enemy.UpdatesBehaved);
    }
}
