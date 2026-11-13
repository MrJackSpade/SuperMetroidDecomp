namespace SuperMetroid.Core.Rooms.RoomObjects;

using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Verification access to <see cref="RoomObjectSystem"/> members production does not use.</summary>
internal static class RoomObjectSystemAccess
{
    /// <summary>Identifies PLM headers supported by the room population loader and colored-door dispatcher.</summary>
    extension(RoomObjectSystem)
    {
        /// <summary>
        /// Returns whether a room-authored header has a setup owner in the sequential loader.
        /// This is a read-only inventory seam for private-ROM audits; production still performs
        /// dispatch and contextual failure from <see cref="RoomObjectPopulationLoader.LoadRoomPopulation"/> itself.
        /// </summary>
        internal static bool IsSupportedRoomPopulationHeader(RoomObjectKind header)
        {
            if (header is RoomObjectKind.ScrollTrigger or
                RoomObjectKind.RightwardsScrollExtension or
                RoomObjectKind.LeftwardsScrollExtension or
                RoomObjectKind.DownwardsScrollExtension or
                RoomObjectKind.UpwardsScrollExtension or
                RoomObjectKind.MotherBrainGlass or RoomObjectKind.BombTorizoHand or
                RoomObjectKind.MapStation or RoomObjectKind.EnergyStation or
                RoomObjectKind.MissileStation or RoomObjectKind.ElevatorPlatform or
                RoomObjectKind.SaveStation or
                RoomObjectKind.SpeedBoosterEscape or
                RoomObjectKind.WreckedShipAttic or
                RoomObjectKind.NoobTube or
                RoomObjectKind.MetroidRoomClearance or
                RoomObjectKind.MotherBrainEscapeRoomGate or
                RoomObjectKind.DownwardGate or RoomObjectKind.DownwardGateShotBlock ||
                EyeDoorObjects.IsEyeDoorKind(header) || DraygonCannonObjects.IsDraygonCannonKind(header))
            {
                return true;
            }
            if (RoomObjectSystem.IsColoredDoorHeader(header) ||
                GreyDoorObjects.TryIdentifyGreyDoor(header, out _))
                return true;
            return CollectibleObjects.TryIdentifyPermanentCollectible(header, out _, out _);
        }

        /// <summary>Recognizes the twelve directional yellow, green, and red door headers.</summary>
        internal static bool IsColoredDoorHeader(RoomObjectKind header) => header is
            RoomObjectKind.YellowDoorFacingLeft or RoomObjectKind.YellowDoorFacingRight or
            RoomObjectKind.YellowDoorFacingUp or RoomObjectKind.YellowDoorFacingDown or
            RoomObjectKind.GreenDoorFacingLeft or RoomObjectKind.GreenDoorFacingRight or
            RoomObjectKind.GreenDoorFacingUp or RoomObjectKind.GreenDoorFacingDown or
            RoomObjectKind.RedDoorFacingLeft or RoomObjectKind.RedDoorFacingRight or
            RoomObjectKind.RedDoorFacingUp or RoomObjectKind.RedDoorFacingDown;
    }

    /// <summary>Inspects live door, collectible, and glass PLMs and drives scoped PLM verification operations.</summary>
    extension(RoomObjectSystem self)
    {
        /// <summary>Every coloured door cap in the room.</summary>
        internal IReadOnlyList<ColoredDoorObjectSnapshot> ColoredDoors =>
        [
            .. ActiveSlots(self).Select(slot => new ColoredDoorSlot(slot, Door: slot.Behaviour as ColoredDoorObject))
                .Where(entry => entry.Door is not null)
                .Select(entry => new ColoredDoorObjectSnapshot(
                    entry.Slot.Kind,
                    entry.Slot.Block,
                    entry.Slot.NativeRoomArgument,
                    (entry.Door ?? throw new InvalidOperationException("Verification requires entry.door.")).Color,
                    entry.Door.Orientation,
                    entry.Door.Phase, checked((byte)entry.Door.HitCounter))),
        ];

