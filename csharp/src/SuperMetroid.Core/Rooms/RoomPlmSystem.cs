using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Frame-steppable room PLM owner translating movement-triggered breakable terrain and its
/// ROM-authored bank-$84 instruction lists.
/// </summary>
/// <remarks>
/// A PLM is not a Samus animation. It is an independent room object which keeps running
/// after the rope disconnects, owns collision/BTS mutation, and requests a BG1 redraw when
/// an instruction changes its level word. Keeping this state outside <c>SamusGrappleState</c>
/// is essential for the respawning block: Samus is long gone when its original word returns.
///
/// The retail allocation contains 40 word-indexed slots ($00 through $4E) and searches from
/// the highest slot downward. The C# array uses logical indices 0..39 but preserves that
/// search and handler order. The translated opcode surface is deliberately limited to the
/// collision-bomb, projectile-reaction bomb, and breakable-grapple lists. Encountering any
/// other pointer fails instead of silently inventing an effect for a still-untranslated PLM
/// family.
/// </remarks>
public sealed partial class RoomPlmSystem
{
    private const int SlotCount = 40;

    private readonly PlmSlot[] _slots = Enumerable
        .Range(0, SlotCount)
        .Select(_ => new PlmSlot())
        .ToArray();
    private readonly List<PlmSoundRequest> _soundRequests = new();
    private readonly List<PlmTilemapUpdate> _tilemapUpdates = new();

    /// <summary>Sound commands emitted during the most recent handler pass.</summary>
    public IReadOnlyList<PlmSoundRequest> SoundRequests => _soundRequests;

    /// <summary>Visible BG1 mutations emitted during the most recent handler pass.</summary>
    public IReadOnlyList<PlmTilemapUpdate> TilemapUpdates => _tilemapUpdates;

    /// <summary>Number of occupied native-equivalent PLM slots.</summary>
    public int ActiveCount => _slots.Count(slot => slot.Active);

    /// <summary>
    /// Clears the room-owned allocation during <c>$82:E3C0</c>'s destination-room setup.
    /// PLMs never survive a door transition; retaining one would let its old block index
    /// mutate unrelated level data in the newly decompressed room.
    /// </summary>
    public void Reset()
    {
        foreach (PlmSlot slot in _slots)
        {
            slot.Active = false;
            slot.HeaderPointer = 0;
            slot.BlockIndex = 0;
            slot.RestoreLevelWord = 0;
            slot.InstructionPointer = 0;
            slot.InstructionTimer = 0;
            slot.PreInstruction = 0;
            slot.RoomArgument = 0;
            slot.LoopTimer = 0;
            slot.Item = null;
            slot.Scroll = null;
            slot.ColoredDoor = null;
            slot.GreyDoor = null;
            slot.Station = null;
            slot.IsElevatorPlatform = false;
            slot.Treadmill = null;
        }
        _soundRequests.Clear();
        _tilemapUpdates.Clear();
        _stationActivationEvents.Clear();
        _saveStationLockedOut = false;
        // The progression owner belongs to the room population just discarded. Holding
        // it past Reset would let an accidentally reused PLM slot persist a hit into the
        // previous runtime/system-state instance.
        _coloredDoorSystem = null;
        _greyDoorSystem = null;
        _greyDoorArea = AreaId.Crateria;
        _isTourianStatueFinished = null;
        _hasEvent = null;
        _setEvent = null;
        ResetSpeedBoosterEscapeState();
        ResetMotherBrainGlassState();
        ResetCollectibleState();
        ResetBombTorizoHandState();
    }

    /// <summary>
    /// Spawns Botwoon's hardcoded wall PLM at room block (15,4), preserving the header's
    /// setup routine and the retail descending 40-slot allocation order.
    /// </summary>
    /// <remarks>
    /// Header <c>$B797</c> has an RTS setup and clears an already-defeated room on the next
    /// handler pass. Header <c>$B79B</c> runs setup <c>$84:AB28</c>, which delays its crumble
    /// list for exactly 64 PLM frames. The live list itself moves one block downward after
    /// each four-frame, four-image row and repeats nine times using PLM_Timers.
    /// </remarks>
    /// <returns>False only when all 40 native PLM slots are occupied.</returns>
    public bool TrySpawnBotwoonWall(RoomLevelData level, ushort header)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (header is not (
            RoomPlmHeaders.ClearBotwoonWall or
            RoomPlmHeaders.CrumbleBotwoonWall))
        {
            throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Botwoon wall header must be $B797 (clear) or $B79B (crumble).");
        }

        // Both `$B3:9590` and `$B3:9ADD` pass the literal hardcoded coordinates (15,4).
        // Validate through RoomLevelData rather than allowing a malformed fixture to install
        // a PLM whose later vertical draws would address unrelated host memory.
        int blockIndex = level.GetBlockIndex(15, 4);
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.LoopTimer = 0;
            slot.InstructionPointer = header == RoomPlmHeaders.ClearBotwoonWall
                ? RoomPlmInstructionLists.ClearBotwoonWall
                : RoomPlmInstructionLists.CrumbleBotwoonWall;

