using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile hits on enemies: the collision prelude, reflection, the intro Mother Brain missile, and the start of the impact explosion.</summary>
[RoomLifetime]
internal sealed class ProjectileEnemyImpacts
{
    /// <summary>Owns projectile clearing when reflection retires a Super Missile link.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Receives the Super Missile enemy-hit quake request.</summary>
    private readonly ProjectileEarthquakes _earthquakes;
    /// <summary>Projectile records whose direction, damage, and animation change on enemy contact.</summary>
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
    public void ApplyEnemyCollisionPrelude(SamusProjectile projectile, bool markCollisionState)
    {
        SamusProjectile shot = Ensure.NotNull(projectile);
        if (!shot.HasEnemyCollisionPayload)
            return;

        if (shot.Type.Family == SamusProjectileFamily.SuperMissile)
        {
            // The collisions start the enemy-hit quake before running the enemy's
            // shot reaction. This remains observable even when gold Ninja armor ignores a
            // fresh, not-yet-linked Super Missile.
            _earthquakes.RequestSuperMissileEarthquake(SuperMissileDefinitions.EnemyHitEarthquake);
        }

        if (markCollisionState)
            shot.IsSpent = true;
    }

    /// <summary>
    /// The name for impacts whose geometry is specifically an extended
    /// frame hitbox. The behavior is the same projectile prelude, not a separate fix.
    /// </summary>
    public void ApplyFrameHitboxCollisionPrelude(SamusProjectile projectile, bool markCollisionState) =>
        ApplyEnemyCollisionPrelude(projectile, markCollisionState);

    /// <summary>
    /// Reflects the projectile after an enemy reaction has replaced
    /// the projectile direction. Reflection preserves the projectile family/type and world
    /// position, but reconstructs its damage, direction-specific animation, radii,
    /// and the family's behaviour exactly as the game does.
    /// </summary>
    public void ReflectFromEnemy(SamusState samus, SamusProjectile projectile)
    {
        Ensure.NotNull(samus);
        SamusProjectile shot = Ensure.NotNull(projectile);
        if (!shot.HasEnemyCollisionPayload)
            return;

        SamusProjectileFamily family = shot.Type.Family;
        if (family == SamusProjectileFamily.SuperMissile)
        {
            // Reflection deletes the projectile in the place the Super Missile names as its link
            // (UnlinkedSuperMissileLink) before repurposing the owner; it does not broadly delete
            // unrelated Super Missiles.
            if (shot.SuperMissileLinkPlace is { } linkPlace && linkPlace != shot.Place && _pool.Shots[linkPlace].IsActive)
                _animation.ClearProjectile(_pool.Shots[linkPlace]);
        }

        if (family == SamusProjectileFamily.Beam)
        {
            // Beam reflection alone reinitializes velocity immediately. Missile families
            // retain their old velocity until they re-ignite on their next update, at
            // which point the new direction takes effect.
            BeamFiring.InitializePowerBeamVelocity(samus.ProjectileInheritance, shot, _unauthoredFiring);
        }

        SamusProjectileKind kind;
        SamusProjectileSegment? segment;
        if (family == SamusProjectileFamily.Beam)
        {
            kind = SamusProjectileSelection.BeamShot(shot.Type.BeamCombination, shot.Type.IsChargedBeam);
            segment = SamusProjectileSelection.BeamShotAnimation(shot.Type.BeamCombination, shot.Type.IsChargedBeam, shot.Direction);
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

        shot.Damage = SamusProjectileDamage.Read(kind);
        shot.Playback = SamusProjectilePlayback.Start(segment);
        shot.Radius = SamusProjectilePlayback.InitialRadius(segment);
        shot.FrameHold = WrappingCountdown.Start(1);

        if (family == SamusProjectileFamily.Missile)
        {
            shot.Behavior = SamusShotBehaviors.Missile;
            shot.MissileIgnition = SamusMissileIgnition.PendingAfterReflection;
            shot.SuperMissileLinkPlace = null;
        }
        else if (family == SamusProjectileFamily.SuperMissile)
        {
            shot.Behavior = SamusShotBehaviors.SuperMissile;
            shot.MissileIgnition = SamusMissileIgnition.PendingAfterReflection;
            shot.SuperMissileLinkPlace = null;
        }
        else
        {
            shot.Behavior = !shot.Type.BeamCombination.HasWave
                ? SamusShotBehaviors.Beam
                : SamusShotBehaviors.WaveBeamFourUpdateTrail;
        }
    }

    /// <summary>
    /// Applies intro Mother Brain's external projectile test.
    /// </summary>
    /// <remarks>
    /// The cinematic scans the shots from the highest place down for the first ordinary missile
    /// and only kills it once its signed X coordinate
    /// is below 84. This is not terrain collision, so the actor owns the predicate while
    /// the projectile system owns the exact impact conversion.
    /// </remarks>
    public bool TryImpactIntroMotherBrainMissile(
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(sharedProjectiles);

        foreach (SamusProjectile shot in _pool.InUpdateOrder)
        {
            if (!shot.Type.IsFamily(SamusProjectileFamily.Missile))
                continue;

            if (CoordinateAxis.Difference(shot.Position.X.Whole, 84) >= 0)
                return false;

            _terrainCollision.KillMissile(shot, sharedProjectiles);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Converts one live beam or missile into its ordinary impact animation after
    /// an enemy-owned overlap test accepts it. Enemy shot AI owns damage/counters; this
    /// projectile owner alone changes the shot and the shared cooldown.
    /// </summary>
    public bool TryStartEnemyImpact(
        SamusBombProjectileSystem sharedProjectiles,
        SamusProjectile projectile,
        bool blocksPlasmaBeam = false)
    {
        Ensure.NotNull(sharedProjectiles);
        SamusProjectile shot = Ensure.NotNull(projectile);
        if (!shot.HasEnemyCollisionPayload)
            return false;

        if (shot.Behavior is SpecialAttackParticleBehavior)
        {
            // Enemy collision only marks the direction. These particles own
            // hit deletion (and Spazer's paired deletion) in their next update;
            // replacing that response with a beam explosion loses their family lifecycle.
            // Plasma normally pierces, unless the target explicitly stops Plasma beams.
            ApplyEnemyCollisionPrelude(shot,
                blocksPlasmaBeam || !shot.Type.BeamCombination.HasPlasma);
            return true;
        }

        if (shot.IsSpent)
            return false;

        switch (shot.Type.Family)
        {
            case SamusProjectileFamily.Beam:
                // Enemy collision marks the shot as having hit only when the
                // target blocks Plasma or the shot lacks Plasma. Ordinary penetrating
                // beams, not just special beam-combo particles, retain their lifecycle.
                if (!blocksPlasmaBeam &&
                    shot.Type.BeamCombination.HasPlasma)
                    return true;
                // Enemy collision only marks the shot; enemy hits never reach the projectile kill's sound.
                ProjectileTerrainCollision.KillBeam(shot);
                return true;
            case SamusProjectileFamily.Missile:
            case SamusProjectileFamily.SuperMissile:
                _terrainCollision.KillMissile(shot, sharedProjectiles);
                return true;
            case SamusProjectileFamily.PowerBomb:
            case SamusProjectileFamily.Bomb:
            case SamusProjectileFamily.BeamExplosion:
            case SamusProjectileFamily.MissileExplosion:
                // Existing explosion, bomb, and unknown family shots cannot repeatedly
                // increment an enemy hit counter merely because their art still overlaps.
                return false;
            default:
                throw new InvalidOperationException($"Undefined SamusProjectileFamily {shot.Type.Family}.");
        }
    }
}
