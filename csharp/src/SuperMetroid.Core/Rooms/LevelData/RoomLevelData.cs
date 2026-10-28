using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Decompressed native room allocation: BG1 level words, one-byte BTS metadata, BG2 level
/// words, and the combined CRE/area visual block definitions.
/// </summary>
/// <remarks>
/// Bank $82 installs these planes together, but their consumers are different: bank $80
/// expands the low visual bits for VRAM streaming while bank $94 reads BG1's high nibble
/// and the parallel BTS byte for collision. Keeping one object prevents rendering and
/// physics from quietly loading different room revisions or indexing conventions.
/// </remarks>
[RoomLifetime]
public sealed class RoomLevelData
{
    /// <summary>The solid word an out-of-room block reads as: the allocation is prefilled with solid air.</summary>
    private static readonly ushort PrefilledSolidWord = new RoomLevelWord(0).WithCollisionType(RoomCollisionType.SolidBlock).Raw;

    /// <summary>Mutable logical BG1 level words shared by terrain collision and room rendering.</summary>
    private readonly ushort[] _foregroundEntries;
    /// <summary>Parallel BTS bytes interpreted according to each BG1 word's collision nibble.</summary>
    internal readonly byte[] BehaviorBytes;
    /// <summary>Logical BG2 level words kept in the room's authored row-major dimensions.</summary>
    internal readonly ushort[] BackgroundEntries;
    /// <summary>Combined CRE and area 8-by-8 tile definitions selected by visual block numbers, four tiles per block.</summary>
    private readonly BackgroundTile[] _blockDefinitions;
    /// <summary>Native-sized BG1 allocation, including the decompressed tail used by streaming.</summary>
    internal readonly ushort[] StreamingForegroundAllocation;
    /// <summary>Native-sized BG2 allocation, including its decompressed tail.</summary>
    internal readonly ushort[] StreamingBackgroundAllocation;
    /// <summary>Editable BG1 visual references kept separate from collision words.</summary>
    internal readonly ushort[] VisualStreamingForegroundAllocation;
    /// <summary>Editable BG2 visual references used by background streaming.</summary>
    internal readonly ushort[] VisualStreamingBackgroundAllocation;
    /// <summary>Bounded level-data storage for PLM writes outside the authored room plane.</summary>
    internal readonly ushort[] PlmForegroundAllocation;
    /// <summary>BTS storage paired with native PLM indices, including their bounded tail.</summary>
    internal readonly byte[] PlmBehaviorAllocation;
    /// <summary>The BG1 plane last published for display, until the next visual write; derived, never saved.</summary>
    [NonSerialized] private RoomLevelPlane? foregroundPlane;
    /// <summary>The BG2 plane last published for display, until the next definition write; derived, never saved.</summary>
    [NonSerialized] private RoomLevelPlane? backgroundPlane;