        /// <summary>Views of every grey door cap in the room.</summary>
        internal IReadOnlyList<GreyDoorObjectSnapshot> GreyDoorSnapshots =>
        [
            .. ActiveSlots(self).Select(slot => new GreyDoorSlot(slot, Door: slot.Behaviour as GreyDoorObject))
                .Where(entry => entry.Door is not null)
                .Select(entry => new GreyDoorObjectSnapshot(
                    entry.Slot.Kind,
                    entry.Slot.Block,
                    entry.Slot.NativeRoomArgument,
                    (entry.Door ?? throw new InvalidOperationException("Verification requires entry.door.")).Orientation,
                    entry.Door.Condition,
                    entry.Door.Phase, checked((byte)entry.Door.HitCounter), entry.Door)),
        ];

        /// <summary>Views of every eye-door eye, body and bottom in the room.</summary>
        internal IReadOnlyList<EyeDoorObjectSnapshot> EyeDoorSnapshots =>
        [
            .. ActiveSlots(self).Select(slot => new EyeDoorSlot(slot, Door: slot.Behaviour as EyeDoorEyeObject, Part: slot.Behaviour as EyeDoorPartObject))
                .Where(entry => entry.Door is not null || entry.Part is not null)
                .Select(entry => new EyeDoorObjectSnapshot(
                    entry.Slot.Kind,
                    entry.Slot.Block,
                    entry.Slot.NativeRoomArgument,
                    entry.Door is not null ? EyeDoorComponent.Eye
                        : (entry.Part ?? throw new InvalidOperationException("Verification requires an eye-door part.")).Component,
                    checked((byte)(entry.Door?.HitCounter ?? 0)),
                    entry.Slot.Behaviour,
                    entry.Door is { } eye
                        ? (eye.TakesShots ? DoorsNativeWords.EyeDoorMissileWake : RoomObjectFrameBehaviorDefinitions.SpawnDefault)
                        : entry.Part is { WaitsForOpenedDoor: true } ? DoorsNativeWords.EyeDoorOpenedDoorWake : RoomObjectFrameBehaviorDefinitions.SpawnDefault)),
        ];

        /// <summary>
        /// Publishes a projectile hit to a resident coloured or grey cap through the production trigger. A cap whose shot
        /// wake is gone (opening, closing or converted) no longer reacts, so the hit is refused.
        /// </summary>
        internal bool TryNotifyColoredDoorHit(Position16<RoomBlocks> block, SamusProjectileType projectileType)
        {
            if (self.TryNotifyEyeDoorHit(block, projectileType))
                return true;
            foreach (var slot in ActiveSlots(self))
            {
                bool? takesShots = slot.Block != block ? null : slot.Behaviour switch
                {
                    ColoredDoorObject colored => colored.TakesShots,
                    GreyDoorObject grey => grey.Waits != GreyDoorWait.Nothing,
                    _ => null,
                };
                if (takesShots is not { } reached)
                    continue;
                if (!reached)
                    return false;
                return self.ProjectileTriggers.TryNotifyResidentProjectileHit(block, projectileType);
            }
            return false;
        }


        /// <summary>Whether the cartridge's room population installed PLM <c>$D6DE</c>.</summary>
        internal bool MotherBrainGlassWasLoaded => self.MotherBrainGlass.MotherBrainGlassWasLoaded;

        /// <summary>The glass's hit count, kept after the glass leaves the room.</summary>
        internal int MotherBrainGlassRoomArgument => LiveGlass(self) is { } glass
            ? glass.Hits
            : self.MotherBrainGlass.MotherBrainGlassLastRoomArgument;

        /// <summary>The live glass, or null once it has left the room.</summary>
        internal MotherBrainGlassObject? LiveMotherBrainGlass => LiveGlass(self);

