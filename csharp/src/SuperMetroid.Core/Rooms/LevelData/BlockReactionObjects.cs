using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Block reactions spawned by Samus and her weapons: grapple, bomb, crumble, Speed Booster, shot and bombed special blocks.</summary>
[RoomLifetime]
internal sealed class BlockReactionObjects
{
    /// <summary>The room's area.</summary>
    private readonly AreaId _area;
    /// <summary>Handles the animal-escape reaction reserved among the shot-block behaviours.</summary>
    private readonly ICrittersEscapeReaction _escapeAnimals;
    /// <summary>Allocates reaction room objects in descending slot order.</summary>
    private readonly RoomObjectSlotTable _slotTable;
    /// <summary>
    /// The blocks broken open: each is the block a breakup started at, whose breakup has shown its last stage and has not
    /// grown back since.
    /// </summary>
    private readonly HashSet<Position16<RoomBlocks>> _brokenOpenBlocks = [];

    /// <summary>What the parent blocks the setups write show: their installed pictures (#1580).</summary>
    private readonly IRoomObjectDrawing _drawing;

    /// <summary>Connects block reactions to the active room, escape-animal branch, shared room-object pool and installed pictures.</summary>
    internal BlockReactionObjects(AreaId area, ICrittersEscapeReaction escapeAnimals, RoomObjectSlotTable slotTable, IRoomObjectDrawing drawing)
    {
        _area = area;
        _escapeAnimals = escapeAnimals;
        _slotTable = slotTable;
        _drawing = Ensure.NotNull(drawing);
    }

