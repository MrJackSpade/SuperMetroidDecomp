using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="RoomPlmSystem"/> members production does not use.</summary>
internal static class RoomPlmSystemAccess
{
    /// <summary>Identifies PLM headers supported by the room population loader and colored-door dispatcher.</summary>
    extension(RoomPlmSystem)
    {
        /// <summary>
        /// Returns whether a room-authored header has a setup owner in the sequential loader.
        /// This is a read-only inventory seam for private-ROM audits; production still performs
        /// dispatch and contextual failure from <see cref="PlmPopulationLoader.LoadRoomPopulation"/> itself.
        /// </summary>
        internal static bool IsSupportedRoomPopulationHeader(PlmHeaderId header)
        {
            if (header is PlmHeaderId.ScrollTrigger or
                PlmHeaderId.RightwardsScrollExtension or
                PlmHeaderId.LeftwardsScrollExtension or
                PlmHeaderId.DownwardsScrollExtension or
                PlmHeaderId.UpwardsScrollExtension or
                PlmHeaderId.MotherBrainGlass or PlmHeaderId.BombTorizoHand or
                PlmHeaderId.MapStation or PlmHeaderId.EnergyStation or
                PlmHeaderId.MissileStation or PlmHeaderId.ElevatorPlatform or
                PlmHeaderId.SaveStation or
                PlmHeaderId.SpeedBoosterEscape or
                PlmHeaderId.WreckedShipAttic or
                PlmHeaderId.NoobTube or
                PlmHeaderId.SetMetroidsClearedStatesWhenRequired or
                PlmHeaderId.MotherBrainEscapeRoomGate or
                PlmHeaderId.DownwardGate or PlmHeaderId.DownwardGateShotBlock ||
                EyeDoorPlms.IsEyeDoorHeader(header) || DraygonCannonPlms.IsDraygonCannonHeader(header))
            {
                return true;
            }
            if (RoomPlmSystem.IsColoredDoorHeader(header) ||
                GreyDoorPlms.TryIdentifyGreyDoor(header, out _))
                return true;
            return CollectiblePlms.TryIdentifyPermanentCollectible(header, out _, out _);
        }

        /// <summary>Recognizes the twelve directional yellow, green, and red door headers.</summary>
        internal static bool IsColoredDoorHeader(PlmHeaderId header) => header is
            PlmHeaderId.YellowDoorFacingLeft or PlmHeaderId.YellowDoorFacingRight or
            PlmHeaderId.YellowDoorFacingUp or PlmHeaderId.YellowDoorFacingDown or
            PlmHeaderId.GreenDoorFacingLeft or PlmHeaderId.GreenDoorFacingRight or
            PlmHeaderId.GreenDoorFacingUp or PlmHeaderId.GreenDoorFacingDown or
            PlmHeaderId.RedDoorFacingLeft or PlmHeaderId.RedDoorFacingRight or
            PlmHeaderId.RedDoorFacingUp or PlmHeaderId.RedDoorFacingDown;
    }

    /// <summary>Inspects live door, collectible, and glass PLMs and drives scoped PLM verification operations.</summary>
    extension(RoomPlmSystem self)
    {
        /// <summary>Every resident colored-door actor in the shared native PLM pool.</summary>
        internal IReadOnlyList<ColoredDoorPlmSnapshot> ColoredDoors =>
        [
            .. ActiveSlots(self).Select(slot => (slot, door: slot.Family as ColoredDoorPlmState))
                .Where(entry => entry.door is not null)
                .Select(entry => new ColoredDoorPlmSnapshot(
                    entry.slot.HeaderPointer,
                    entry.slot.BlockIndex,
                    checked((ushort)entry.slot.RoomArgument),
                    (entry.door ?? throw new InvalidOperationException("Verification requires entry.door.")).Color,
                    entry.slot.Animation is ColoredDoorPlmAnimation colored ? colored.Orientation
                        : throw new InvalidOperationException($"Coloured cap at block {entry.slot.BlockIndex} runs {entry.slot.Animation}."),
                    ColoredPhase(entry.slot), checked((byte)entry.door.HitCounter))),
        ];