        /// <summary>
        /// Runs the production timed-frame draw for the sole live PLM, exactly as the `$84`
        /// interpreter dispatches a timer/draw pair, without injecting a synthetic program.
        /// </summary>
        internal void DrawSoleObjectFrameForVerification(RoomLevelData level,
            RoomObjectDraw draw)
        {
            RoomObjectSlot[] active = [.. ActiveSlots(self)];
            if (active.Length != 1)
                throw new InvalidOperationException("Draw probe requires exactly one active PLM slot.");
            self.Drawing.Draw(level, active[0].Block, draw);
        }

        /// <summary>
        /// Publishes the projectile word to the resident eye controller at a type-$C/BTS-$44
        /// block. The following PLM pre-instruction performs the missile-family filtering.
        /// </summary>
        internal bool TryNotifyEyeDoorHit(
            Position16<RoomBlocks> block,
            SamusProjectileType projectileType)
        {
            foreach (var slot in ActiveSlots(self))
            {
                if (slot.Block != block || slot.Behaviour is not EyeDoorEyeObject)
                    continue;

                // The pending hit is single. A later collision before the eye's update replaces it rather than
                // accumulating a host-side queue.
                slot.ProjectileHit = new ResidentObjectProjectileHit(projectileType.Family);
                return true;
            }
            return false;
        }

        /// <summary>Every live elevator-platform PLM, in processing order.</summary>
        internal IReadOnlyList<RoomObjectSlotSnapshot> ElevatorPlatforms => self.PopulationSlots
            .Where(snapshot => snapshot.Kind == RoomObjectKind.ElevatorPlatform)
            .ToArray();

        /// <summary>Views of the occupied slots, in the table's processing order.</summary>
        internal IReadOnlyList<RoomObjectSlotSnapshot> PopulationSlots =>
        [
            .. self.SlotTable.InProcessingOrder
                .Select((slot, position) => new IndexedObjectSlot(slot, position))
                .Where(entry => entry.Slot.Active)
                .Select(entry => new RoomObjectSlotSnapshot(
                    ProcessingPosition: entry.Index,
                    Kind: entry.Slot.Kind,
                    Block: entry.Slot.Block,
                    RoomArgument: entry.Slot.NativeRoomArgument,
                    ProjectileHit: entry.Slot.ProjectileHit,
                    Behaviour: entry.Slot.Behaviour,
                    SpentCap: entry.Slot.Argument is SpentDoorArgument)),
        ];

        /// <summary>
        /// Publishes contact with BTS $47-$4D to its resident station. This is the same generic
        /// type-$B collision seam used by item and scroll PLMs; no room identity participates.
        /// </summary>
        internal bool TryNotifyStationTouch(RoomLevelData level, Position16<RoomBlocks> accessBlock, byte behavior)
            => self.TryNotifyStationTouch(level, accessBlock, RoomBlockBehaviorCodec.Decode(behavior));

        /// <summary>Typed BTS overload used by room collision dispatch.</summary>
        internal bool TryNotifyStationTouch(RoomLevelData level, Position16<RoomBlocks> accessBlock, RoomBlockBehavior behavior)
            => self.Stations.TryNotifyStationCollision(level, accessBlock, behavior, SamusPoseId.LastUnauthoredPose.ForRoomCollision(), Axis.Horizontal, true, StationSetupGate.Bypassed);

        /// <summary>
        /// Runs setup <c>$84:CFB5</c> for BTS one or two and installs the corresponding PLM.
        /// </summary>
        /// <returns>False only when every PLM slot is occupied.</returns>
        internal bool TrySpawnBreakableGrappleBlock(
            RoomLevelData level,
            Position16<RoomBlocks> position,
            byte behavior)
            => self.BlockReactions.TrySpawnBreakableGrappleBlock(
                level,
                position,
                RoomBlockBehaviorCodec.Decode(behavior));

