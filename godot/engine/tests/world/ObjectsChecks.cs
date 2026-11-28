using Engine.Actors;
using Engine.Collision;
using Engine.Input;
using Engine.Shared;
using Engine.Tests.Actors;
using Engine.World;
using Engine.World.Objects;
using Godot;

namespace Engine.Tests.World;

/// <summary>Checks of the room objects: barriers, items, stations, carriers, platforms, spawners, hazards and boss encounters.</summary>
public sealed class ObjectsChecks
{
    private static readonly string[] OpenRoom =
    [
        "................", "................", "................", "................", "................", "################",
    ];

    private static readonly HitKind Missile = new("missile");
    private static readonly HitKind Super = new("super missile");
    private static readonly HitKind Beam = new("beam");

    [Check]
    public void ABarrierIsWornDownOnlyByTheDamageItAccepts()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        Barrier door = PlaceBarrier(world, "Door", 96, 40);
        door.MaxHealth = 5;
        door.DamagedBy = new Godot.Collections.Dictionary<string, int> { ["missile"] = 1, ["super missile"] = 5 };
        world.Setup();
        Collider part = EntityNode.ColliderOf(door, ColliderRole.Hurtbox) ?? throw new CheckFailedException("no hurtbox");

        Expect.Equal(HitResult.Absorbed, door.TakeHit(new Hit(100, HitSide.Player, Beam), part));
        Expect.Equal(HitResult.Hurt, door.TakeHit(new Hit(100, HitSide.Player, Missile), part));
        Expect.Equal(HitResult.Destroyed, door.TakeHit(new Hit(100, HitSide.Player, Super), part));
        Expect.True(door.IsOpen);
        Expect.True(world.Room.IsGone("Door"));
    }

    [Check]
    public void AClosedBarrierBlocksThePlayerAndAnOpenOneDoesNot()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        Barrier door = PlaceBarrier(world, "Door", 96, 40);
        door.AnyHit = 1;
        world.Setup();

        world.Step(60, GameAction.Right);
        Expect.Equal(Pixels.FromWhole(91), world.Player.Position.X);

        door.Open();
        world.Step(30, GameAction.Right);
        Expect.True(world.Player.Position.X > Pixels.FromWhole(112));
    }

    [Check]
    public void ABarrierHeldShutCannotBeDamagedUntilItsConditionHolds()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        var enemy = world.Place(new StandInEnemy { Name = "Guard" }, 200, 40);
        Barrier door = PlaceBarrier(world, "Door", 96, 40);
        door.AnyHit = 1;
        door.HeldShutBy = Condition.Of(ConditionKind.RoomHasNoEnemies);
        world.Setup();
        Collider part = EntityNode.ColliderOf(door, ColliderRole.Hurtbox) ?? throw new CheckFailedException("no hurtbox");

        Expect.True(door.IsSealed);
        Expect.Equal(HitResult.Absorbed, door.TakeHit(new Hit(1, HitSide.Player, Beam), part));

        enemy.Die();
        Expect.False(door.IsSealed);
        Expect.Equal(HitResult.Destroyed, door.TakeHit(new Hit(1, HitSide.Player, Beam), part));
    }

    [Check]
    public void AnOpenedAndRememberedBarrierIsAPlainOneNextTime()
    {
        void Arrange(StandInWorld world, out Barrier door)
        {
            door = PlaceBarrier(world, "Door", 96, 40);
            door.MaxHealth = 5;
            door.DamagedBy = new Godot.Collections.Dictionary<string, int> { ["missile"] = 1 };
            door.RememberedAs = "red door opened";
            door.Remembered = Barrier.RememberedAsWhat.Plain;
        }

        using var first = new StandInWorld(OpenRoom).StartAt(40, 80);
        Arrange(first, out Barrier door);
        first.Setup();
        Collider part = EntityNode.ColliderOf(door, ColliderRole.Hurtbox) ?? throw new CheckFailedException("no hurtbox");
        for (int hit = 0; hit < 5; hit++)
            door.TakeHit(new Hit(1, HitSide.Player, Missile), part);
        Expect.True(first.Session.Progress.Has("red door opened"));

        using var second = new StandInWorld(OpenRoom).StartAt(40, 80);
        second.Session.Progress.Record("red door opened");
        Arrange(second, out Barrier again);
        second.Setup();
        Collider againPart = EntityNode.ColliderOf(again, ColliderRole.Hurtbox) ?? throw new CheckFailedException("no hurtbox");
        Expect.False(again.IsOpen);
        Expect.Equal(HitResult.Destroyed, again.TakeHit(new Hit(1, HitSide.Player, Beam), againPart));
    }

    [Check]
    public void ABarrierToldToOpenAndCloseDoesSo()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        Barrier gate = PlaceBarrier(world, "Gate", 96, 40);
        world.Setup();
        gate.Triggered("open");
        Expect.True(gate.IsOpen);
        gate.Triggered("close");
        Expect.False(gate.IsOpen);
        Expect.Throws<ArgumentException>(() => gate.Triggered("explode"));
    }

    [Check]
    public void AnItemIsCollectedOnceAndRemembered()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        world.Assets.With("items/tank.json", """
            { "name": "missile tank", "expands": "missiles", "expands_by": 5, "message": "missile_tank" }
            """);
        world.Place(new Item { Name = "Tank", TypeFile = "items/tank.json", RememberedAs = "tank in open room" }, 100, 72);
        world.Setup();

        world.Step(60, GameAction.Right);
        Expect.Equal(5, world.Player.Inventory.Capacity("missiles"));
        Expect.Equal(5, world.Player.Inventory.Amount("missiles"));
        Expect.True(world.Session.Progress.Has("tank in open room"));
        Expect.Equal("missile_tank", world.Session.TakeMessage()?.Message ?? "");
    }

    [Check]
    public void AnItemAlreadyCollectedIsNotThere()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        world.Assets.With("items/tank.json", """{ "name": "missile tank", "expands": "missiles", "expands_by": 5 }""");
        world.Session.Progress.Record("tank in open room");
        world.Place(new Item { Name = "Tank", TypeFile = "items/tank.json", RememberedAs = "tank in open room" }, 100, 72);
        world.Step(1);
        Expect.Equal(0, world.Room.Objects.Count);
    }

    [Check]
    public void AnItemBehindABlockCannotBeTouchedUntilTheBlockIsGone()
    {
        using var world = new StandInWorld(
            "................", "................", "................", "................", "...S............", "################")
            .StartAt(40, 80);
        world.Assets.With("items/boots.json", """{ "name": "boots", "grants_equipment": "boots" }""");
        var item = world.Place(new Item { Name = "Boots", TypeFile = "items/boots.json", RememberedAs = "boots" }, 56, 72);
        world.Setup();
        Expect.True(item.IsCovered);

        world.Room.Terrain.TakeHit(new Hit(1, HitSide.Player, new HitKind("shot")), new PixelRect(48, 64, 16, 16), atOnce: true);
        Expect.False(item.IsCovered);
        world.Step(30, GameAction.Right);
        Expect.True(world.Player.Inventory.Owns("boots"));
    }

    [Check]
    public void ARefillStationHoldsThePlayerThenFillsWhatItLists()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(100, 80);
        var station = world.Place(new Station
        {
            Name = "Refill", Kind = Station.Does.Refills, Refills = [Inventory.Energy], Size = new Vector2I(32, 48), WorkUpdates = 10,
        }, 84, 32);
        world.Setup();
        world.Player.Inventory.Lose(Inventory.Energy, 50);

        world.Step(1);
        Expect.True(world.Player.State is UsingStationState);
        world.Step(5, GameAction.Right);
        Expect.Equal(Pixels.FromWhole(100), world.Player.Position.X);

        world.Step(10);
        Expect.Equal(99, world.Player.Inventory.Amount(Inventory.Energy));
        Expect.True(world.Player.State.InControl);

        // Full now, and still standing in it: it does not start again.
        world.Step(5);
        Expect.False(world.Player.State is UsingStationState);
    }

    [Check]
    public void ASaveStationAsksForASaveAtItself()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(100, 80);
        world.Place(new Station { Name = "Save", Kind = Station.Does.Saves, Size = new Vector2I(32, 48), WorkUpdates = 3 }, 84, 32);
        world.Step(6);
        (string Room, string EntryPoint)? request = world.Session.TakeSaveRequest();
        Expect.True(request is not null);
        Expect.Equal("Save", request?.EntryPoint ?? "");
    }

    [Check]
    public void ASpawnerKeepsUpToItsLimitAliveAndReplacesTheDead()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        var spawner = world.Place(new StandInSpawner { Name = "Pipe", IntervalUpdates = 5, MaxAlive = 2 }, 200, 40);
        world.Setup();

        world.Step(20);
        Expect.Equal(2, world.Room.Enemies.Count);
        Expect.Equal(2, spawner.AliveCount);

        world.Room.Enemies[0].Die();
        world.Step(10);
        Expect.Equal(2, world.Room.Enemies.Count);
    }

    [Check]
    public void AHazardHurtsThePlayerWhoTouchesIt()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        world.Place(new Hazard { Name = "Spikes", Size = new Vector2I(16, 16), HitDamage = 20 }, 70, 64);
        world.Setup();
        world.Step(30, GameAction.Right);
        Expect.Equal(79, world.Player.Inventory.Amount(Inventory.Energy));
    }

    [Check]
    public void ABossEncounterWakesTheBossAndRecordsItsDefeat()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        var boss = world.Place(new StandInEnemy { Name = "Boss" }, 220, 40);
        var door = PlaceBarrier(world, "Exit", 16, 40);
        door.AnyHit = 1;
        door.HeldShutBy = Condition.Of(ConditionKind.FactRecorded, "boss beaten");
        var encounter = world.Place(new BossEncounter
        {
            Name = "Fight", Boss = new NodePath("../Boss"), RememberedAs = "boss beaten", Size = new Vector2I(64, 96),
        }, 100, 0);
        world.Setup();

        world.Step(5);
        Expect.True(boss.IsAsleep);
        Expect.Equal(0, boss.UpdatesBehaved);
        Expect.True(door.IsSealed);

        world.Step(40, GameAction.Right);
        Expect.True(encounter.HasStarted);
        Expect.False(boss.IsAsleep);

        boss.Die();
        world.Step(2);
        Expect.True(encounter.IsFinished);
        Expect.True(world.Session.Progress.Has("boss beaten"));
        Expect.False(door.IsSealed);
    }

    /// <summary>Places a barrier 16 pixels wide and 40 high with its top-left at the given pixel.</summary>
    private static Barrier PlaceBarrier(StandInWorld world, string name, int x, int y)
    {
        var barrier = new Barrier { Name = name };
        StandInWorld.AddCollider(barrier, ColliderRole.Solid, new PixelRect(0, 0, 16, 40));
        StandInWorld.AddCollider(barrier, ColliderRole.Hurtbox, new PixelRect(0, 0, 16, 40));
        return world.Place(barrier, x, y);
    }
}
