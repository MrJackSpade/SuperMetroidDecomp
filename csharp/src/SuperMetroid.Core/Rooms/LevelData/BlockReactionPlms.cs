using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Block-reaction PLMs spawned by Samus and her weapons: grapple, bomb, crumble, Speed Booster, shot and bombed special blocks.</summary>
[RoomLifetime]
internal sealed class BlockReactionPlms
{
    /// <summary>The room's area.</summary>
    private readonly AreaId _area;
    /// <summary>Handles the animal-escape reaction reserved by the shot-block BTS table.</summary>
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

    /// <summary>Starts a respawning or permanent grapple-block program while preserving its original level word for restoration.</summary>
    public bool TrySpawnBreakableGrappleBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts)
    {
        Ensure.NotNull(level);
        if (!bts.IsBreakableGrappleReaction)
            throw new ArgumentOutOfRangeException(nameof(bts), "Breakable grapple BTS must be one or two.");

        // Spawn_PLM at $84:84E7 probes $4E,$4C,...,$00. Matching it matters if multiple
        // PLMs mutate the same room on one frame because the handler uses the same order.
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        slot.BlockIndex = blockIndex;
        slot.RestoreLevelWord = block.PackedWord.Raw;
        slot.StartAnimation(new BlockReactionPlmAnimation(bts.GrappleReactionIndex == 1
            ? BlockReactionPlmProgram.RespawningBreakableGrappleBlock
            : BlockReactionPlmProgram.PermanentBreakableGrappleBlock));
        slot.InstructionTimer = WrappingCountdown.Start(1);

        // Setup_CFB5 saves the complete original level word but clears only the low BTS
        // byte. It deliberately leaves collision type E intact until the first PLM pass
        // later in this same gameplay frame draws $E0B7.
        level.SetBehavior(blockIndex, RoomBlockBehaviorValues.None);
        return true;
    }

    /// <summary>Starts a timed collision bomb block and substitutes its native visual parent before the first PLM pass.</summary>
    public bool TrySpawnCollisionBombBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts)
    {
        Ensure.NotNull(level);
        if (!bts.IsNormalReactionIndex(8))
        {
            throw new ArgumentOutOfRangeException(
                nameof(bts),
                "Collision bomb-block BTS must be in the native table range zero through seven.");
        }

        // `$84:84ED-$84F7` searches the same descending slot order used by every other
        // gameplay-spawned PLM. Do not coalesce neighboring pieces: a native collision scan
        // can allocate more than one independently timed object in a single movement call.
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        slot.BlockIndex = blockIndex;

        // This is not the original visual word. Setup_CE83 deliberately replaces all
        // low twelve bits with visual block `$058`; multi-block restoration lists then
        // add type-$5/$D extension words around this type-$F parent.
        slot.RestoreLevelWord = new RoomLevelWord(RoomPlmVisualBlockDefinitions.CollisionBombParent)
            .WithCollisionType(block.CollisionType).Raw;
        slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmPrograms.CollisionBomb(bts.NormalReactionIndex)));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        level.SetForegroundEntry(blockIndex, RoomPlmVisualBlockDefinitions.CollisionBombParent);
        return true;
    }

    /// <summary>
    /// Spawns the bank-$84 crumble actor selected when downward Samus collision reaches a
    /// type-$B special block with BTS `$00-$07`.
    /// </summary>
    /// <remarks>
    /// This is setup <c>$84:CE37</c>. Bank $94 only permits activation for collision
    /// direction three (down): setup synthesizes <c>(levelWord &amp; $F000) | $00BC</c>,
    /// replaces the live collision nibble with type-eight solid, and delays list execution for four PLM handler
    /// passes. BTS `$00-$03` reverse their animation and restore the synthesized parent;
    /// `$04-$07` end on air. The collision dispatcher independently returns solid for the
    /// triggering frame, so allocation success is not a collision-result substitute.
    /// </remarks>
    public bool TrySpawnSamusContactCrumbleBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts)
    {
        Ensure.NotNull(level);
        if (!bts.IsNormalReactionIndex(8))
        {
            throw new ArgumentOutOfRangeException(
                nameof(bts),
                "Samus-contact crumble BTS must be in the native table range zero through seven.");
        }

        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        slot.Header =
            RoomPlmHeaders.ContactCrumbleByReactionIndex(bts.NormalReactionIndex);
        slot.BlockIndex = blockIndex;
        RoomLevelWord restoreWord = block.PackedWord
            .WithVisualBlockIndex(RoomPlmVisualBlockDefinitions.ContactCrumbleParent);
        slot.RestoreLevelWord = restoreWord.Raw;
        slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmPrograms.ContactCrumble(bts.NormalReactionIndex)));
        slot.InstructionTimer = WrappingCountdown.Start(4);
        level.SetForegroundEntry(
            blockIndex,
            restoreWord.WithCollisionType(RoomCollisionType.SolidBlock).Raw);
        return true;
    }

    /// <summary>
    /// Runs setup <c>$84:CDEA</c> for a type-$B special block selected by either the
    /// area-independent BTS <c>$0E/$0F</c> entries or Brinstar's BTS <c>$82-$85</c> table.
    /// </summary>
    /// <remarks>
    /// Native setup accepts boost-counter stage four and all six directional shinespark
    /// poses. On acceptance it preserves the collision nibble in the eventual restore word,
    /// substitutes visual block <c>$0B6</c>, and clears the live collision nibble before the
    /// PLM handler draws the first crumble frame. Rejected contact deletes the temporary
    /// slot synchronously and remains solid in the caller.
    /// </remarks>
    /// <returns>True when contact passes: accepted setup, or an exhausted native pool.</returns>
    public bool TrySpawnSamusSpeedBoosterBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts,
        IRoomSamus samus)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!SpeedBoosterBlocks.TryResolve(
                bts,
                _area,
                out SpeedBoosterBlockPlmDefinition definition))
        {
            return false;
        }

        // Bank $94's special-block dispatcher clears carry while indexing BTS.
        // Spawn_PLM returns that carry unchanged when every slot is occupied:
        // setup never runs, so its no-Speed-Booster rejection cannot make this
        // contact solid. Preserve the block word; this is not a break animation.
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
        slot.RestoreLevelWord = block.PackedWord
            .WithVisualBlockIndex(RoomPlmVisualBlockDefinitions.SpeedBoosterParent)
            .Raw;
        slot.StartAnimation(new SpeedBoosterBlockPlmAnimation(definition.Program));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        level.SetForegroundEntry(
            blockIndex,
            slot.RestoreLevelWord
                .WithCollisionType(RoomCollisionType.Air)
                .Raw);
        return true;
    }

    /// <summary>Selects the normal-bomb or Power Bomb reaction list; only the latter executes its packed sound instruction.</summary>
    public bool TrySpawnBombReactionBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts,
        SamusProjectileTypeWord projectileType)
    {
        Ensure.NotNull(level);
        if (!bts.IsNormalReactionIndex(16))
        {
            throw new ArgumentOutOfRangeException(
                nameof(bts),
                "Bomb-reaction BTS must be in the native table range zero through fifteen.");
        }

        SamusProjectileFamily projectileFamily = projectileType.Family;
        if (projectileFamily is not (SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb))
        {
            throw new ArgumentOutOfRangeException(
                nameof(projectileType),
                "Bomb-reaction setup accepts only normal-bomb or power-bomb projectile families.");
        }

        // `$84:84ED-$84F7` searches from native slot `$4E` toward `$00`. Importantly, the
        // full-pool path never calls setup and therefore must not mutate live level data.
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        slot.BlockIndex = blockIndex;
        slot.InstructionTimer = WrappingCountdown.Start(1);

        if (!bts.IsNormalReactionIndex(8))
        {
            // Table entries 8..15 are all `$84:B62F`, whose setup is a bare RTS and
            // whose instruction list is the one-word delete stream at `$84:AAE3`.
            slot.RestoreLevelWord = 0;
            PlmProgramStart.Delete.StartOn(slot);
            return true;
        }

        // Setup_CEDA discards the original low twelve bits rather than preserving the
        // visible tile number. Dimension-specific final draw lists reconstruct linked
        // extension words; the 1x1 respawn tail uses this exact PLM_Vars value.
        slot.RestoreLevelWord = block.PackedWord.WithVisualWord(RoomPlmVisualBlockDefinitions.CollisionBombParent).Raw;
        // `$84:CF0C-$CF13` adds three only for normal bombs, skipping the reaction sound
        // `{Instruction_PLM_QueueSound_Y_Lib2_Max3, $0A}`: a normal bomb starts the shared break tail.
        slot.StartAnimation(new BlockReactionPlmAnimation(projectileFamily == SamusProjectileFamily.Bomb
            ? BlockReactionPlmPrograms.BombBreak(bts.NormalReactionIndex)
            : BlockReactionPlmPrograms.ReactionBomb(bts.NormalReactionIndex)));

        level.SetForegroundEntry(blockIndex, slot.RestoreLevelWord.WithPlmDeactivation().Raw);
        return true;
    }

    /// <summary>Selects a shot-block PLM by BTS, weapon family, area table, and air-versus-solid collision semantics.</summary>
    public bool TrySpawnProjectileShotBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts,
        SamusProjectileTypeWord projectileType,
        bool solidBlock)
    {
        Ensure.NotNull(level);
        if (bts.Value == EscapeAnimalWallDefinitions.ReactionBts)
            return _escapeAnimals.TrySpawnCrittersEscapeReaction(level, blockIndex, projectileType);
        bool areaDependent = bts.UsesAreaReactionTable;
        if (areaDependent && !bts.IsAreaReactionIndex(8))
        {
            throw new UnsupportedShotBlockReactionException(
                nameof(bts),
                "Area-dependent shootable BTS must address one of its eight native entries.");
        }
        if (!areaDependent &&
            !bts.IsNormalReactionIndex(16) &&
            !bts.IsShootableCollisionProbe && !bts.IsUnusedShootableReaction)
        {
            throw new UnsupportedShotBlockReactionException(
                nameof(bts),
                $"Shootable BTS ${bts.Value:X2} requires a specialized reaction outside the breakable/nothing PLM dispatcher.");
        }

        // `$94:9E55` checks the sign bit before Spawn_PLM for shootable air. Its solid-block
        // sibling `$94:9E73` instead performs the area-table lookup and allocates the retail
        // no-op entry. This early return is therefore collision-nibble-specific.
        if (areaDependent && !solidBlock)
            return false;

        SamusProjectileFamily projectileFamily = projectileType.Family;

        // `$84:CF2E/$CF67` clear the newly allocated PLM header when the weapon family is
        // wrong. Observably that is identical to returning with no active slot: setup never
        // changes the live word, and the next handler has nothing to process. Perform this
        // gate before the host allocation loop while preserving every accepted native path.
        if (bts.RequiresPowerBombReaction && projectileFamily is not (
            SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb))
            return false;
        if (bts.RequiresSuperMissileReaction && projectileFamily is not (
            SamusProjectileFamily.SuperMissile or SamusProjectileFamily.Bomb))
            return false;

        // Spawn_PLM scans from native slot `$4E` down and performs no setup mutation when
        // full. The projectile still receives the collision nibble's normal carry result.
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        slot.BlockIndex = blockIndex;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.RestoreLevelWord = 0;

        if (areaDependent)
        {
            // Every retail area-table target at `$94:9FC6-$9FD4` is the nothing entry.
            // Spawn_PLM still consumes a slot until its delete instruction runs.
            PlmProgramStart.Delete.StartOn(slot);
            return true;
        }

        if (bts.IsShootableCollisionProbe)
        {
            // Setup ends the projectile scan and forces its collision counter;
            // the caller publishes those scratch effects only after successful
            // allocation. This actor does not mutate terrain and deletes next pass.
            PlmProgramStart.Delete.StartOn(slot);
            return true;
        }

        if (bts.IsRespawningReaction)
        {
            // `$84:CE6B` synthesizes `$x052`, stores it for the 384-frame restoration,
            // and clears type bits `$4000/$2000/$1000` through `AND $8FFF` immediately.
            slot.RestoreLevelWord = block.PackedWord.WithVisualWord(RoomPlmVisualBlockDefinitions.ShotBlockParent).Raw;
            slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmPrograms.RespawningShot(bts.NormalReactionIndex)));
            level.SetForegroundEntry(
                blockIndex,
                slot.RestoreLevelWord.WithPlmDeactivation().Raw);
            return true;
        }

        if (bts.IsPermanentReaction)
        {
            // `$84:B3C1` keeps no restoration word: BTS four through seven are
            // permanent. The current word loses the shootable collision bits before
            // the first animated breaking frame runs later in this gameplay pass.
            slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmPrograms.PermanentShot(bts.NormalReactionIndex - 4)));
            level.SetForegroundEntry(
                blockIndex,
                block.PackedWord.WithPlmDeactivation().Raw);
            return true;
        }

        if (bts.RequiresPowerBombReaction)
        {
            if (projectileFamily == SamusProjectileFamily.Bomb)
            {
                // A normal bomb does not break this block. `$84:CF2E` redirects to the
                // one-frame visible power-bomb diagnostic without touching collision.
                slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmProgram.BombedPowerBombBlockUnused));
                return true;
            }

            // A power bomb synthesizes `$x057`, applies `AND $8FFF`, and retains the
            // exact list selected by header `$D084/$D088`. BTS eight uses the ordinary
            // four-frame breakup; BTS nine uses its shorter 3/2/1-frame counterpart.
            slot.RestoreLevelWord = block.PackedWord.WithVisualWord(RoomPlmVisualBlockDefinitions.PowerBombBlockParent).Raw;
            slot.StartAnimation(new BlockReactionPlmAnimation(bts.NormalReactionIndex == 8
                ? BlockReactionPlmProgram.RespawningPowerBombBlock
                : BlockReactionPlmProgram.PermanentPowerBombBlock));
            level.SetForegroundEntry(
                blockIndex,
                slot.RestoreLevelWord.WithPlmDeactivation().Raw);
            return true;
        }

        if (bts.RequiresSuperMissileReaction)
        {
            if (projectileFamily == SamusProjectileFamily.Bomb)
            {
                // `$84:CF67` gives ordinary bombs the analogous one-frame Super Missile
                // reveal. It deliberately leaves the live collision word untouched.
                slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmProgram.BombedSuperMissileBlockUnused));
                return true;
            }

            // Super Missiles synthesize `$x09F`. Header `$D08C` owns the respawning
            // `$CB71` list and `$D090` owns the permanent `$CC0B` list.
            slot.RestoreLevelWord = block.PackedWord.WithVisualWord(RoomPlmVisualBlockDefinitions.SuperMissileBlockParent).Raw;
            slot.StartAnimation(new BlockReactionPlmAnimation(bts.NormalReactionIndex == 10
                ? BlockReactionPlmProgram.RespawningSuperMissileBlock
                : BlockReactionPlmProgram.PermanentSuperMissileBlock));
            level.SetForegroundEntry(
                blockIndex,
                slot.RestoreLevelWord.WithPlmDeactivation().Raw);
            return true;
        }

        // `$94:9EA6` entries C..F all point to `$84:B62F`. Its empty setup leaves the
        // block alone and its one-word `$AAE3` list deletes on the next handler pass.
        PlmProgramStart.Delete.StartOn(slot);
        return true;
    }

    /// <summary>Dispatches normal-bomb special blocks through the area-independent or Brinstar reaction table.</summary>
    public bool TrySpawnBombedSpecialBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts,
        AreaId areaIndex,
        SamusProjectileTypeWord projectileType)
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
        if (!bts.UsesAreaReactionTable)
        {
            if (!bts.IsNormalReactionIndex(BombSpecialBlockReactions.Count))
                throw new ArgumentOutOfRangeException(nameof(bts),
                    "Area-independent special-block BTS is outside the cartridge reaction table.");

            program = BombSpecialBlockReactions.ProgramAt(bts.NormalReactionIndex);
        }
        else
        {
            int areaBehavior = bts.AreaReactionIndex;
            if (!bts.IsAreaReactionIndex(8))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bts),
                    "Area-dependent bomb-special BTS must address one of its eight native entries.");
            }

            program = areaIndex == AreaId.Brinstar && areaBehavior is >= 2 and <= 5
                ? PlmProgramStart.Typed(SpeedBoosterBlockProgram.BombReveal)
                : PlmProgramStart.Delete;
        }

        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        slot.RestoreLevelWord = 0;
        program.StartOn(slot);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        return true;
    }
}