    /// <summary>Copies the logical BG1/BTS/BG2 planes and visual block definitions into one mutable room allocation, retaining bounded native streaming tails and prefilled PLM storage; an installed layout changes only initial visual references.</summary>
    /// <param name="size">Room size in 16-pixel blocks: the width is the row-major stride, 1..255, and the height is positive.</param>
    /// <param name="planes">The logical BG1, BTS and BG2 planes, each exactly width times height entries in row-major order.</param>
    /// <param name="blockDefinitions">Combined CRE then area definitions: four 8-by-8 tiles per 16-pixel visual block; length must be divisible by four.</param>
    /// <param name="streaming">Optional copied native BG1/BG2 allocations including decompressed overread tails; an empty allocation uses its logical plane, and a supplied allocation cannot be shorter.</param>
    /// <param name="doors">Optional door list; null is suitable only for fixtures that never resolve a type-$9 door collision.</param>
    /// <param name="visualLayout">Optional matching-sized initial BG1/BG2 artwork; collision words, BTS, and native allocation tails remain unchanged.</param>
    /// <exception cref="ArgumentOutOfRangeException">Width is outside 1..255 or height is nonpositive.</exception>
    /// <exception cref="ArgumentException">A logical plane, streaming allocation, or block-definition byte extent has an invalid size.</exception>
    /// <exception cref="InvalidDataException">The selected visual layout does not match the room dimensions.</exception>
    /// <exception cref="OverflowException">The logical dimension product exceeds the signed 32-bit range.</exception>
    public RoomLevelData(
        Extent16<RoomBlocks> size,
        RoomLevelPlanes planes,
        ReadOnlySpan<BackgroundTile> blockDefinitions,
        RoomLevelStreaming streaming = default,
        IReadOnlyList<DoorListEntry>? doors = null,
        RoomVisualLayout? visualLayout = null)
    {
        ReadOnlySpan<ushort> foregroundEntries = planes.Foreground;
        ReadOnlySpan<byte> behaviorBytes = planes.Behavior;
        ReadOnlySpan<ushort> backgroundEntries = planes.Background;
        ReadOnlySpan<ushort> streamingForegroundAllocation = streaming.Foreground;
        ReadOnlySpan<ushort> streamingBackgroundAllocation = streaming.Background;
        if (size.X is <= 0 or > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(size), "Room width must be 1..255 blocks.");
        if (size.Y <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Room height must be positive.");

        int expectedBlocks = size.Area;
        if (foregroundEntries.Length != expectedBlocks)
            throw new ArgumentException("BG1 must contain exactly one word per room block.", nameof(planes));
        if (behaviorBytes.Length != expectedBlocks)
            throw new ArgumentException("BTS must contain exactly one byte per room block.", nameof(planes));
        if (backgroundEntries.Length != expectedBlocks)
            throw new ArgumentException("BG2 must contain exactly one word per room block.", nameof(planes));
        if (blockDefinitions.Length % RoomLevelPlane.DefinitionTiles != 0)
            throw new ArgumentException("Each visual block definition is exactly four tiles.", nameof(blockDefinitions));

        Size = size;
        Doors = doors;
        _foregroundEntries = foregroundEntries.ToArray();
        BehaviorBytes = behaviorBytes.ToArray();
        BackgroundEntries = backgroundEntries.ToArray();
        _blockDefinitions = blockDefinitions.ToArray();

        // The cartridge's initial 17-column fill can read past logical BG1 into adjacent
        // decompressed planes at the room's bottom edge. LandingSiteStreamingData supplies
        // that complete native allocation. Synthetic/general callers default to logical
        // BG1 only, which remains sufficient for in-bounds streaming and all collision.
        StreamingForegroundAllocation = streamingForegroundAllocation.IsEmpty
            ? _foregroundEntries.ToArray()
            : streamingForegroundAllocation.ToArray();
        if (StreamingForegroundAllocation.Length < expectedBlocks)
        {
            throw new ArgumentException(
                "The streaming BG1 allocation cannot be shorter than logical BG1.",
                nameof(streaming));
        }

        // Room PLM coordinates are converted to a row-major native block index before
        // their setup callback runs. Retail contains two deliberately off-room colored
        // doors whose setup writes into the harmless prefilled tail of level_data. Keep a
        // bounded image of that native allocation so those writes retain cartridge order
        // without exposing adjacent host memory or weakening logical rendering/collision
        // bounds. Copy the decompressed allocation because its tail is exactly what the
        // cartridge would observe before the PLM population is constructed.
        int plmAllocationLength = Math.Max(
            RoomLevelMemoryLayout.PrefilledStreamingWordCount,
            StreamingForegroundAllocation.Length);
        PlmForegroundAllocation = new ushort[plmAllocationLength];
        Array.Fill(PlmForegroundAllocation, RoomLevelMemoryLayout.PrefilledLevelWord);
        StreamingForegroundAllocation.CopyTo(PlmForegroundAllocation, 0);
        PlmBehaviorAllocation = new byte[plmAllocationLength];
        BehaviorBytes.CopyTo(PlmBehaviorAllocation, 0);

        // BG2 is copied to another region of the same pre-cleared WRAM allocation and can
        // be overread by the same 17-column/row camera fill. Keep its logical plane exact
        // for collision-independent inspection while giving bank $80 the native tail when
        // a cartridge loader supplies it.
        StreamingBackgroundAllocation = streamingBackgroundAllocation.IsEmpty
            ? BackgroundEntries.ToArray()
            : streamingBackgroundAllocation.ToArray();
        if (StreamingBackgroundAllocation.Length < expectedBlocks)
        {
            throw new ArgumentException(
                "The streaming BG2 allocation cannot be shorter than logical BG2.",
                nameof(streaming));
        }

        // Collision and PLM scripts retain the unmodified native words. Only bank $80's
        // visual producer sees an installed layout, and only in the authored room plane;
        // native overread tails, BTS adjacency, and out-of-room prefill remain unchanged.
        VisualStreamingForegroundAllocation = visualLayout is null
            ? StreamingForegroundAllocation
            : StreamingForegroundAllocation.ToArray();
        VisualStreamingBackgroundAllocation = visualLayout is null
            ? StreamingBackgroundAllocation
            : StreamingBackgroundAllocation.ToArray();
        if (visualLayout is not null)
        {
            if (visualLayout.Size != size)
                throw new InvalidDataException("Installed room layout dimensions do not match the native allocation.");
            for (int index = 0; index < expectedBlocks; index++)
            {
                VisualStreamingForegroundAllocation[index] =
                    new RoomLevelWord(_foregroundEntries[index]).WithVisualWord(visualLayout.ForegroundVisualWords.Span[index]).Raw;
                VisualStreamingBackgroundAllocation[index] =
                    new RoomLevelWord(BackgroundEntries[index]).WithVisualWord(visualLayout.BackgroundVisualWords.Span[index]).Raw;
            }
        }
    }

    /// <summary>
    /// Logical room size in 16-pixel blocks. The width is native <c>room_width_in_blocks</c>, used as
    /// every row's index stride; the height comes from the room header.
    /// </summary>
    public Extent16<RoomBlocks> Size { get; }

    /// <summary>
    /// The entries this room's type-$9 blocks select by index. Synthetic collision fixtures
    /// deliberately leave it null; cartridge room loading always supplies it.
    /// </summary>
    public IReadOnlyList<DoorListEntry>? Doors { get; }

    /// <summary>
    /// Door definition selected by the most recent native type-$9 collision, mirroring
    /// the side effect on WRAM <c>door_def_ptr</c> made by <c>$94:938B/$93CE</c>.
    /// </summary>
    public DoorDefinition? PendingDoorTransition { get; internal set; }

    /// <summary>
    /// Bank-$94 sets WRAM <c>elevator_flags</c> when a type-$9 block resolves to the special
    /// low-bank pseudo destination. EnemyMain runs before Samus collision, so the runtime
    /// consumes this publication at the beginning of the following gameplay frame.
    /// </summary>
    public bool ElevatorDoorContactPending { get; internal set; }

    /// <summary>Read-only logical BG1 words, including collision type in bits 12–15.</summary>
    public ReadOnlyMemory<ushort> ForegroundEntries => _foregroundEntries;

    /// <summary>CRE definitions followed by area definitions, four tiles per visual block.</summary>
    public ReadOnlyMemory<BackgroundTile> BlockDefinitions => _blockDefinitions;

    /// <summary>
    /// BG1 as the level data shows it (#1346). The same immutable plane is returned until a block or definition write,
    /// so frames that change nothing share it.
    /// </summary>
    public RoomLevelPlane ForegroundPlane => foregroundPlane ??= new(Size, VisualStreamingForegroundAllocation, PublishedDefinitions());

    /// <summary>The layer-2 plane as the level data shows it, shared until a definition write.</summary>
    public RoomLevelPlane BackgroundPlane => backgroundPlane ??= new(Size, VisualStreamingBackgroundAllocation, PublishedDefinitions());

    /// <summary>The block definitions both planes of a publication share.</summary>
    private ImmutableArray<BackgroundTile> PublishedDefinitions() =>
        foregroundPlane?.Definitions ?? backgroundPlane?.Definitions ?? [.. _blockDefinitions];

    /// <summary>
    /// Converts a 16-pixel block coordinate using the same row-major multiplication as
    /// bank $94. Invalid coordinates fail loudly instead of wrapping into unrelated WRAM.
    /// </summary>
    public int GetBlockIndex(Position16<RoomBlocks> block)
    {
        if (block.X >= (uint)Size.X)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block X {block.X} lies outside the {Size.X}-block room width.");
        if (block.Y >= (uint)Size.Y)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block Y {block.Y} lies outside the {Size.Y}-block room height.");
        return Size.RowMajorIndex(block);
    }

    /// <summary>
    /// Reproduces <c>Spawn_Room_PLM</c>'s unchecked byte-coordinate multiplication while
    /// bounding the result to the translated image of bank-$7F level memory.
    /// </summary>
    /// <remarks>
    /// This is intentionally distinct from <see cref="GetBlockIndex"/>. A PLM setup that
    /// requires a logical room block must still use the strict API and fail at that actual
    /// consumption point. Native-memory setup and draw operations may use the bounded PLM
    /// allocation, matching the two shipped off-room colored-door records safely.
    /// </remarks>
    internal int GetPlmBlockIndex(Position16<RoomBlocks> block)
    {
        if (block.X < 0 || block.Y < 0)
            throw new ArgumentOutOfRangeException(nameof(block), $"PLM block {block} has a negative coordinate.");
        int blockIndex = checked(block.Y * Size.X + block.X);
        if ((uint)blockIndex >= (uint)PlmForegroundAllocation.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(block),
                $"PLM block ({block.X},{block.Y}) resolves to native index {blockIndex}, " +
                $"outside the {PlmForegroundAllocation.Length}-word safe level allocation.");
        }

        return blockIndex;
    }

