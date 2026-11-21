using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile hits on enemies: marking a shot's hit, reflection, the intro Mother Brain missile, and the start
/// of the impact explosion.</summary>
[RoomLifetime]
internal sealed class ProjectileEnemyImpacts
{
    /// <summary>Owns projectile clearing when reflection retires a Super Missile link.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Receives the Super Missile enemy-hit quake request.</summary>
    private readonly ProjectileEarthquakes _earthquakes;
    /// <summary>The shots in the pool, whose direction, damage, and animation change on enemy contact.</summary>
    private readonly SamusShotPool _pool;
    /// <summary>Converts missile hits to the same impact state as terrain collision.</summary>
    private readonly ProjectileTerrainCollision _terrainCollision;
    /// <summary>The speeds a reflected Spazer and Plasma beam relaunches at.</summary>
    private readonly UnauthoredBeamFiring<SamusBeamCombination> _unauthoredFiring;

    /// <summary>Binds enemy-impact handling to the shot pool and its animation, quake, and terrain services.</summary>
    internal ProjectileEnemyImpacts(ProjectileAnimation animation, ProjectileEarthquakes earthquakes, SamusShotPool pool,
        ProjectileTerrainCollision terrainCollision, UnauthoredBeamFiring<SamusBeamCombination> unauthoredFiring)
    {
        _animation = animation;
        _earthquakes = earthquakes;
        _pool = pool;
        _terrainCollision = terrainCollision;
        _unauthoredFiring = Ensure.NotNull(unauthoredFiring);
    }
    /// <summary>
    /// Applies the projectile-owned side effects that enemy collision performs before any
    /// enemy reaction which returns without entering ordinary impact conversion. This is
    /// shared by extended hitboxes and radius-based custom reactions such as shutters.
    /// </summary>
    public void TouchEnemyWithoutImpact(SamusProjectileOccupant held, bool markCollisionState)
    {
        SamusProjectile shot = Ensure.NotNull(held).Projectile;
        if (!held.HasEnemyCollisionPayload)
            return;

        if (held.Type.Family == SamusProjectileFamily.SuperMissile)
        {
            // The collisions start the enemy-hit quake before running the enemy's
            // shot reaction. This remains observable even when gold Ninja armor ignores a
            // fresh, not-yet-linked Super Missile.
            _earthquakes.RequestSuperMissileEarthquake(SuperMissileDefinitions.EnemyHitEarthquake);
        }

        if (markCollisionState)
            held.Spend();
    }

