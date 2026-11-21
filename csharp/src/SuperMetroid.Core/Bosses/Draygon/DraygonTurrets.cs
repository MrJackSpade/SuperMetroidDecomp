using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
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

    /// <summary>
    /// The room's wall cannons whose room objects were destroyed or set up destroyed (#2177): their turrets no longer fire.
    /// The cannons belong to the room, which keeps them whether or not Draygon is alive in it.
    /// </summary>
    private readonly HashSet<DraygonWallCannon> _disabledCannons = [];

    /// <summary>Stops the turret of <paramref name="cannon"/> firing, as its wall cannon is destroyed.</summary>
    internal void DisableCannon(DraygonWallCannon cannon) => _disabledCannons.Add(cannon);

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonTurrets(EnemyRoomBinding room, EnemyProjectileEngine projectiles, DraygonShots shots)
    {
        _room = room;
        _projectiles = projectiles;
        _shots = shots;
    }

    /// <summary>Fires a wall turret on the turrets' cadence, drawing a random number each time.</summary>
    internal void ObserveDraygonTurretCadence(
        SamusState samus,
        int frameCounter)
    {
        if (!DraygonCadenceDefinitions.TurretTurn.IsDue(frameCounter))
            return;

        // The turret cadence tests the frame counter, then advances the room RNG even when the
        // chosen turret is disabled; the spawn itself is SpawnDraygonWallTurret.
        int random = _room.Random.Generate();
        SpawnDraygonWallTurret(samus, random);
    }

    /// <summary>Fires a wall turret and runs the wall turret projectile's setup.</summary>
    private void SpawnDraygonWallTurret(
        SamusState samus,
        int random)
    {
        if (DraygonCannonData.FiringCannon.Choose(random) is not DraygonTurretTurn.FiresFrom { Cannon: var cannon } ||
            _disabledCannons.Contains(cannon))
            return;

        EnemyProjectile? turret = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.DraygonWallTurret, _shots.WallTurretShot, graphics: SpriteGraphics.SharedIn(EnemyShades.InProjectileLook), new DraygonProjectileAnimation(_shots.WallTurretShot, DraygonProjectileSegment.WallTurretBloom));
        if (turret is null)
            return;

        turret.Position = FixedPosition<RoomPixels>.FromWhole(DraygonCannonData.FiringOrigins[cannon]);

        // The shot flies along the direction from the turret to Samus at the wall turret's speed, its direction on
        // each axis taken from that direction every update.
        TrigonometryAngle flightAngle = EnemyMath.CalculateAimAngle(
            new(samus.Position.X.Whole - turret.Position.X.Whole, samus.Position.Y.Whole - turret.Position.Y.Whole));
        DraygonParts.SetDraygonProjectileVelocity(_shots.WallTurretShot, turret, flightAngle, DraygonProjectileSpeeds.WallTurret);

        // The shot holds still until its muzzle bloom reaches the step that sets it flying.
    }
}
