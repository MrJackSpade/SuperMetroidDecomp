using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Block reactions spawned by Samus and her weapons: grapple, bomb, crumble, Speed Booster, shot and bombed special blocks.</summary>
[RoomLifetime]
internal sealed class BlockReactionObjects
{
    /// <summary>Frees the animals a shot block holds in when a shot that breaks it hits.</summary>
    private readonly ICrittersEscapeReaction _escapeAnimals;
    /// <summary>The room's places, where a reaction's room object takes the first free one in update order.</summary>
    private readonly RoomObjectPlaceList _places;
    /// <summary>
    /// The blocks broken open: each is the block a breakup started at, whose breakup has shown its last stage and has not
    /// grown back since.
    /// </summary>
    private readonly HashSet<Position16<RoomBlocks>> _brokenOpenBlocks = [];

    /// <summary>What the parent blocks the setups write show: their installed pictures (#1580).</summary>
    private readonly IRoomObjectDrawing _drawing;

    /// <summary>Builds the block reactions from the escaping animals' reaction, the room's places and the installed pictures.</summary>
    internal BlockReactionObjects(ICrittersEscapeReaction escapeAnimals, RoomObjectPlaceList places, IRoomObjectDrawing drawing)
    {
        _escapeAnimals = escapeAnimals;
        _places = places;
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

        // The spawn takes the first free place in update order. Matching it matters if multiple room objects mutate the
        // same room on one frame, because the room-object update runs in the same order.
        if (_places.Spawn() is not { } place)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(position);
        place.Block = position;
        place.Become(respawning
            ? new RespawningGrappleBlockObject(block.Terrain)
            : new BlockReactionObject(BlockReactionChangeDefinitions.PermanentBreakableGrappleBlock, block.Terrain));

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

        // The reaction takes the first free place in update order, as every room object spawned in play does, and a full
        // room loses the spawn. One movement can set off several neighbouring blocks, each timed on its own.
        if (_places.Spawn() is not { } place)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(position);
        place.Block = position;

        // This is not the block's earlier appearance. The setup deliberately replaces the block and its mirroring with the
        // bomb-block parent; multi-block restorations then add extension blocks around this parent.
        RoomBlockAppearance parent = _drawing.SetupBlock(RoomObjectSetupBlock.BombBlockParent);
        place.Become(new BlockReactionObject(BlockReactionChanges.CollisionBomb(behavior.Bombable),
            new RoomTerrainBlock(parent, block.CollisionType)));
        level.SetTerrain(position, new RoomTerrainBlock(parent, RoomCollisionType.Air));
        return true;
    }

    /// <summary>
    /// Spawns the room object that breaks up <paramref name="crumble"/> when Samus's downward collision reaches the special
    /// block that selects it.
    /// </summary>
    /// <remarks>
    /// Only downward collision activates it: the setup keeps the block's collision kind and mirroring with the crumble
    /// parent's block as the restore block, makes the live block solid, and delays the change for four room-object
    /// passes. Crumble blocks that grow back reverse their animation and restore the parent; the others end on air. The
    /// collision check returns solid for the triggering frame on its own, whether or not the crumble took a place.
    /// </remarks>
    public bool TrySpawnSamusContactCrumbleBlock(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        ContactCrumbleBlock crumble)
    {
        Ensure.NotNull(level);
        BlockReactionChange change = BlockReactionChanges.ContactCrumble(crumble);

        if (_places.Spawn() is not { } place)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(position);
        place.Block = position;
        // The struck block keeps only its collision; the parent shows unmirrored, whatever the block's mirroring (#1581).
        RoomTerrainBlock restore = block.Terrain.WithAppearance(_drawing.SetupBlock(RoomObjectSetupBlock.CrumbleBlockParent));
        place.Become(new BlockReactionObject(change, restore));
        level.SetTerrain(position, restore.WithCollision(RoomCollisionType.SolidBlock));
        return true;
    }

    /// <summary>
    /// Runs the Speed Booster block setup for a special block selected by either the Speed Booster blocks every area has or
    /// Brinstar's own.
    /// </summary>
    /// <remarks>
    /// The setup accepts boost-counter stage four and all six directional shinespark poses. On acceptance it preserves
    /// the collision kind in the eventual restore block, substitutes the Speed Booster parent's block, and makes the
    /// live block air before the
    /// room objects' step draws the first crumble frame. Rejected contact deletes the temporary
    /// place synchronously and the block stays solid to the bomb.
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
        if (!SpeedBoosterBlocks.TryResolve(behavior, out BlockReactionChange? crumble))
        {
            return false;
        }

        // A Speed Booster block lets Samus through unless its reaction refuses her. When every place is held no reaction
        // takes a place, so nothing can refuse her: she passes and the block stays as it is, unbroken.
        if (_places.IsFull)
            return true;