        /// <summary>Debugger-stable views of every resident grey-door PLM.</summary>
        internal IReadOnlyList<GreyDoorPlmSnapshot> GreyDoorSnapshots =>
        [
            .. ActiveSlots(self).Select(slot => (slot, door: slot.Family as GreyDoorPlmState))
                .Where(entry => entry.door is not null)
                .Select(entry => new GreyDoorPlmSnapshot(
                    entry.slot.HeaderPointer,
                    entry.slot.BlockIndex,
                    checked((ushort)entry.slot.RoomArgument),
                    entry.slot.Animation is GreyDoorPlmAnimation grey ? grey.Variant.Orientation
                        : throw new InvalidOperationException($"Grey cap at block {entry.slot.BlockIndex} runs {entry.slot.Animation}."),
                    (entry.door ?? throw new InvalidOperationException("Verification requires entry.door.")).Condition,
                    GreyPhase(entry.slot), checked((byte)entry.door.HitCounter), entry.slot.Animation)),
        ];

        /// <summary>Debugger-visible projections of every live eye-door component.</summary>
        internal IReadOnlyList<EyeDoorPlmSnapshot> EyeDoorSnapshots =>
        [
            .. ActiveSlots(self).Select(slot => (slot, door: slot.Family as EyeDoorPlms.EyeDoorPlmState))
                .Where(entry => entry.door is not null)
                .Select(entry => new EyeDoorPlmSnapshot(
                    entry.slot.HeaderPointer,
                    entry.slot.BlockIndex,
                    checked((ushort)entry.slot.RoomArgument),
                    ((entry.door ?? throw new InvalidOperationException("Verification requires entry.door."))).Component,
                    checked((byte)(entry.door).HitCounter),
                    entry.slot.Animation,
                    PlmPreInstructionNativeRoutines.Of(entry.slot.PreInstruction))),
        ];

        /// <summary>
        /// Publishes a projectile hit to a resident coloured or grey cap through the production trigger. A cap whose shot
        /// wake is gone (opening, closing or converted) no longer reacts, so the hit is refused.
        /// </summary>
        internal bool TryNotifyColoredDoorHit(int blockIndex, SamusProjectileTypeWord projectileType)
        {
            if (self.TryNotifyEyeDoorHit(blockIndex, projectileType))
                return true;
            foreach (var slot in ActiveSlots(self))
            {
                if (slot.BlockIndex != blockIndex || slot.Family is not (ColoredDoorPlmState or GreyDoorPlmState))
                    continue;
                if (slot.PreInstruction == PlmPreInstructions.Inert)
                    return false;
                return self.ProjectileTriggers.TryNotifyResidentProjectileHit(blockIndex, projectileType);
            }
            return false;
        }


        /// <summary>Whether the cartridge's room population installed PLM <c>$D6DE</c>.</summary>
        internal bool MotherBrainGlassWasLoaded => self.MotherBrainGlass.MotherBrainGlassWasLoaded;

        /// <summary>Current native room argument, retained after the header deletes itself.</summary>
        internal int MotherBrainGlassRoomArgument => MotherBrainGlassSlot(self) is { } slot
            ? slot.RoomArgument
            : self.MotherBrainGlass.MotherBrainGlassLastRoomArgument;

        /// <summary>The glass's typed program, or null after deletion.</summary>
        internal PlmAnimation? MotherBrainGlassAnimation => MotherBrainGlassSlot(self)?.Animation;

        /// <summary>
        /// Runs the production timed-frame draw for the sole live PLM, exactly as the `$84`
        /// interpreter dispatches a timer/draw pair, without injecting a synthetic program.
        /// </summary>
        internal void DrawSolePlmFrameForVerification(RoomLevelData level,
            BackgroundTilemapStreamer streamer, ushort drawPointer)
        {
            PlmSlot[] active = [.. ActiveSlots(self)];
            if (active.Length != 1)
                throw new InvalidOperationException("Draw probe requires exactly one active PLM slot.");
            self.Drawing.DrawPlmInstruction(level, streamer, active[0].HeaderPointer, active[0].BlockIndex, drawPointer, default(PlmView));
        }

        /// <summary>
        /// Redirects the sole live PLM to a constructed bank-$84 program for interpreter tests.
        /// This is not a gameplay operation: the real setup still owns allocation, block
        /// position, restore word and BTS, while the test controls only the next instruction.
        /// Keeping synthetic opcodes at nonretail addresses avoids rewriting an immutable
        /// compiled cartridge program in a fake address space.
        /// </summary>
        internal void StartSoleProgramForVerification(PlmProgramStart start)
        {
            PlmSlot[] active = [.. self.SlotTable.Slots.Where(slot => slot.Active)];
            if (active.Length != 1)
                throw new InvalidOperationException("Program probe requires exactly one active PLM slot.");
            start.StartOn(active[0]);
            active[0].InstructionTimer = NativeCountdown.Start(1);
        }