    /// <summary>Starts a respawning or permanent grapple-block change while preserving its original terrain for restoration.</summary>
    public bool TrySpawnBreakableGrappleBlock(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        RoomBlockBehavior behavior)
    {
        Ensure.NotNull(level);
        bool respawning = behavior.Grapple == GrappleReaction.RespawningBreakableAnchor;
        if (!respawning && behavior.Grapple != GrappleReaction.PermanentBreakableAnchor)
            throw new ArgumentOutOfRangeException(nameof(behavior), "The behaviour is not a breakable grapple block.");

        // Spawning searches the room-object slots from the last down to the first. Matching it matters if multiple
        // room objects mutate the same room on one frame because the handler uses the same order.
        if (_slotTable.Spawn() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(position);
        slot.Block = position;
        slot.RestoreTerrain = block.Terrain;
        slot.Become(respawning
            ? new RespawningGrappleBlockObject()
            : new BlockReactionObject(BlockReactionChangeDefinitions.PermanentBreakableGrappleBlock));

        // The setup saves the complete original block but clears only its behaviour. It deliberately leaves the
        // Grapple-block collision intact until the first room-object pass later in this same gameplay frame draws the intact
        // Grapple block.
        level.SetBehavior(position, RoomBlockBehavior.None);
        return true;
    }

    /// <summary>Starts a timed collision bomb block and substitutes its parent appearance before the first room-object pass.</summary>
    public bool TrySpawnCollisionBombBlock(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        RoomBlockBehavior behavior)
    {
        Ensure.NotNull(level);
        if (!BombableReactions.IsBreak(behavior.Bombable))
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Collision bomb-block behaviour must be one of the eight bomb-block breaks.");
        }

        // The reaction spawn searches the same descending slot order used by every other
        // gameplay-spawned room object. Do not coalesce neighboring pieces: one collision scan
        // can allocate more than one independently timed object in a single movement call.
        if (_slotTable.Spawn() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(position);
        slot.Block = position;

        // This is not the block's earlier appearance. The setup deliberately replaces the block and its mirroring with the
        // bomb-block parent; multi-block restorations then add extension blocks around this parent.
        RoomBlockAppearance parent = _drawing.SetupBlock(RoomObjectSetupBlock.BombBlockParent);
        slot.RestoreTerrain = new RoomTerrainBlock(parent, block.CollisionType);
        slot.Become(new BlockReactionObject(BlockReactionChanges.CollisionBomb(behavior.Bombable)));
        level.SetTerrain(position, new RoomTerrainBlock(parent, RoomCollisionType.Air));
        return true;
    }

    /// <summary>
    /// Spawns the room object that breaks up <paramref name="crumble"/> when Samus's downward collision reaches the special
    /// block that selects it.
    /// </summary>
    /// <remarks>
    /// Only downward collision activates it: the setup keeps the block's collision kind and mirroring with the crumble
    /// parent's block as the restore block, makes the live block solid, and delays the change for four room-object handler
    /// passes. Crumble blocks that grow back reverse their animation and restore the parent; the others end on air. The
    /// collision dispatcher independently returns solid for the triggering frame, so allocation success is not a
    /// collision-result substitute.
    /// </remarks>
    public bool TrySpawnSamusContactCrumbleBlock(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        ContactCrumbleBlock crumble)
    {
        Ensure.NotNull(level);
        BlockReactionChange change = BlockReactionChanges.ContactCrumble(crumble);

        if (_slotTable.Spawn() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(position);
        slot.Block = position;
        // The struck block keeps only its collision; the parent shows unmirrored, whatever the block's mirroring (#1581).
        RoomTerrainBlock restore = block.Terrain.WithAppearance(_drawing.SetupBlock(RoomObjectSetupBlock.CrumbleBlockParent));
        slot.RestoreTerrain = restore;
        slot.Become(new BlockReactionObject(change));
        level.SetTerrain(position, restore.WithCollision(RoomCollisionType.SolidBlock));
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
    /// room-object handler draws the first crumble frame. Rejected contact deletes the temporary
    /// slot synchronously and remains solid in the caller.
    /// </remarks>
    /// <returns>True when contact passes: accepted setup, or an exhausted room-object pool.</returns>
    public bool TrySpawnSamusSpeedBoosterBlock(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        RoomBlockBehavior behavior,
        IRoomSamus samus)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!SpeedBoosterBlocks.TryResolve(
                behavior,
                _area,
                out BlockReactionChange? crumble))
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

        if (_slotTable.Spawn() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(position);
        slot.Block = position;
        // The struck block keeps only its collision; the parent shows unmirrored, whatever the block's mirroring (#1581).
        RoomTerrainBlock restore = block.Terrain.WithAppearance(_drawing.SetupBlock(RoomObjectSetupBlock.SpeedBoosterBlockParent));
        slot.RestoreTerrain = restore;
        slot.Become(new BlockReactionObject(crumble));
        level.SetTerrain(position, restore.WithCollision(RoomCollisionType.Air));
        return true;
    }

    /// <summary>Selects the normal-bomb or Power Bomb reaction list; only the latter runs its packed sound cue.</summary>
    public bool TrySpawnBombReactionBlock(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        RoomBlockBehavior behavior,
        SamusProjectileType projectileType)
    {
        Ensure.NotNull(level);
        if (behavior.Bombable is BombableReaction.PastTable or BombableReaction.AreaIgnored)
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Bomb-reaction behaviour must be one of the bomb table's breaks or its empty reaction.");
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
        if (_slotTable.Spawn() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(position);
        slot.Block = position;

        if (behavior.Bombable == BombableReaction.Nothing)
        {
            // The empty reaction's setup does nothing, and it leaves on its first update.
            slot.Become(new BlockReactionObject(BlockReactionChange.MadeAtSpawn));
            return true;
        }

        // The setup discards the earlier appearance rather than preserving the visible block. Dimension-specific final
        // draws reconstruct linked extension blocks; the 1x1 respawn tail uses this exact restore block.
        RoomTerrainBlock restore = block.Terrain.WithAppearance(_drawing.SetupBlock(RoomObjectSetupBlock.BombBlockParent));
        slot.RestoreTerrain = restore;
        // Only normal bombs skip the reaction sound: a normal bomb starts the shared break tail.
        slot.Become(new BlockReactionObject(projectileFamily == SamusProjectileFamily.Bomb
            ? BlockReactionChanges.BombBreak(behavior.Bombable)
            : BlockReactionChanges.ReactionBomb(behavior.Bombable)));

        level.SetTerrain(position, restore.Deactivated());
        return true;
    }

    /// <summary>Selects a shot-block room object by behaviour, weapon family, area table, and air-versus-solid collision semantics.</summary>
    public bool TrySpawnProjectileShotBlock(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        RoomBlockBehavior behavior,
        SamusProjectileType projectileType,
        bool solidBlock)
    {
        Ensure.NotNull(level);
        if (behavior == EscapeAnimalWallDefinitions.ReactionBehavior)
            return _escapeAnimals.TrySpawnCrittersEscapeReaction(level, position, projectileType);
        bool areaDependent = ShotBlockReactions.IsArea(behavior.Shootable);
        if (behavior.Shootable == ShootableReaction.PastAreaTable)
        {
            throw new UnsupportedShotBlockReactionException(
                nameof(behavior),
                "Area-dependent shootable behaviour must name one of its area's eight shootable entries.");
        }
        ShotBlockReaction? reaction = ShotBlockReactions.Of(behavior.Shootable);
        if (!areaDependent && reaction is null)
        {
            throw new UnsupportedShotBlockReactionException(
                nameof(behavior),
                $"Shootable behaviour {behavior} requires a specialized reaction outside the breakable/nothing room-object dispatcher.");
        }

        // Shootable air checks the area-table selector before spawning. Its solid-block sibling instead performs the
        // area-table lookup and allocates the retail no-op entry. This early return is therefore collision-kind-specific.
        if (areaDependent && !solidBlock)
            return false;

        SamusProjectileFamily projectileFamily = projectileType.Family;

        // The power-bomb and Super Missile setups clear the newly allocated room object when the weapon family is wrong.
        // Observably that is identical to returning with no active slot: setup never changes the live block, and the
        // next handler has nothing to process. Perform this
        // gate before the room-object allocation loop while preserving every accepted path.
        if (reaction is ShotBlockReaction.RespawningPowerBombBreak or ShotBlockReaction.PermanentPowerBombBreak &&
            projectileFamily is not (SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb))
            return false;
        if (reaction is ShotBlockReaction.RespawningSuperMissileBreak or ShotBlockReaction.PermanentSuperMissileBreak &&
            projectileFamily is not (SamusProjectileFamily.SuperMissile or SamusProjectileFamily.Bomb))
            return false;

        // The spawn scans from the highest slot down and changes nothing when full. The projectile still receives the
        // collision kind's normal result.
        if (_slotTable.Spawn() is not { } slot)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(position);
        slot.Block = position;

        switch (reaction)
        {
            case null:
                // Every retail area-table target is the nothing entry. The spawn still holds a place until its first
                // update.
                slot.Become(new BlockReactionObject(BlockReactionChange.MadeAtSpawn));
                return true;

            case ShotBlockReaction.CollisionProbe:
                // Setup ends the projectile scan and forces its collision counter;
                // the caller publishes those scan effects only after successful
                // allocation. This object does not change terrain and leaves on its first update.
                slot.Become(new BlockReactionObject(BlockReactionChange.MadeAtSpawn));
                return true;

            case ShotBlockReaction.RespawningBreak:
                // The setup keeps the collision kind with the shot-block parent's appearance for the 384-frame
                // restoration, and deactivates the live block immediately.
                RestoreAsParent(level, slot, block, RoomObjectSetupBlock.ShotBlockParent,
                    new BlockReactionObject(BlockReactionChanges.RespawningShot(behavior.Shootable)));
                return true;

            case ShotBlockReaction.PermanentBreak:
                // The deactivating setup keeps no restore block: these breaks are permanent. The current block loses its
                // shootable collision before the first animated breaking frame runs later in this pass.
                slot.Become(new BlockReactionObject(BlockReactionChanges.PermanentShot(behavior.Shootable)));
                level.SetTerrain(position, block.Terrain.Deactivated());
                return true;

            case ShotBlockReaction.RespawningPowerBombBreak:
            case ShotBlockReaction.PermanentPowerBombBreak:
                if (projectileFamily == SamusProjectileFamily.Bomb)
                {
                    // A normal bomb does not break this block. The setup redirects to the one-frame visible power-bomb
                    // reveal without touching collision.
                    slot.Become(new BlockReactionObject(BlockReactionChangeDefinitions.BombedPowerBombBlockReveal));
                    return true;
                }

                // A power bomb keeps the collision kind with the power-bomb parent's appearance, deactivates the live
                // block, and runs its definition's change. The respawning block uses the ordinary four-frame breakup; the
                // permanent one its shorter 3/2/1-frame counterpart.
                RestoreAsParent(level, slot, block, RoomObjectSetupBlock.PowerBombBlockParent,
                    new BlockReactionObject(reaction == ShotBlockReaction.RespawningPowerBombBreak
                        ? BlockReactionChangeDefinitions.RespawningPowerBombBlock
                        : BlockReactionChangeDefinitions.PermanentPowerBombBlock));
                return true;

            case ShotBlockReaction.RespawningSuperMissileBreak:
            case ShotBlockReaction.PermanentSuperMissileBreak:
                if (projectileFamily == SamusProjectileFamily.Bomb)
                {
                    // Ordinary bombs get the analogous one-frame Super Missile reveal. It deliberately leaves the live
                    // block untouched.
                    slot.Become(new BlockReactionObject(BlockReactionChangeDefinitions.BombedSuperMissileBlockReveal));
                    return true;
                }

                // Super Missiles keep the collision kind with the Super Missile parent's appearance; one definition owns the
                // respawning change and the other the permanent one.
                RestoreAsParent(level, slot, block, RoomObjectSetupBlock.SuperMissileBlockParent,
                    new BlockReactionObject(reaction == ShotBlockReaction.RespawningSuperMissileBreak
                        ? BlockReactionChangeDefinitions.RespawningSuperMissileBlock
                        : BlockReactionChangeDefinitions.PermanentSuperMissileBlock));
                return true;

            case ShotBlockReaction.Nothing:
                // The empty reaction and the empty entries leave the block alone, and the object leaves on its first
                // update.
                slot.Become(new BlockReactionObject(BlockReactionChange.MadeAtSpawn));
                return true;

            default:
                throw new InvalidOperationException($"Undefined {nameof(ShotBlockReaction)} {reaction}.");
        }
    }

    /// <summary>Dispatches normal-bomb special blocks through the area-independent or Brinstar reaction table.</summary>
    public bool TrySpawnBombedSpecialBlock(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        RoomBlockBehavior behavior,
        AreaId areaIndex,
        SamusProjectileType projectileType)
    {
        Ensure.NotNull(level);
        _ = AreaIds.Require(areaIndex);
        if (projectileType.Family == SamusProjectileFamily.PowerBomb)
            return false;
        if (projectileType.Family != SamusProjectileFamily.Bomb)
        {
            throw new ArgumentOutOfRangeException(
                nameof(projectileType),
                "The bomb-special setup accepts only normal-bomb and Power-Bomb families.");
        }

        BlockReactionChange change;
        SpecialBlockReaction special = behavior.Special;
        if (!BombSpecialBlockReactions.IsArea(special))
        {
            if (special == SpecialBlockReaction.PastTable)
                throw new ArgumentOutOfRangeException(nameof(behavior),
                    "Area-independent special-block behaviour has no special-block reaction.");

            change = BombSpecialBlockReactions.ChangeFor(special);
        }
        else
        {
            if (special == SpecialBlockReaction.PastAreaTable)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(behavior),
                    "Area-dependent bomb-special behaviour must name one of its area's eight bomb-special entries.");
            }

            change = areaIndex == AreaId.Brinstar && SpeedBoosterBlockObjectDefinitions.BrinstarBlocks.ContainsKey(special)
                ? SpeedBoosterBlockObjectDefinitions.BombReveal
                : BlockReactionChange.MadeAtSpawn;
        }

        if (_slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = position;
        slot.Become(new BlockReactionObject(change));
        return true;
    }

    /// <summary>
    /// Records the image a block reaction has just shown at its block: a breakup's last stage breaks the block open, and
    /// any other image, or the block's saved terrain restored as it grows back, closes it again.
    /// </summary>
    /// <param name="block">The reaction's block.</param>
    /// <param name="draw">The image's draw; null when the image restored the block's saved terrain.</param>
    internal void RecordImageShown(Position16<RoomBlocks> block, RoomObjectDraw? draw)
    {
        if (draw is RoomObjectFrameDraw<ShotBlockObjectDraws.Draw> { Frame.BreaksOpen: true })
            _brokenOpenBlocks.Add(block);
        else
            _brokenOpenBlocks.Remove(block);
    }

    /// <summary>
    /// Whether a breakup that started at <paramref name="block"/> has shown its last stage, breaking the block open,
    /// and the block has not grown back since.
    /// </summary>
    internal bool IsBrokenOpen(Position16<RoomBlocks> block) => _brokenOpenBlocks.Contains(block);

    /// <summary>
    /// Runs a breaking shot reaction's setup that keeps the struck block's collision kind under <paramref name="parent"/>'s
    /// installed picture as the block to restore, and deactivates the live block until the reaction's first image.
    /// </summary>
    private void RestoreAsParent(RoomLevelData level, RoomObjectSlot slot, RoomCollisionBlock block, RoomObjectSetupBlock parent,
        BlockReactionObject reaction)
    {
        RoomTerrainBlock restore = block.Terrain.WithAppearance(_drawing.SetupBlock(parent));
        slot.RestoreTerrain = restore;
        slot.Become(reaction);
        level.SetTerrain(slot.Block, restore.Deactivated());
    }
}
