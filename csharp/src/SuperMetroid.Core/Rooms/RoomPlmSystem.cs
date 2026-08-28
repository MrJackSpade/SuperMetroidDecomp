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
public sealed class RoomPlmSystem
{
    private const int SlotCount = 40;
    private const ushort RespawningInstructionList = 0xcd6a;
    private const ushort NonRespawningInstructionList = 0xcda9;
    private const ushort DeleteInstruction = 0x86bc;
    private const ushort DrawPlmBlockInstruction = 0x8b17;
    private const ushort QueueSoundLibrary2Maximum6Instruction = 0x8c10;
    private const ushort QueueSoundLibrary2Maximum3Instruction = 0x8c46;
    private const ushort QueueSoundLibrary2Maximum1Instruction = 0x8c79;
    private const ushort QueueSoundLibrary2Maximum1DirectInstruction = 0x8c7c;
    private const ushort GotoInstruction = 0x8724;
    private const ushort SetPlmBtsTo1Instruction = 0xcd93;
    private const ushort DeleteInstructionList = 0xaae3;

    // `$94:936B` selects these eight entry IDs from BTS 0..7. Their setup pointer is common,
    // so storing the post-setup instruction-list pointer is sufficient after we reproduce
    // `$84:CE83-$CED9` synchronously in TrySpawnCollisionBombBlock.
    private static readonly ushort[] CollisionBombInstructionLists =
    [
        0xcc35, // BTS 0: 1x1, respawning
        0xcc5f, // BTS 1: 2x1, respawning
        0xcc8b, // BTS 2: 1x2, respawning
        0xccb7, // BTS 3: 2x2, respawning
        0xcce3, // BTS 4: 1x1, permanent
        0xccff, // BTS 5: 2x1, permanent
        0xcd1b, // BTS 6: 1x2, permanent
        0xcd37, // BTS 7: 2x2, permanent
    ];

    // `$94:A012` selects these bank-$84 entry IDs for both type-$7 bombable air and type-$F
    // bombable blocks. All eight entries share setup `$84:CEDA`; these are the instruction
    // list pointers installed by Spawn_PLM before that setup examines the projectile type.
    private static readonly ushort[] ReactionBombInstructionLists =
    [
        0xcc3c, // BTS 0: 1x1, respawning
        0xcc66, // BTS 1: 2x1, respawning
        0xcc92, // BTS 2: 1x2, respawning
        0xccbe, // BTS 3: 2x2, respawning
        0xccea, // BTS 4: 1x1, permanent
        0xcd06, // BTS 5: 2x1, permanent
        0xcd22, // BTS 6: 1x2, permanent
        0xcd3e, // BTS 7: 2x2, permanent
    ];

    // `$94:9EA6` maps non-area shootable BTS zero through seven to these entry PLMs.
    // We store the post-setup instruction pointers because Spawn_PLM runs the entry setup
    // synchronously. The first four lists restore their parent after the native 384-frame
    // blank hold; the latter four end after the breaking frames and leave air behind.
    private static readonly ushort[] RespawningShotInstructionLists =
    [
        0xcadf, // BTS 0: 1x1 respawning shot block
        0xcb02, // BTS 1: 2x1 respawning shot block
        0xcb27, // BTS 2: 1x2 respawning shot block
        0xcb4c, // BTS 3: 2x2 respawning shot block
    ];

    private static readonly ushort[] PermanentShotInstructionLists =
    [
        0xcbb7, // BTS 4: 1x1 permanent shot block
        0xcbcc, // BTS 5: 2x1 permanent shot block
        0xcbe1, // BTS 6: 1x2 permanent shot block
        0xcbf6, // BTS 7: 2x2 permanent shot block
    ];

