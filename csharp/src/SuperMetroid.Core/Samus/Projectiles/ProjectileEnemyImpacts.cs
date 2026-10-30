using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile hits on enemies: the collision prelude, reflection, the intro Mother Brain missile, and the start of the impact explosion.</summary>
[RoomLifetime]
internal sealed class ProjectileEnemyImpacts
{
    /// <summary>Owns projectile clearing when reflection retires a Super Missile link.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Receives the native Super Missile enemy-hit quake request.</summary>
    private readonly ProjectileEarthquakes _earthquakes;
    /// <summary>Projectile records whose direction, damage, and animation change on enemy contact.</summary>
    private readonly ProjectileSlotTable _slotTable;
    /// <summary>Converts missile hits to the same impact state as terrain collision.</summary>
    private readonly ProjectileTerrainCollision _terrainCollision;

    /// <summary>Binds enemy-impact handling to the projectile slots and their animation, quake, and terrain services.</summary>
    internal ProjectileEnemyImpacts(ProjectileAnimation animation, ProjectileEarthquakes earthquakes, ProjectileSlotTable slotTable, ProjectileTerrainCollision terrainCollision)
    {
        _animation = animation;
        _earthquakes = earthquakes;
        _slotTable = slotTable;
        _terrainCollision = terrainCollision;
    }
    /// <summary>
    /// Applies the projectile-owned side effects that `$A0:9CC8-$9CD6` performs before any
    /// enemy callback which returns without entering ordinary impact conversion. This is
    /// shared by extended hitboxes and radius-based custom reactions such as shutters.
    /// </summary>
    public void ApplyEnemyCollisionPrelude(int slotIndex, bool markCollisionState)
    {
        if ((uint)slotIndex >= SamusProjectileSystem.SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        SamusProjectileSlot slot = _slotTable.Slots[slotIndex];
        if (!slot.HasEnemyCollisionPayload)
            return;

        if (slot.Type.Family == SamusProjectileFamily.SuperMissile)
        {
            // The collision walkers write the enemy-hit quake before dispatching the enemy's
            // shot callback. This remains observable even when gold Ninja armor ignores a
            // fresh, not-yet-linked Super Missile.
            _earthquakes.RequestSuperMissileEarthquake(SuperMissileDefinitions.EnemyHitEarthquakeType);
        }

        if (markCollisionState)
            slot.IsSpent = true;
    }

    /// <summary>
    /// Compatibility name retained for callers whose geometry is specifically an extended
    /// spritemap. The behavior is the same bank-$A0 projectile prelude, not a separate fix.
    /// </summary>
    public void ApplyExtendedEnemyCollisionPrelude(int slotIndex, bool markCollisionState) =>
        ApplyEnemyCollisionPrelude(slotIndex, markCollisionState);

    /// <summary>
    /// Ports <c>ProjectileReflection</c> at `$90:BE00` after an enemy callback has replaced
    /// the projectile direction. Reflection preserves the projectile family/type and world
    /// position, but reconstructs its bank-$93 damage, direction-specific animation, radii,
    /// and family pre-instruction exactly as the native routine does.
    /// </summary>
    public void ReflectFromEnemy(SamusState samus, int slotIndex)
    {
        Ensure.NotNull(samus);
        if ((uint)slotIndex >= SamusProjectileSystem.SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        SamusProjectileSlot slot = _slotTable.Slots[slotIndex];
        if (!slot.HasEnemyCollisionPayload)
            return;

        SamusProjectileFamily family = slot.Type.Family;
        if (family == SamusProjectileFamily.SuperMissile)
        {
            // `$90:BE17` clears the slot the Super Missile names as its link before
            // repurposing the owner; it does not broadly delete unrelated Super Missiles.
            if (slot.SuperMissileLinkSlot is { } linkIndex && linkIndex != slotIndex && _slotTable.Slots[linkIndex].IsActive)
                _animation.ClearProjectile(_slotTable.Slots[linkIndex]);
        }

        if (family == SamusProjectileFamily.Beam)
        {
            // Beam reflection alone reinitializes velocity immediately. Missile families
            // retain their old velocity until they re-ignite on their next update, at
            // which point the new direction takes effect.
            BeamFiring.InitializePowerBeamVelocity(samus.ProjectileInheritance, slot);
        }

        SamusProjectileSelectorTable selectorTable;
        int selectorIndex;
        if (family == SamusProjectileFamily.Beam)
        {
            selectorTable = slot.Type.IsChargedBeam
                ? SamusProjectileSelectorTable.ChargedBeam
                : SamusProjectileSelectorTable.UnchargedBeam;
            selectorIndex = slot.Type.BeamCombination.TableIndex;
        }
        else if (family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
        {
            selectorTable = SamusProjectileSelectorTable.NonBeam;
            selectorIndex = family.TableRow;
        }
        else
        {
            // The native reflection producer only selects beams and the two missile
            // families. Reaching this method with a bomb/explosion payload is corrupt
            // scheduler state rather than an unimplemented reflection behavior.
            throw new InvalidDataException(
                $"Projectile family {family} is not reflectable.");
        }

        SamusProjectileHeader header = SamusProjectileSelection.Select(selectorTable, selectorIndex);
        slot.Damage = SamusProjectileDamage.Read(header);
        SamusProjectileProgram? program = SamusProjectileHeaders.Program(header, slot.Direction.ToAimIndex());
        slot.Cursor = SamusProjectilePrograms.Start(program);
        slot.Radius = SamusProjectilePrograms.InitialRadius(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);

        if (family == SamusProjectileFamily.Missile)
        {
            slot.PreInstruction = SamusProjectilePreInstruction.Missile;
            slot.MissileIgnition = SamusMissileIgnition.PendingAfterReflection;
            slot.SuperMissileLinkSlot = null;
        }
        else if (family == SamusProjectileFamily.SuperMissile)
        {
            slot.PreInstruction = SamusProjectilePreInstruction.SuperMissile;
            slot.MissileIgnition = SamusMissileIgnition.PendingAfterReflection;
            slot.SuperMissileLinkSlot = null;
        }
        else
        {
            slot.PreInstruction = !slot.Type.BeamCombination.HasWave
                ? SamusProjectilePreInstruction.NoWaveBeam
                : SamusProjectilePreInstruction.WaveBeamFourFrameTrail;
        }
    }

    /// <summary>
    /// Applies intro Mother Brain's external projectile test at <c>$8B:B78A..B7BA</c>.
    /// </summary>
    /// <remarks>
    /// The cinematic scans the slots from the highest down for the first ordinary missile
    /// and only kills it once its signed X coordinate
    /// is below $54. This is not terrain collision, so the actor owns the predicate while
    /// the projectile system owns the exact bank-$90 impact conversion.
    /// </remarks>
    public bool TryImpactIntroMotherBrainMissile(
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(sharedProjectiles);

        foreach (SamusProjectileSlot slot in _slotTable.InUpdateOrder)
        {
            if (!slot.Type.IsFamily(SamusProjectileFamily.Missile))
                continue;

            if (unchecked((short)(slot.Position.X.Whole - 84)) >= 0)
                return false;

            _terrainCollision.KillMissile(slot, sharedProjectiles);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Converts one live beam or missile into its ordinary bank-$93 impact animation after
    /// an enemy-owned overlap test accepts it. Enemy shot AI owns damage/counters; this
    /// projectile owner alone mutates the parallel slot arrays and shared cooldown.
    /// </summary>
    public bool TryStartEnemyImpact(
        SamusBombProjectileSystem sharedProjectiles,
        int slotIndex,
        bool blocksPlasmaBeam = false)
    {
        Ensure.NotNull(sharedProjectiles);
        if ((uint)slotIndex >= SamusProjectileSystem.SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        SamusProjectileSlot slot = _slotTable.Slots[slotIndex];
        if (!slot.HasEnemyCollisionPayload)
            return false;

        if (slot.PreInstruction is SamusProjectilePreInstruction.IceCombo or
            SamusProjectilePreInstruction.IceComboOutward or SamusProjectilePreInstruction.WaveCombo or
            SamusProjectilePreInstruction.SpazerCombo or SamusProjectilePreInstruction.SpazerComboFalling or
            SamusProjectilePreInstruction.PlasmaCombo)
        {
            // Enemy collision only marks the native direction word. These particles own
            // hit deletion (and Spazer's paired deletion) in their next pre-instruction;
            // replacing that handler with a beam explosion loses their family lifecycle.
            // Plasma normally pierces, unless the target explicitly stops Plasma beams.
            ApplyEnemyCollisionPrelude(slotIndex,
                blocksPlasmaBeam || !slot.Type.BeamCombination.HasPlasma);
            return true;
        }

        if (slot.IsSpent)
            return false;

        switch (slot.Type.Family)
        {
            case SamusProjectileFamily.Beam:
                // Native enemy collision marks direction bit $10 only when the
                // target blocks Plasma or the shot lacks Plasma. Ordinary penetrating
                // beams, not just special beam-combo particles, retain their lifecycle.
                if (!blocksPlasmaBeam &&
                    slot.Type.BeamCombination.HasPlasma)
                    return true;
                // $A0:9A07 only marks the shot; enemy hits never reach Kill_Projectile's sound.
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