        bool activeSpeedBooster = samus.IsActivelySpeedBoosting;
        bool activeShinespark = samus.IsShinesparking;
        if (!activeSpeedBooster && !activeShinespark)
            return false;

        if (_places.Spawn() is not { } place)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(position);
        place.Block = position;
        // The struck block keeps only its collision; the parent shows unmirrored, whatever the block's mirroring (#1581).
        RoomTerrainBlock restore = block.Terrain.WithAppearance(_drawing.SetupBlock(RoomObjectSetupBlock.SpeedBoosterBlockParent));
        place.Become(new BlockReactionObject(crumble, restore));
        level.SetTerrain(position, restore.WithCollision(RoomCollisionType.Air));
        return true;
    }

    /// <summary>Selects the normal-bomb or Power Bomb reactions; only the latter plays its block's sound.</summary>
    public bool TrySpawnBombReactionBlock(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        RoomBlockBehavior behavior,
        SamusProjectileType projectileType)
    {
        Ensure.NotNull(level);
        if (behavior.Bombable is not ({ } bombable and not BombableReaction.Inert))
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Bomb-reaction behaviour must be one of the bomb-block breaks or the empty reaction.");
        }

        SamusProjectileFamily projectileFamily = projectileType.Family;
        if (projectileFamily is not (SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb))
        {
            throw new ArgumentOutOfRangeException(
                nameof(projectileType),
                "Bomb-reaction setup accepts only normal-bomb or power-bomb projectile families.");
        }

        // The spawn takes the first free place in update order. Importantly, a full room never calls setup and
        // therefore must not change the live terrain.
        if (_places.Spawn() is not { } place)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(position);
        place.Block = position;

        if (bombable == BombableReaction.Nothing)
        {
            // The empty reaction's setup does nothing, and it leaves on its first update.
            place.Become(new BlockReactionObject(BlockReactionChange.MadeAtSpawn));
            return true;
        }

        // The setup discards the earlier appearance rather than preserving the visible block. Dimension-specific final
        // draws reconstruct linked extension blocks; a 1x1 block respawns with this exact restore block.
        RoomTerrainBlock restore = block.Terrain.WithAppearance(_drawing.SetupBlock(RoomObjectSetupBlock.BombBlockParent));
        // Only normal bombs skip the reaction sound: a normal bomb sets off the block's break directly.
        place.Become(new BlockReactionObject(projectileFamily == SamusProjectileFamily.Bomb
            ? BlockReactionChanges.BombBreak(bombable)
            : BlockReactionChanges.ReactionBomb(bombable), restore));

        level.SetTerrain(position, restore.Deactivated());
        return true;
    }

    /// <summary>Selects a shot-block room object by behaviour, weapon family, and air-versus-solid collision semantics.</summary>
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
        if (ShotBlockReactions.Of(behavior.Shootable) is not { } reaction)
        {
            throw new UnsupportedShotBlockReactionException(
                nameof(behavior),
                $"Shootable behaviour {behavior} has no reaction, or requires a specialized reaction outside the breakable/nothing room-object response.");
        }

        // A reaction that sets its object off only in a shootable block leaves shootable air before spawning; its
        // solid-block sibling places the object that does nothing. A reaction that sets its object off only in solid
        // blocks spawns nothing outside one.
        if (behavior.Shootable is { } shootable && shootable.SetsOffOnlyInSolidBlocks && !solidBlock)
            return false;

        SamusProjectileFamily projectileFamily = projectileType.Family;

        // A Power Bomb or Super Missile block breaks only for its own weapon or a bomb; any other shot leaves the block as it
        // is and takes no place.
        if (reaction is ShotBlockReaction.RespawningPowerBombBreak or ShotBlockReaction.PermanentPowerBombBreak &&
            projectileFamily is not (SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb))
            return false;
        if (reaction is ShotBlockReaction.RespawningSuperMissileBreak or ShotBlockReaction.PermanentSuperMissileBreak &&
            projectileFamily is not (SamusProjectileFamily.SuperMissile or SamusProjectileFamily.Bomb))
            return false;

        // The spawn takes the first free place in update order and changes nothing when the room is full. The projectile still receives the
        // collision kind's normal result.
        if (_places.Spawn() is not { } place)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(position);
        place.Block = position;

        switch (reaction)
        {
            case ShotBlockReaction.CollisionProbe:
                // Setting it up ends the shot's span of blocks, which a shot's collision does only once the room
                // object has taken a place. This object does not change terrain and leaves on its first update.
                place.Become(new BlockReactionObject(BlockReactionChange.MadeAtSpawn));
                return true;

            case ShotBlockReaction.RespawningBreak:
                // The setup keeps the collision kind with the shot-block parent's appearance for the 384-frame
                // restoration, and deactivates the live block immediately.
                RestoreAsParent(level, place, block, RoomObjectSetupBlock.ShotBlockParent,
                    BlockReactionChanges.RespawningShot(behavior.Shootable));
                return true;

            case ShotBlockReaction.PermanentBreak:
                // The deactivating setup keeps no restore block: these breaks are permanent. The current block loses its
                // shootable collision before the first animated breaking frame runs later in this pass.
                place.Become(new BlockReactionObject(BlockReactionChanges.PermanentShot(behavior.Shootable)));
                level.SetTerrain(position, block.Terrain.Deactivated());
                return true;

            case ShotBlockReaction.RespawningPowerBombBreak:
            case ShotBlockReaction.PermanentPowerBombBreak:
                if (projectileFamily == SamusProjectileFamily.Bomb)
                {
                    // A normal bomb does not break this block. The setup redirects to the one-frame visible power-bomb
                    // reveal without touching collision.
                    place.Become(new BlockReactionObject(BlockReactionChangeDefinitions.BombedPowerBombBlockReveal));
                    return true;
                }

                // A power bomb keeps the collision kind with the power-bomb parent's appearance, deactivates the live
                // block, and runs its definition's change. The respawning block uses the ordinary four-frame breakup; the
                // permanent one its shorter 3/2/1-frame counterpart.
                RestoreAsParent(level, place, block, RoomObjectSetupBlock.PowerBombBlockParent,
                    reaction == ShotBlockReaction.RespawningPowerBombBreak
                        ? BlockReactionChangeDefinitions.RespawningPowerBombBlock
                        : BlockReactionChangeDefinitions.PermanentPowerBombBlock);
                return true;

            case ShotBlockReaction.RespawningSuperMissileBreak:
            case ShotBlockReaction.PermanentSuperMissileBreak:
                if (projectileFamily == SamusProjectileFamily.Bomb)
                {
                    // Ordinary bombs get the analogous one-frame Super Missile reveal. It deliberately leaves the live
                    // block untouched.
                    place.Become(new BlockReactionObject(BlockReactionChangeDefinitions.BombedSuperMissileBlockReveal));
                    return true;
                }

                // Super Missiles keep the collision kind with the Super Missile parent's appearance; one definition owns the
                // respawning change and the other the permanent one.
                RestoreAsParent(level, place, block, RoomObjectSetupBlock.SuperMissileBlockParent,
                    reaction == ShotBlockReaction.RespawningSuperMissileBreak
                        ? BlockReactionChangeDefinitions.RespawningSuperMissileBlock
                        : BlockReactionChangeDefinitions.PermanentSuperMissileBlock);
                return true;

            case ShotBlockReaction.Nothing:
                // The empty reactions leave the block alone, and the object still holds a place until its first update.
                place.Become(new BlockReactionObject(BlockReactionChange.MadeAtSpawn));
                return true;

            default:
                throw new InvalidOperationException($"Undefined {nameof(ShotBlockReaction)} {reaction}.");
        }
    }

    /// <summary>Spawns the room object of a special block a normal bomb hits, by the block's reaction.</summary>
    public bool TrySpawnBombedSpecialBlock(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        RoomBlockBehavior behavior,
        SamusProjectileType projectileType)
    {
        Ensure.NotNull(level);
        if (projectileType.Family == SamusProjectileFamily.PowerBomb)
            return false;
        if (projectileType.Family != SamusProjectileFamily.Bomb)
        {
            throw new ArgumentOutOfRangeException(
                nameof(projectileType),
                "The bomb-special setup accepts only normal-bomb and Power-Bomb families.");
        }

        if (behavior.Special is null)
            throw new ArgumentOutOfRangeException(nameof(behavior), "The special block's behaviour has no special-block reaction.");
        BlockReactionChange change = BombSpecialBlockReactions.ChangeFor(behavior.Special);

        if (_places.Spawn() is not { } place)
            return false;

        place.Block = position;
        place.Become(new BlockReactionObject(change));
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
        if (draw is RoomObjectFrameDraw<ShotBlockObjectDraws.BreakupDraw> { Frame.BreaksOpen: true })
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
    /// installed picture as the block <paramref name="change"/>'s reaction restores, and deactivates the live block until
    /// the reaction's first image.
    /// </summary>
    private void RestoreAsParent(RoomLevelData level, RoomObjectPlace place, RoomCollisionBlock block, RoomObjectSetupBlock parent,
        BlockReactionChange change)
    {
        RoomTerrainBlock restore = block.Terrain.WithAppearance(_drawing.SetupBlock(parent));
        place.Become(new BlockReactionObject(change, restore));
        level.SetTerrain(place.Block, restore.Deactivated());
    }
}
