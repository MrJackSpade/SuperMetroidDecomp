using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Tourian corpse species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class DeadTourianBehavior : IEnemySpecies
{
    /// <summary>Owns the corpse tile scratch area and room RNG used during decomposition.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies the enemy counter used to pace corpse dust sounds.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Emits dust when a rotting-table row finishes.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Provides installed corpse art and queues its VRAM transfers.</summary>
    private readonly DeadMonsterFrame _deadMonsters;

    /// <summary>Connects corpse AI to the room scratch state, projectiles, and installed art.</summary>
    internal DeadTourianBehavior(EnemyRoomBinding room, EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, DeadMonsterFrame deadMonsters)
    {
        _deadMonsters = deadMonsters;
        _room = room;
        _frameContext = frameContext;
        _projectiles = projectiles;
    }

    /// <summary>The shared inert AI function selected after all rotting rows finish.</summary>
    private const ushort DeadTourianCorpseNoOperationFunction = 0xda63;

    /// <summary>Dead Zoomer's callback addresses and three installed art variants.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadZoomerProfile = new(
        DeadTourianCorpseSpecies.Zoomer,
        WaitFunction: 0xda69,
        PreRotFunction: 0xda94,
        RottingFunction: 0xdad0,
        TouchAndShotFunction: 0xdcf8,
        PowerBombFunction: 0xdced,
        Variants: CorpseLayout(columns: 3, rows: 2, workBufferOffset: 0x0940, sheetSources: SheetRun(0x0a60, 0x0060, 3)));

    /// <summary>Dead Ripper's callback addresses and two installed art variants.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadRipperProfile = new(
        DeadTourianCorpseSpecies.Ripper,
        WaitFunction: 0xda73,
        PreRotFunction: 0xda99,
        RottingFunction: 0xdae6,
        TouchAndShotFunction: 0xdd08,
        PowerBombFunction: 0xdcfd,
        Variants: CorpseLayout(columns: 3, rows: 2, workBufferOffset: 0x0b80, sheetSources: SheetRun(0x0a00, 0x0180, 2)));

    /// <summary>Dead Skree's callback addresses and three installed art variants.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadSkreeProfile = new(
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

    /// <summary>Correlates each live enemy slot with the corpse program initialized for that slot.</summary>
    internal readonly DeadTourianCorpseEnemyState?[] _deadTourianCorpseStates =
        new DeadTourianCorpseEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Discards all corpse program state before loading another room.</summary>
    internal void ResetDeadTourianCorpseRoomState() =>
        Array.Clear(_deadTourianCorpseStates);

    /// <summary>Recognizes the three Tourian corpse enemy headers handled by this owner.</summary>
    internal static bool IsDeadTourianCorpseDefinition(EnemyDefinitionId definitionPointer) =>
        definitionPointer is EnemyDefinitionId.CorpseZoomer or EnemyDefinitionId.CorpseRipper or EnemyDefinitionId.CorpseSkree;

    /// <summary>Selects the species-specific native callbacks and art layout for a corpse header.</summary>
    private static DeadTourianCorpseProfile ProfileForDeadTourianCorpse(EnemyDefinitionId definitionPointer) =>
        definitionPointer switch
        {
            EnemyDefinitionId.CorpseZoomer => DeadZoomerProfile,
            EnemyDefinitionId.CorpseRipper => DeadRipperProfile,
            EnemyDefinitionId.CorpseSkree => DeadSkreeProfile,
            _ => throw new ArgumentOutOfRangeException(nameof(definitionPointer)),
        };

    /// <summary>Checks that the header's touch AI points to its species' rot trigger.</summary>
    private static bool HasDeadTourianCorpseTouchOrShotCallback(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer))
            return false;
        return slot.Definition.TouchAiPointer ==
            ProfileForDeadTourianCorpse(slot.EnemyDefinitionPointer).TouchAndShotFunction;
    }

    /// <summary>Checks that the header's shot AI points to its species' rot trigger.</summary>
    internal static bool HasDeadTourianCorpseShotCallback(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer))
            return false;
        return slot.Definition.ShotAiPointer ==
            ProfileForDeadTourianCorpse(slot.EnemyDefinitionPointer).TouchAndShotFunction;
    }

    /// <summary>Checks that the header's power-bomb AI points to its species' rot trigger.</summary>
    private static bool HasDeadTourianCorpsePowerBombCallback(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer))
            return false;
        return slot.Definition.PowerBombReactionPointer ==
            ProfileForDeadTourianCorpse(slot.EnemyDefinitionPointer).PowerBombFunction;
    }

    /// <summary>Ports initializers <c>$A9:D849/$D876/$D89F</c>, validating the art variant before registering state and copying tiles into work RAM.</summary>
    internal void InitializeDeadTourianCorpse(RoomEnemySlot slot)
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
        ReadOnlySpan<byte> installedTiles = _deadMonsters.InstalledCorpseTiles();

        ushort yLimit = unchecked((ushort)(definition.EntryCount - 1));
        ushort lateMoveEntryIndex = unchecked((ushort)(yLimit - 1));
        var state = new DeadTourianCorpseEnemyState(slot, profile.Species, variantIndex, definition.ConfigurationPointer, new CorpseRottingProgram { TablePointer = definition.RottingTablePointer, VramTablePointer = definition.VramTransferPointer, CopyFunction = definition.CopyFunction, MoveFunction = definition.MoveFunction, RotationTablePointer = definition.RotationTablePointer, FinishFunction = definition.FinishFunction, EntryCount = definition.EntryCount, YLimit = yLimit, LateMoveEntryIndex = lateMoveEntryIndex, WrapOffset = definition.WrapOffset }, profile: profile, variant: variant);
        _deadTourianCorpseStates[slot.SlotIndex] = state;

        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        slot.VariableA = profile.WaitFunction;
        DeadSidehopperBehavior.SetDeadSidehopperInstruction(slot, definition.InitialInstructionPointer);
        CorpseRottingTableProcessor.Initialize(
            _room.Scratch.RotTable(definition.RottingTablePointer, definition.EntryCount));
        InitializeDeadTourianCorpseGraphics(variant, installedTiles);
    }

    /// <summary>Runs shared dispatcher <c>$A9:D8DB</c>: collision starts a 16-frame delay, then the corpse rots until its table is exhausted.</summary>
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
                    slot.Properties | DeadSidehopperBehavior.DeadMonsterInteractionRejectedProperty));
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
        if ((slot.Properties & DeadSidehopperBehavior.DeadMonsterInteractionRejectedProperty) == 0)
            TriggerDeadTourianCorpseRotting(slot);
    }

    /// <summary>Advances the rot table once and queues its art transfer, entering inert AI when it completes.</summary>
    private void RunDeadTourianCorpseRotting(
        RoomEnemySlot slot,
        DeadTourianCorpseEnemyState state)
    {
        state.ProcessCallCount++;
        bool stillRotting = CorpseRottingTableProcessor.Step(
            _room.Scratch.RotTable(state.TablePointer, state.EntryCount),
            state.YLimit,
            state.LateMoveEntryIndex,
            (yOffset, move) => CopyOrMoveDeadTourianCorpsePixelRow(state, yOffset, move),
            entryIndex => FinishDeadTourianCorpseRow(slot, state, entryIndex));
        if (!stillRotting)
            slot.VariableA = DeadTourianCorpseNoOperationFunction;
        _deadMonsters.AppendVramTransfers(state.VramTablePointer);
    }

    /// <summary>Copies the selected corpse variant from installed art into the room's tile work buffer.</summary>
    private void InitializeDeadTourianCorpseGraphics(DeadTourianCorpseVariant variant,
        ReadOnlySpan<byte> installedTiles)
    {
        WorkRamByteTable tiles = _room.Scratch.CorpseTiles;
        foreach (DeadTourianCorpseGraphicsCopy copy in variant.InitialGraphicsCopies)
        {
            for (int byteIndex = 0; byteIndex < copy.Length; byteIndex++)
            {
                int sourceOffset = copy.SourceOffset + byteIndex;
                tiles[copy.DestinationOffset + byteIndex] = installedTiles[sourceOffset];
            }
        }
    }

    /// <summary>Copies a decomposing pixel row across each tile column, clearing its source during move steps.</summary>
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
                _deadMonsters.WriteWorkWord(destinationWord, _deadMonsters.ReadWorkWord(sourceWord));
                _deadMonsters.WriteWorkWord(
                    destinationWord + 8,
                    _deadMonsters.ReadWorkWord(sourceWord + 8));
            }
            if (move)
            {
                _deadMonsters.WriteWorkWord(sourceWord, 0);
                _deadMonsters.WriteWorkWord(sourceWord + 8, 0);
            }
        }
    }

    /// <summary>Spawns randomized dust for a completed rot row and gates its sound on the enemy counter.</summary>
    private void FinishDeadTourianCorpseRow(
        RoomEnemySlot slot,
        DeadTourianCorpseEnemyState state,
        ushort entryIndex)
    {
        state.FinishedEntryCount++;
        state.LastFinishedEntryIndex = entryIndex;
        ushort random = _room.Random.Read();
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(unchecked((ushort)(slot.Position.X.Whole + (random & 0x001a) - 14)), unchecked((ushort)(slot.Position.Y.Whole + 16))),
            MiscDustAnimation.CorpseDustCloud);
        state.DustSpawnCount++;
        if ((_frameContext._randomEnemyCounter & 7) == 0)
            _deadMonsters.LastSoundEffect = SoundEffectLibrary2Sounds.DeadMonsterDust.Value;
    }

    /// <summary>Rejects uninitialized slots and slots whose corpse state belongs to another enemy.</summary>
    private DeadTourianCorpseEnemyState RequireDeadTourianCorpseState(RoomEnemySlot slot)
    {
        DeadTourianCorpseEnemyState state = _deadTourianCorpseStates[slot.SlotIndex] ??
            throw new InvalidDataException(
                $"Enemy slot {slot.SlotIndex} has no dead Tourian corpse state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("Dead Tourian corpse state belongs to another slot.");
        return state;
    }

    /// <summary>Groups one corpse species' native callbacks with its available art layouts.</summary>
    internal sealed record DeadTourianCorpseProfile(
        DeadTourianCorpseSpecies Species,
        ushort WaitFunction,
        ushort PreRotFunction,
        ushort RottingFunction,
        ushort TouchAndShotFunction,
        ushort PowerBombFunction,
        DeadTourianCorpseVariant[] Variants);

    /// <summary>Describes tile-column offsets and sheet copies for one corpse art variant.</summary>
    internal sealed record DeadTourianCorpseVariant(
        ushort MaximumY,
        ushort[] ColumnWordOffsets,
        DeadTourianCorpseGraphicsCopy[] InitialGraphicsCopies);

    /// <summary>Copies a contiguous tile run from the installed sheet into corpse work RAM.</summary>
    internal readonly record struct DeadTourianCorpseGraphicsCopy(
        int SourceOffset,
        int DestinationOffset,
        int Length);

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetDeadTourianCorpseRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }

    /// <summary>Routes bomb hits through the shared shot callback, which begins decomposition.</summary>
    internal void RegisterDeadTourianBombReactions(EnemyAiTable table)
    {
        table.AddBombSpecialAfterNoOp(c => HasDeadTourianCorpseShotCallback(c.Enemy) &&
                                           c.SelectedShotAiPointer == c.Enemy.Definition.ShotAiPointer,
            c => TriggerDeadTourianCorpseRotting(c.Enemy));
    }

    /// <summary>Registers the three corpse headers with their common initialization routine.</summary>
    internal void RegisterDeadTourianInitializationAi(EnemyAiTable table)
    {

        EnemyInitializationAiHandler shared119 = (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeDeadTourianCorpse(slot);
        };

        table.RegisterInitialization(EnemyAiRoutine.InitAI_CorpseZoomer, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseZoomer, shared119);

        table.RegisterInitialization(EnemyAiRoutine.InitAI_CorpseRipper, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseRipper, shared119);

        table.RegisterInitialization(EnemyAiRoutine.InitAI_CorpseSkree, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSkree, shared119);
    }

    /// <summary>Routes all three species to the shared wait, delay, and rotting dispatcher.</summary>
    internal void RegisterDeadTourianMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_HurtAI_CorpseEnemies, slot => IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer), (slot, frame, nmiFrameCounter8) =>
        {
            RunDeadTourianCorpseMain(slot, frame.Samus);
        });
    }

    /// <summary>Registers the power-bomb response, which ignores corpses already marked decomposed.</summary>
    internal void RegisterDeadTourianPowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(HasDeadTourianCorpsePowerBombCallback)
        {
            ReplacesCommon = (enemy, _) => ResolveDeadTourianCorpsePowerBomb(enemy),
        });
    }

    /// <summary>Registers shot handling that applies the collision prelude and starts decomposition without health damage.</summary>
    internal void RegisterDeadTourianShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(HasDeadTourianCorpseShotCallback)
        {
            // `$DCF8/$DD08/$DD18` run after the ordinary projectile prelude, set process-offscreen plus $0400, and
            // select their species rot callback. No vulnerability, health damage, or projectile impact follows.
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                TriggerDeadTourianCorpseRotting(c.Enemy);
                return true;
            },
        });
    }

    /// <summary>Starts decomposition on physical contact without applying ordinary enemy contact damage.</summary>
    internal void RegisterDeadTourianTouch(EnemyAiTable table)
    {
        // Dead Zoomer/Ripper/Skree headers intentionally point touch and shot at the same private callback.
        // Physical contact begins decomposition; these already-dead actors never apply ordinary contact damage.
        table.AddTouch(HasDeadTourianCorpseTouchOrShotCallback,
            context => TriggerDeadTourianCorpseRotting(context.Slot));
    }
}
