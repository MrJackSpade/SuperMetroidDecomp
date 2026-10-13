using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>Block-reaction PLMs spawned by Samus and her weapons: grapple, bomb, crumble, Speed Booster, shot and bombed special blocks.</summary>
internal sealed class BlockReactionPlms
{
    private readonly PlmRoomBindings _bindings;
    private readonly EscapeAnimalPlms _escapeAnimals;
    private readonly PlmSlotTable _slotTable;

    internal BlockReactionPlms(PlmRoomBindings bindings, EscapeAnimalPlms escapeAnimals, PlmSlotTable slotTable)
    {
        _bindings = bindings;
        _escapeAnimals = escapeAnimals;
        _slotTable = slotTable;
    }

    /// <summary>Typed BTS overload used by grapple collision dispatch.</summary>
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
        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = block.LevelWord;
            slot.InstructionPointer = bts.GrappleReactionIndex == 1
                ? RoomPlmInstructionLists.RespawningBreakableGrappleBlock
                : RoomPlmInstructionLists.PermanentBreakableGrappleBlock;
            slot.InstructionTimer = NativeCountdown.Start(1);

            // Setup_CFB5 saves the complete original level word but clears only the low BTS
            // byte. It deliberately leaves collision type E intact until the first PLM pass
            // later in this same gameplay frame draws $E0B7.
            level.SetBehavior(blockIndex, RoomBlockBehaviorValues.None);
            return true;
        }

        return false;
    }

    /// <summary>Typed BTS overload used by Samus collision dispatch.</summary>
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
        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;

            // This is not the original visual word. Setup_CE83 deliberately replaces all
            // low twelve bits with visual block `$058`; multi-block restoration lists then
            // add type-$5/$D extension words around this type-$F parent.
            slot.RestoreLevelWord = new RoomLevelWord(RoomPlmVisualBlockIndexes.CollisionBombParent)
                .WithCollisionType(block.CollisionType).Raw;
            slot.InstructionPointer = RoomPlmInstructionLists.CollisionBombByReactionIndex(bts.NormalReactionIndex);
            slot.InstructionTimer = NativeCountdown.Start(1);
            level.SetForegroundEntry(blockIndex, RoomPlmVisualBlockIndexes.CollisionBombParent);
            return true;
        }

        return false;
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

        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer =
                RoomPlmHeaders.ContactCrumbleByReactionIndex(bts.NormalReactionIndex);
            slot.BlockIndex = blockIndex;
            RoomLevelWord restoreWord = new RoomLevelWord(block.LevelWord)
                .WithVisualBlockIndex(RoomPlmVisualBlockIndexes.ContactCrumbleParent);
            slot.RestoreLevelWord = restoreWord.Raw;
            slot.InstructionPointer =
                RoomPlmInstructionLists.ContactCrumbleByReactionIndex(bts.NormalReactionIndex);
            slot.InstructionTimer = NativeCountdown.Start(4);
            level.SetForegroundEntry(
                blockIndex,
                restoreWord.WithCollisionType(RoomCollisionType.SolidBlock).Raw);
            return true;
        }

        return false;
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
        SamusState samus)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!SpeedBoosterBlockPlmDefinitions.TryResolve(
                bts,
                _bindings.Area,
                out SpeedBoosterBlockPlmDefinition definition))
        {
            return false;
        }

        // Bank $94's special-block dispatcher clears carry while indexing BTS.
        // Spawn_PLM returns that carry unchanged when every slot is occupied:
        // setup never runs, so its no-Speed-Booster rejection cannot make this
        // contact solid. Preserve the block word; this is not a break animation.
        if (_slotTable._slots.All(slot => slot.Active))
            return true;

        bool activeSpeedBooster = samus.HorizontalSpeed.IsActivelySpeedBoosting;
        bool activeShinespark = samus.Pose is >= SamusPoseId.ShinesparkHorizontalRightPose
            and <= SamusPoseId.ShinesparkDiagonalLeftPose;
        if (!activeSpeedBooster && !activeShinespark)
            return false;

        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = definition.HeaderPointer;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = new RoomLevelWord(block.LevelWord)
                .WithVisualBlockIndex(RoomPlmVisualBlockIndexes.SpeedBoosterParent)
                .Raw;
            slot.InstructionPointer = definition.InstructionPointer;
            slot.InstructionTimer = NativeCountdown.Start(1);
            level.SetForegroundEntry(
                blockIndex,
                new RoomLevelWord(slot.RestoreLevelWord)
                    .WithCollisionType(RoomCollisionType.Air)
                    .Raw);
            return true;
        }

        return false;
    }

    /// <summary>Typed BTS overload used by bomb collision dispatch.</summary>
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
        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.InstructionTimer = NativeCountdown.Start(1);

            if (!bts.IsNormalReactionIndex(8))
            {
                // Table entries 8..15 are all `$84:B62F`, whose setup is a bare RTS and
                // whose instruction list is the one-word delete stream at `$84:AAE3`.
                slot.RestoreLevelWord = 0;
                slot.InstructionPointer = RoomPlmInstructionLists.Delete;
                return true;
            }

            // Setup_CEDA discards the original low twelve bits rather than preserving the
            // visible tile number. Dimension-specific final draw lists reconstruct linked
            // extension words; the 1x1 respawn tail uses this exact PLM_Vars value.
            slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0058));
            ushort instructionPointer = RoomPlmInstructionLists.ReactionBombByReactionIndex(bts.NormalReactionIndex);

            // `$84:CF0C-$CF13` adds three only for normal bombs. The skipped bytes are
            // `{Instruction_PLM_QueueSound_Y_Lib2_Max3, $0A}` in the odd-byte operand form.
            slot.InstructionPointer = projectileFamily == SamusProjectileFamily.Bomb
                ? unchecked((ushort)(instructionPointer + 3))
                : instructionPointer;

            ushort temporaryLevelWord = unchecked((ushort)(slot.RestoreLevelWord & 0x8fff));
            level.SetForegroundEntry(blockIndex, temporaryLevelWord);
            return true;
        }

        return false;
    }

    /// <summary>Typed BTS overload used by projectile and grapple collision dispatch.</summary>
    public bool TrySpawnProjectileShotBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts,
        SamusProjectileTypeWord projectileType,
        bool solidBlock)
    {
        Ensure.NotNull(level);
        if (bts.Value == EscapeAnimalPlmRomData.ReactionBts)
            return _escapeAnimals.TrySpawnCrittersEscapeReaction(level, blockIndex, projectileType);
        bool areaDependent = bts.UsesAreaReactionTable;
        if (areaDependent && !bts.IsAreaReactionIndex(8))
        {
            throw new ArgumentOutOfRangeException(
                nameof(bts),
                "Area-dependent shootable BTS must address one of its eight native entries.");
        }
        if (!areaDependent &&
            !bts.IsNormalReactionIndex(16) &&
            !bts.IsShootableCollisionProbe && !bts.IsUnusedShootableReaction)
        {
            throw new ArgumentOutOfRangeException(
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

        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.InstructionTimer = NativeCountdown.Start(1);
            slot.RestoreLevelWord = 0;

            if (areaDependent)
            {
                // Every retail area-table target at `$94:9FC6-$9FD4` is the nothing entry.
                // Spawn_PLM still consumes a slot until its delete instruction runs.
                slot.InstructionPointer = RoomPlmInstructionLists.Delete;
                return true;
            }

            if (bts.IsShootableCollisionProbe)
            {
                // Setup ends the projectile scan and forces its collision counter;
                // the caller publishes those scratch effects only after successful
                // allocation. This actor does not mutate terrain and deletes next pass.
                slot.InstructionPointer = RoomPlmInstructionLists.Delete;
                return true;
            }

            if (bts.IsRespawningReaction)
            {
                // `$84:CE6B` synthesizes `$x052`, stores it for the 384-frame restoration,
                // and clears type bits `$4000/$2000/$1000` through `AND $8FFF` immediately.
                slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0052));
                slot.InstructionPointer =
                    RoomPlmInstructionLists.RespawningShotBySize(bts.NormalReactionIndex);
                level.SetForegroundEntry(
                    blockIndex,
                    unchecked((ushort)(slot.RestoreLevelWord & 0x8fff)));
                return true;
            }

            if (bts.IsPermanentReaction)
            {
                // `$84:B3C1` keeps no restoration word: BTS four through seven are
                // permanent. The current word loses the shootable collision bits before
                // the first animated breaking frame runs later in this gameplay pass.
                slot.InstructionPointer =
                    RoomPlmInstructionLists.PermanentShotBySize(bts.NormalReactionIndex - 4);
                level.SetForegroundEntry(
                    blockIndex,
                    unchecked((ushort)(block.LevelWord & 0x8fff)));
                return true;
            }

            if (bts.RequiresPowerBombReaction)
            {
                if (projectileFamily == SamusProjectileFamily.Bomb)
                {
                    // A normal bomb does not break this block. `$84:CF2E` redirects to the
                    // one-frame visible power-bomb diagnostic without touching collision.
                    slot.InstructionPointer = RoomPlmInstructionLists.BombedPowerBombBlockUnused;
                    return true;
                }

                // A power bomb synthesizes `$x057`, applies `AND $8FFF`, and retains the
                // exact list selected by header `$D084/$D088`. BTS eight uses the ordinary
                // four-frame breakup; BTS nine uses its shorter 3/2/1-frame counterpart.
                slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0057));
                slot.InstructionPointer = bts.NormalReactionIndex == 8
                    ? RoomPlmInstructionLists.RespawningPowerBombBlock
                    : RoomPlmInstructionLists.PermanentPowerBombBlock;
                level.SetForegroundEntry(
                    blockIndex,
                    unchecked((ushort)(slot.RestoreLevelWord & 0x8fff)));
                return true;
            }

            if (bts.RequiresSuperMissileReaction)
            {
                if (projectileFamily == SamusProjectileFamily.Bomb)
                {
                    // `$84:CF67` gives ordinary bombs the analogous one-frame Super Missile
                    // reveal. It deliberately leaves the live collision word untouched.
                    slot.InstructionPointer = RoomPlmInstructionLists.BombedSuperMissileBlockUnused;
                    return true;
                }

                // Super Missiles synthesize `$x09F`. Header `$D08C` owns the respawning
                // `$CB71` list and `$D090` owns the permanent `$CC0B` list.
                slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x009f));
                slot.InstructionPointer = bts.NormalReactionIndex == 10
                    ? RoomPlmInstructionLists.RespawningSuperMissileBlock
                    : RoomPlmInstructionLists.PermanentSuperMissileBlock;
                level.SetForegroundEntry(
                    blockIndex,
                    unchecked((ushort)(slot.RestoreLevelWord & 0x8fff)));
                return true;
            }

            // `$94:9EA6` entries C..F all point to `$84:B62F`. Its empty setup leaves the
            // block alone and its one-word `$AAE3` list deletes on the next handler pass.
            slot.InstructionPointer = RoomPlmInstructionLists.Delete;
            return true;
        }

        // Spawn_PLM scans from native slot `$4E` down and performs no setup mutation when
        // full. The projectile still receives the collision nibble's normal carry result.
        return false;
    }

    /// <summary>Typed BTS overload used by bomb collision dispatch.</summary>
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

        ushort instructionPointer;
        if (!bts.UsesAreaReactionTable)
        {
            if (!bts.IsNormalReactionIndex(BombSpecialBlockReactions.Count))
                throw new ArgumentOutOfRangeException(nameof(bts),
                    "Area-independent special-block BTS is outside the cartridge reaction table.");

            instructionPointer = BombSpecialBlockReactions.InstructionListAt(bts.NormalReactionIndex);
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

            instructionPointer = areaIndex == AreaId.Brinstar && areaBehavior is >= 2 and <= 5
                ? RoomPlmInstructionLists.BombReactionSpeedBlock
                : RoomPlmInstructionLists.Delete;
        }

        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.InstructionPointer = instructionPointer;
            slot.InstructionTimer = NativeCountdown.Start(1);
            return true;
        }

        return false;
    }
}
