using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Block-reaction PLMs spawned by Samus and her weapons: grapple, bomb, crumble, Speed Booster, shot and bombed special blocks.</summary>
[RoomLifetime]
internal sealed class BlockReactionPlms
{
    /// <summary>The room's area.</summary>
    private readonly AreaId _area;
    /// <summary>Handles the animal-escape reaction reserved among the shot-block behaviours.</summary>
    private readonly ICrittersEscapeReaction _escapeAnimals;
    /// <summary>Allocates reaction PLMs in descending native slot order.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Connects block reactions to the active room, escape-animal branch, and shared PLM pool.</summary>
    internal BlockReactionPlms(AreaId area, ICrittersEscapeReaction escapeAnimals, PlmSlotTable slotTable)
    {
        _area = area;
        _escapeAnimals = escapeAnimals;
        _slotTable = slotTable;
    }

    /// <summary>Starts a respawning or permanent grapple-block program while preserving its original terrain for restoration.</summary>
    public bool TrySpawnBreakableGrappleBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior behavior)
    {
        Ensure.NotNull(level);
        if (!behavior.IsBreakableGrappleReaction)
            throw new ArgumentOutOfRangeException(nameof(behavior), "Breakable grapple behaviour must be one or two.");

        // Spawn_PLM at $84:84E7 probes $4E,$4C,...,$00. Matching it matters if multiple
        // PLMs mutate the same room on one frame because the handler uses the same order.
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        slot.BlockIndex = blockIndex;
        slot.RestoreTerrain = block.Terrain;
        slot.StartAnimation(new BlockReactionPlmAnimation(behavior.GrappleReactionIndex == 1
            ? BlockReactionPlmProgram.RespawningBreakableGrappleBlock
            : BlockReactionPlmProgram.PermanentBreakableGrappleBlock));
        slot.InstructionTimer = WrappingCountdown.Start(1);

        // The setup saves the complete original block but clears only its behaviour. It deliberately leaves the
        // Grapple-block collision intact until the first PLM pass later in this same gameplay frame draws the intact
        // Grapple block.
        level.SetBehavior(blockIndex, RoomBlockBehaviorValues.None);
        return true;
    }

    /// <summary>Starts a timed collision bomb block and substitutes its parent appearance before the first PLM pass.</summary>
    public bool TrySpawnCollisionBombBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior behavior)
    {
        Ensure.NotNull(level);
        if (!behavior.IsNormalReactionIndex(8))
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Collision bomb-block behaviour must be in the native table range zero through seven.");
        }

        // `$84:84ED-$84F7` searches the same descending slot order used by every other
        // gameplay-spawned PLM. Do not coalesce neighboring pieces: a native collision scan
        // can allocate more than one independently timed object in a single movement call.
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        slot.BlockIndex = blockIndex;

        // This is not the original appearance. The setup deliberately replaces the block and its mirroring with the
        // bomb-block parent; multi-block restorations then add extension blocks around this parent.
        slot.RestoreTerrain = new RoomTerrainBlock(RoomPlmVisualBlockDefinitions.CollisionBombParent, block.CollisionType);
        slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmPrograms.CollisionBomb(behavior.NormalReactionIndex)));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        level.SetTerrain(blockIndex, new RoomTerrainBlock(RoomPlmVisualBlockDefinitions.CollisionBombParent, RoomCollisionType.Air));
        return true;
    }

    /// <summary>
    /// Spawns the crumble actor selected when downward Samus collision reaches a special block with a crumble behaviour,
    /// zero through seven.
    /// </summary>
    /// <remarks>
    /// Only downward collision activates it: the setup keeps the block's collision kind and mirroring with the crumble
    /// parent's block as the restore block, makes the live block solid, and delays the program for four PLM handler
    /// passes. Behaviours zero through three reverse their animation and restore the parent; four through seven end on
    /// air. The collision dispatcher independently returns solid for the triggering frame, so allocation success is not
    /// a collision-result substitute.
    /// </remarks>
    public bool TrySpawnSamusContactCrumbleBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior behavior)
    {
        Ensure.NotNull(level);
        if (!behavior.IsNormalReactionIndex(8))
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Samus-contact crumble behaviour must be in the native table range zero through seven.");
        }

        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        slot.Header =
            RoomPlmHeaders.ContactCrumbleByReactionIndex(behavior.NormalReactionIndex);
        slot.BlockIndex = blockIndex;
        RoomTerrainBlock restore = block.Terrain.WithBlock(RoomPlmVisualBlockDefinitions.ContactCrumbleParent.Block);
        slot.RestoreTerrain = restore;
        slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmPrograms.ContactCrumble(behavior.NormalReactionIndex)));
        slot.InstructionTimer = WrappingCountdown.Start(4);
        level.SetTerrain(blockIndex, restore.WithCollision(RoomCollisionType.SolidBlock));
        return true;
    }

    /// <summary>
    /// Runs the Speed Booster block setup for a special block selected by either the area-independent Speed Booster
    /// behaviours or Brinstar's area-specific ones.
    /// </summary>
    /// <remarks>
    /// The setup accepts boost-counter stage four and all six directional shinespark poses. On acceptance it preserves
    /// the collision kind in the eventual restore block, substitutes the Speed Booster parent's block, and makes the
    /// live block air before the
    /// PLM handler draws the first crumble frame. Rejected contact deletes the temporary
    /// slot synchronously and remains solid in the caller.
    /// </remarks>
    /// <returns>True when contact passes: accepted setup, or an exhausted native pool.</returns>
    public bool TrySpawnSamusSpeedBoosterBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior behavior,
        IRoomSamus samus)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!SpeedBoosterBlocks.TryResolve(
                behavior,
                _area,
                out SpeedBoosterBlockPlmDefinition definition))
        {
            return false;
        }

        // The special-block dispatcher reports a pass-through contact before spawning. When every slot is occupied the
        // spawn returns that result unchanged: setup never runs, so its no-Speed-Booster rejection cannot make this
        // contact solid. Preserve the block; this is not a break animation.
        if (_slotTable.IsFull)
            return true;

        bool activeSpeedBooster = samus.IsActivelySpeedBoosting;
        bool activeShinespark = samus.IsShinesparking;
        if (!activeSpeedBooster && !activeShinespark)
            return false;

        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        slot.Header = definition.Header;
        slot.BlockIndex = blockIndex;
        slot.RestoreTerrain = block.Terrain.WithBlock(RoomPlmVisualBlockDefinitions.SpeedBoosterParent.Block);
        slot.StartAnimation(new SpeedBoosterBlockPlmAnimation(definition.Program));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        level.SetTerrain(blockIndex, slot.RestoreTerrain.WithCollision(RoomCollisionType.Air));
        return true;
    }

    /// <summary>Selects the normal-bomb or Power Bomb reaction list; only the latter executes its packed sound instruction.</summary>
    public bool TrySpawnBombReactionBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior behavior,
        SamusProjectileType projectileType)
    {
        Ensure.NotNull(level);
        if (!behavior.IsNormalReactionIndex(16))
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Bomb-reaction behaviour must be in the native table range zero through fifteen.");
        }

        SamusProjectileFamily projectileFamily = projectileType.Family;
        if (projectileFamily is not (SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb))
        {
            throw new ArgumentOutOfRangeException(
                nameof(projectileType),
                "Bomb-reaction setup accepts only normal-bomb or power-bomb projectile families.");
        }

        // The spawn searches from the highest slot down. Importantly, the full-pool path never calls setup and
        // therefore must not change the live terrain.
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        slot.BlockIndex = blockIndex;
        slot.InstructionTimer = WrappingCountdown.Start(1);

        if (!behavior.IsNormalReactionIndex(8))
        {
            // Table entries 8..15 are all the empty reaction, whose setup does nothing and whose program deletes.
            slot.RestoreTerrain = PlmSlotDefinitions.ClearedRestoreTerrain;
            PlmProgramStart.Delete.StartOn(slot);
            return true;
        }

        // The setup discards the original appearance rather than preserving the visible block. Dimension-specific final
        // draws reconstruct linked extension blocks; the 1x1 respawn tail uses this exact restore block.
        slot.RestoreTerrain = block.Terrain.WithAppearance(RoomPlmVisualBlockDefinitions.CollisionBombParent);
        // Only normal bombs skip the reaction sound: a normal bomb starts the shared break tail.
        slot.StartAnimation(new BlockReactionPlmAnimation(projectileFamily == SamusProjectileFamily.Bomb
            ? BlockReactionPlmPrograms.BombBreak(behavior.NormalReactionIndex)
            : BlockReactionPlmPrograms.ReactionBomb(behavior.NormalReactionIndex)));

        level.SetTerrain(blockIndex, slot.RestoreTerrain.Deactivated());
        return true;
    }

    /// <summary>Selects a shot-block PLM by behaviour, weapon family, area table, and air-versus-solid collision semantics.</summary>
    public bool TrySpawnProjectileShotBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior behavior,
        SamusProjectileType projectileType,
        bool solidBlock)
    {
        Ensure.NotNull(level);
        if (behavior == EscapeAnimalWallDefinitions.ReactionBehavior)
            return _escapeAnimals.TrySpawnCrittersEscapeReaction(level, blockIndex, projectileType);
        bool areaDependent = behavior.UsesAreaReactionTable;
        if (areaDependent && !behavior.IsAreaReactionIndex(8))
        {
            throw new UnsupportedShotBlockReactionException(
                nameof(behavior),
                "Area-dependent shootable behaviour must address one of its eight native entries.");
        }
        if (!areaDependent &&
            !behavior.IsNormalReactionIndex(16) &&
            !behavior.IsShootableCollisionProbe && !behavior.IsUnusedShootableReaction)
        {
            throw new UnsupportedShotBlockReactionException(
                nameof(behavior),
                $"Shootable behaviour {behavior} requires a specialized reaction outside the breakable/nothing PLM dispatcher.");
        }

        // Shootable air checks the area-table selector before spawning. Its solid-block sibling instead performs the
        // area-table lookup and allocates the retail no-op entry. This early return is therefore collision-kind-specific.
        if (areaDependent && !solidBlock)
            return false;

        SamusProjectileFamily projectileFamily = projectileType.Family;

        // The power-bomb and Super Missile setups clear the newly allocated PLM when the weapon family is wrong.
        // Observably that is identical to returning with no active slot: setup never changes the live block, and the
        // next handler has nothing to process. Perform this
        // gate before the host allocation loop while preserving every accepted native path.
        if (behavior.RequiresPowerBombReaction && projectileFamily is not (
            SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb))
            return false;
        if (behavior.RequiresSuperMissileReaction && projectileFamily is not (
            SamusProjectileFamily.SuperMissile or SamusProjectileFamily.Bomb))
            return false;

        // The spawn scans from the highest slot down and changes nothing when full. The projectile still receives the
        // collision kind's normal result.
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        slot.BlockIndex = blockIndex;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.RestoreTerrain = PlmSlotDefinitions.ClearedRestoreTerrain;

        if (areaDependent)
        {
            // Every retail area-table target is the nothing entry. The spawn still consumes a slot until its delete
            // instruction runs.
            PlmProgramStart.Delete.StartOn(slot);
            return true;
        }

        if (behavior.IsShootableCollisionProbe)
        {
            // Setup ends the projectile scan and forces its collision counter;
            // the caller publishes those scratch effects only after successful
            // allocation. This actor does not mutate terrain and deletes next pass.
            PlmProgramStart.Delete.StartOn(slot);
            return true;
        }

        if (behavior.IsRespawningReaction)
        {
            // The setup keeps the collision kind with the shot-block parent's appearance for the 384-frame restoration,
            // and deactivates the live block immediately.
            slot.RestoreTerrain = block.Terrain.WithAppearance(RoomPlmVisualBlockDefinitions.ShotBlockParent);
            slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmPrograms.RespawningShot(behavior.NormalReactionIndex)));
            level.SetTerrain(blockIndex, slot.RestoreTerrain.Deactivated());
            return true;
        }

        if (behavior.IsPermanentReaction)
        {
            // The deactivating setup keeps no restore block: behaviours four through seven are permanent. The current
            // block loses its shootable collision before the first animated breaking frame runs later in this pass.
            slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmPrograms.PermanentShot(behavior.NormalReactionIndex - 4)));
            level.SetTerrain(blockIndex, block.Terrain.Deactivated());
            return true;
        }

        if (behavior.RequiresPowerBombReaction)
        {
            if (projectileFamily == SamusProjectileFamily.Bomb)
            {
                // A normal bomb does not break this block. The setup redirects to the one-frame visible power-bomb
                // reveal without touching collision.
                slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmProgram.BombedPowerBombBlockUnused));
                return true;
            }

            // A power bomb keeps the collision kind with the power-bomb parent's appearance, deactivates the live block,
            // and runs its header's program. Behaviour eight uses the ordinary four-frame breakup; nine uses its shorter
            // 3/2/1-frame counterpart.
            slot.RestoreTerrain = block.Terrain.WithAppearance(RoomPlmVisualBlockDefinitions.PowerBombBlockParent);
            slot.StartAnimation(new BlockReactionPlmAnimation(behavior.NormalReactionIndex == 8
                ? BlockReactionPlmProgram.RespawningPowerBombBlock
                : BlockReactionPlmProgram.PermanentPowerBombBlock));
            level.SetTerrain(blockIndex, slot.RestoreTerrain.Deactivated());
            return true;
        }

        if (behavior.RequiresSuperMissileReaction)
        {
            if (projectileFamily == SamusProjectileFamily.Bomb)
            {
                // Ordinary bombs get the analogous one-frame Super Missile reveal. It deliberately leaves the live block
                // untouched.
                slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmProgram.BombedSuperMissileBlockUnused));
                return true;
            }

            // Super Missiles keep the collision kind with the Super Missile parent's appearance; one header owns the
            // respawning program and the other the permanent one.
            slot.RestoreTerrain = block.Terrain.WithAppearance(RoomPlmVisualBlockDefinitions.SuperMissileBlockParent);
            slot.StartAnimation(new BlockReactionPlmAnimation(behavior.NormalReactionIndex == 10
                ? BlockReactionPlmProgram.RespawningSuperMissileBlock
                : BlockReactionPlmProgram.PermanentSuperMissileBlock));
            level.SetTerrain(blockIndex, slot.RestoreTerrain.Deactivated());
            return true;
        }

        // Entries C..F are all the empty reaction. Its setup leaves the block alone and its program deletes on the next
        // handler pass.
        PlmProgramStart.Delete.StartOn(slot);
        return true;
    }

    /// <summary>Dispatches normal-bomb special blocks through the area-independent or Brinstar reaction table.</summary>
    public bool TrySpawnBombedSpecialBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior behavior,
        AreaId areaIndex,
        SamusProjectileType projectileType)
    {
        Ensure.NotNull(level);
        _ = AreaIds.ToIndex(areaIndex);
        if (projectileType.Family == SamusProjectileFamily.PowerBomb)
            return false;
        if (projectileType.Family != SamusProjectileFamily.Bomb)
        {
            throw new ArgumentOutOfRangeException(
                nameof(projectileType),
                "The bomb-special setup accepts only normal-bomb and Power-Bomb families.");
        }

        PlmProgramStart program;
        if (!behavior.UsesAreaReactionTable)
        {
            if (!behavior.IsNormalReactionIndex(BombSpecialBlockReactions.Count))
                throw new ArgumentOutOfRangeException(nameof(behavior),
                    "Area-independent special-block behaviour is outside the cartridge reaction table.");

            program = BombSpecialBlockReactions.ProgramAt(behavior.NormalReactionIndex);
        }
        else
        {
            int areaBehavior = behavior.AreaReactionIndex;
            if (!behavior.IsAreaReactionIndex(8))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(behavior),
                    "Area-dependent bomb-special behaviour must address one of its eight native entries.");
            }

            program = areaIndex == AreaId.Brinstar && areaBehavior is >= 2 and <= 5
                ? PlmProgramStart.Typed(SpeedBoosterBlockProgram.BombReveal)
                : PlmProgramStart.Delete;
        }

        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        slot.RestoreTerrain = PlmSlotDefinitions.ClearedRestoreTerrain;
        program.StartOn(slot);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        return true;
    }
}
