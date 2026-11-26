using Engine.Actors;
using Engine.Collision;
using Engine.Input;
using Engine.Shared;
using Engine.Tests.World;
using Engine.World;

namespace Engine.Tests.Actors;

/// <summary>Checks of hits landing: enemies and their reactions, projectiles and their settings, weapons, drops and body powers.</summary>
public sealed class CombatChecks
{
    private static readonly string[] OpenRoom =
    [
        "................", "................", "................", "................", "................", "################",
    ];

    private static readonly HitKind Beam = new("beam");
    private static readonly HitKind Ice = new("ice");
    private static readonly HitKind Plasma = new("plasma");

    private const string ToughEnemy = """
        {
          "health": 100, "contact_damage": 10,
          "reactions": [
            { "kinds": ["beam"], "damage": "half" },
            { "kinds": ["beam", "plasma"], "damage": "double", "unhittable_updates": 5 },
            { "kinds": ["ice"], "damage": "full", "status": "frozen", "status_when": "instead_of_dying", "status_updates": 50 }
          ]
        }
        """;

    [Check]
    public void TheReactionNamingTheMostOfAHitsKindsApplies()
    {
        using var world = WorldWithEnemy(out StandInEnemy enemy);
        Collider part = EntityNode.ColliderOf(enemy, ColliderRole.Hurtbox) ?? throw new CheckFailedException("no hurtbox");

        Expect.Equal(HitResult.Hurt, enemy.TakeHit(new Hit(20, HitSide.Player, Beam), part));
        Expect.Equal(90, enemy.Health.Current);

        Expect.Equal(HitResult.Hurt, enemy.TakeHit(new Hit(20, HitSide.Player, Beam, Plasma), part));
        Expect.Equal(50, enemy.Health.Current);

        // The plasma reaction leaves it briefly unhittable.
        Expect.Equal(HitResult.Absorbed, enemy.TakeHit(new Hit(20, HitSide.Player, Beam), part));
        world.Step(6);
        Expect.Equal(HitResult.Hurt, enemy.TakeHit(new Hit(20, HitSide.Player, Beam), part));
        Expect.Equal(40, enemy.Health.Current);
    }

    [Check]
    public void AHitWithNoMatchingReactionDoesItsFullDamage()
    {
        using var world = WorldWithEnemy(out StandInEnemy enemy);
        Collider part = EntityNode.ColliderOf(enemy, ColliderRole.Hurtbox) ?? throw new CheckFailedException("no hurtbox");
        enemy.TakeHit(new Hit(30, HitSide.Player, new HitKind("bomb")), part);
        Expect.Equal(70, enemy.Health.Current);
    }

    [Check]
    public void AStatusInsteadOfDyingFreezesTheEnemyAndLeavesItAlive()
    {
        using var world = WorldWithEnemy(out StandInEnemy enemy);
        Collider part = EntityNode.ColliderOf(enemy, ColliderRole.Hurtbox) ?? throw new CheckFailedException("no hurtbox");
        Collider solid = EntityNode.ColliderOf(enemy, ColliderRole.Solid) ?? throw new CheckFailedException("no solid");

        // A hit that would not kill does plain damage and no status.
        enemy.TakeHit(new Hit(40, HitSide.Player, Ice), part);
        Expect.Equal(60, enemy.Health.Current);
        Expect.False(enemy.IsFrozen);

        // A hit that would kill freezes instead.
        Expect.Equal(HitResult.Hurt, enemy.TakeHit(new Hit(500, HitSide.Player, Ice), part));
        Expect.True(enemy.IsFrozen);
        Expect.False(enemy.IsDead);
        Expect.Equal(60, enemy.Health.Current);
        Expect.True(solid.Enabled);

        int before = enemy.UpdatesBehaved;
        world.Step(10);
        Expect.Equal(before, enemy.UpdatesBehaved);

        world.Step(45);
        Expect.False(enemy.IsFrozen);
        Expect.False(solid.Enabled);
    }

