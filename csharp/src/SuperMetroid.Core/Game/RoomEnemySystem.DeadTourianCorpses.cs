using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Dead Zoomer, Ripper, and Skree actors sharing bank-$A9's corpse-rotting engine.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>$A0:EDFF, EnemyHeaders_CorpseZoomer: dead Zoomer header initialized by $A9:D849, with parameter-1 variants 0, 2, and 4 sharing the corpse-rotting engine.</summary>
    public const ushort DeadZoomerDefinition = 0xedff;
    /// <summary>$A0:EE3F, EnemyHeaders_CorpseRipper: dead Ripper header initialized by $A9:D876, with parameter-1 variants 0 and 2 sharing the corpse-rotting engine.</summary>
    public const ushort DeadRipperDefinition = 0xee3f;
    /// <summary>$A0:EE7F, EnemyHeaders_CorpseSkree: dead Skree header initialized by $A9:D89F, with parameter-1 variants 0, 2, and 4 sharing the corpse-rotting engine.</summary>
    public const ushort DeadSkreeDefinition = 0xee7f;

    /// <summary>Bank-$A9 dispatcher value used after every corpse row has finished rotting.</summary>
    private const ushort DeadTourianCorpseNoOperationFunction = 0xda63;

    /// <summary>Zoomer-specific callbacks and tile-layout variants for the shared corpse engine.</summary>
    private static readonly DeadTourianCorpseProfile DeadZoomerProfile = new(
        DeadTourianCorpseSpecies.Zoomer,
        WaitFunction: 0xda69,
        PreRotFunction: 0xda94,
        RottingFunction: 0xdad0,
        TouchAndShotFunction: 0xdcf8,
        PowerBombFunction: 0xdced,
        Variants: CorpseLayout(columns: 3, rows: 2, workBufferOffset: 0x0940, sheetSources: SheetRun(0x0a60, 0x0060, 3)));

    /// <summary>Ripper-specific callbacks and tile-layout variants for the shared corpse engine.</summary>
    private static readonly DeadTourianCorpseProfile DeadRipperProfile = new(
        DeadTourianCorpseSpecies.Ripper,
        WaitFunction: 0xda73,
        PreRotFunction: 0xda99,
        RottingFunction: 0xdae6,
        TouchAndShotFunction: 0xdd08,
        PowerBombFunction: 0xdcfd,
        Variants: CorpseLayout(columns: 3, rows: 2, workBufferOffset: 0x0b80, sheetSources: SheetRun(0x0a00, 0x0180, 2)));

    /// <summary>Skree-specific callbacks and tile-layout variants for the shared corpse engine.</summary>
    private static readonly DeadTourianCorpseProfile DeadSkreeProfile = new(
        DeadTourianCorpseSpecies.Skree,
        WaitFunction: 0xda6e,
        PreRotFunction: 0xda9e,
        RottingFunction: 0xdafc,
        TouchAndShotFunction: 0xdd18,
        PowerBombFunction: 0xdd0d,
        Variants: CorpseLayout(columns: 2, rows: 4, workBufferOffset: 0x0640, sheetSources: [0x02a0, 0x00e0, 0x01c0]));

    /// <summary>
    /// Builds a corpse's variants from its tile layout. Each corpse is a block of
    /// <paramref name="columns"/> by <paramref name="rows"/> 4bpp 8x8 tiles; successive variants
    /// stack in the dead-monster work buffer from <paramref name="workBufferOffset"/>. Each art
    /// row copies from the installed tile sheet (one $200-byte sheet row per tile row) at the
    /// variant's authored sheet position. Column word offsets are the block's tile columns, and
    /// the rot depth is the block height less two pixels.
    /// </summary>
    private static DeadTourianCorpseVariant[] CorpseLayout(
        int columns, int rows, int workBufferOffset, ReadOnlySpan<int> sheetSources)
    {
        const int tileBytes = 32, sheetRowBytes = 0x200, tileWords = tileBytes / 2;
        var variants = new DeadTourianCorpseVariant[sheetSources.Length];
        for (int variant = 0; variant < variants.Length; variant++)
        {
            int block = workBufferOffset + variant * columns * rows * tileBytes;
            var columnWords = new ushort[columns];
            for (int column = 0; column < columns; column++)
                columnWords[column] = (ushort)(block / 2 + tileWords * column);
            var copies = new DeadTourianCorpseGraphicsCopy[rows];
            for (int row = 0; row < rows; row++)
                copies[row] = new(sheetSources[variant] + sheetRowBytes * row,
                    block + columns * tileBytes * row, columns * tileBytes);
            variants[variant] = new((ushort)(8 * rows - 2), columnWords, copies);
        }
        return variants;
    }

    /// <summary>Sheet positions spaced evenly across one tile-sheet row.</summary>
    private static int[] SheetRun(int first, int step, int count)
    {
        var sources = new int[count];
        for (int index = 0; index < count; index++) sources[index] = first + step * index;
        return sources;
    }

    /// <summary>Initialized corpse state indexed by physical enemy slot.</summary>
    private readonly DeadTourianCorpseEnemyState?[] _deadTourianCorpseStates =
        new DeadTourianCorpseEnemyState?[MaximumEnemyCount];

    /// <summary>Clears per-slot corpse state when the room changes.</summary>
    private void ResetDeadTourianCorpseRoomState() =>
        Array.Clear(_deadTourianCorpseStates);

    /// <summary>Tests whether a native definition pointer belongs to the three corpse families.</summary>
    private static bool IsDeadTourianCorpseDefinition(ushort definitionPointer) =>
        definitionPointer is DeadZoomerDefinition or DeadRipperDefinition or DeadSkreeDefinition;

    /// <summary>Resolves the species-specific callback and variant metadata for a corpse header.</summary>
    private static DeadTourianCorpseProfile ProfileForDeadTourianCorpse(ushort definitionPointer) =>
        definitionPointer switch
        {
            DeadZoomerDefinition => DeadZoomerProfile,
            DeadRipperDefinition => DeadRipperProfile,
            DeadSkreeDefinition => DeadSkreeProfile,
            _ => throw new ArgumentOutOfRangeException(nameof(definitionPointer)),
        };

    /// <summary>Checks whether the slot's touch callback uses the shared corpse rot handler.</summary>
    private static bool HasDeadTourianCorpseTouchOrShotCallback(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer))
            return false;
        return slot.Definition.TouchAiPointer ==
            ProfileForDeadTourianCorpse(slot.EnemyDefinitionPointer).TouchAndShotFunction;
    }

    /// <summary>Checks whether the slot's shot callback uses the shared corpse rot handler.</summary>
    private static bool HasDeadTourianCorpseShotCallback(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer))
            return false;
        return slot.Definition.ShotAiPointer ==
            ProfileForDeadTourianCorpse(slot.EnemyDefinitionPointer).TouchAndShotFunction;
    }

    /// <summary>Checks whether the slot has its species-specific corpse power-bomb callback.</summary>
    private static bool HasDeadTourianCorpsePowerBombCallback(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer))
            return false;
        return slot.Definition.PowerBombReactionPointer ==
            ProfileForDeadTourianCorpse(slot.EnemyDefinitionPointer).PowerBombFunction;
    }

    /// <summary>Ports initializers <c>$A9:D849/$D876/$D89F</c>.</summary>
    private void InitializeDeadTourianCorpse(RoomEnemySlot slot)
    {
        DeadTourianCorpseProfile profile =
            ProfileForDeadTourianCorpse(slot.EnemyDefinitionPointer);
        if ((slot.Parameter1 & 1) != 0 || slot.Parameter1 / 2 >= profile.Variants.Length)
        {
            throw new InvalidDataException(
                $"Dead {profile.Species} parameter 1 ${slot.Parameter1:X4} has no retail variant.");
        }

        int variantIndex = slot.Parameter1 / 2;
        DeadTourianCorpseVariant variant = profile.Variants[variantIndex];
        DeadTourianCorpseDefinition definition =
            DeadTourianCorpseDefinitions.For(profile.Species, variantIndex);
        if (definition.EntryCount == 0)
            throw new InvalidDataException($"Dead {profile.Species} has zero corpse rows.");

        // Missing resources must not leave a registered corpse, changed slot or partially
        // initialized WRAM behind when the host reports a recoverable error.
        ReadOnlySpan<byte> installedTiles = InstalledDeadTourianCorpseTiles();

        ushort yLimit = unchecked((ushort)(definition.EntryCount - 1));
        ushort lateMoveEntryIndex = unchecked((ushort)(yLimit - 1));
        var state = new DeadTourianCorpseEnemyState(
            slot,
            profile.Species,
            variantIndex,
            definition.ConfigurationPointer,
            definition.RottingTablePointer,
            definition.VramTransferPointer,
            definition.CopyFunction,
            definition.MoveFunction,
            definition.RotationTablePointer,
            definition.FinishFunction,
            definition.EntryCount,
            yLimit,
            lateMoveEntryIndex,
            definition.WrapOffset,
            profile,
            variant);
        _deadTourianCorpseStates[slot.SlotIndex] = state;

        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        slot.VariableA = profile.WaitFunction;
        SetDeadSidehopperInstruction(slot, definition.InitialInstructionPointer);
        CorpseRottingTableProcessor.Initialize(
            _bus!,
            0x7e0000 | definition.RottingTablePointer,
            definition.EntryCount);
        InitializeDeadTourianCorpseGraphics(variant, installedTiles);
    }

    /// <summary>All three headers point main AI at shared dispatcher <c>$A9:D8DB</c>.</summary>
    private void RunDeadTourianCorpseMain(RoomEnemySlot slot, SamusState? samus)
    {
        DeadTourianCorpseEnemyState state = RequireDeadTourianCorpseState(slot);
        DeadTourianCorpseProfile profile = state.Profile;
        if (slot.VariableA == profile.WaitFunction)
        {
            if (samus?.Kinematics.DidCollideWithSolidEnemy(slot.NativeIndex) == true)
                slot.VariableA = profile.PreRotFunction;
            return;
        }

        if (slot.VariableA == profile.PreRotFunction)
        {
            state.PreRotDelayCounter = unchecked((ushort)(state.PreRotDelayCounter + 1));
            if (state.PreRotDelayCounter >= 16)
            {
                slot.VariableA = profile.RottingFunction;
                slot.Properties = unchecked((ushort)(
                    slot.Properties | DeadMonsterInteractionRejectedProperty));
            }
            return;
        }

        if (slot.VariableA == profile.RottingFunction)
        {
            RunDeadTourianCorpseRotting(slot, state);
            return;
        }

        if (slot.VariableA != DeadTourianCorpseNoOperationFunction)
        {
            throw new InvalidDataException(
                $"Dead {profile.Species} function $A9:{slot.VariableA:X4} is not translated.");
        }
    }

    /// <summary>Touch and shot callbacks for these three corpse-only actors are identical.</summary>
    private void TriggerDeadTourianCorpseRotting(RoomEnemySlot slot)
    {
        DeadTourianCorpseEnemyState state = RequireDeadTourianCorpseState(slot);
        slot.VariableA = state.Profile.RottingFunction;
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>Power-bomb callbacks reject actors whose decomposition already set $0400.</summary>
    private void ResolveDeadTourianCorpsePowerBomb(RoomEnemySlot slot)
    {
        RequireDeadTourianCorpseState(slot);
        if ((slot.Properties & DeadMonsterInteractionRejectedProperty) == 0)
            TriggerDeadTourianCorpseRotting(slot);
    }

    /// <summary>Advances the shared row scheduler, performs pixel-row work, and queues tile transfers.</summary>
    private void RunDeadTourianCorpseRotting(
        RoomEnemySlot slot,
        DeadTourianCorpseEnemyState state)
    {
        state.ProcessCallCount++;
        bool stillRotting = CorpseRottingTableProcessor.Step(
            _bus!,
            EnemyWorkMemory,
            0x7e0000 | state.TablePointer,
            state.EntryCount,
            state.YLimit,
            state.LateMoveEntryIndex,
            (yOffset, move) => CopyOrMoveDeadTourianCorpsePixelRow(state, yOffset, move),
            entryIndex => FinishDeadTourianCorpseRow(slot, state, entryIndex));
        if (!stillRotting)
            slot.VariableA = DeadTourianCorpseNoOperationFunction;
        AppendDeadMonsterVramTransfers(state.VramTablePointer);
    }

    /// <summary>Copies the selected corpse's authored tile-sheet rows into the shared work buffer.</summary>
    private void InitializeDeadTourianCorpseGraphics(DeadTourianCorpseVariant variant,
        ReadOnlySpan<byte> installedTiles)
    {
        foreach (DeadTourianCorpseGraphicsCopy copy in variant.InitialGraphicsCopies)
        {
            for (int byteIndex = 0; byteIndex < copy.Length; byteIndex++)
            {
                int sourceOffset = copy.SourceOffset + byteIndex;
                _bus!.WriteByte(
                    DeadMonsterWorkBufferAddress + copy.DestinationOffset + byteIndex,
                    installedTiles[sourceOffset]);
            }
        }
    }

    /// <summary>Copies one rotated pixel row downward and optionally clears its source bitplanes.</summary>
    private void CopyOrMoveDeadTourianCorpsePixelRow(
        DeadTourianCorpseEnemyState state,
        ushort yOffset,
        bool move)
    {
        ushort sourceOffset = unchecked((ushort)(
            DeadMonsterRottingDefinitions.RotationOffset(state.RotationTablePointer, yOffset) + (yOffset & 7) * 2));
        ushort destinationOffset = (yOffset & 7) >= 6
            ? unchecked((ushort)(state.WrapOffset + sourceOffset))
            : sourceOffset;

        foreach (ushort columnWordOffset in state.Variant.ColumnWordOffsets)
        {
            int sourceWord = sourceOffset / 2 + columnWordOffset;
            int destinationWord = destinationOffset / 2 + columnWordOffset + 1;
            if (yOffset < state.Variant.MaximumY)
            {
                WriteDeadMonsterWorkWord(destinationWord, ReadDeadMonsterWorkWord(sourceWord));
                WriteDeadMonsterWorkWord(
                    destinationWord + 8,
                    ReadDeadMonsterWorkWord(sourceWord + 8));
            }
            if (move)
            {
                WriteDeadMonsterWorkWord(sourceWord, 0);
                WriteDeadMonsterWorkWord(sourceWord + 8, 0);
            }
        }
    }

    /// <summary>Records completed-row effects, spawning dust and periodically requesting crumble audio.</summary>
    private void FinishDeadTourianCorpseRow(
        RoomEnemySlot slot,
        DeadTourianCorpseEnemyState state,
        ushort entryIndex)
    {
        state.FinishedEntryCount++;
        state.LastFinishedEntryIndex = entryIndex;
        ushort random = RequireRandomNumber();
        SpawnRoomGraphicsDustExplosion(
            unchecked((ushort)(slot.XPosition + (random & 0x001a) - 14)),
            unchecked((ushort)(slot.YPosition + 16)),
            animationIndex: 10);
        state.DustSpawnCount++;
        if ((_randomEnemyCounter & 7) == 0)
            LastDeadSidehopperSoundEffect = 0x0010;
    }

    /// <summary>Appends the selected terminated DMA list to this frame's VRAM transfer queue.</summary>
    private void AppendDeadMonsterVramTransfers(ushort vramTablePointer)
    {
        foreach (DeadMonsterVramTransferDefinition record in
                 DeadMonsterRottingDefinitions.ForTransferTable(vramTablePointer))
            _deadSidehopperFrameVramTransfers.Add(new VramWriteEntry(
                record.SizeInBytes, record.SourceAddress, record.EncodedVramDestination));
    }

    /// <summary>Returns initialized corpse state only when it belongs to the supplied enemy slot.</summary>
    private DeadTourianCorpseEnemyState RequireDeadTourianCorpseState(RoomEnemySlot slot)
    {
        DeadTourianCorpseEnemyState state = _deadTourianCorpseStates[slot.SlotIndex] ??
            throw new InvalidDataException(
                $"Enemy slot {slot.SlotIndex} has no dead Tourian corpse state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("Dead Tourian corpse state belongs to another slot.");
        return state;
    }

    /// <summary>Species-level callback addresses and the available visual variants.</summary>
    /// <param name="Species">Corpse family selected by the enemy definition.</param>
    /// <param name="WaitFunction">Native function that waits for solid Samus collision.</param>
    /// <param name="PreRotFunction">Native delay state entered after the initial collision.</param>
    /// <param name="RottingFunction">Native function dispatched while the corpse rows decompose.</param>
    /// <param name="TouchAndShotFunction">Shared native touch and projectile callback address.</param>
    /// <param name="PowerBombFunction">Species-specific native power-bomb reaction address.</param>
    /// <param name="Variants">Tile layouts and source graphics for each supported parameter variant.</param>
    internal sealed record DeadTourianCorpseProfile(
        DeadTourianCorpseSpecies Species,
        ushort WaitFunction,
        ushort PreRotFunction,
        ushort RottingFunction,
        ushort TouchAndShotFunction,
        ushort PowerBombFunction,
        DeadTourianCorpseVariant[] Variants);

    /// <summary>Tile dimensions and graphics placement for one corpse parameter variant.</summary>
    /// <param name="MaximumY">Exclusive pixel-row bound used by the rotting scheduler.</param>
    /// <param name="ColumnWordOffsets">Work-buffer word offsets for each tile column.</param>
    /// <param name="InitialGraphicsCopies">Authored sheet-to-work-buffer copies for initial corpse art.</param>
    internal sealed record DeadTourianCorpseVariant(
        ushort MaximumY,
        ushort[] ColumnWordOffsets,
        DeadTourianCorpseGraphicsCopy[] InitialGraphicsCopies);

    /// <summary>One contiguous graphics row copied from the installed sheet to corpse work memory.</summary>
    /// <param name="SourceOffset">Byte offset in the installed tile sheet.</param>
    /// <param name="DestinationOffset">Byte offset in the shared dead-monster work buffer.</param>
    /// <param name="Length">Number of bytes copied for this row.</param>
    internal readonly record struct DeadTourianCorpseGraphicsCopy(
        int SourceOffset,
        int DestinationOffset,
        int Length);
}

/// <summary>The three non-sidehopper dead-monster graphics families.</summary>
public enum DeadTourianCorpseSpecies
{
    /// <summary>Three 3-by-2-tile Zoomer corpse variants; native wait/pre-rot/rotting functions are $A9:DA69/$DA94/$DAD0.</summary>
    Zoomer,
    /// <summary>Two 3-by-2-tile Ripper corpse variants; native wait/pre-rot/rotting functions are $A9:DA73/$DA99/$DAE6.</summary>
    Ripper,
    /// <summary>Three 2-by-4-tile Skree corpse variants; native wait/pre-rot/rotting functions are $A9:DA6E/$DA9E/$DAFC.</summary>
    Skree,
}

/// <summary>Typed extended state for one dead Zoomer, Ripper, or Skree.</summary>
public sealed class DeadTourianCorpseEnemyState
{
    /// <summary>Creates the decoded state for one corpse slot and its selected native variant.</summary>
    /// <param name="slot">Enemy slot whose common variables control the corpse lifecycle.</param>
    /// <param name="species">Corpse family selecting callback and graphics metadata.</param>
    /// <param name="variantIndex">Zero-based corpse variant selected by parameter one.</param>
    /// <param name="configurationPointer">Native initialization-record pointer.</param>
    /// <param name="tablePointer">WRAM pointer to the mutable four-byte rotting entries.</param>
    /// <param name="vramTablePointer">Native DMA-list pointer queued during each rotting update.</param>
    /// <param name="copyFunction">Native nondestructive pixel-row copy function identity.</param>
    /// <param name="moveFunction">Native destructive pixel-row move function identity.</param>
    /// <param name="rotationTablePointer">Native tile-row rotation table pointer.</param>
    /// <param name="finishFunction">Native completed-row callback identity.</param>
    /// <param name="entryCount">Number of row entries in the rotting table.</param>
    /// <param name="yLimit">Exclusive row-height boundary used by completion checks.</param>
    /// <param name="lateMoveEntryIndex">First entry using destructive moves during final delays.</param>
    /// <param name="wrapOffset">Work-buffer displacement for rows crossing tile boundaries.</param>
    /// <param name="profile">Species callbacks and available variants.</param>
    /// <param name="variant">Tile layout and initial graphics copies for the selected variant.</param>
    internal DeadTourianCorpseEnemyState(
        RoomEnemySlot slot,
        DeadTourianCorpseSpecies species,
        int variantIndex,
        ushort configurationPointer,
        ushort tablePointer,
        ushort vramTablePointer,
        ushort copyFunction,
        ushort moveFunction,
        ushort rotationTablePointer,
        ushort finishFunction,
        ushort entryCount,
        ushort yLimit,
        ushort lateMoveEntryIndex,
        ushort wrapOffset,
        RoomEnemySystem.DeadTourianCorpseProfile profile,
        RoomEnemySystem.DeadTourianCorpseVariant variant)
    {
        Slot = slot;
        Species = species;
        VariantIndex = variantIndex;
        ConfigurationPointer = configurationPointer;
        TablePointer = tablePointer;
        VramTablePointer = vramTablePointer;
        CopyFunction = copyFunction;
        MoveFunction = moveFunction;
        RotationTablePointer = rotationTablePointer;
        FinishFunction = finishFunction;
        EntryCount = entryCount;
        YLimit = yLimit;
        LateMoveEntryIndex = lateMoveEntryIndex;
        WrapOffset = wrapOffset;
        Profile = profile;
        Variant = variant;
    }

    /// <summary>Owning room slot, whose variable A holds the bank-$A9 wait, pre-rot, rotting, or completed no-operation function; state is registered for this slot until room reset.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>Corpse family selected by the enemy header, determining native callbacks and the tiled artwork layout.</summary>
    public DeadTourianCorpseSpecies Species { get; }
    /// <summary>Zero-based variant within the species, obtained by dividing the even native parameter-1 table-byte offset by two; Rippers allow 0–1, Zoomers and Skrees 0–2.</summary>
    public int VariantIndex { get; }
    /// <summary>Bank-$A9 pointer to the selected 16-byte corpse initialization record in the eight-record catalog beginning at $DD88.</summary>
    public ushort ConfigurationPointer { get; }
    /// <summary>Bank-$7E WRAM pointer to mutable four-byte rotting entries, each containing a signed pixel-row Y offset and an update-delay word; initialized with descending Y and staggered delays.</summary>
    public ushort TablePointer { get; }
    /// <summary>Bank-$A9 pointer to the terminated tile-data DMA list queued after every rotting call, including the call that finishes the corpse.</summary>
    public ushort VramTablePointer { get; }
    /// <summary>Bank-$A9 identity of the variant's nondestructive pixel-row copy routine, used by the shared scheduler during the final delay updates.</summary>
    public ushort CopyFunction { get; }
    /// <summary>Bank-$A9 identity of the variant's destructive pixel-row move routine, which copies the row downward and clears its source bitplanes.</summary>
    public ushort MoveFunction { get; }
    /// <summary>Bank-$A9 pointer to the species' tile-row byte-offset table at $E24C/$E252/$E258, used to locate 4bpp pixel rows in the corpse work buffer.</summary>
    public ushort RotationTablePointer { get; }
    /// <summary>$A9:DC08, CorpseRotEntryFinishedHook_Normal: row-completion callback that spawns dust below the corpse and periodically queues the crumble sound.</summary>
    public ushort FinishFunction { get; }
    /// <summary>Number of four-byte rotting entries and native sprite height in pixels: 16 for Zoomer/Ripper, 32 for Skree.</summary>
    public ushort EntryCount { get; }
    /// <summary>Exclusive pixel-row Y limit and final entry index, equal to sprite height minus one; reaching it completes an entry.</summary>
    public ushort YLimit { get; }
    /// <summary>First entry index that uses destructive moves even during the last delay updates, equal to sprite height minus two.</summary>
    public ushort LateMoveEntryIndex { get; }
    /// <summary>Byte displacement added when rows 6 or 7 move across an 8-pixel tile boundary: 84 for three-column Zoomer/Ripper blocks, 52 for two-column Skree blocks.</summary>
    public ushort WrapOffset { get; }
    /// <summary>Native Corpse.preRotDelayTimer in slot variable B; increments once per pre-rot AI call after solid Samus collision, switching to rotting at 16. Touch/shot callbacks enter rotting directly.</summary>
    public ushort PreRotDelayCounter
    {
        get => Slot.VariableB;
        internal set => Slot.VariableB = value;
    }
    /// <summary>Port diagnostic count of shared rotting-scheduler calls for this corpse; the final completion call is included.</summary>
    public uint ProcessCallCount { get; internal set; }
    /// <summary>Port diagnostic count of row-table completion callbacks, including the final entry before the AI becomes a no-operation.</summary>
    public uint FinishedEntryCount { get; internal set; }
    /// <summary>Port diagnostic count of dust spawn requests from completed rows; each uses animation 10 at corpse Y plus 16 pixels and a native RNG-masked horizontal offset.</summary>
    public uint DustSpawnCount { get; internal set; }
    /// <summary>Zero-based index of the most recently completed rotting-table entry, or $FFFF before any entry finishes.</summary>
    public ushort LastFinishedEntryIndex { get; internal set; } = ushort.MaxValue;

    // Callback profiles remain assembly-internal. Public debugger state exposes only the
    // stable ROM-derived words above; actor code keeps typed access without parallel maps.
    /// <summary>Species-level callback metadata retained for shared dispatcher decisions.</summary>
    internal RoomEnemySystem.DeadTourianCorpseProfile Profile { get; }
    /// <summary>Selected tile layout used by pixel-row work and body-buffer addressing.</summary>
    internal RoomEnemySystem.DeadTourianCorpseVariant Variant { get; }
}