        /// <summary>Current PLM instruction countdown, or zero after deletion.</summary>
        internal ushort MotherBrainGlassInstructionTimer => MotherBrainGlassSlot(self) is { } slot
            ? slot.InstructionTimer.RawValue
            : (ushort)0;

        /// <summary>
        /// Publishes the projectile word to the resident eye controller at a type-$C/BTS-$44
        /// block. The following PLM pre-instruction performs the missile-family filtering.
        /// </summary>
        internal bool TryNotifyEyeDoorHit(
            int blockIndex,
            SamusProjectileTypeWord projectileType)
        {
            foreach (var slot in self.SlotTable.Slots)
            {
                if (!slot.Active || slot.BlockIndex != blockIndex ||
                    slot.Family is not EyeDoorPlms.EyeDoorPlmState door ||
                    door.Component != EyeDoorComponent.Eye)
                {
                    continue;
                }

                // PLM_Timers is a single pending word. A later collision before the handler
                // pass replaces it rather than accumulating a host-side queue.
                slot.LoopTimer = projectileType.Raw;
                door.HasPendingHit = true;
                return true;
            }
            return false;
        }

        /// <summary>Every live elevator-platform PLM in native physical-slot order.</summary>
        internal IReadOnlyList<RoomPlmSlotSnapshot> ElevatorPlatforms => self.PopulationSlots
            .Where(snapshot => self.SlotTable.Slots[snapshot.NativeSlotIndex].HeaderPointer == PlmHeaderId.ElevatorPlatform)
            .ToArray();

        /// <summary>Physical-slot views in cartridge handler order, highest ID first.</summary>
        internal IReadOnlyList<RoomPlmSlotSnapshot> PopulationSlots
        {
            get
            {
                var snapshots = new RoomPlmSlotSnapshot[self.SlotTable.ActiveCount];
                int next = 0;
                for (int index = self.SlotTable.Slots.Count - 1; index >= 0; index--)
                    if (self.SlotTable.Slots[index].Active) snapshots[next++] = self.Snapshot(index);
                return snapshots;
            }
        }

        /// <summary>Reads one physical PLM slot's header, block, argument, and active state.</summary>
        internal RoomPlmSlotSnapshot Snapshot(int index)
        {
            PlmSlot slot = self.SlotTable.Slots[index];
            return new RoomPlmSlotSnapshot(
                NativeSlotIndex: index,
                HeaderPointer: slot.HeaderPointer,
                BlockIndex: slot.BlockIndex,
                RoomArgument: checked((ushort)slot.RoomArgument),
                PreInstruction: PlmPreInstructionNativeRoutines.Of(slot.PreInstruction),
                InstructionTimer: slot.InstructionTimer.RawValue,
                LoopTimer: checked((ushort)slot.LoopTimer),
                Animation: slot.Animation);
        }

        /// <summary>Visible BG1 mutations emitted during the most recent handler pass.</summary>
        internal IReadOnlyList<PlmTilemapUpdate> TilemapUpdates => self.Drawing.TilemapUpdates;

        /// <summary>
        /// Publishes contact with BTS $47-$4D to its resident station. This is the same generic
        /// type-$B collision seam used by item and scroll PLMs; no room identity participates.
        /// </summary>
        internal bool TryNotifyStationTouch(int accessBlockIndex, byte behavior)
            => self.TryNotifyStationTouch(accessBlockIndex, new RoomBlockBehavior(behavior));

        /// <summary>Typed BTS overload used by room collision dispatch.</summary>
        internal bool TryNotifyStationTouch(int accessBlockIndex, RoomBlockBehavior behavior)
            => self.Stations.TryNotifyStationCollision(accessBlockIndex, behavior, (SamusPoseId)byte.MaxValue, true, true, 0, true);

