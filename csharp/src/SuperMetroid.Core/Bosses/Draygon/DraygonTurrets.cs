using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
[RoomLifetime]
internal sealed class DraygonTurrets
{
    /// <summary>Provides the active room bus, random source, and persistent boss state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates Draygon's goop, eye drops, and other enemy projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonTurrets(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>Checks Draygon's frame cadence for turret and eye direction updates.</summary>
    internal void ObserveDraygonTurretCadence(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounterLow)
    {
        if ((frameCounterLow & 63) != 0)
            return;

        // $A5:87AA tests NMI_FrameCounter ($05B6), then advances the room RNG even when the
        // chosen turret is disabled; the spawn itself is SpawnDraygonWallTurret.
        int random = _room.Random.Generate();
        state.TurretCadenceChecks++;
        if (samus is not null)
            SpawnDraygonWallTurret(state, samus, random);
    }

    /// <summary>Ports <c>HandleFiringWallTurret</c> and initializer <c>$86:8D40</c>.</summary>
    internal void SpawnDraygonWallTurret(
        DraygonEnemyState state,
        SamusState samus,
        int random)
    {
        DraygonCannonTarget target = DraygonCannonData.FiringTarget(
            DraygonCannonData.FiringOrder[random & 3]);
        if (state.DisabledCannonWords.Contains(target.DisabledWord))
            return;

        RoomEnemyProjectileSlot? turret = _projectiles.AllocateEnemyProjectile();
        if (turret is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            turret,
            RoomEnemyProjectileKind.DraygonWallTurret,
            graphics: SpriteGraphics.CharacterZeroIn(SharedSpriteColouring.EnemyProjectiles));
        turret.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileProgram.WallTurretBloom));
        turret.Position = FixedPosition<RoomPixels>.FromWhole(target.Origin);

        // PlaceAndAim negates the common zero-up angle, then adds $40 before retaining the
        // low byte. ConvertAngleToXy stores unsigned magnitudes; the pre-instruction applies
        // signs from this retained angle on every frame.
        ByteAngle flightAngle = EnemyMath.CalculateBearing(
            new(unchecked((short)(samus.Position.X.Whole - turret.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - turret.Position.Y.Whole)))).ToTableAngle();
        DraygonParts.SetDraygonProjectileVelocity(turret, flightAngle, DraygonProjectileSpeeds.WallTurret);

        // The definition says $8DFF, but init replaces it with RTS until the 84-frame muzzle
        // bloom reaches instruction $8CF6 and explicitly enables flight.
        turret.PreInstruction = EnemyProjectilePreInstruction.RTS_868D54;
        state.WallTurretsSpawned++;
    }
}
