using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
[RoomLifetime]
internal sealed class DraygonTurrets
{
    /// <summary>The loaded room's bindings: its game-state services, random source and the boss state that outlives the room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates Draygon's goop, eye drops, and other enemy projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The goop and turret shots, whose turret behaviour the turrets' shots are bound to.</summary>
    private readonly DraygonShots _shots;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonTurrets(EnemyRoomBinding room, EnemyProjectileEngine projectiles, DraygonShots shots)
    {
        _room = room;
        _projectiles = projectiles;
        _shots = shots;
    }

    /// <summary>Checks Draygon's frame cadence for turret and eye direction updates.</summary>
    internal void ObserveDraygonTurretCadence(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounter)
    {
        if (!DraygonCadenceDefinitions.TurretTurnEverySixtyFourthUpdate.IsDue(frameCounter))
            return;

        // The turret cadence tests the frame counter, then advances the room RNG even when the
        // chosen turret is disabled; the spawn itself is SpawnDraygonWallTurret.
        int random = _room.Random.Generate();
        if (samus is not null)
            SpawnDraygonWallTurret(state, samus, random);
    }

    /// <summary>Fires a wall turret and runs the wall turret projectile's initializer.</summary>
    internal void SpawnDraygonWallTurret(
        DraygonEnemyState state,
        SamusState samus,
        int random)
    {
        if (DraygonCannonData.FiringOrder[DraygonRandomDefinitions.TurretChoice.Of(random)] is not { } cannon)
            return;
        DraygonCannonTarget target = DraygonCannonData.FiringTarget(cannon);
        if (state.DisabledCannons.Contains(target.Cannon))
            return;

        EnemyProjectile? turret = _projectiles.AllocateEnemyProjectile();
        if (turret is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            turret,
            RoomEnemyProjectileKind.DraygonWallTurret,
            _shots.WallTurretShot,
            graphics: SpriteGraphics.SharedIn(EnemyShades.InProjectileLook));
        turret.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileSegment.WallTurretBloom));
        turret.Position = FixedPosition<RoomPixels>.FromWhole(target.Origin);

        // The aim negates the common zero-up angle, then adds a quarter turn (64), keeping the angle within
        // one turn. The speed conversion stores unsigned magnitudes; the shot's frame behaviour applies
        // signs from this retained angle on every frame.
        TrigonometryAngle flightAngle = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, turret.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, turret.Position.Y.Whole))).ToTrigonometryAngle();
        DraygonParts.SetDraygonProjectileVelocity(turret, flightAngle, DraygonProjectileSpeeds.WallTurret);

        // The definition names the flight step, but init replaces it with an empty step until the 84-frame muzzle
        // bloom reaches its enable step and explicitly enables flight.
        turret.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
    }
}