    /// <summary>
    /// Reads a level word past the authored plane from the native level allocation: the
    /// rest of the decompressed BTS/BG2 stream, then the <c>$82:EA73</c> $8000 prefill,
    /// including any PLM writes there. Indices past the allocation are unbounded memory.
    /// </summary>
    internal ushort ReadAllocationTailLevelWord(int blockIndex)
    {
        if (IsLogicalBlockIndex(blockIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex), "The index is inside the authored room plane.");
        if ((uint)blockIndex >= (uint)PlmForegroundAllocation.Length)
        {
            throw new NotSupportedException(
                $"Native block {blockIndex} lies past the {PlmForegroundAllocation.Length}-word level allocation.");
        }
        return PlmForegroundAllocation[blockIndex];
    }

    /// <summary>Whether a native PLM block index belongs to the authored logical plane.</summary>
    internal bool IsLogicalBlockIndex(int blockIndex) =>
        (uint)blockIndex < (uint)_foregroundEntries.Length;

    /// <summary>Reads a block through the bounded level/BTS allocation used by PLM code.</summary>
    internal RoomCollisionBlock GetPlmCollisionBlockByIndex(int blockIndex)
    {
        if ((uint)blockIndex >= (uint)PlmForegroundAllocation.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return new RoomCollisionBlock(
            blockIndex,
            PlmForegroundAllocation[blockIndex],
            new RoomBlockBehavior(PlmBehaviorAllocation[blockIndex]));
    }

    /// <summary>
    /// Applies a bank-$84 level-data write inside the bounded native PLM allocation.
    /// Logical writes remain visible to collision and streaming; tail writes remain safely
    /// isolated just as their cartridge addresses remain outside the authored room.
    /// </summary>
    internal void SetPlmForegroundEntry(int blockIndex, int levelWord)
        => SetPlmForegroundEntry(blockIndex, levelWord,
            RoomLevelWord.FromRaw(levelWord).VisualWord);

    /// <summary>
    /// Applies a native PLM word to collision while selecting an independent visual
    /// block reference for BG1. The visual operand may not set a collision bit.
    /// </summary>
    internal void SetPlmForegroundEntry(int blockIndex, int levelWord, int visualWord)
    {
        if ((uint)blockIndex >= (uint)PlmForegroundAllocation.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        if (!RoomLevelWord.IsValidVisualWord(visualWord))
            throw new ArgumentOutOfRangeException(nameof(visualWord));

        RoomLevelWord word = RoomLevelWord.FromRaw(levelWord);
        PlmForegroundAllocation[blockIndex] = word.Raw;
        if (!IsLogicalBlockIndex(blockIndex))
            return;

        _foregroundEntries[blockIndex] = word.Raw;
        if (blockIndex < StreamingForegroundAllocation.Length)
            StreamingForegroundAllocation[blockIndex] = word.Raw;
        if (blockIndex < VisualStreamingForegroundAllocation.Length)
            VisualStreamingForegroundAllocation[blockIndex] = word.WithVisualWord(visualWord).Raw;
        foregroundPlane = null;
    }

    /// <summary>Applies a bank-$84 BTS write, one byte (0 through 255), through the bounded native PLM allocation.</summary>
    internal void SetPlmBehavior(int blockIndex, int behavior)
    {
        if ((uint)blockIndex >= (uint)PlmBehaviorAllocation.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));

        Ensure.BetweenInclusive(0, byte.MaxValue, behavior);
        PlmBehaviorAllocation[blockIndex] = (byte)behavior;
        if (IsLogicalBlockIndex(blockIndex))
            BehaviorBytes[blockIndex] = (byte)behavior;
    }

    /// <summary>Typed overload for a bounded native PLM BTS write.</summary>
    internal void SetPlmBehavior(int blockIndex, RoomBlockBehavior behavior) =>
        SetPlmBehavior(blockIndex, behavior.ToBtsByte());

    /// <summary>Returns both collision inputs consumed for one block by bank $94.</summary>
    public RoomCollisionBlock GetCollisionBlock(Position16<RoomBlocks> block)
    {
        int index = GetBlockIndex(block);
        return GetCollisionBlockByIndex(index);
    }

    /// <summary>
    /// Reads the logical room block or reproduces bank <c>$82</c>'s prefilled collision
    /// allocation when a bank-<c>$94</c> scan crosses the authored room boundary.
    /// </summary>
    /// <remarks>
    /// <c>$82:EA73-$EA90</c> fills the complete <c>$6400</c>-byte level-data allocation
    /// with <c>$8000</c> before decompressing the smaller logical room plane. Native
    /// collision code indexes that allocation without a logical-width/height guard, so
    /// nearby out-of-room probes encounter an unconditional type-eight solid. Rendering
    /// and streaming remain logical-sized; collision callers share this seam instead of
    /// independently fabricating wrapping or empty terrain.
    /// </remarks>
    internal RoomCollisionBlock GetCollisionBlockOrPrefilledSolid(Position16<RoomBlocks> block)
    {
        if (!Size.Contains(block))
        {
            return new RoomCollisionBlock(
                Index: -1,
                PackedWord: PrefilledSolidWord,
                Bts: new RoomBlockBehavior(0));
        }

        return GetCollisionBlock(block);
    }

    /// <summary>
    /// Applies the same prefilled-allocation rule after a collision extension has already
    /// reduced its target to a native row-major index.
    /// </summary>
    internal RoomCollisionBlock GetCollisionBlockByIndexOrPrefilledSolid(int blockIndex)
    {
        if ((uint)blockIndex >= (uint)_foregroundEntries.Length)
        {
            return new RoomCollisionBlock(
                Index: -1,
                PackedWord: PrefilledSolidWord,
                Bts: new RoomBlockBehavior(0));
        }

        return GetCollisionBlockByIndex(blockIndex);
    }

    /// <summary>
    /// Reads a native row-major index after bank-$94 type-$5/$D extension arithmetic.
    /// </summary>
    public RoomCollisionBlock GetCollisionBlockByIndex(int blockIndex)
    {
        if ((uint)blockIndex >= (uint)_foregroundEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return new RoomCollisionBlock(
            blockIndex,
            _foregroundEntries[blockIndex],
            new RoomBlockBehavior(BehaviorBytes[blockIndex]));
    }

    /// <summary>
    /// Converts unsigned room-world pixels to a block coordinate exactly as the native
    /// <c>LSR ×4</c>/<c>&gt;&gt; 4</c> collision paths do.
    /// </summary>
    public RoomCollisionBlock GetCollisionBlockAtPixel(Position16<RoomPixels> position) =>
        GetCollisionBlock(position.ToRoomBlock());

    /// <summary>
    /// Resolves a type-$9 BTS byte through the active room's door list exactly as bank $94
    /// does, then publishes the normal-door transition request for the top-level dispatcher.
    /// </summary>
    public DoorListEntry ResolveDoorCollision(
        int behavior,
        SamusCollisionPose samusPose,
        bool publishDoorSideEffects = true)
    {
        if (Doors is not { } doors)
        {
            // Every cartridge-backed room header supplies this pointer. Its nullable form
            // exists only so focused synthetic collision fixtures need not fabricate bank-
            // $8F metadata when they contain no type-$9 blocks. Reaching a door in such a
            // fixture is therefore an invalid host composition, not missing door behavior.
            throw new InvalidOperationException(
                "Door collision requires cartridge room metadata; this level has no door list.");
        }

        // Bit seven is not part of the list index. Native elevator/special-door BTS values
        // share the same seven-bit table lookup before the destination-room high bit decides
        // whether collision is solid or starts game state $09.
        int index = behavior & RoomDoorListDefinitions.BtsIndexMask;
        if (index >= doors.Count)
            throw new InvalidDataException($"Door index {index} is outside the room's {doors.Count}-entry door list.");
        DoorListEntry entry = doors[index];
        // The native door handler normally publishes either a real transition or the
        // elevator-contact flag. The desktop runtime collapses the outer transition states
        // into one room-load call, however, so destination-side elevator return scans must
        // inspect the same type-$9 block without republishing either side effect. Keep the
        // door lookup and solid/passable classification intact while gating only the writes.
        if (publishDoorSideEffects && entry.Door is { } door)
            PendingDoorTransition ??= door;
        // `$94:938B/$94:93CE` treat the pseudo destination as solid for every pose, but
        // publish elevator_flags only while samus_pose is below $09. This prevents running,
        // aerial, morph, and damage poses that merely brush the block from arming the actor.
        else if (publishDoorSideEffects && entry.IsElevatorPseudoDoor &&
                 samusPose is SamusCollisionPose.StandingNormal or SamusCollisionPose.StandingOther)
            ElevatorDoorContactPending = true;
        return entry;
    }

    /// <summary>Consumes the native-equivalent <c>door_def_ptr</c> publication once.</summary>
    public DoorDefinition? ConsumePendingDoorTransition()
    {
        DoorDefinition? pending = PendingDoorTransition;
        PendingDoorTransition = null;
        return pending;
    }

    /// <summary>Consumes the one-frame elevator pseudo-door collision publication.</summary>
    public bool ConsumeElevatorDoorContact()
    {
        bool pending = ElevatorDoorContactPending;
        ElevatorDoorContactPending = false;
        return pending;
    }

    /// <summary>
    /// Replaces one complete native <c>level_data</c> word and keeps the bank-$80
    /// streaming allocation coherent with the collision plane.
    /// </summary>
    /// <remarks>
    /// Bank-$84 PLM draw instructions write the full word, including the collision nibble,
    /// visual block number, and parent flip bits. A narrower "change the graphic" helper
    /// would be incorrect here: breakable grapple frames intentionally alternate between
    /// type-$E grapple terrain and type-$0 air.
    /// </remarks>
    public void SetForegroundEntry(int blockIndex, int levelWord)
    {
        if ((uint)blockIndex >= (uint)_foregroundEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));

        RoomLevelWord word = RoomLevelWord.FromRaw(levelWord);
        _foregroundEntries[blockIndex] = word.Raw;
        PlmForegroundAllocation[blockIndex] = word.Raw;
        if (blockIndex < StreamingForegroundAllocation.Length)
            StreamingForegroundAllocation[blockIndex] = word.Raw;
        if (blockIndex < VisualStreamingForegroundAllocation.Length)
            VisualStreamingForegroundAllocation[blockIndex] = word.Raw;
        foregroundPlane = null;
    }

    /// <summary>
    /// Replaces the low BTS byte paired with one level-data block.
    /// </summary>
    /// <remarks>
    /// The cartridge stores BTS bytes in a word-addressable allocation and preserves the
    /// neighboring high byte with <c>AND #$FF00</c>. This model already exposes each logical
    /// low byte separately, so assigning this element is the equivalent operation.
    /// </remarks>
    public void SetBehavior(int blockIndex, int behavior)
    {
        if ((uint)blockIndex >= (uint)BehaviorBytes.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        Ensure.BetweenInclusive(0, byte.MaxValue, behavior);
        BehaviorBytes[blockIndex] = (byte)behavior;
        PlmBehaviorAllocation[blockIndex] = (byte)behavior;
    }

    /// <summary>Writes a typed BTS value while retaining the byte-exact room format.</summary>
    public void SetBehavior(int blockIndex, RoomBlockBehavior behavior) =>
        SetBehavior(blockIndex, behavior.ToBtsByte());

    /// <summary>
    /// Replaces one 8x8 child tile in the combined CRE/area block-definition table. Item
    /// instruction <c>$84:8764</c> uses this exact mutable range to bind one of four rotating
    /// item-GFX uploads to visual blocks <c>$08E-$095</c>.
    /// </summary>
    /// <remarks>
    /// This is intentionally indexed in child tiles, matching native <c>TileTable,X</c>. It is
    /// not a general level-block replacement: the room's level words continue to select
    /// the same block while its four 8x8 children acquire cartridge-defined tiles and
    /// colour variants.
    /// </remarks>
    public void SetBlockDefinitionTile(int tileIndex, BackgroundTile tile)
    {
        if ((uint)tileIndex >= (uint)_blockDefinitions.Length)
            throw new ArgumentOutOfRangeException(nameof(tileIndex));

        _blockDefinitions[tileIndex] = tile;
        foregroundPlane = null;
        backgroundPlane = null;
    }
}