        /// <summary>
        /// Spawns the bank-$84 collision PLM selected by type-$F BTS zero through seven.
        /// </summary>
        /// <remarks>
        /// The caller has already satisfied setup <c>$84:CE83</c>'s speed/screw pose gate. Setup
        /// saves <c>(levelWord &amp; $F000) | $0058</c> in <c>PLM_Vars</c>, then clears only the
        /// collision nibble in level data and returns carry clear so Samus continues moving.
        /// BTS 0..3 later redraw a linked 1x1/2x1/1x2/2x2 collision shape after the exact
        /// 384-frame hold; BTS 4..7 delete after their four-frame break animation.
        /// </remarks>
        /// <returns>
        /// True when a slot was claimed. False preserves <c>Spawn_PLM</c>'s full-pool
        /// behavior: setup never ran, so the level word remains untouched even though bank $94
        /// inherited carry clear and lets the current movement scan continue.
        /// </returns>
        internal bool TrySpawnCollisionBombBlock(
            RoomLevelData level,
            Position16<RoomBlocks> position,
            byte behavior)
            => self.BlockReactions.TrySpawnCollisionBombBlock(level, position, RoomBlockBehaviorCodec.Decode(behavior));

        /// <summary>
        /// Spawns the bank-$84 shot/bombed/grappled-reaction PLM selected by bombable BTS.
        /// </summary>
        /// <remarks>
        /// This is setup <c>$84:CEDA</c>, not the collision setup above. A normal bomb family
        /// (<c>$0500</c>) advances the entry instruction pointer by three bytes, deliberately
        /// skipping its leading sound-$0A opcode because the bomb explosion already owns its
        /// sound. A power bomb (<c>$0300</c>) retains that opcode. Both accepted projectile
        /// families synthesize <c>(levelWord &amp; $F000) | $0058</c> for later restoration and
        /// then apply <c>AND $8FFF</c> to live terrain. Thus a type-$F solid bomb block remains
        /// temporarily type-$8 solid until the same frame's PLM pass draws its first air frame,
        /// while a type-$7 bombable-air parent becomes ordinary air immediately.
        ///
        /// BTS 8..15 point at <c>PLMEntries_nothing</c>. Native code still allocates a slot and
        /// deletes it on the next handler pass, so this implementation retains that otherwise
        /// invisible resource/timing effect. A negative BTS is filtered by bank $94 before this
        /// method is called because it denotes an area-dependent/duplicate path.
        /// </remarks>
        /// <returns>False only when every PLM slot is occupied.</returns>
        internal bool TrySpawnBombReactionBlock(
            RoomLevelData level,
            Position16<RoomBlocks> position,
            byte behavior,
            SamusProjectileType projectileType)
            => self.BlockReactions.TrySpawnBombReactionBlock(
                level,
                position,
                RoomBlockBehaviorCodec.Decode(behavior),
                projectileType);

        /// <summary>
        /// Runs the normal-bomb branch of the shootable-air/block entries selected at
        /// <c>$94:9EA6</c> and installs their exact bank-$84 instruction list.
        /// </summary>
        /// <remarks>
        /// BTS 0..3 use setup <c>$84:CE6B</c>: synthesize <c>$x052</c> for restoration and
        /// apply <c>AND $8FFF</c> to the live word. BTS 4..7 use setup <c>$84:B3C1</c>, which
        /// applies that AND directly to the original word and never restores it. BTS 8/9 and
        /// A/B normally require a power bomb or super missile; a normal bomb redirects to the
        /// tiny reveal lists at <c>$C91C/$C922</c>. BTS C..F still allocate the retail
        /// <c>PLMEntries_nothing</c> slot and delete it during the next handler pass. Negative
        /// type-$C BTS uses an eight-entry area table whose retail entries are also all no-ops;
        /// it retains that allocation even though type-$4 takes an early return in bank $94.
        /// </remarks>
        internal bool TrySpawnBombedShootableBlock(
            RoomLevelData level,
            Position16<RoomBlocks> position,
            byte behavior,
            SamusProjectileType projectileType)
            => self.TrySpawnBombedShootableBlock(
                level,
                position,
                RoomBlockBehaviorCodec.Decode(behavior),
                projectileType);

