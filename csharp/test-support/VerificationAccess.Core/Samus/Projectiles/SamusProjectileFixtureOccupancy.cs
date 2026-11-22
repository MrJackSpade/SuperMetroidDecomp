using System.Runtime.CompilerServices;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Fixture occupancy of Samus's shot and bomb places (#2558): a check that places a projectile of a type without firing it
/// gives it the damage a projectile of that type deals as it takes its place, as production's firing, release or placing
/// does, so no fixture invents a damage for a type.
/// </summary>
internal static class SamusProjectileFixtureOccupancy
{
    /// <summary>
    /// The damage a projectile of <paramref name="type"/> deals as it takes its place: an echo's, a beam shot's of its
    /// combination and charge, an explosion's, or a missile's, Super Missile's, bomb's or Power Bomb's.
    /// </summary>
    internal static int DamageAsPlaced(SamusProjectileType type)
    {
        if (type.Echo is { } echo)
            return SamusProjectileDamage.Read(SamusProjectileSelection.Echo(echo));
        if (type.Family == SamusProjectileFamily.Beam)
            return SamusProjectileDamage.Read(SamusProjectileSelection.BeamShot(type.BeamCombination, type.IsChargedBeam));
        if (type.Family is SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion)
            return SamusProjectileDamageDefinitions.ExplosionDamage;
        return SamusProjectileDamage.Read(NonBeam(type.Family));
    }

    /// <summary>The kind of a missile, Super Missile, bomb or Power Bomb of <paramref name="family"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The family is no missile or bomb family.</exception>
    internal static SamusProjectileKind NonBeam(SamusProjectileFamily family) =>
        family == SamusProjectileFamily.Missile ? SamusProjectileSelection.Missile(super: false)
        : family == SamusProjectileFamily.SuperMissile ? SamusProjectileSelection.Missile(super: true)
        : SamusProjectileSelection.Bomb(BombKindOf(family));

    /// <summary>The bomb of <paramref name="family"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The family is no bomb family.</exception>
    internal static SamusBombKind BombKindOf(SamusProjectileFamily family) =>
        family == SamusProjectileFamily.Bomb ? SamusBombKind.Bomb
        : family == SamusProjectileFamily.PowerBomb ? SamusBombKind.PowerBomb
        : throw new ArgumentOutOfRangeException(nameof(family), family, "The family is no bomb family.");

    /// <summary>
    /// A shot place, the first of the pool's, holding a projectile of <paramref name="type"/> at rest at the room's origin
    /// with the damage its type deals: the place a check that runs one projectile's own steps builds.
    /// </summary>
    /// <param name="type">What the projectile is.</param>
    internal static SamusProjectile ShotPlace(SamusProjectileType type)
    {
        var place = new SamusProjectile(0);
        _ = place.OccupyAsFired(type);
        return place;
    }

    /// <summary>Fixture occupancy of a shot place.</summary>
    extension(SamusProjectile shot)
    {
        /// <summary>Takes the place with a projectile of <paramref name="type"/> dealing the damage its type deals.</summary>
        /// <returns>The projectile now holding the place.</returns>
        internal SamusProjectileOccupant OccupyAsFired(SamusProjectileType type) => shot.Occupy(type, DamageAsPlaced(type));

        /// <summary>
        /// Takes the place with a projectile of <paramref name="type"/> dealing <paramref name="damage"/>, at rest at the
        /// room's origin, as a check's fixture places one.
        /// </summary>
        /// <returns>The projectile now holding the place.</returns>
        internal SamusProjectileOccupant Occupy(SamusProjectileType type, int damage) =>
            shot.Occupy(type, damage, FixedPosition<RoomPixels>.Zero);
    }

    /// <summary>Fixture occupancy of the shot pool.</summary>
    extension(SamusShotPool pool)
    {
        /// <summary>
        /// Takes <paramref name="place"/>, ending the shot there, with a projectile of <paramref name="type"/> dealing the
        /// damage its type deals, as firing does.
        /// </summary>
        /// <returns>The place the projectile now holds.</returns>
        internal SamusProjectile OccupyAsFired(int place, SamusProjectileType type) =>
            pool.Occupy(place, type, DamageAsPlaced(type)).Projectile;
    }

    /// <summary>Fixture occupancy of a bomb place.</summary>
    extension(SamusBomb bomb)
    {
        /// <summary>Takes the place with a bomb of <paramref name="type"/> dealing the damage its family's bomb definition gives.</summary>
        /// <returns>The bomb now holding the place.</returns>
        internal SamusBombOccupant OccupyAsPlaced(SamusProjectileType type) => bomb.Occupy(BombKindOf(type.Family), DamageAsPlaced(type));
    }

    /// <summary>The room with no terrain and the new Samus each bomb system's fixture passes run in, made once per system.</summary>
    private static readonly ConditionalWeakTable<SamusBombProjectileSystem, OpenRoomBombPass> OpenRoomPasses = [];

    /// <summary>Fixture runs of Samus's bombs.</summary>
    extension(SamusBombProjectileSystem bombs)
    {
        /// <summary>
        /// Runs the bombs' share of one projectile pass (<see cref="SamusBombProjectileSystem.HandleBombs"/>) in a room with
        /// no terrain, beside a new Samus, the bomb-jump overlap left to the pass's end: each placed bomb's fuse runs, and
        /// it plays the animation its update leaves. Every pass of one system runs in the same room beside the same Samus.
        /// </summary>
        internal void HandleBombsInOpenRoom()
        {
            OpenRoomBombPass pass = OpenRoomPasses.GetValue(bombs, _ => new OpenRoomBombPass());
            bombs.HandleBombs(pass.Terrain, pass.Samus, BombJumpOverlapPublication.Deferred);
        }
    }

    /// <summary>A room with no terrain and a new Samus, for a bomb system's fixture passes.</summary>
    private sealed class OpenRoomBombPass
    {
        /// <summary>A room of sixteen by sixteen blocks with no terrain or room objects.</summary>
        internal SamusTerrain Terrain { get; } =
            new RoomTerrain(SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(16, 16)), SamusRoomObjectFixtures.Uninstalled);

        /// <summary>Samus, new.</summary>
        internal SamusState Samus { get; } = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus();
    }
}