        /// <summary>
        /// Runs setup <c>$84:CFB5</c> for BTS one or two and installs the corresponding PLM.
        /// </summary>
        /// <returns>False only when all 40 native slots are occupied.</returns>
        internal bool TrySpawnBreakableGrappleBlock(
            RoomLevelData level,
            int blockIndex,
            byte behavior)
            => self.BlockReactions.TrySpawnBreakableGrappleBlock(
                level,
                blockIndex,
                new RoomBlockBehavior(behavior));

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
        /// True when a native slot was allocated. False preserves <c>Spawn_PLM</c>'s full-pool
        /// behavior: setup never ran, so the level word remains untouched even though bank $94
        /// inherited carry clear and lets the current movement scan continue.
        /// </returns>
        internal bool TrySpawnCollisionBombBlock(
            RoomLevelData level,
            int blockIndex,
            byte behavior)
            => self.BlockReactions.TrySpawnCollisionBombBlock(level, blockIndex, new RoomBlockBehavior(behavior));

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
        /// <returns>False only when all 40 native slots are occupied.</returns>
        internal bool TrySpawnBombReactionBlock(
            RoomLevelData level,
            int blockIndex,
            byte behavior,
            SamusProjectileTypeWord projectileType)
            => self.BlockReactions.TrySpawnBombReactionBlock(
                level,
                blockIndex,
                new RoomBlockBehavior(behavior),
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
            int blockIndex,
            byte behavior,
            SamusProjectileTypeWord projectileType)
            => self.TrySpawnBombedShootableBlock(
                level,
                blockIndex,
                new RoomBlockBehavior(behavior),
                projectileType);