    /// <summary>
    /// Reflects the projectile after an enemy reaction has replaced the projectile direction. Reflection keeps the
    /// projectile family, type and world position, and sets up its damage, direction-specific animation, radii and the
    /// family's behaviour afresh.
    /// </summary>
    public void ReflectFromEnemy(SamusState samus, SamusProjectileOccupant held)
    {
        Ensure.NotNull(samus);
        SamusProjectile shot = Ensure.NotNull(held).Projectile;
        if (!held.HasEnemyCollisionPayload)
            return;

        SamusProjectileFamily family = held.Type.Family;
        if (family == SamusProjectileFamily.SuperMissile)
        {
            // Reflection deletes the projectile in the place the Super Missile names as its link
            // (UnlinkedSuperMissileLink) before repurposing the owner; it does not broadly delete
            // unrelated Super Missiles.
            if (shot.KindStateAs<SuperMissileShotState>().LinkChainPosition is { } linkPlace && linkPlace != shot.Place &&
                _pool.Shots[linkPlace].IsActive)
                _animation.ClearProjectile(_pool.Shots[linkPlace]);
        }

        if (family == SamusProjectileFamily.Beam)
        {
            // Beam reflection alone reinitializes velocity immediately. Missile families
            // retain their old velocity until they re-ignite on their next update, at
            // which point the new direction takes effect.
            BeamFiring.InitializePowerBeamVelocity(samus.ProjectileInheritance, held, _unauthoredFiring);
        }

        SamusProjectileKind kind;
        SamusProjectileSegment? segment;
        if (family == SamusProjectileFamily.Beam)
        {
            kind = SamusProjectileSelection.BeamShot(held.Type.BeamCombination, held.Type.IsChargedBeam);
            segment = SamusProjectileSelection.BeamShotAnimation(held.Type.BeamCombination, held.Type.IsChargedBeam, shot.Direction);
        }
        else if (family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
        {
            kind = SamusProjectileSelection.NonBeam(family);
            segment = SamusProjectileKinds.Animation(kind, shot.Direction);
        }
        else
        {
            // The reflection producer only selects beams and the two missile
            // families. Reaching this method with a bomb/explosion payload is corrupt
            // scheduler state rather than an unimplemented reflection behavior.
            throw new InvalidDataException(
                $"Projectile family {family} is not reflectable.");
        }

        held.Damage = SamusProjectileDamage.Read(kind);
        held.Animation = SamusProjectilePlayback.Start(segment);
        held.Radius = SamusProjectilePlayback.InitialRadius(segment);

        if (family == SamusProjectileFamily.Missile)
            held.Install(SamusShotBehaviors.Missile, new MissileShotState(new SamusMissileIgnition.PendingAfterReflection()));
        else if (family == SamusProjectileFamily.SuperMissile)
        {
            held.Install(SamusShotBehaviors.SuperMissile,
                new SuperMissileShotState(new SamusMissileIgnition.PendingAfterReflection(), linkPlace: null));
        }
        else
        {
            held.Install(!held.Type.BeamCombination.HasWave
                ? SamusShotBehaviors.Beam
                : SamusShotBehaviors.WaveBeamTrail, kindState: null);
        }
    }

    /// <summary>
    /// Applies intro Mother Brain's external projectile test.
    /// </summary>
    /// <remarks>
    /// The first missile in update order explodes once it crosses <paramref name="impactLine"/>, flying left; the
    /// cinematic sets the line and the projectile system explodes the missile as a wall does.
    /// </remarks>
    public bool TryImpactIntroMotherBrainMissile(
        SamusBombProjectileSystem samusBombs,
        int impactLine)
    {
        Ensure.NotNull(samusBombs);

        foreach (SamusProjectile shot in _pool.InUpdateOrder)
        {
            if (shot.Occupant is not { } held || !held.Type.IsFamily(SamusProjectileFamily.Missile))
                continue;

            // The missile flies left from Samus toward the brain and is killed on crossing the line, so its position never
            // leaves the room's first screens.
            if (shot.Position.X.Whole >= impactLine)
                return false;

            _terrainCollision.KillMissile(held, samusBombs);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Converts one live beam or missile into its ordinary impact animation after
    /// an enemy-owned overlap test accepts it. The enemy's shot reaction deals the damage; this
    /// projectile owner alone changes the shot and the shared cooldown.
    /// </summary>
    public bool TryStartEnemyImpact(
        SamusBombProjectileSystem samusBombs,
        SamusProjectileOccupant held,
        bool blocksPlasmaBeam = false)
    {
        Ensure.NotNull(samusBombs);
        SamusProjectile shot = Ensure.NotNull(held).Projectile;
        if (!held.HasEnemyCollisionPayload)
            return false;

        if (shot.Behavior is { EndsItselfAfterEnemyHit: true })
        {
            // Enemy collision only marks the direction. These particles own
            // hit deletion (and Spazer's paired deletion) in their next update;
            // replacing that response with a beam explosion loses their family lifecycle.
            // Plasma normally pierces, unless the target explicitly stops Plasma beams.
            TouchEnemyWithoutImpact(held,
                blocksPlasmaBeam || !held.Type.BeamCombination.HasPlasma);
            return true;
        }

        if (shot.IsSpent)
            return false;

        switch (held.Type.Family)
        {
            case SamusProjectileFamily.Beam:
                // Enemy collision marks the shot as having hit only when the
                // target blocks Plasma or the shot lacks Plasma. Ordinary penetrating
                // beams, not just special beam-combo particles, retain their lifecycle.
                if (!blocksPlasmaBeam &&
                    held.Type.BeamCombination.HasPlasma)
                    return true;
                // Enemy collision only marks the shot; enemy hits never reach the projectile kill's sound.
                ProjectileTerrainCollision.KillBeam(held);
                return true;
            case SamusProjectileFamily.Missile:
            case SamusProjectileFamily.SuperMissile:
                _terrainCollision.KillMissile(held, samusBombs);
                return true;
            case SamusProjectileFamily.PowerBomb:
            case SamusProjectileFamily.Bomb:
            case SamusProjectileFamily.BeamExplosion:
            case SamusProjectileFamily.MissileExplosion:
                // Existing explosion, bomb, and unknown family shots cannot repeatedly
                // increment an enemy hit counter merely because their art still overlaps.
                return false;
            default:
                throw new InvalidOperationException($"Undefined SamusProjectileFamily {held.Type.Family}.");
        }
    }
}
