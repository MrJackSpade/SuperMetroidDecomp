using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Core.Hardware.SnesAddressMath;

namespace SuperMetroid.Core.Game;

/// <summary>Projectile hits on enemies: the collision prelude, reflection, the intro Mother Brain missile, and the start of the impact explosion.</summary>
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

        SamusProjectileSlot slot = _slotTable._slots[slotIndex];
        if (!slot.HasEnemyCollisionPayload)
            return;

        if (slot.PackedType.Family == SamusProjectileFamily.SuperMissile)
        {
            // The collision walkers write the enemy-hit quake before dispatching the enemy's
            // shot callback. This remains observable even when gold Ninja armor ignores a
            // fresh, not-yet-linked Super Missile.
            _earthquakes.RequestSuperMissileEarthquake(SamusProjectileRomData.NonBeam.SuperMissileEnemyHitEarthquakeType);
        }

        if (markCollisionState)
            slot.Direction = slot.PackedDirection.WithCollisionLifecycleState();
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
    public void ReflectFromEnemy(ISnesAddressSpace bus, int slotIndex)
    {
        Ensure.NotNull(bus);
        if ((uint)slotIndex >= SamusProjectileSystem.SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        SamusProjectileSlot slot = _slotTable._slots[slotIndex];
        if (!slot.HasEnemyCollisionPayload)
            return;

        SamusProjectileFamily family = slot.PackedType.Family;
        if (family == SamusProjectileFamily.SuperMissile)
        {
            // A live Super Missile stores its invisible collision-link byte index in the
            // variable's low byte. `$90:BE17` clears that exact slot before repurposing the
            // owner; it does not broadly delete unrelated Super Missile records.
            int linkIndex = (SnesLittleEndian.LowByte(slot.Variable)) >> 1;
            if ((uint)linkIndex < SamusProjectileSystem.SlotCount && linkIndex != slotIndex && _slotTable._slots[linkIndex].IsActive)
                _animation.ClearProjectile(_slotTable._slots[linkIndex]);
        }

        if (family == SamusProjectileFamily.Beam)
        {
            // Beam reflection alone reinitializes velocity immediately. Missile families
            // retain their old velocity until variable `$00F0` crosses into `$01F0` in the
            // next Missile_Func1 call, at which point the new direction takes effect.
            BeamFiring.InitializePowerBeamVelocity(bus, slot);
        }

        int dataPointerTable;
        int dataPointerIndex;
        if (family == SamusProjectileFamily.Beam)
        {
            dataPointerTable = slot.PackedType.IsChargedBeam
                ? SamusProjectileRomData.Beams.ChargedDataPointers
                : SamusProjectileRomData.Beams.UnchargedDataPointers;
            dataPointerIndex = slot.PackedType.BeamCombination.TableIndex;
        }
        else if (family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
        {
            dataPointerTable = SamusProjectileRomData.NonBeam.DataPointers;
            dataPointerIndex = SnesLittleEndian.HighByte(slot.PackedType.FamilyValue);
        }
        else
        {
            // The native reflection producer only selects beams and the two missile
            // families. Reaching this method with a bomb/explosion payload is corrupt
            // scheduler state rather than an unimplemented reflection behavior.
            throw new InvalidDataException(
                $"Projectile family ${slot.PackedType.FamilyValue:X3} is not reflectable.");
        }

        ushort dataPointer = SamusProjectileSelectionDefinitions.ReadWord(dataPointerTable + dataPointerIndex * 2);
        int data = SamusProjectileRomData.Banks.Projectile | dataPointer;
        slot.Damage = SamusProjectileDamageDefinitions.Read(data);
        slot.InstructionPointer = SamusProjectileSelectionDefinitions.ReadWord(
            AddWithinBank(data, 2 + slot.PackedDirection.DirectionIndex * 2));
        slot.Radius = slot.Radius.WithX(SamusProjectileRadiusDefinitions.ReadByte(
            SamusProjectileRomData.Banks.Projectile |
                unchecked((ushort)(slot.InstructionPointer + 4))));
        slot.Radius = slot.Radius.WithY(SamusProjectileRadiusDefinitions.ReadByte(
            SamusProjectileRomData.Banks.Projectile |
                unchecked((ushort)(slot.InstructionPointer + 5))));
        slot.InstructionTimer = NativeCountdown.Start(1);

        if (family == SamusProjectileFamily.Missile)
        {
            slot.PreInstruction = SamusProjectilePreInstruction.Missile;
            slot.Variable = 240;
        }
        else if (family == SamusProjectileFamily.SuperMissile)
        {
            slot.PreInstruction = SamusProjectilePreInstruction.SuperMissile;
            slot.Variable = 240;
        }
        else
        {
            slot.PreInstruction = !slot.PackedType.BeamCombination.HasWave
                ? SamusProjectilePreInstruction.NoWaveBeam
                : SamusProjectilePreInstruction.WaveBeamFourFrameTrail;
        }
    }

    /// <summary>
    /// Applies intro Mother Brain's external projectile test at <c>$8B:B78A..B7BA</c>.
    /// </summary>
    /// <remarks>
    /// The cinematic scans native byte indices $08 down through $00 for the first ordinary
    /// missile (<c>type &amp; $0FFF == $0100</c>) and only kills it once its signed X coordinate
    /// is below $54. This is not terrain collision, so the actor owns the predicate while
    /// the projectile system owns the exact bank-$90 impact conversion.
    /// </remarks>
    public bool TryImpactIntroMotherBrainMissile(
        ISnesAddressSpace bus,
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(sharedProjectiles);

        for (int slotIndex = SamusProjectileSystem.SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot slot = _slotTable._slots[slotIndex];
            if ((slot.Type & 0x0fff) != 0x0100)
                continue;

            if (unchecked((short)(slot.Position.X.Whole - 0x0054)) >= 0)
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
        ISnesAddressSpace bus,
        SamusBombProjectileSystem sharedProjectiles,
        int slotIndex,
        bool blocksPlasmaBeam = false)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(sharedProjectiles);
        if ((uint)slotIndex >= SamusProjectileSystem.SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        SamusProjectileSlot slot = _slotTable._slots[slotIndex];
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
                blocksPlasmaBeam || !slot.PackedType.BeamCombination.HasPlasma);
            return true;
        }

        if (slot.PackedDirection.HasLowByteLifecycleState)
            return false;

        switch (slot.PackedType.Family)
        {
            case SamusProjectileFamily.Beam:
                // Native enemy collision marks direction bit $10 only when the
                // target blocks Plasma or the shot lacks Plasma. Ordinary penetrating
                // beams, not just special beam-combo particles, retain their lifecycle.
                if (!blocksPlasmaBeam &&
                    slot.PackedType.BeamCombination.HasPlasma)
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
                throw new InvalidOperationException($"Undefined SamusProjectileFamily {slot.PackedType.Family}.");
        }
    }
}