        /// <summary>Typed BTS overload used by bomb collision dispatch.</summary>
        internal bool TrySpawnBombedShootableBlock(
            RoomLevelData level,
            int blockIndex,
            RoomBlockBehavior bts,
            SamusProjectileTypeWord projectileType)
        {
            Ensure.NotNull(level);
            bool areaDependent = bts.UsesAreaReactionTable;
            if (areaDependent && !bts.IsAreaReactionIndex(8))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bts),
                    "Area-dependent shootable BTS must address one of its eight native entries.");
            }
            if (!areaDependent && !bts.IsNormalReactionIndex(16))
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
            for (int slotIndex = self.SlotTable.Slots.Count - 1; slotIndex >= 0; slotIndex--)
            {
                PlmSlot slot = self.SlotTable.Slots[slotIndex];
                if (slot.Active)
                    continue;

                RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
                slot.Clear();
                slot.Active = true;
                slot.BlockIndex = blockIndex;
                slot.InstructionTimer = NativeCountdown.Start(1);
                slot.RestoreLevelWord = 0;

                if (areaDependent)
                {
                    // `$94:9E8D-$9E9E` indexes the current area's eight-entry table before
                    // Spawn_PLM. Every retail entry at `$94:9F46-$9FC4` is PLMEntries_nothing.
                    PlmProgramStart.Delete.StartOn(slot);
                    return true;
                }

                if (bts.IsRespawningReaction)
                {
                    // CE6B throws away the original visual/BTS low twelve bits. The generated
                    // `$x052` word is both PLM_Vars and the source of the temporary collision
                    // word, exactly like the retail setup's two consecutive stores.
                    slot.RestoreLevelWord = unchecked((ushort)((block.PackedWord.Raw & 0xf000) | 0x0052));
                    slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmPrograms.RespawningShot(bts.NormalReactionIndex)));
                    level.SetForegroundEntry(
                        blockIndex,
                        unchecked((ushort)(slot.RestoreLevelWord.Raw & 0x8fff)));
                    return true;
                }

                if (bts.IsPermanentReaction)
                {
                    // Setup_DeactivatePLM does not synthesize a restore word. Clearing bits
                    // `$7000` turns type-$4 air into ordinary air and type-$C solid into type-$8
                    // solid until the same-frame list draws its first breaking frame.
                    slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmPrograms.PermanentShot(bts.NormalReactionIndex - 4)));
                    level.SetForegroundEntry(
                        blockIndex,
                        unchecked((ushort)(block.PackedWord.Raw & 0x8fff)));
                    return true;
                }

                if (bts.RequiresPowerBombReaction)
                {
                    // CF2E sees projectile family `$0500` and replaces the entry's normal
                    // power-bomb animation pointer with the one-frame visible `$C057` reveal.
                    slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmProgram.BombedPowerBombBlockUnused));
                    return true;
                }

                if (bts.RequiresSuperMissileReaction)
                {
                    // CF67 performs the analogous redirect to visible super-missile word
                    // `$C09F`; it neither clears collision nor queues the shot-block sound.
                    slot.StartAnimation(new BlockReactionPlmAnimation(BlockReactionPlmProgram.BombedSuperMissileBlockUnused));
                    return true;
                }

                PlmProgramStart.Delete.StartOn(slot);
                return true;
            }

            return false;
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
            int blockIndex,
            byte behavior,
            SamusProjectileTypeWord projectileType,
            bool solidBlock)
            => self.BlockReactions.TrySpawnProjectileShotBlock(
                level,
                blockIndex,
                new RoomBlockBehavior(behavior),
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
            int blockIndex,
            byte behavior,
            AreaId areaIndex,
            SamusProjectileTypeWord projectileType)
            => self.BlockReactions.TrySpawnBombedSpecialBlock(
                level,
                blockIndex,
                new RoomBlockBehavior(behavior),
                areaIndex,
                projectileType);

        /// <summary>
        /// Executes <c>PLM_Handler</c>'s timer/instruction portion for all translated slots.
        /// </summary>
        /// <remarks>
        /// The caller supplies current layer-1 coordinates because native DrawPLM clips before
        /// queuing VRAM work. Level-data writes always occur; only the PPU-ring update is clipped.
        /// Returned updates are also exposed through the verification <c>TilemapUpdates</c> property so a debugger
        /// can inspect the exact block and destination before the runtime executes them.
        /// </remarks>
        internal IReadOnlyList<PlmTilemapUpdate> Step(
            RoomLevelData level,
            BackgroundTilemapStreamer streamer,
            PlmView view,
            RoomScrollGrid? scrolls = null,
            ushort enemyDeaths = 0,
            byte enemyDeathQuota = 0)
            => self.Step(level, streamer, view, new PlmFrameInputs { Scrolls = scrolls, EnemyDeaths = enemyDeaths, EnemyDeathQuota = enemyDeathQuota, ControllerNewInput = 0 });
    }

    /// <summary>Filters the physical PLM pool to slots whose active bit is set.</summary>
    private static IEnumerable<PlmSlot> ActiveSlots(RoomPlmSystem plms) =>
        plms.SlotTable.Slots.Where(slot => slot.Active);

    /// <summary>The live Mother Brain glass PLM slot, as the system's own slot search finds it.</summary>
    private static PlmSlot? MotherBrainGlassSlot(RoomPlmSystem plms) =>
        plms.MotherBrainGlass.TryGetMotherBrainGlassSlot(out PlmSlot? slot) ? slot : null;

    /// <summary>The test view of a coloured cap's phase, from the program its typed animation runs.</summary>
    private static ColoredDoorPhase ColoredPhase(PlmSlot slot) => slot.Animation switch
    {
        ColoredDoorPlmAnimation { Program: ColoredDoorProgram.Closing } => ColoredDoorPhase.Closing,
        ColoredDoorPlmAnimation { Program: ColoredDoorProgram.Waiting or ColoredDoorProgram.Sleep } => ColoredDoorPhase.Waiting,
        ColoredDoorPlmAnimation { Program: ColoredDoorProgram.Hit } => ColoredDoorPhase.Flashing,
        ColoredDoorPlmAnimation { Program: ColoredDoorProgram.Open } => ColoredDoorPhase.Opening,
        _ => throw new InvalidOperationException($"Coloured cap at block {slot.BlockIndex} runs {slot.Animation?.RunningProgram}."),
    };

    /// <summary>The test view of a grey cap's phase, from the program its typed animation runs.</summary>
    private static GreyDoorPhase GreyPhase(PlmSlot slot) => slot.Animation switch
    {
        GreyDoorPlmAnimation { Program: GreyDoorProgram.Closing } => GreyDoorPhase.Closing,
        GreyDoorPlmAnimation { Program: GreyDoorProgram.Waiting or GreyDoorProgram.Sleep } => GreyDoorPhase.Locked,
        GreyDoorPlmAnimation { Program: GreyDoorProgram.Activate or GreyDoorProgram.Flash or GreyDoorProgram.Hit } => GreyDoorPhase.Flashing,
        GreyDoorPlmAnimation { Program: GreyDoorProgram.Open } => GreyDoorPhase.Opening,
        _ => throw new InvalidOperationException($"Grey cap at block {slot.BlockIndex} runs {slot.Animation?.RunningProgram}."),
    };
}