    // `$94:9DA4` deliberately repeats the four crumble dimensions for BTS 4..7. These
    // one-frame lists do not destroy the special block. They replace an invisible/variant
    // type-$B word with the retail visible crumble art so the player learns its property.
    private static readonly ushort[] CrumbleRevealInstructionLists =
    [
        0xc8ec, // 1x1 reveal
        0xc8f2, // 2x1 reveal
        0xc8f8, // 1x2 reveal
        0xc8fe, // 2x2 reveal
    ];

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
            slot.BlockIndex = 0;
            slot.RestoreLevelWord = 0;
            slot.InstructionPointer = 0;
            slot.InstructionTimer = 0;
        }
        _soundRequests.Clear();
        _tilemapUpdates.Clear();
    }

    /// <summary>
    /// Runs setup <c>$84:CFB5</c> for BTS one or two and installs the corresponding PLM.
    /// </summary>
    /// <returns>False only when all 40 native slots are occupied.</returns>
    public bool TrySpawnBreakableGrappleBlock(
        RoomLevelData level,
        int blockIndex,
        byte behavior)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (behavior is not (1 or 2))
            throw new ArgumentOutOfRangeException(nameof(behavior), "Breakable grapple BTS must be one or two.");

        // Spawn_PLM at $84:84E7 probes $4E,$4C,...,$00. Matching it matters if multiple
        // PLMs mutate the same room on one frame because the handler uses the same order.
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = block.LevelWord;
            slot.InstructionPointer = behavior == 1
                ? RespawningInstructionList
                : NonRespawningInstructionList;
            slot.InstructionTimer = 1;

            // Setup_CFB5 saves the complete original level word but clears only the low BTS
            // byte. It deliberately leaves collision type E intact until the first PLM pass
            // later in this same gameplay frame draws $E0B7.
            level.SetBehavior(blockIndex, 0);
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
    {
        ArgumentNullException.ThrowIfNull(level);
        if (behavior > 7)
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
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
            slot.Active = true;
            slot.BlockIndex = blockIndex;

            // This is not the original visual word. Setup_CE83 deliberately replaces all
            // low twelve bits with visual block `$058`; multi-block restoration lists then
            // add type-$5/$D extension words around this type-$F parent.
            slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0058));
            slot.InstructionPointer = CollisionBombInstructionLists[behavior];
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
        ushort projectileType)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (behavior > 15)
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Bomb-reaction BTS must be in the native table range zero through fifteen.");
        }

        ushort projectileFamily = unchecked((ushort)(projectileType & 0x0f00));
        if (projectileFamily is not (0x0500 or 0x0300))
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
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.InstructionTimer = 1;

            if (behavior >= 8)
            {
                // Table entries 8..15 are all `$84:B62F`, whose setup is a bare RTS and
                // whose instruction list is the one-word delete stream at `$84:AAE3`.
                slot.RestoreLevelWord = 0;
                slot.InstructionPointer = DeleteInstructionList;
                return true;
            }

            // Setup_CEDA discards the original low twelve bits rather than preserving the
            // visible tile number. Dimension-specific final draw lists reconstruct linked
            // extension words; the 1x1 respawn tail uses this exact PLM_Vars value.
            slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0058));
            ushort instructionPointer = ReactionBombInstructionLists[behavior];

            // `$84:CF0C-$CF13` adds three only for normal bombs. The skipped bytes are
            // `{Instruction_PLM_QueueSound_Y_Lib2_Max3, $0A}` in the odd-byte operand form.
            slot.InstructionPointer = projectileFamily == 0x0500
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
        ushort projectileType)
    {
        ArgumentNullException.ThrowIfNull(level);
        bool areaDependent = (behavior & 0x80) != 0;
        if (areaDependent && (behavior & 0x7f) > 7)
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Area-dependent shootable BTS must address one of its eight native entries.");
        }
        if (!areaDependent && behavior > 15)
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Translated normal-bomb shootable BTS must be in range zero through fifteen.");
        }
        if ((projectileType & 0x0f00) != 0x0500)
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
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.InstructionTimer = 1;
            slot.RestoreLevelWord = 0;

            if (areaDependent)
            {
                // `$94:9E8D-$9E9E` indexes the current area's eight-entry table before
                // Spawn_PLM. Every retail entry at `$94:9F46-$9FC4` is PLMEntries_nothing.
                slot.InstructionPointer = DeleteInstructionList;
                return true;
            }

            if (behavior < 4)
            {
                // CE6B throws away the original visual/BTS low twelve bits. The generated
                // `$x052` word is both PLM_Vars and the source of the temporary collision
                // word, exactly like the retail setup's two consecutive stores.
                slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0052));
                slot.InstructionPointer = RespawningShotInstructionLists[behavior];
                level.SetForegroundEntry(
                    blockIndex,
                    unchecked((ushort)(slot.RestoreLevelWord & 0x8fff)));
                return true;
            }

            if (behavior < 8)
            {
                // Setup_DeactivatePLM does not synthesize a restore word. Clearing bits
                // `$7000` turns type-$4 air into ordinary air and type-$C solid into type-$8
                // solid until the same-frame list draws its first breaking frame.
                slot.InstructionPointer = PermanentShotInstructionLists[behavior - 4];
                level.SetForegroundEntry(
                    blockIndex,
                    unchecked((ushort)(block.LevelWord & 0x8fff)));
                return true;
            }

            if (behavior is 8 or 9)
            {
                // CF2E sees projectile family `$0500` and replaces the entry's normal
                // power-bomb animation pointer with the one-frame visible `$C057` reveal.
                slot.InstructionPointer = 0xc91c;
                return true;
            }

            if (behavior is 10 or 11)
            {
                // CF67 performs the analogous redirect to visible super-missile word
                // `$C09F`; it neither clears collision nor queues the shot-block sound.
                slot.InstructionPointer = 0xc922;
                return true;
            }

            slot.InstructionPointer = DeleteInstructionList;
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
        ushort projectileType,
        bool solidBlock)
    {
        ArgumentNullException.ThrowIfNull(level);
        bool areaDependent = (behavior & 0x80) != 0;
        if (areaDependent && (behavior & 0x7f) > 7)
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Area-dependent shootable BTS must address one of its eight native entries.");
        }
        if (!areaDependent && behavior > 15)
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Area-independent shootable BTS must be zero through fifteen.");
        }

        // `$94:9E55` checks the sign bit before Spawn_PLM for shootable air. Its solid-block
        // sibling `$94:9E73` instead performs the area-table lookup and allocates the retail
        // no-op entry. This early return is therefore collision-nibble-specific.
        if (areaDependent && !solidBlock)
            return false;

        ushort projectileFamily = unchecked((ushort)(projectileType & 0x0f00));

        // `$84:CF2E/$CF67` clear the newly allocated PLM header when the weapon family is
        // wrong. Observably that is identical to returning with no active slot: setup never
        // changes the live word, and the next handler has nothing to process. Perform this
        // gate before the host allocation loop while preserving every accepted native path.
        if (behavior is 8 or 9 && projectileFamily is not (0x0300 or 0x0500))
            return false;
        if (behavior is 10 or 11 && projectileFamily is not (0x0200 or 0x0500))
            return false;

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.InstructionTimer = 1;
            slot.RestoreLevelWord = 0;

            if (areaDependent)
            {
                // Every retail area-table target at `$94:9FC6-$9FD4` is the nothing entry.
                // Spawn_PLM still consumes a slot until its delete instruction runs.
                slot.InstructionPointer = DeleteInstructionList;
                return true;
            }

            if (behavior < 4)
            {
                // `$84:CE6B` synthesizes `$x052`, stores it for the 384-frame restoration,
                // and clears type bits `$4000/$2000/$1000` through `AND $8FFF` immediately.
                slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0052));
                slot.InstructionPointer = RespawningShotInstructionLists[behavior];
                level.SetForegroundEntry(
                    blockIndex,
                    unchecked((ushort)(slot.RestoreLevelWord & 0x8fff)));
                return true;
            }

            if (behavior < 8)
            {
                // `$84:B3C1` keeps no restoration word: BTS four through seven are
                // permanent. The current word loses the shootable collision bits before
                // the first animated breaking frame runs later in this gameplay pass.
                slot.InstructionPointer = PermanentShotInstructionLists[behavior - 4];
                level.SetForegroundEntry(
                    blockIndex,
                    unchecked((ushort)(block.LevelWord & 0x8fff)));
                return true;
            }

            if (behavior is 8 or 9)
            {
                if (projectileFamily == 0x0500)
                {
                    // A normal bomb does not break this block. `$84:CF2E` redirects to the
                    // one-frame visible power-bomb diagnostic without touching collision.
                    slot.InstructionPointer = 0xc91c;
                    return true;
                }

                // A power bomb synthesizes `$x057`, applies `AND $8FFF`, and retains the
                // exact list selected by header `$D084/$D088`. BTS eight uses the ordinary
                // four-frame breakup; BTS nine uses its shorter 3/2/1-frame counterpart.
                slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0057));
                slot.InstructionPointer = behavior == 8 ? (ushort)0xcb94 : (ushort)0xcc20;
                level.SetForegroundEntry(
                    blockIndex,
                    unchecked((ushort)(slot.RestoreLevelWord & 0x8fff)));
                return true;
            }

            if (behavior is 10 or 11)
            {
                if (projectileFamily == 0x0500)
                {
                    // `$84:CF67` gives ordinary bombs the analogous one-frame Super Missile
                    // reveal. It deliberately leaves the live collision word untouched.
                    slot.InstructionPointer = 0xc922;
                    return true;
                }

                // Super Missiles synthesize `$x09F`. Header `$D08C` owns the respawning
                // `$CB71` list and `$D090` owns the permanent `$CC0B` list.
                slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x009f));
                slot.InstructionPointer = behavior == 10 ? (ushort)0xcb71 : (ushort)0xcc0b;
                level.SetForegroundEntry(
                    blockIndex,
                    unchecked((ushort)(slot.RestoreLevelWord & 0x8fff)));
                return true;
            }

            // `$94:9EA6` entries C..F all point to `$84:B62F`. Its empty setup leaves the
            // block alone and its one-word `$AAE3` list deletes on the next handler pass.
            slot.InstructionPointer = DeleteInstructionList;
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
        byte areaIndex,
        ushort projectileType)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (areaIndex > 7)
            throw new ArgumentOutOfRangeException(nameof(areaIndex), "Native area index must be zero through seven.");
        if ((projectileType & 0x0f00) != 0x0500)
        {
            throw new ArgumentOutOfRangeException(
                nameof(projectileType),
                "The bomb-special setup accepts the normal-bomb family in this translated path.");
        }

        ushort instructionPointer;
        if ((behavior & 0x80) == 0)
        {
            if (behavior > 15)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(behavior),
                    "Area-independent special-block BTS must be zero through fifteen.");
            }

            instructionPointer = behavior switch
            {
                <= 7 => CrumbleRevealInstructionLists[behavior & 3],
                >= 14 => 0xc928,
                _ => DeleteInstructionList,
            };
        }
        else
        {
            int areaBehavior = behavior & 0x7f;
            if (areaBehavior > 7)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(behavior),
                    "Area-dependent bomb-special BTS must address one of its eight native entries.");
            }

            instructionPointer = areaIndex == 1 && areaBehavior is >= 2 and <= 5
                ? (ushort)0xc928
                : DeleteInstructionList;
        }

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

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
        ushort bg1XOffset)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(streamer);
        _soundRequests.Clear();
        _tilemapUpdates.Clear();

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
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
                bg1XOffset);
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
        ushort bg1XOffset)
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
                case QueueSoundLibrary2Maximum1Instruction:
                    // `$84:8C79` is the shot-block queue form. It has the same odd-byte
                    // operand layout as `$8C10/$8C46`, but permits only one pending sound.
                    byte singleSoundId = bus.ReadByte(
                        0x840000 | unchecked((ushort)(slot.InstructionPointer + 2)));
                    _soundRequests.Add(new PlmSoundRequest(2, singleSoundId, MaximumQueued: 1));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case QueueSoundLibrary2Maximum1DirectInstruction:
                    // `$84:8C7C` is the direct LDA/JSL form used by the power-bomb-gated
                    // shot-block lists; `$8C79` enters the same max-one queue routine through
                    // a short branch. Both consume the identical odd-byte sound operand.
                    byte directSingleSoundId = bus.ReadByte(
                        0x840000 | unchecked((ushort)(slot.InstructionPointer + 2)));
                    _soundRequests.Add(new PlmSoundRequest(
                        2,
                        directSingleSoundId,
                        MaximumQueued: 1));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case QueueSoundLibrary2Maximum6Instruction:
                    // $84:8C10 consumes one byte after its pointer. The following timer's
                    // low byte is read as A's harmless high byte by the 16-bit LDA.
                    byte soundId = bus.ReadByte(
                        0x840000 | unchecked((ushort)(slot.InstructionPointer + 2)));
                    _soundRequests.Add(new PlmSoundRequest(Library: 2, soundId, MaximumQueued: 6));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case QueueSoundLibrary2Maximum3Instruction:
                    // `$84:8C46` has the same odd-byte operand layout as `$8C10`, but the
                    // collision-bomb list's crumble sound `$06` uses the stricter queue cap.
                    byte cappedSoundId = bus.ReadByte(
                        0x840000 | unchecked((ushort)(slot.InstructionPointer + 2)));
                    _soundRequests.Add(new PlmSoundRequest(2, cappedSoundId, MaximumQueued: 3));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case GotoInstruction:
                    // `$84:8724` replaces Y with the following little-endian pointer. All
                    // eight collision entry lists use it to share their dimension-specific
                    // respawning/permanent animation tail.
                    slot.InstructionPointer = ReadBank84Word(
                        bus,
                        unchecked((ushort)(slot.InstructionPointer + 2)));
                    continue;

                case SetPlmBtsTo1Instruction:
                    level.SetBehavior(slot.BlockIndex, 1);
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;

                case DrawPlmBlockInstruction:
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

                case DeleteInstruction:
                    slot.Active = false;
                    return;

                default:
                    throw new InvalidDataException(
                        $"Movement-owned PLM reached unsupported bank-$84 instruction ${instruction:X4}.");
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
                if ((uint)x >= (uint)level.WidthInBlocks ||
                    (uint)y >= (uint)level.HeightInBlocks)
                {
                    throw new InvalidDataException(
                        $"PLM draw list ${drawPointer:X4} targets out-of-room block ({x},{y}).");
                }

                ushort levelWord = ReadBank84Word(bus, cursor);
                cursor = unchecked((ushort)(cursor + 2));
                DrawLevelWord(
                    level,
                    streamer,
                    level.GetBlockIndex(x, y),
                    levelWord,
                    layer1XPosition,
                    layer1YPosition,
                    bg1XOffset);
            }

            byte relativeX = bus.ReadByte(0x840000 | cursor);
            byte relativeY = bus.ReadByte(0x840000 | unchecked((ushort)(cursor + 1)));
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
        level.SetForegroundEntry(blockIndex, levelWord);
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
            bus.ReadByte(0x840000 | address) |
            (bus.ReadByte(0x840000 | unchecked((ushort)(address + 1))) << 8)));

    private sealed class PlmSlot
    {
        public bool Active { get; set; }
        public int BlockIndex { get; set; }
        /// <summary>
        /// Native <c>PLM_Vars</c>. Grapple setup saves the original word; collision-bomb
        /// setup synthesizes the dimension-parent restoration word <c>$x058</c> instead.
        /// </summary>
        public ushort RestoreLevelWord { get; set; }
        public ushort InstructionPointer { get; set; }
        public ushort InstructionTimer { get; set; }
    }
}

/// <summary>Observable call to one of the cartridge's three queued-sound libraries.</summary>
public readonly record struct PlmSoundRequest(byte Library, byte SoundId, byte MaximumQueued);