    [Check]
    public void AFrozenEnemyCanBeStoodOn()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(120, 40);
        world.Assets.With("enemy.json", ToughEnemy);
        var enemy = world.Place(new StandInEnemy { Name = "Enemy", DataFile = "enemy.json" }, 120, 72);
        world.Setup();
        enemy.BeginStatus(Enemy.Frozen, 500);

        world.StepUntilLanded();

        // The enemy's top is at pixel 64.
        Expect.Equal(Pixels.FromWhole(64), world.Player.Position.Y);
        Expect.Equal(99, world.Player.Inventory.Amount(Inventory.Energy));
    }

    [Check]
    public void AnEnemyHurtsThePlayerItTouches()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(100, 80);
        world.Assets.With("enemy.json", ToughEnemy);
        world.Place(new StandInEnemy { Name = "Enemy", DataFile = "enemy.json" }, 104, 72);
        world.Step(1);
        Expect.Equal(89, world.Player.Inventory.Amount(Inventory.Energy));
        Expect.True(world.Player.State is KnockedBackState);
    }

    [Check]
    public void ADestroyedEnemyLeavesTheRoomAndIsRecordedAsGone()
    {
        using var world = WorldWithEnemy(out StandInEnemy enemy);
        Collider part = EntityNode.ColliderOf(enemy, ColliderRole.Hurtbox) ?? throw new CheckFailedException("no hurtbox");
        Expect.True(world.Room.HasLiveEnemies);

        Expect.Equal(HitResult.Destroyed, enemy.TakeHit(new Hit(500, HitSide.Player, new HitKind("bomb")), part));
        Expect.False(world.Room.HasLiveEnemies);
        Expect.True(world.Room.IsGone("Enemy"));

        world.Step(1);
        Expect.Equal(0, world.Room.Enemies.Count);
    }

    [Check]
    public void ADropFollowsWhatThePlayerNeeds()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80);
        world.Assets
            .With("pickups/energy.json", """{ "name": "energy", "adds_to": "energy", "quantity": 5, "lasts_updates": 100 }""")
            .With("pickups/missile.json", """{ "name": "missile", "adds_to": "missiles", "quantity": 2, "lasts_updates": 100 }""")
            .With("enemy.json", """
                { "health": 1, "contact_damage": 0,
                  "drops": { "skip_useless": true,
                             "drops": [ { "pickup": "pickups/energy.json", "chance": 1 }, { "pickup": "pickups/missile.json", "chance": 1 } ] } }
                """);
        var first = world.Place(new StandInEnemy { Name = "First", DataFile = "enemy.json" }, 200, 72);
        var second = world.Place(new StandInEnemy { Name = "Second", DataFile = "enemy.json" }, 220, 72);
        world.Setup();

        // Full of energy and unable to hold missiles, the player has no use for either: nothing drops.
        first.Die();
        world.Step(1);
        Expect.Equal(0, world.Room.Pickups.Count);

        // Able to hold missiles, a missile is the only useful drop.
        world.Player.Inventory.SetCapacity("missiles", 10);
        second.Die();
        world.Step(1);
        Expect.Equal(1, world.Room.Pickups.Count);
        Expect.Equal("missiles", world.Room.Pickups[0].Type.AddsTo);
    }

    [Check]
    public void APickupIsCollectedOnTouchAndOtherwiseRunsOut()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80).Setup();
        world.Assets.With("pickups/missile.json", """{ "name": "missile", "adds_to": "missiles", "quantity": 2, "lasts_updates": 5 }""");
        world.Player.Inventory.SetCapacity("missiles", 10);
        PickupType type = PickupType.Read(world.Assets.Library.ReadData("pickups/missile.json"));

        world.Room.Spawn(Pickup.Of(type, PixelPosition.FromWhole(42, 70)));
        world.Step(1);
        Expect.Equal(2, world.Player.Inventory.Amount("missiles"));
        world.Step(1);
        Expect.Equal(0, world.Room.Pickups.Count);

        world.Room.Spawn(Pickup.Of(type, PixelPosition.FromWhole(200, 70)));
        world.Step(7);
        Expect.Equal(0, world.Room.Pickups.Count);
        Expect.Equal(2, world.Player.Inventory.Amount("missiles"));
    }

    [Check]
    public void AShotTravelsHurtsTheEnemyItMeetsAndEnds()
    {
        using var world = WorldWithEnemy(out StandInEnemy enemy);
        var shot = Projectile.Of(
            new Hit(30, HitSide.Player, new HitKind("bomb")), PixelPosition.FromWhole(60, 72),
            new PixelVelocity(Pixels.FromWhole(4), Pixels.Zero), new ProjectileSettings(), world.Player);
        world.Room.Spawn(shot);

        world.Step(40);

        Expect.Equal(70, enemy.Health.Current);
        Expect.True(shot.HasEnded);
        Expect.Equal(0, world.Room.Projectiles.Count);
    }

    [Check]
    public void APiercingShotGoesOnThroughAndHurtsEachEnemyOnce()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(24, 80);
        world.Assets.With("enemy.json", ToughEnemy);
        var first = world.Place(new StandInEnemy { Name = "First", DataFile = "enemy.json" }, 120, 72);
        var second = world.Place(new StandInEnemy { Name = "Second", DataFile = "enemy.json" }, 180, 72);
        world.Setup();
        var settings = new ProjectileSettings { MeetingTarget = ProjectileSettings.Target.PassesThrough };
        world.Room.Spawn(Projectile.Of(
            new Hit(30, HitSide.Player, new HitKind("bomb")), PixelPosition.FromWhole(60, 72),
            new PixelVelocity(Pixels.FromWhole(2), Pixels.Zero), settings, world.Player));

        world.Step(80);

        Expect.Equal(70, first.Health.Current);
        Expect.Equal(70, second.Health.Current);
    }

    [Check]
    public void AShotMeetingAWallEndsOrPassesThroughAsItsSettingsSay()
    {
        string[] walled = ["................", "................", "................", "........#.......", "........#.......", "################"];

        using (var stops = new StandInWorld(walled).StartAt(24, 80).Setup())
        {
            var shot = Projectile.Of(new Hit(1, HitSide.Player, Beam), PixelPosition.FromWhole(60, 72),
                new PixelVelocity(Pixels.FromWhole(4), Pixels.Zero), new ProjectileSettings(), stops.Player);
            stops.Room.Spawn(shot);
            stops.Step(30);
            Expect.True(shot.HasEnded);
            Expect.True(shot.Position.X < Pixels.FromWhole(128));
        }

        using var passes = new StandInWorld(walled).StartAt(24, 80).Setup();
        var through = Projectile.Of(new Hit(1, HitSide.Player, Beam), PixelPosition.FromWhole(60, 72),
            new PixelVelocity(Pixels.FromWhole(4), Pixels.Zero),
            new ProjectileSettings { MeetingTerrain = ProjectileSettings.Terrain.PassesThrough, ViewMargin = -1 }, passes.Player);
        passes.Room.Spawn(through);
        passes.Step(30);
        Expect.False(through.HasEnded);
        Expect.True(through.Position.X > Pixels.FromWhole(150));
    }

    [Check]
    public void AShotThatHitsTerrainBreaksTheBlockItMeets()
    {
        using var world = new StandInWorld(
            "................", "................", "................", "................", "........S.......", "################")
            .StartAt(24, 80).Setup();
        world.Room.Spawn(Projectile.Of(new Hit(1, HitSide.Player, new HitKind("shot")), PixelPosition.FromWhole(60, 72),
            new PixelVelocity(Pixels.FromWhole(4), Pixels.Zero),
            new ProjectileSettings { MeetingTerrain = ProjectileSettings.Terrain.EndsAndHits }, world.Player));
        world.Step(30);
        Expect.True(world.Room.Terrain.Blocks[0].Broken);
    }

    [Check]
    public void AShotLastsOnlyAsLongAsItsSettingsSay()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(24, 80).Setup();
        var shot = Projectile.Of(new Hit(1, HitSide.Player, Beam), PixelPosition.FromWhole(60, 40),
            new PixelVelocity(Pixels.FromParts(0, 1000), Pixels.Zero), new ProjectileSettings { LastsUpdates = 5 }, world.Player);
        world.Room.Spawn(shot);
        world.Step(4);
        Expect.False(shot.HasEnded);
        world.Step(2);
        Expect.True(shot.HasEnded);
    }

    [Check]
    public void AWeaponFiresOnAPressWithinItsCooldownLimitAndCost()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(24, 80).Setup();
        world.Assets.With("weapons/launcher.json", """
            { "name": "launcher", "damage": 30, "kinds": ["missile"], "speed": 3, "cooldown": 10, "max_live": 2,
              "costs": "missiles", "cost": 1,
              "projectile": { "width": 8, "height": 8, "view_margin": -1, "meeting_terrain": "passes_through" } }
            """);
        world.Player.Weapons.Add(WeaponStats.Read(world.Assets.Library.ReadData("weapons/launcher.json")));
        world.Player.Inventory.SetCapacity("missiles", 5);
        world.Player.Inventory.Add("missiles", 5);

        world.Press(GameAction.Attack);
        Expect.Equal(1, world.Room.Projectiles.Count);
        Expect.Equal(4, world.Player.Inventory.Amount("missiles"));

        // Still cooling down: a second press does nothing.
        world.Step(1);
        world.Press(GameAction.Attack);
        Expect.Equal(1, world.Room.Projectiles.Count);

        world.Step(10);
        world.Press(GameAction.Attack);
        Expect.Equal(2, world.Room.Projectiles.Count);

        // Two live is the limit.
        world.Step(11);
        world.Press(GameAction.Attack);
        Expect.Equal(2, world.Room.Projectiles.Count);
        Expect.Equal(3, world.Player.Inventory.Amount("missiles"));
    }

    [Check]
    public void SwitchedOnEquipmentAddsItsPropertiesToAShot()
    {
        using var assets = new Content.TempAssets().With("beam.json", """
            { "name": "beam", "damage": 20, "kinds": ["beam"], "speed": 4,
              "charge_updates": 60, "charged_damage_times": 3, "charged_kind": "charged",
              "properties": [ { "equipment": "ice beam", "kinds": ["ice"], "damage": 10 },
                              { "equipment": "plasma beam", "kinds": ["plasma"], "damage": 100 } ] }
            """);
        WeaponStats beam = WeaponStats.Read(assets.Library.ReadData("beam.json"));
        var inventory = new Inventory();
        inventory.Grant("ice beam");

        Hit plain = beam.HitFor(inventory, charged: false);
        Expect.Equal(30, plain.Damage);
        Expect.True(plain.Carries(Beam) && plain.Carries(Ice) && !plain.Carries(Plasma));

        Hit charged = beam.HitFor(inventory, charged: true);
        Expect.Equal(90, charged.Damage);
        Expect.True(charged.Carries(new HitKind("charged")));
    }

    [Check]
    public void AnActiveBodyPowerHitsWhatThePlayersBodyTouches()
    {
        using var world = new StandInWorld(
            "................", "................", "................", "................", "...S............", "################")
            .StartAt(40, 80);
        world.Setup();
        var power = new BodyPower("ram", 50, new HitKind("shot"));
        world.Player.AddBodyPower(power);

        // The block's left face is at pixel 48; the player walks into it.
        world.Step(20, GameAction.Right);
        Expect.False(world.Room.Terrain.Blocks[0].Broken);

        power.IsActive = true;
        world.Step(2, GameAction.Right);
        Expect.True(world.Room.Terrain.Blocks[0].Broken);
    }

    /// <summary>A world with one tough stand-in enemy named "Enemy", well clear of the player.</summary>
    private static StandInWorld WorldWithEnemy(out StandInEnemy enemy)
    {
        var world = new StandInWorld(OpenRoom).StartAt(24, 80);
        world.Assets.With("enemy.json", ToughEnemy);
        enemy = world.Place(new StandInEnemy { Name = "Enemy", DataFile = "enemy.json" }, 120, 72);
        world.Setup();
        return world;
    }
}
