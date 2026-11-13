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
    private readonly ProjectileSlotTable _slotTable;
    /// <summary>Converts missile hits to the same impact state as terrain collision.</summary>
    private readonly ProjectileTerrainCollision _terrainCollision;
    /// <summary>The speeds a reflected Spazer and Plasma beam relaunches at.</summary>
    private readonly UnauthoredBeamFiring<SamusBeamCombination> _unauthoredFiring;

    /// <summary>Binds enemy-impact handling to the projectile slots and their animation, quake, and terrain services.</summary>
    internal ProjectileEnemyImpacts(ProjectileAnimation animation, ProjectileEarthquakes earthquakes, ProjectileSlotTable slotTable,
        ProjectileTerrainCollision terrainCollision, UnauthoredBeamFiring<SamusBeamCombination> unauthoredFiring)
    {
        _animation = animation;
        _earthquakes = earthquakes;
        _slotTable = slotTable;
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
        SamusProjectile slot = Ensure.NotNull(projectile);
        if (!slot.HasEnemyCollisionPayload)
            return;

        if (slot.Type.Family == SamusProjectileFamily.SuperMissile)
        {
            // The collision walkers write the enemy-hit quake before dispatching the enemy's
            // shot reaction. This remains observable even when gold Ninja armor ignores a
            // fresh, not-yet-linked Super Missile.
            _earthquakes.RequestSuperMissileEarthquake(SuperMissileDefinitions.EnemyHitEarthquake);
        }

        if (markCollisionState)
            slot.IsSpent = true;
    }

    /// <summary>
    /// Compatibility name retained for callers whose geometry is specifically an extended
    /// frame hitbox. The behavior is the same projectile prelude, not a separate fix.
    /// </summary>
    public void ApplyFrameHitboxCollisionPrelude(SamusProjectile projectile, bool markCollisionState) =>
        ApplyEnemyCollisionPrelude(projectile, markCollisionState);

    /// <summary>
    /// Reflects the projectile after an enemy reaction has replaced
    /// the projectile direction. Reflection preserves the projectile family/type and world
    /// position, but reconstructs its damage, direction-specific animation, radii,
    /// and family frame behaviour exactly as the game does.
    /// </summary>
    public void ReflectFromEnemy(SamusState samus, SamusProjectile projectile)
    {
        Ensure.NotNull(samus);
        SamusProjectile slot = Ensure.NotNull(projectile);
        if (!slot.HasEnemyCollisionPayload)
            return;

        SamusProjectileFamily family = slot.Type.Family;
        if (family == SamusProjectileFamily.SuperMissile)
        {
            // Reflection deletes the projectile in the place the Super Missile names as its link
            // (UnlinkedSuperMissileLink) before repurposing the owner; it does not broadly delete
            // unrelated Super Missiles.
            if (slot.SuperMissileLinkPlace is { } linkPlace && linkPlace != slot.Place && _slotTable.Slots[linkPlace].IsActive)
                _animation.ClearProjectile(_slotTable.Slots[linkPlace]);
        }

        if (family == SamusProjectileFamily.Beam)
        {
            // Beam reflection alone reinitializes velocity immediately. Missile families
            // retain their old velocity until they re-ignite on their next update, at
            // which point the new direction takes effect.
            BeamFiring.InitializePowerBeamVelocity(samus.ProjectileInheritance, slot, _unauthoredFiring);
        }

        SamusProjectileKind kind;
        SamusProjectileSegment? segment;
        if (family == SamusProjectileFamily.Beam)
        {
            kind = SamusProjectileSelection.BeamShot(slot.Type.BeamCombination, slot.Type.IsChargedBeam);
            segment = SamusProjectileSelection.BeamShotAnimation(slot.Type.BeamCombination, slot.Type.IsChargedBeam, slot.Direction);
        }
        else if (family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
        {
            kind = SamusProjectileSelection.NonBeam(family);
            segment = SamusProjectileKinds.Animation(kind, slot.Direction);
        }
        else
        {
            // The reflection producer only selects beams and the two missile
            // families. Reaching this method with a bomb/explosion payload is corrupt
            // scheduler state rather than an unimplemented reflection behavior.
            throw new InvalidDataException(
                $"Projectile family {family} is not reflectable.");
        }

        slot.Damage = SamusProjectileDamage.Read(kind);
        slot.Playback = SamusProjectilePlayback.Start(segment);
        slot.Radius = SamusProjectilePlayback.InitialRadius(segment);
        slot.FrameHold = WrappingCountdown.Start(1);

        if (family == SamusProjectileFamily.Missile)
        {
            slot.FrameBehavior = SamusProjectileFrameBehavior.Missile;
            slot.MissileIgnition = SamusMissileIgnition.PendingAfterReflection;
            slot.SuperMissileLinkPlace = null;
        }
        else if (family == SamusProjectileFamily.SuperMissile)
        {
            slot.FrameBehavior = SamusProjectileFrameBehavior.SuperMissile;
            slot.MissileIgnition = SamusMissileIgnition.PendingAfterReflection;
            slot.SuperMissileLinkPlace = null;
        }
        else
        {
            slot.FrameBehavior = !slot.Type.BeamCombination.HasWave
                ? SamusProjectileFrameBehavior.NoWaveBeam
                : SamusProjectileFrameBehavior.WaveBeamFourFrameTrail;
        }
    }

    /// <summary>
    /// Applies intro Mother Brain's external projectile test.
    /// </summary>
    /// <remarks>
    /// The cinematic scans the slots from the highest down for the first ordinary missile
    /// and only kills it once its signed X coordinate
    /// is below 84. This is not terrain collision, so the actor owns the predicate while
    /// the projectile system owns the exact impact conversion.
    /// </remarks>
    public bool TryImpactIntroMotherBrainMissile(
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(sharedProjectiles);

        foreach (SamusProjectile slot in _slotTable.InUpdateOrder)
        {
            if (!slot.Type.IsFamily(SamusProjectileFamily.Missile))
                continue;

            if (CoordinateAxis.Difference(slot.Position.X.Whole, 84) >= 0)
                return false;

            _terrainCollision.KillMissile(slot, sharedProjectiles);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Converts one live beam or missile into its ordinary impact animation after
    /// an enemy-owned overlap test accepts it. Enemy shot AI owns damage/counters; this
    /// projectile owner alone mutates the parallel slot arrays and shared cooldown.
    /// </summary>
    public bool TryStartEnemyImpact(
        SamusBombProjectileSystem sharedProjectiles,
        SamusProjectile projectile,
        bool blocksPlasmaBeam = false)
    {
        Ensure.NotNull(sharedProjectiles);
        SamusProjectile slot = Ensure.NotNull(projectile);
        if (!slot.HasEnemyCollisionPayload)
            return false;

        if (slot.FrameBehavior is SamusProjectileFrameBehavior.IceCombo or
            SamusProjectileFrameBehavior.IceComboOutward or SamusProjectileFrameBehavior.WaveCombo or
            SamusProjectileFrameBehavior.SpazerCombo or SamusProjectileFrameBehavior.SpazerComboFalling or
            SamusProjectileFrameBehavior.PlasmaCombo)
        {
            // Enemy collision only marks the direction. These particles own
            // hit deletion (and Spazer's paired deletion) in their next frame behaviour;
            // replacing that handler with a beam explosion loses their family lifecycle.
            // Plasma normally pierces, unless the target explicitly stops Plasma beams.
            ApplyEnemyCollisionPrelude(slot,
                blocksPlasmaBeam || !slot.Type.BeamCombination.HasPlasma);
            return true;
        }

        if (slot.IsSpent)
            return false;

        switch (slot.Type.Family)
        {
            case SamusProjectileFamily.Beam:
                // Enemy collision marks the shot as having hit only when the
                // target blocks Plasma or the shot lacks Plasma. Ordinary penetrating
                // beams, not just special beam-combo particles, retain their lifecycle.
                if (!blocksPlasmaBeam &&
                    slot.Type.BeamCombination.HasPlasma)
                    return true;
                // Enemy collision only marks the shot; enemy hits never reach the projectile kill's sound.
                ProjectileTerrainCollision.KillBeam(slot);
                return true;
            case SamusProjectileFamily.Missile:
            case SamusProjectileFamily.SuperMissile:
                _terrainCollision.KillMissile(slot, sharedProjectiles);
                return true;
            case SamusProjectileFamily.PowerBomb:
            case SamusProjectileFamily.Bomb:
            case SamusProjectileFamily.BeamExplosion:
            case SamusProjectileFamily.MissileExplosion:
                // Existing explosion, bomb, and unknown family slots cannot repeatedly
                // increment an enemy hit counter merely because their art still overlaps.
                return false;
            default:
                throw new InvalidOperationException($"Undefined SamusProjectileFamily {slot.Type.Family}.");
        }
    }
}