            // SpawnHardcodedPLM initializes a new slot's instruction timer to one. Only the
            // live crumble header replaces it: setup `$84:AB28` writes $0040 to the separate
            // PLM instruction-timer allocation before returning to the enemy initializer.
            slot.InstructionTimer = header == RoomPlmHeaders.CrumbleBotwoonWall
                ? (ushort)64
                : (ushort)1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns Spore Spawn's hardcoded 2x2 ceiling mutation at room block (7,30).
    /// Header $B78F animates three crumble frames and then falls through to $AB21's clear;
    /// header $B793 starts directly at that clear list for an already-defeated room.
    /// </summary>
    /// <returns>False only when all 40 native PLM slots are occupied.</returns>
    public bool TrySpawnSporeSpawnCeiling(RoomLevelData level, ushort header)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (header is not (
                RoomPlmHeaders.CrumbleSporeSpawnCeiling or
                RoomPlmHeaders.ClearSporeSpawnCeiling))
        {
            throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Spore Spawn ceiling header must be $B78F (crumble) or $B793 (clear).");
        }

        int blockIndex = level.GetBlockIndex(7, 30);
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.LoopTimer = 0;
            slot.InstructionPointer = header == RoomPlmHeaders.CrumbleSporeSpawnCeiling
                ? RoomPlmInstructionLists.CrumbleSporeSpawnCeiling
                : RoomPlmInstructionLists.ClearSporeSpawnCeiling;
            slot.InstructionTimer = 1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns one of Crocomire's five hardcoded arena PLMs at its literal room-block origin.
    /// </summary>
    /// <remarks>
    /// The instruction pointers are the words selected by entries $84:B747-$B757 after
    /// their no-op setup. The shared instruction interpreter below already understands the
    /// positive timer/draw pair and $86BC deletion used by every one of these lists.
    /// </remarks>
    /// <returns>False only when all 40 native PLM slots are occupied.</returns>
    public bool TrySpawnCrocomireArenaMutation(
        RoomLevelData level,
        byte blockX,
        byte blockY,
        ushort header)
    {
        ArgumentNullException.ThrowIfNull(level);
        ushort instructionPointer = header switch
        {
            RoomPlmHeaders.ClearCrocomireBridge => RoomPlmInstructionLists.ClearCrocomireBridge,
            RoomPlmHeaders.CrumbleCrocomireBridgeBlock => RoomPlmInstructionLists.CrumbleCrocomireBridgeBlock,
            RoomPlmHeaders.ClearCrocomireBridgeBlock => RoomPlmInstructionLists.ClearCrocomireBridgeBlock,
            RoomPlmHeaders.ClearCrocomireInvisibleWall => RoomPlmInstructionLists.ClearCrocomireInvisibleWall,
            RoomPlmHeaders.CreateCrocomireInvisibleWall => RoomPlmInstructionLists.CreateCrocomireInvisibleWall,
            _ => throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Crocomire arena header must be $B747, $B74B, $B74F, $B753, or $B757."),
        };

        int blockIndex = level.GetBlockIndex(blockX, blockY);
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.LoopTimer = 0;
            slot.InstructionPointer = instructionPointer;
            slot.InstructionTimer = 1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns Shitroid's hardcoded ten-block vertical wall mutation at `$84:B763/$B767`.
    /// Setup executes synchronously inside <c>SpawnHardcodedPLM</c>: `$B763` clears the
    /// collision nibble and `$B767` replaces it with type eight while preserving the low
    /// twelve bits of every authored level word.
    /// </summary>
    /// <returns>False when all forty native PLM slots are occupied; no terrain is changed.</returns>
    public bool TrySpawnShitroidWallMutation(
        RoomLevelData level,
        byte blockX,
        byte blockY,
        ushort header)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (header is not (
            RoomPlmHeaders.ClearBabyMetroidInvisibleWall or
            RoomPlmHeaders.CreateBabyMetroidInvisibleWall))
        {
            throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Shitroid wall header must be $B763 or $B767.");
        }

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = header;
            slot.BlockIndex = level.GetBlockIndex(blockX, blockY);
            slot.RestoreLevelWord = 0;
            slot.LoopTimer = 0;
            slot.PreInstruction = 0;
            slot.RoomArgument = 0;
            slot.InstructionPointer = RoomPlmInstructionLists.Delete;
            slot.InstructionTimer = 1;

            // Both setup routines walk downward through ten blocks in the same column.
            // The mutation is part of setup, so it must occur before the PLM handler's
            // instruction timer is allowed to consume and delete this allocated slot.
            for (int rowOffset = 0; rowOffset < 10; rowOffset++)
            {
                int blockIndex = level.GetBlockIndex(blockX, blockY + rowOffset);
                ushort current = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
                ushort replacement = header == RoomPlmHeaders.CreateBabyMetroidInvisibleWall
                    ? unchecked((ushort)((current & 0x0fff) | 0x8000))
                    : unchecked((ushort)(current & 0x0fff));
                level.SetForegroundEntry(blockIndex, replacement);
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Allocates one of Mother Brain's literal fake-death room mutations from
    /// <c>$84:B673-$84:B6C7</c>. Every listed header uses setup $B3D0 (deactivate) and a
    /// one-frame draw/delete list, so the shared PLM interpreter—not boss-specific terrain
    /// painting—owns the actual room edit and tilemap upload.
    /// </summary>
    /// <returns>False only when all forty native PLM slots are occupied.</returns>
    public bool TrySpawnMotherBrainMutation(
        RoomLevelData level,
        byte blockX,
        byte blockY,
        ushort header)
    {
        ArgumentNullException.ThrowIfNull(level);
        ushort instructionPointer = header switch
        {
            RoomPlmHeaders.FillMotherBrainsWall => RoomPlmInstructionLists.FillMotherBrainsWall,
            RoomPlmHeaders.MotherBrainsBackgroundRow2 => RoomPlmInstructionLists.MotherBrainsBackgroundRow2,
            RoomPlmHeaders.MotherBrainsBackgroundRow3 => RoomPlmInstructionLists.MotherBrainsBackgroundRow3,
            RoomPlmHeaders.MotherBrainsBackgroundRow4 => RoomPlmInstructionLists.MotherBrainsBackgroundRow4,
            RoomPlmHeaders.MotherBrainsBackgroundRow5 => RoomPlmInstructionLists.MotherBrainsBackgroundRow5,
            RoomPlmHeaders.MotherBrainsBackgroundRow6 => RoomPlmInstructionLists.MotherBrainsBackgroundRow6,
            RoomPlmHeaders.MotherBrainsBackgroundRow7 => RoomPlmInstructionLists.MotherBrainsBackgroundRow7,
            RoomPlmHeaders.MotherBrainsBackgroundRow8 => RoomPlmInstructionLists.MotherBrainsBackgroundRow8,
            RoomPlmHeaders.MotherBrainsBackgroundRow9 => RoomPlmInstructionLists.MotherBrainsBackgroundRow9,
            RoomPlmHeaders.MotherBrainsBackgroundRowA => RoomPlmInstructionLists.MotherBrainsBackgroundRowA,
            RoomPlmHeaders.MotherBrainsBackgroundRowB => RoomPlmInstructionLists.MotherBrainsBackgroundRowB,
            RoomPlmHeaders.MotherBrainsBackgroundRowC => RoomPlmInstructionLists.MotherBrainsBackgroundRowC,
            RoomPlmHeaders.MotherBrainsBackgroundRowD => RoomPlmInstructionLists.MotherBrainsBackgroundRowD,
            RoomPlmHeaders.ClearMotherBrainCeilingBlock => RoomPlmInstructionLists.ClearMotherBrainCeilingBlock,
            RoomPlmHeaders.ClearMotherBrainCeilingTube => RoomPlmInstructionLists.ClearMotherBrainCeilingTube,
            RoomPlmHeaders.ClearMotherBrainBottomMiddleSideTube => RoomPlmInstructionLists.ClearMotherBrainBottomMiddleSideTube,
            RoomPlmHeaders.ClearMotherBrainBottomMiddleTubes => RoomPlmInstructionLists.ClearMotherBrainBottomMiddleTubes,
            RoomPlmHeaders.ClearMotherBrainBottomLeftTube => RoomPlmInstructionLists.ClearMotherBrainBottomLeftTube,
            RoomPlmHeaders.ClearMotherBrainBottomRightTube => RoomPlmInstructionLists.ClearMotherBrainBottomRightTube,
            _ => throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Mother Brain mutation header is outside $B673-$B6C7's authored set."),
        };

        // SpawnHardcodedPLM probes native slots $4E,$4C,...,$00. Do not reserve a boss
        // slot or coalesce adjacent row requests: their allocation order is observable.
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = level.GetBlockIndex(blockX, blockY);
            slot.RestoreLevelWord = 0;
            slot.LoopTimer = 0;
            slot.InstructionPointer = instructionPointer;
            slot.InstructionTimer = 1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Runs setup <c>$84:CFB5</c> for BTS one or two and installs the corresponding PLM.
    /// </summary>
    /// <returns>False only when all 40 native slots are occupied.</returns>
    public bool TrySpawnBreakableGrappleBlock(
        RoomLevelData level,
        int blockIndex,
        byte behavior)
        => TrySpawnBreakableGrappleBlock(
            level,
            blockIndex,
            new RoomBlockBehavior(behavior));

    /// <summary>Typed BTS overload used by grapple collision dispatch.</summary>
    public bool TrySpawnBreakableGrappleBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (!bts.IsBreakableGrappleReaction)
            throw new ArgumentOutOfRangeException(nameof(bts), "Breakable grapple BTS must be one or two.");

        // Spawn_PLM at $84:84E7 probes $4E,$4C,...,$00. Matching it matters if multiple
        // PLMs mutate the same room on one frame because the handler uses the same order.
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = block.LevelWord;
            slot.InstructionPointer = bts.GrappleReactionIndex == 1
                ? RoomPlmInstructionLists.RespawningBreakableGrappleBlock
                : RoomPlmInstructionLists.PermanentBreakableGrappleBlock;
            slot.InstructionTimer = 1;

            // Setup_CFB5 saves the complete original level word but clears only the low BTS
            // byte. It deliberately leaves collision type E intact until the first PLM pass
            // later in this same gameplay frame draws $E0B7.
            level.SetBehavior(blockIndex, RoomBlockBehaviorValues.None);
            return true;
        }

        return false;
    }

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
    public bool TrySpawnCollisionBombBlock(
        RoomLevelData level,
        int blockIndex,
        byte behavior)
        => TrySpawnCollisionBombBlock(level, blockIndex, new RoomBlockBehavior(behavior));

    /// <summary>Typed BTS overload used by Samus collision dispatch.</summary>
    public bool TrySpawnCollisionBombBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (!bts.IsNormalReactionIndex(8))
        {
            throw new ArgumentOutOfRangeException(
                nameof(bts),
                "Collision bomb-block BTS must be in the native table range zero through seven.");
        }

        // `$84:84ED-$84F7` searches the same descending slot order used by every other
        // gameplay-spawned PLM. Do not coalesce neighboring pieces: a native collision scan
        // can allocate more than one independently timed object in a single movement call.
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;

            // This is not the original visual word. Setup_CE83 deliberately replaces all
            // low twelve bits with visual block `$058`; multi-block restoration lists then
            // add type-$5/$D extension words around this type-$F parent.
            slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0058));
            slot.InstructionPointer = RoomPlmInstructionLists.CollisionBombByReactionIndex[bts.NormalReactionIndex];
            slot.InstructionTimer = 1;
            level.ClearCollisionType(blockIndex);
            return true;
        }

        return false;
    }

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
    public bool TrySpawnBombReactionBlock(
        RoomLevelData level,
        int blockIndex,
        byte behavior,
        SamusProjectileTypeWord projectileType)
        => TrySpawnBombReactionBlock(
            level,
            blockIndex,
            new RoomBlockBehavior(behavior),
            projectileType);

    /// <summary>Typed BTS overload used by bomb collision dispatch.</summary>
    public bool TrySpawnBombReactionBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts,
        SamusProjectileTypeWord projectileType)
    {
        ArgumentNullException.ThrowIfNull(level);
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
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.InstructionTimer = 1;

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
            ushort instructionPointer = RoomPlmInstructionLists.ReactionBombByReactionIndex[bts.NormalReactionIndex];

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
    public bool TrySpawnBombedShootableBlock(
        RoomLevelData level,
        int blockIndex,
        byte behavior,
        SamusProjectileTypeWord projectileType)
        => TrySpawnBombedShootableBlock(
            level,
            blockIndex,
            new RoomBlockBehavior(behavior),
            projectileType);

    /// <summary>Typed BTS overload used by bomb collision dispatch.</summary>
    public bool TrySpawnBombedShootableBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts,
        SamusProjectileTypeWord projectileType)
    {
        ArgumentNullException.ThrowIfNull(level);
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
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.InstructionTimer = 1;
            slot.RestoreLevelWord = 0;

            if (areaDependent)
            {
                // `$94:9E8D-$9E9E` indexes the current area's eight-entry table before
                // Spawn_PLM. Every retail entry at `$94:9F46-$9FC4` is PLMEntries_nothing.
                slot.InstructionPointer = RoomPlmInstructionLists.Delete;
                return true;
            }

            if (bts.IsRespawningReaction)
            {
                // CE6B throws away the original visual/BTS low twelve bits. The generated
                // `$x052` word is both PLM_Vars and the source of the temporary collision
                // word, exactly like the retail setup's two consecutive stores.
                slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0052));
                slot.InstructionPointer =
                    RoomPlmInstructionLists.RespawningShotBySize[bts.NormalReactionIndex];
                level.SetForegroundEntry(
                    blockIndex,
                    unchecked((ushort)(slot.RestoreLevelWord & 0x8fff)));
                return true;
            }

            if (bts.IsPermanentReaction)
            {
                // Setup_DeactivatePLM does not synthesize a restore word. Clearing bits
                // `$7000` turns type-$4 air into ordinary air and type-$C solid into type-$8
                // solid until the same-frame list draws its first breaking frame.
                slot.InstructionPointer =
                    RoomPlmInstructionLists.PermanentShotBySize[bts.NormalReactionIndex - 4];
                level.SetForegroundEntry(
                    blockIndex,
                    unchecked((ushort)(block.LevelWord & 0x8fff)));
                return true;
            }

            if (bts.RequiresPowerBombReaction)
            {
                // CF2E sees projectile family `$0500` and replaces the entry's normal
                // power-bomb animation pointer with the one-frame visible `$C057` reveal.
                slot.InstructionPointer = RoomPlmInstructionLists.BombedPowerBombBlockUnused;
                return true;
            }

            if (bts.RequiresSuperMissileReaction)
            {
                // CF67 performs the analogous redirect to visible super-missile word
                // `$C09F`; it neither clears collision nor queues the shot-block sound.
                slot.InstructionPointer = RoomPlmInstructionLists.BombedSuperMissileBlockUnused;
                return true;
            }

            slot.InstructionPointer = RoomPlmInstructionLists.Delete;
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
    /// Super-Missile-family gate. Entries C through F are the retail no-op PLM header.
    ///
    /// Negative BTS behaves differently for the two collision nibbles. Shootable air exits
    /// without spawning anything, while shootable solid indexes an area table whose retail
    /// entries are all `PLMEntries_nothing`; preserve that otherwise invisible allocation.
    /// The normal-bomb `$0500` reveal redirects are retained because bombed block reactions
    /// call the same table. All other rejected families reproduce setup's cleared PLM header:
    /// no terrain mutation and no live slot survives the synchronous Spawn_PLM call.
    /// </remarks>
    public bool TrySpawnProjectileShotBlock(
        RoomLevelData level,
        int blockIndex,
        byte behavior,
        SamusProjectileTypeWord projectileType,
        bool solidBlock)
        => TrySpawnProjectileShotBlock(
            level,
            blockIndex,
            new RoomBlockBehavior(behavior),
            projectileType,
            solidBlock);

    /// <summary>Typed BTS overload used by projectile and grapple collision dispatch.</summary>
    public bool TrySpawnProjectileShotBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts,
        SamusProjectileTypeWord projectileType,
        bool solidBlock)
    {
        ArgumentNullException.ThrowIfNull(level);
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
                "Area-independent shootable BTS must be zero through fifteen.");
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

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.InstructionTimer = 1;
            slot.RestoreLevelWord = 0;

            if (areaDependent)
            {
                // Every retail area-table target at `$94:9FC6-$9FD4` is the nothing entry.
                // Spawn_PLM still consumes a slot until its delete instruction runs.
                slot.InstructionPointer = RoomPlmInstructionLists.Delete;
                return true;
            }

            if (bts.IsRespawningReaction)
            {
                // `$84:CE6B` synthesizes `$x052`, stores it for the 384-frame restoration,
                // and clears type bits `$4000/$2000/$1000` through `AND $8FFF` immediately.
                slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0052));
                slot.InstructionPointer =
                    RoomPlmInstructionLists.RespawningShotBySize[bts.NormalReactionIndex];
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
                    RoomPlmInstructionLists.PermanentShotBySize[bts.NormalReactionIndex - 4];
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

    /// <summary>
    /// Spawns the special-block reveal selected by <c>$94:9D71-$9E53</c> for a normal bomb.
    /// </summary>
    /// <remarks>
    /// Nonnegative BTS 0..7 selects a dimensioned crumble reveal, 8..D selects the native
    /// no-op entry, and E/F reveals a speed-booster block. A negative BTS selects an
    /// eight-word area table: only Brinstar entries 2..5 reveal speed blocks; every other
    /// bomb-special area entry is <c>PLMEntries_nothing</c>. Setup <c>$84:CFA0</c> accepts
    /// normal bombs without mutating terrain, so the visible type-$B word arrives on the
    /// first PLM handler pass and the object deletes on the following pass.
    /// </remarks>
    public bool TrySpawnBombedSpecialBlock(
        RoomLevelData level,
        int blockIndex,
        byte behavior,
        AreaId areaIndex,
        SamusProjectileTypeWord projectileType)
        => TrySpawnBombedSpecialBlock(
            level,
            blockIndex,
            new RoomBlockBehavior(behavior),
            areaIndex,
            projectileType);

    /// <summary>Typed BTS overload used by bomb collision dispatch.</summary>
    public bool TrySpawnBombedSpecialBlock(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts,
        AreaId areaIndex,
        SamusProjectileTypeWord projectileType)
    {
        ArgumentNullException.ThrowIfNull(level);
        _ = AreaIds.ToIndex(areaIndex);
        if (projectileType.Family != SamusProjectileFamily.Bomb)
        {
            throw new ArgumentOutOfRangeException(
                nameof(projectileType),
                "The bomb-special setup accepts the normal-bomb family in this translated path.");
        }

        ushort instructionPointer;
        if (!bts.UsesAreaReactionTable)
        {
            if (!bts.IsNormalReactionIndex(16))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bts),
                    "Area-independent special-block BTS must be zero through fifteen.");
            }

            instructionPointer = bts.NormalReactionIndex switch
            {
                <= 7 => RoomPlmInstructionLists.CrumbleRevealBySize[bts.ReactionSizeIndex],
                >= 14 => RoomPlmInstructionLists.BombReactionSpeedBlock,
                _ => RoomPlmInstructionLists.Delete,
            };
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

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.InstructionPointer = instructionPointer;
            slot.InstructionTimer = 1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Executes <c>PLM_Handler</c>'s timer/instruction portion for all translated slots.
    /// </summary>
    /// <remarks>
    /// The caller supplies current layer-1 coordinates because native DrawPLM clips before
    /// queuing VRAM work. Level-data writes always occur; only the PPU-ring update is clipped.
    /// Returned updates are also exposed through <see cref="TilemapUpdates"/> so a debugger
    /// can inspect the exact block and destination before the runtime executes them.
    /// </remarks>
    public IReadOnlyList<PlmTilemapUpdate> Step(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset,
        RoomScrollGrid? scrolls = null,
        ushort enemyDeaths = 0,
        byte enemyDeathQuota = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(streamer);
        _soundRequests.Clear();
        _tilemapUpdates.Clear();
        _stationActivationEvents.Clear();
        BeginMotherBrainGlassFrame();
        BeginCollectibleFrame();
        BeginBombTorizoHandFrame();

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (!slot.Active)
                continue;

            if (TryStepScrollPlm(bus, level, scrolls, slot))
                continue;

            if (TryStepWreckedShipTreadmill(level, streamer, slot))
                continue;

            if (TryStepStation(
                    bus,
                    level,
                    streamer,
                    slot,
                    layer1XPosition,
                    layer1YPosition,
                    bg1XOffset))
            {
                continue;
            }

            if (TryStepCollectible(
                    bus,
                    level,
                    streamer,
                    slot,
                    layer1XPosition,
                    layer1YPosition,
                    bg1XOffset))
            {
                continue;
            }

            if (TryStepColoredDoor(
                    bus,
                    level,
                    streamer,
                    slot,
                    layer1XPosition,
                    layer1YPosition,
                    bg1XOffset))
            {
                continue;
            }

            if (TryStepGreyDoor(
                    bus,
                    level,
                    streamer,
                    slot,
                    layer1XPosition,
                    layer1YPosition,
                    bg1XOffset,
                    enemyDeaths,
                    enemyDeathQuota))
            {
                continue;
            }

            RunMetroidsClearedPreInstruction(slot, enemyDeaths, enemyDeathQuota);
            RunSpeedBoosterEscapePreInstruction(bus, slot);
            RunWreckedShipAtticPreInstruction(slot);
            RunBombTorizoHandPreInstruction(slot);
            RunMotherBrainGlassPreInstruction(slot);
            if (!slot.Active)
                continue;

            slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
            if (slot.InstructionTimer != 0)
                continue;

            ExecuteInstructionStream(
                bus,
                level,
                streamer,
                slot,
                layer1XPosition,
                layer1YPosition,
                bg1XOffset,
                scrolls);
        }

        return _tilemapUpdates;
    }

    private void ExecuteInstructionStream(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset,
        RoomScrollGrid? scrolls)
    {
        // An instruction list may execute multiple negative instruction words before it
        // reaches a positive timer/draw pair. The guard catches corrupt ROM/test data while
        // remaining far above the longest straight-line chain in these two retail lists.
        for (int dispatchCount = 0; dispatchCount < 16; dispatchCount++)
        {
            ushort instruction = ReadBank84Word(bus, slot.InstructionPointer);
            if ((instruction & 0x8000) == 0)
            {
                ushort drawPointer = ReadBank84Word(
                    bus,
                    unchecked((ushort)(slot.InstructionPointer + 2)));
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
                slot.InstructionTimer = instruction;
                DrawRomInstruction(
                    bus,
                    level,
                    streamer,
                    slot.BlockIndex,
                    drawPointer,
                    layer1XPosition,
                    layer1YPosition,
                    bg1XOffset);
                return;
            }

            switch (instruction)
            {
                case RoomPlmInstructionCodes.InstallPreInstruction:
                    // `$84:86C1` is shared infrastructure, not a Mother Brain special.
                    // The operand is the bank-$84 pre-instruction run before this slot's
                    // timer on subsequent handler passes. Both glass and Bomb Torizo's
                    // sleeping hand use this exact four-byte form.
                    slot.PreInstruction = ReadBank84Word(
                        bus,
                        unchecked((ushort)(slot.InstructionPointer + 2)));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
                    continue;

                case RoomPlmInstructionCodes.QueueSoundLibrary2Maximum1:
                    // `$84:8C79` is the shot-block queue form. It has the same odd-byte
                    // operand layout as `$8C10/$8C46`, but permits only one pending sound.
                    byte singleSoundId = bus.ReadByte(
                        Bank84(unchecked((ushort)(slot.InstructionPointer + 2))));
                    _soundRequests.Add(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, singleSoundId), MaximumQueued: 1));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstructionCodes.QueueSoundLibrary2Maximum1Direct:
                    // `$84:8C7C` is the direct LDA/JSL form used by the power-bomb-gated
                    // shot-block lists; `$8C79` enters the same max-one queue routine through
                    // a short branch. Both consume the identical odd-byte sound operand.
                    byte directSingleSoundId = bus.ReadByte(
                        Bank84(unchecked((ushort)(slot.InstructionPointer + 2))));
                    _soundRequests.Add(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, directSingleSoundId), MaximumQueued: 1));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6:
                    // $84:8C10 consumes one byte after its pointer. The following timer's
                    // low byte is read as A's harmless high byte by the 16-bit LDA.
                    byte soundId = bus.ReadByte(
                        Bank84(unchecked((ushort)(slot.InstructionPointer + 2))));
                    _soundRequests.Add(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, soundId), MaximumQueued: 6));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstructionCodes.QueueSoundLibrary2Maximum3:
                    // `$84:8C46` has the same odd-byte operand layout as `$8C10`, but the
                    // collision-bomb list's crumble sound `$06` uses the stricter queue cap.
                    byte cappedSoundId = bus.ReadByte(
                        Bank84(unchecked((ushort)(slot.InstructionPointer + 2))));
                    _soundRequests.Add(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, cappedSoundId), MaximumQueued: 3));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstructionCodes.QueueSoundLibrary3Maximum6:
                    // Door lists use `$84:8C19` for open/close sounds. Like the adjacent
                    // library-two opcodes, the 16-bit native load intentionally consumes
                    // only one argument byte before advancing Y by one.
                    byte doorSoundId = bus.ReadByte(
                        Bank84(unchecked((ushort)(slot.InstructionPointer + 2))));
                    _soundRequests.Add(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, doorSoundId), MaximumQueued: 6));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstructionCodes.Goto:
                    // `$84:8724` replaces Y with the following little-endian pointer. All
                    // eight collision entry lists use it to share their dimension-specific
                    // respawning/permanent animation tail.
                    slot.InstructionPointer = ReadBank84Word(
                        bus,
                        unchecked((ushort)(slot.InstructionPointer + 2)));
                    continue;

                case RoomPlmInstructionCodes.SetEightBitTimer:
                    // `$84:874E` consumes an odd one-byte operand into PLM_Timers, not the
                    // instruction countdown. Botwoon's list seeds nine vertical rows here.
                    slot.LoopTimer = bus.ReadByte(
                        Bank84(unchecked((ushort)(slot.InstructionPointer + 2))));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstructionCodes.DecrementTimerAndGoto:
                    // `$84:873F` always decrements the independent PLM_Timers word. A
                    // nonzero result jumps through the following pointer; zero consumes it.
                    slot.LoopTimer = unchecked((ushort)(slot.LoopTimer - 1));
                    if (slot.LoopTimer != 0)
                    {
                        slot.InstructionPointer = ReadBank84Word(
                            bus,
                            unchecked((ushort)(slot.InstructionPointer + 2)));
                    }
                    else
                    {
                        slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
                    }
                    continue;

                case RoomPlmInstructionCodes.SetBotwoonScrollsBlue:
                    if (scrolls is null)
                    {
                        throw new InvalidOperationException(
                            "Botwoon's scroll PLM requires the active room scroll grid.");
                    }
                    // `$84:AB51` performs one 16-bit store of $0101 at Scrolls. Express the
                    // two raw leading cells semantically while retaining the same WRAM mirror.
                    scrolls.SetLogicalState(0, 0, RoomScrollState.Blue);
                    scrolls.SetLogicalState(1, 0, RoomScrollState.Blue);
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;

                case RoomPlmInstructionCodes.MoveBotwoonPlmDownOneBlock:
                    // Native PLM_BlockIndices are byte offsets, so AB59 adds room width
                    // twice. C# stores logical word indexes; adding width once is identical.
                    slot.BlockIndex = checked(slot.BlockIndex + level.WidthInBlocks);
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;

                case RoomPlmInstructionCodes.SetPlmBtsToOne:
                    level.SetBehavior(slot.BlockIndex, 1);
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;

                case RoomPlmInstructionCodes.SetPlmBtsFromByte:
                    // `$84:8AF1` consumes the byte immediately following the opcode. Door
                    // closing lists use it after their final visual frame to restore the
                    // shootable blue-door BTS before drawing the permanent closed cap.
                    level.SetBehavior(
                        slot.BlockIndex,
                        bus.ReadByte(Bank84(unchecked((ushort)(slot.InstructionPointer + 2)))));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstructionCodes.DrawPlmBlock:
                    // $84:8B17 restores PLM_Vars to level data, builds a one-block custom
                    // draw list, sets timer one, and exits the handler. Deletion therefore
                    // occurs on the next PLM pass rather than this restoration pass.
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    slot.InstructionTimer = 1;
                    DrawLevelWord(
                        level,
                        streamer,
                        slot.BlockIndex,
                        slot.RestoreLevelWord,
                        layer1XPosition,
                        layer1YPosition,
                        bg1XOffset);
                    return;

                case RoomPlmInstructionCodes.Delete:
                    slot.Active = false;
                    MarkBombTorizoHandDeleted(slot);
                    OnPlmDeleted(slot);
                    // Semantic family references are discriminators, not retained debug
                    // history. A later allocation can reuse this physical native slot and
                    // must never inherit door-only pre-handler behavior.
                    slot.ColoredDoor = null;
                    slot.GreyDoor = null;
                    return;

                case RoomPlmInstructionCodes.Sleep:
                    // Sleep decrements Y back onto itself and exits. Colored-door hit
                    // animations use it as the linked idle target, so expose that semantic
                    // transition while retaining the self-rewinding instruction pointer.
                    slot.InstructionTimer = 1;
                    if (slot.ColoredDoor is not null)
                        slot.ColoredDoor.Phase = ColoredDoorPhase.Waiting;
                    return;

                default:
                    if (TryExecuteBombTorizoHandInstruction(bus, slot, instruction))
                        continue;
                    if (TryExecuteMotherBrainGlassInstruction(bus, slot, instruction))
                        continue;
                    throw new InvalidDataException(
                        $"Movement-owned PLM reached uncatalogued bank-$84 instruction ${instruction:X4}.");
            }
        }

        throw new InvalidDataException("Movement-owned PLM instruction chain did not reach a timer.");
    }

    private void DrawRomInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        int blockIndex,
        ushort drawPointer,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        // `$84:861E-$86B3` treats each record as a direction/count word followed by complete
        // level words. Bit 15 means a vertical column; a clear bit means a horizontal row.
        // After its words, a signed-byte X/Y pair locates another record relative to the PLM
        // origin. A zero pair terminates. The 2x2 bomb art is consequently two horizontal
        // records: row zero, then `{dx=0,dy=1}` and row one.
        int originX = blockIndex % level.WidthInBlocks;
        int originY = blockIndex / level.WidthInBlocks;
        int entryX = originX;
        int entryY = originY;
        ushort cursor = drawPointer;

        // Retail movement-owned draw lists have at most two records and two words apiece.
        // A generous structural guard makes malformed fixtures/ROM deterministic instead of
        // allowing a missing terminator to wander through all of bank $84.
        for (int entryNumber = 0; entryNumber < 32; entryNumber++)
        {
            ushort directionAndCount = ReadBank84Word(bus, cursor);
            bool vertical = (directionAndCount & 0x8000) != 0;
            int count = directionAndCount & 0x7fff;
            if (count is <= 0 or > 0xff)
            {
                throw new InvalidDataException(
                    $"PLM draw list ${drawPointer:X4} has invalid block count {count}.");
            }
            cursor = unchecked((ushort)(cursor + 2));

            for (int blockOffset = 0; blockOffset < count; blockOffset++)
            {
                int x = entryX + (vertical ? 0 : blockOffset);
                int y = entryY + (vertical ? blockOffset : 0);
                int targetBlockIndex;
                try
                {
                    targetBlockIndex = level.GetPlmBlockIndex(x, y);
                }
                catch (ArgumentOutOfRangeException error)
                {
                    throw new InvalidDataException(
                        $"PLM draw list ${drawPointer:X4} targets block ({x},{y}) outside " +
                        "the safe native level allocation.",
                        error);
                }

                ushort levelWord = ReadBank84Word(bus, cursor);
                cursor = unchecked((ushort)(cursor + 2));
                DrawLevelWord(
                    level,
                    streamer,
                    targetBlockIndex,
                    levelWord,
                    layer1XPosition,
                    layer1YPosition,
                    bg1XOffset);
            }

            byte relativeX = bus.ReadByte(Bank84(cursor));
            byte relativeY = bus.ReadByte(Bank84(unchecked((ushort)(cursor + 1))));
            if (relativeX == 0 && relativeY == 0)
                return;

            entryX = originX + unchecked((sbyte)relativeX);
            entryY = originY + unchecked((sbyte)relativeY);
            cursor = unchecked((ushort)(cursor + 2));
        }

        throw new InvalidDataException(
            $"PLM draw list ${drawPointer:X4} did not reach its signed-offset terminator.");
    }

    private void DrawLevelWord(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        int blockIndex,
        ushort levelWord,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        level.SetPlmForegroundEntry(blockIndex, levelWord);
        if (!level.IsLogicalBlockIndex(blockIndex))
            return;

        streamer.SetLevelEntry(blockIndex, levelWord);

        int blockX = blockIndex % level.WidthInBlocks;
        int blockY = blockIndex / level.WidthInBlocks;
        if (!IsInsideNativeDrawWindow(blockX, blockY, layer1XPosition, layer1YPosition))
            return;

        _tilemapUpdates.Add(streamer.BuildPlmLevelBlockUpdate(blockIndex, bg1XOffset));
    }

    private static bool IsInsideNativeDrawWindow(
        int blockX,
        int blockY,
        ushort layer1XPosition,
        ushort layer1YPosition)
    {
        int topBlock = layer1YPosition >> 4;
        if (blockY < topBlock || blockY > topBlock + 15)
            return false;

        // $84:8DF4 calculates floor((layer1X+15)/16)-1, then admits the following
        // seventeen blocks. In ordinary non-wrapped room coordinates this is the partially
        // visible left block plus the sixteen blocks that can intersect the 256-pixel view.
        int leftBlock = ((layer1XPosition + 15) >> 4) - 1;
        return blockX >= leftBlock && blockX < leftBlock + 17;
    }

    private static ushort ReadBank84Word(ISnesAddressSpace bus, ushort address) =>
        unchecked((ushort)(
            bus.ReadByte(Bank84(address)) |
            (bus.ReadByte(Bank84(unchecked((ushort)(address + 1)))) << 8)));

    /// <summary>Explicit CPU-bus boundary for native bank-$84 PLM pointers.</summary>
    private static int Bank84(ushort offset) => (int)new SnesAddress(0x84, offset);

    private sealed class PlmSlot
    {
        public bool Active { get; set; }
        public ushort HeaderPointer { get; set; }
        public int BlockIndex { get; set; }
        /// <summary>
        /// Native <c>PLM_Vars</c>. Grapple setup saves the original word; collision-bomb
        /// setup synthesizes the dimension-parent restoration word <c>$x058</c> instead.
        /// </summary>
        public ushort RestoreLevelWord { get; set; }
        public ushort InstructionPointer { get; set; }
        public ushort InstructionTimer { get; set; }
        /// <summary>Native bank-$84 pre-instruction pointer run before the timer/list pass.</summary>
        public ushort PreInstruction { get; set; }
        /// <summary>Native room argument word owned independently by every physical PLM slot.</summary>
        public ushort RoomArgument { get; set; }
        /// <summary>Native <c>PLM_Timers</c>, distinct from the instruction countdown.</summary>
        public ushort LoopTimer { get; set; }
        /// <summary>
        /// Semantic state for one of bank $84's 63 permanent-item headers. A null value
        /// leaves this physical slot under the ordinary instruction interpreter.
        /// </summary>
        public CollectiblePlmState? Item { get; set; }
        /// <summary>Semantic state for resident bank-$84 scroll trigger header $B703.</summary>
        public ScrollPlmState? Scroll { get; set; }
        /// <summary>Semantic state for resident yellow/green/red door-cap PLMs.</summary>
        public ColoredDoorPlmState? ColoredDoor { get; set; }
        /// <summary>Semantic state for condition-gated grey door-cap PLMs.</summary>
        public GreyDoorPlmState? GreyDoor { get; set; }
        /// <summary>Semantic owner for map/resource/save station instruction families.</summary>
        public StationPlmState? Station { get; set; }
        /// <summary>Marks header $B70B while its animation remains generic list execution.</summary>
        public bool IsElevatorPlatform { get; set; }
        /// <summary>Semantic owner for door-spawned Wrecked Ship treadmill PLMs.</summary>
        public WreckedShipTreadmillPlmState? Treadmill { get; set; }
    }
}

internal sealed class ColoredDoorPlmState(
    ColoredDoorColor color,
    ColoredDoorOrientation orientation,
    ushort closedBlueList,
    ushort hitList,
    ushort openingList,
    ushort coloredClosedDraw,
    byte hitThreshold,
    ColoredDoorPhase phase)
{
    public ColoredDoorColor Color { get; } = color;
    public ColoredDoorOrientation Orientation { get; } = orientation;
    public ushort ClosedBlueList { get; } = closedBlueList;
    public ushort HitList { get; } = hitList;
    public ushort OpeningList { get; } = openingList;
    public ushort ColoredClosedDraw { get; } = coloredClosedDraw;
    public byte HitThreshold { get; } = hitThreshold;
    public ColoredDoorPhase Phase { get; set; } = phase;
    public byte HitCounter { get; set; }
    public bool InitialDrawCompleted { get; set; }
    public bool HasPendingHit { get; set; }
    public SamusProjectileTypeWord PendingProjectileType { get; set; }
}

/// <summary>Observable call to one of the cartridge's three queued-sound libraries.</summary>
public readonly record struct PlmSoundRequest(
    SoundEffectId SoundEffect,
    byte MaximumQueued);