        /// <summary>Typed BTS overload used by bomb collision dispatch.</summary>
        internal bool TrySpawnBombedShootableBlock(
            RoomLevelData level,
            Position16<RoomBlocks> position,
            RoomBlockBehavior bts,
            SamusProjectileType projectileType)
        {
            Ensure.NotNull(level);
            bool areaDependent = bts.Reaction.IsAreaSpecific;
            if (areaDependent && !bts.Reaction.IsAreaEntryBelow(8))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bts),
                    "Area-dependent shootable BTS must address one of its eight native entries.");
            }
            if (!areaDependent && !bts.Reaction.IsCommonEntryBelow(16))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bts),
                    "Translated normal-bomb shootable BTS must be in range zero through fifteen.");
            }
            if (projectileType.Family != SamusProjectileFamily.Bomb)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(projectileType),
                    "This setup translation currently accepts the normal-bomb family only.");
            }

            // Spawn_PLM's descending free-slot search happens before any setup routine. A full
            // pool must therefore leave both level data and BTS completely untouched.
            if (self.SlotTable.Spawn() is not { } slot)
                return false;

            RoomCollisionBlock block = level.GetCollisionBlock(position);
            slot.Block = position;

            if (areaDependent)
            {
                // `$94:9E8D-$9E9E` indexes the current area's eight-entry table before
                // Spawn_PLM. Every retail entry at `$94:9F46-$9FC4` is PLMEntries_nothing.
                slot.Become(new BlockReactionObject(BlockReactionChange.MadeAtSpawn));
                return true;
            }

            ShotBlockReaction? reaction = ShotBlockReactions.Of(bts.Reaction);
            if (reaction == ShotBlockReaction.RespawningBreak)
            {
                // The setup throws away the original appearance. The shot-block parent with the block's collision kind
                // is both the restore block and the source of the deactivated live block.
                RoomTerrainBlock restore = block.Terrain.WithAppearance(self.Drawing.SetupBlock(RoomObjectSetupBlock.ShotBlockParent));
                slot.RestoreTerrain = restore;
                slot.Become(new BlockReactionObject(BlockReactionChanges.RespawningShot(ShotBlockReactions.BreakShape(bts.Reaction))));
                level.SetTerrain(position, restore.Deactivated());
                return true;
            }

            if (reaction == ShotBlockReaction.PermanentBreak)
            {
                // The deactivating setup does not synthesize a restore block. Deactivation turns shootable air into
                // ordinary air and a shootable block into a solid block until the same-frame program draws its first
                // breaking frame.
                slot.Become(new BlockReactionObject(BlockReactionChanges.PermanentShot(ShotBlockReactions.BreakShape(bts.Reaction))));
                level.SetTerrain(position, block.Terrain.Deactivated());
                return true;
            }

            if (reaction is ShotBlockReaction.RespawningPowerBombBreak or ShotBlockReaction.PermanentPowerBombBreak)
            {
                // CF2E sees projectile family `$0500` and replaces the entry's normal
                // power-bomb animation pointer with the one-frame visible `$C057` reveal.
                slot.Become(new BlockReactionObject(BlockReactionChangeDefinitions.BombedPowerBombBlockUnused));
                return true;
            }

            if (reaction is ShotBlockReaction.RespawningSuperMissileBreak or ShotBlockReaction.PermanentSuperMissileBreak)
            {
                // CF67 performs the analogous redirect to visible super-missile word
                // `$C09F`; it neither clears collision nor queues the shot-block sound.
                slot.Become(new BlockReactionObject(BlockReactionChangeDefinitions.BombedSuperMissileBlockUnused));
                return true;
            }

            slot.Become(new BlockReactionObject(BlockReactionChange.MadeAtSpawn));
            return true;
        }

        /// <summary>
        /// Spawns the projectile reaction selected by a type-$4/$C shootable block's BTS byte.
        /// </summary>
        /// <remarks>
        /// `$94:9E55/$9E73` share table `$94:9EA6` for beams, missiles, bombs, and grapple.
        /// Entries zero through three select `$84:CE6B`'s respawning shot-block setup; entries
        /// four through seven select `$84:B3C1`'s permanent deactivation setup. Entries eight
        /// and nine run `$84:CF2E`'s power-bomb-family gate, while A and B run `$84:CF67`'s
        /// Super-Missile-family gate. Entries C through F are the retail no-op PLM header,
        /// and the deliberate seventeenth entry $10 selects one-frame header `$84:B974`.
        ///
        /// Negative BTS behaves differently for the two collision nibbles. Shootable air exits
        /// without spawning anything, while shootable solid indexes an area table whose retail
        /// entries are all `PLMEntries_nothing`; preserve that otherwise invisible allocation.
        /// The normal-bomb `$0500` reveal redirects are retained because bombed block reactions
        /// call the same table. All other rejected families reproduce setup's cleared PLM header:
        /// no terrain mutation and no live slot survives the synchronous Spawn_PLM call.
        /// </remarks>
        internal bool TrySpawnProjectileShotBlock(
            RoomLevelData level,
            Position16<RoomBlocks> position,
            byte behavior,
            SamusProjectileType projectileType,
            bool solidBlock)
            => self.BlockReactions.TrySpawnProjectileShotBlock(
                level,
                position,
                RoomBlockBehaviorCodec.Decode(behavior),
                projectileType,
                solidBlock);

        /// <summary>
        /// Spawns the special-block reveal selected by <c>$94:9D71-$9E53</c> for a bomb-family
        /// boundary visit.
        /// </summary>
        /// <remarks>
        /// Nonnegative BTS 0..7 selects a dimensioned crumble reveal, 8..D selects the native
        /// no-op entry, and E/F reveals a speed-booster block. A negative BTS selects an
        /// eight-word area table: only Brinstar entries 2..5 reveal speed blocks; every other
        /// bomb-special area entry is <c>PLMEntries_nothing</c>. Setup <c>$84:CFA0</c> accepts
        /// normal bombs without mutating terrain, so the visible type-$B word arrives on the
        /// first PLM handler pass and the object deletes on the following pass. Power Bomb
        /// family `$0300` still reaches Spawn_PLM, but `$84:CFA0` clears the new PLM header
        /// synchronously; observably no terrain changes and no active slot survives.
        /// </remarks>
        internal bool TrySpawnBombedSpecialBlock(
            RoomLevelData level,
            Position16<RoomBlocks> position,
            byte behavior,
            AreaId areaIndex,
            SamusProjectileType projectileType)
            => self.BlockReactions.TrySpawnBombedSpecialBlock(
                level,
                position,
                RoomBlockBehaviorCodec.Decode(behavior),
                areaIndex,
                projectileType);

        /// <summary>
        /// Executes <c>PLM_Handler</c>'s timer/instruction portion for all translated slots. Drawn blocks change the level
        /// data, which BG1 shows (#1346).
        /// </summary>
        internal void Step(
            RoomLevelData level,
            RoomScrollGrid? scrolls = null,
            ushort enemyDeaths = 0,
            byte enemyDeathQuota = 0)
        {
            // A check's pass is an update of its own: it begins the record the room objects' sounds join, as the runtime's
            // update begins its own.
            self.Sounds.Requests.BeginUpdate();
            self.Step(level, new RoomObjectFrameInputs { Scrolls = scrolls, EnemyDeaths = enemyDeaths, EnemyDeathQuota = enemyDeathQuota, ControllerNewInput = 0 });
        }
    }

    /// <summary>The room's occupied places, the last in update order first, as the checks have always listed them.</summary>
    private static IEnumerable<RoomObjectSlot> ActiveSlots(RoomObjectSystem roomObjects) =>
        roomObjects.SlotTable.FromLastPlace.Where(slot => slot.Active);

    /// <summary>The live Mother Brain glass, as the system's own search finds it.</summary>
    private static MotherBrainGlassObject? LiveGlass(RoomObjectSystem roomObjects) =>
        roomObjects.MotherBrainGlass.TryGetMotherBrainGlass(out MotherBrainGlassObject? glass) ? glass : null;
}
