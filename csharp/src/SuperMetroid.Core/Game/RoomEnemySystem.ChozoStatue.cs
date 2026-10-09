using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$AA pre-instruction pointers used by the two retail Chozo-statue variants.
/// The numeric values are the actual cartridge addresses stored in <c>ai_preinstr</c>.
/// </summary>
public enum ChozoStatuePreInstruction : ushort
{
    /// <summary>$AA:E7A6, the initializer's one-byte RTL.</summary>
    Idle = 0xe7a6,

    /// <summary>$AA:807B, the bank-common RTL installed by instruction $8074.</summary>
    Cleared = 0x807b,

    /// <summary>$AA:E445, waits for Lower Norfair's hand-trigger PLM.</summary>
    WaitForLowerNorfairHandTrigger = 0xe445,

    /// <summary>$AA:E7AE, waits for Phantoon's boss bit and the Wrecked Ship hand trigger.</summary>
    WaitForWreckedShipHandTrigger = 0xe7ae,

    /// <summary>$AA:E7DA, the no-op installed while the Wrecked Ship statue walks.</summary>
    Walking = 0xe7da,
}

/// <summary>
/// Typed view of the common enemy words reused by the bank-$AA Chozo-statue controller.
/// The wrapper intentionally writes through to the physical slot so debugger watches retain
/// the same layout as WRAM <c>$0FA8..$0FB2</c>.
/// </summary>
public sealed class ChozoStatueState
{
    /// <summary>Common enemy slot whose native words are exposed as the statue's controller state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a typed state view over the statue's physical enemy slot.</summary>
    /// <param name="slot">The room slot containing the statue's common AI variables.</param>
    internal ChozoStatueState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Movement-table byte offset consumed by instruction $AA:E5D8.</summary>
    public ushort MovementTableOffset
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native variable A, initialized to -$0100 when Wrecked Ship wakes.</summary>
    public ushort VariableA
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Native variable B, initialized to +$0100 when Wrecked Ship wakes.</summary>
    public ushort VariableB
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>The literal bank-$AA pre-instruction pointer stored in variable F.</summary>
    public ChozoStatuePreInstruction PreInstruction
    {
        get => (ChozoStatuePreInstruction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}

/// <summary>
/// One cross-bank PLM request made by the Chozo-statue enemy AI. Hardcoded requests carry
/// explicit room-block coordinates; ordinary requests use the block selected by the preceding
/// pixel probe. Retaining the header pointer keeps this seam consumable by bank $84 later.
/// </summary>
/// <param name="HeaderPointer">Bank-$84 PLM header that identifies the structure to spawn.</param>
/// <param name="BlockX">Horizontal room-block coordinate used for placement.</param>
/// <param name="BlockY">Vertical room-block coordinate used for placement.</param>
/// <param name="IsHardcoded">True when the request uses authored coordinates instead of a collision-derived probe position.</param>
public readonly record struct ChozoStatuePlmRequest(
    ushort HeaderPointer,
    int BlockX,
    int BlockY,
    bool IsHardcoded);

public sealed partial class RoomEnemySystem
{
    /// <summary>Native enemy-definition pointer for the palette-only tube-crack record in the room setup.</summary>
    private const ushort N00bTubeCracksDefinition = 0xf0bf;

    /// <summary>Empty bank-$AA spritemap installed because the statue's visible body is supplied by room artwork.</summary>
    private const ushort EmptyBankAaSpritemap = 0x804d;

    /// <summary>Per-slot typed state views for initialized Chozo statues.</summary>
    private readonly ChozoStatueState?[] _chozoStatueStates =
        new ChozoStatueState?[MaximumEnemyCount];
    /// <summary>PLM requests emitted by statue AI and consumed by the room's bank-$84 PLM owner.</summary>
    private readonly List<ChozoStatuePlmRequest> _chozoStatuePlmRequests = new();
    /// <summary>Room integration callback used to apply the statue's Samus-control lock and release.</summary>
    private Action<bool>? _setSamusControlsEnabled;
    /// <summary>Room integration callback used for the statue sequence's native scroll-state writes.</summary>
    private Action<int, RoomScrollState>? _setRoomScrollState;

    /// <summary>Consumes each published request once through the shared bank-$84 slot allocator.</summary>
    public void ApplyPendingChozoStatuePlms(RoomLevelData level, RoomPlmSystem plms)
    {
        while (_chozoStatuePlmRequests.Count != 0)
        {
            plms.TrySpawnChozoStatuePlm(level, _chozoStatuePlmRequests[0]);
            _chozoStatuePlmRequests.RemoveAt(0);
        }
    }

    /// <summary>Last library-two sound requested by either walking sequence.</summary>
    public ushort? LastChozoStatueSoundEffect { get; private set; }

    /// <summary>Last value sent through native <c>CallSomeSamusCode(0/1)</c>.</summary>
    public bool ChozoStatueSamusControlsEnabled { get; private set; } = true;

    /// <summary>FX timer written by instruction $AA:E429.</summary>
    public ushort ChozoStatueFxTimer { get; private set; }

    /// <summary>FX Y velocity written by instruction $AA:E429.</summary>
    public ushort ChozoStatueFxYVelocity { get; private set; }

    /// <summary>FX base Y written by instruction $AA:E436.</summary>
    public ushort ChozoStatueFxBaseYPosition { get; private set; }

    /// <summary>Clears per-room statue state and installs callbacks for the active room's Samus and scroll systems.</summary>
    /// <param name="setSamusControlsEnabled">Callback that applies the statue sequence's control lock and release.</param>
    /// <param name="setRoomScrollState">Callback that applies native scroll-state writes by raw room index.</param>
    private void ResetChozoStatueRoomState(
        Action<bool>? setSamusControlsEnabled,
        Action<int, RoomScrollState>? setRoomScrollState)
    {
        Array.Clear(_chozoStatueStates);
        _chozoStatuePlmRequests.Clear();
        _setSamusControlsEnabled = setSamusControlsEnabled;
        _setRoomScrollState = setRoomScrollState;
        LastChozoStatueSoundEffect = null;
        ChozoStatueSamusControlsEnabled = true;
        ChozoStatueFxTimer = 0;
        ChozoStatueFxYVelocity = 0;
        ChozoStatueFxBaseYPosition = 0;
    }

    /// <summary>
    /// Ports the complete palette-only initializer at $AA:E716. The retail record begins
    /// with property $0200, so it exists solely long enough to replace target palette rows
    /// nine and ten; the scheduler removes it before any main-AI frame.
    /// </summary>
    private void InitializeN00bTubeCracks()
    {
        (TileArtwork?.ChozoAndTubeColors ?? throw new InvalidDataException(
            "Tube crack colors require installed artwork.")).ApplyTubeCracks(_cgram!);
    }

    /// <summary>Ports $AA:E725-$E7A1 for both shipped parameter-two variants.</summary>
    private void InitializeChozoStatue(RoomEnemySlot statue)
    {
        if ((statue.Parameter2 & 1) != 0 || statue.Parameter2 > 2)
        {
            throw new InvalidDataException(
                $"Chozo statue parameter two ${statue.Parameter2:X4} is outside its two-entry ROM table.");
        }

        // The original ORs $2000, $0800, and the otherwise unnamed high bit. Keep $8000 raw:
        // no translated caller has proved a stable semantic name for that property yet.
        statue.Properties = statue.Properties.With(
            EnemyProperties.SolidToSamus |
            EnemyProperties.ProcessInstructions |
            EnemyProperties.ProcessOffScreen);
        statue.SpritemapPointer = EmptyBankAaSpritemap;
        statue.InstructionTimer = 1;
        statue.Timer = 0;
        statue.Parameter1 = 0;
        statue.PaletteIndex = 0;

        var state = new ChozoStatueState(statue)
        {
            PreInstruction = ChozoStatuePreInstruction.Idle,
        };
        _chozoStatueStates[statue.SlotIndex] = state;

        // The native initializer writes enemy_data[0].layer regardless of cur_enemy_index.
        // Every retail population places this singleton in slot zero, but preserve the alias
        // explicitly so malformed/debug populations expose the same cross-slot behavior.
        _slots[0].Layer = 0;

        if (statue.Parameter2 == 0)
        {
            statue.CurrentInstruction =
                ChozoStatueInstructionProgramDefinitions.WreckedShipInitial;
            LoadChozoStatuePalette(wreckedShip: true);
            PublishHardcodedChozoPlm(ChozoStatuePlmRomData.WreckedShipHand, blockX: 0x4a, blockY: 0x17);
            PublishHardcodedChozoPlm(ChozoStatuePlmRomData.BlockSlopeAccess, blockX: 0x17, blockY: 0x1d);
        }
        else
        {
            statue.CurrentInstruction =
                ChozoStatueInstructionProgramDefinitions.LowerNorfairInitial;
            LoadChozoStatuePalette(wreckedShip: false);
            PublishHardcodedChozoPlm(ChozoStatuePlmRomData.LowerNorfairHand, blockX: 0x0c, blockY: 0x1d);
        }
    }

    /// <summary>Loads the palette variant selected by the statue's room population.</summary>
    /// <param name="wreckedShip">True for the Wrecked Ship colors; false for Lower Norfair.</param>
    private void LoadChozoStatuePalette(bool wreckedShip)
    {
        var colors = TileArtwork?.ChozoAndTubeColors ?? throw new InvalidDataException(
            "Chozo statue colors require installed artwork.");
        if (wreckedShip) colors.ApplyWreckedShip(_cgram!);
        else colors.ApplyLowerNorfair(_cgram!);
    }

    /// <summary>
    /// Represents the exact enemy-slot side effect produced by the bank-$84 Chozo hand
    /// trigger. Collision/pose/item admission remains the PLM owner's responsibility; once
    /// admitted, both native trigger setups write parameter one and disable Samus controls.
    /// </summary>
    public void ActivateChozoStatueHandTrigger(RoomLevelData level, int? collisionBlockIndex = null)
    {
        ArgumentNullException.ThrowIfNull(level);
        EnsureLoaded();
        RoomEnemySlot statue = _slots[0];
        if (statue.EnemyDefinitionPointer != ChozoStatueEnemyDefinitions.EnemyDefinitionPointer)
            throw new InvalidOperationException("Chozo hand trigger requires statue $F0FF in enemy slot zero.");

        int triggerBlockX = statue.Parameter2 == 0 ? 0x4a : ChozoStatuePlmRomData.LowerNorfairTriggerX;
        int triggerBlockY = statue.Parameter2 == 0 ? 0x17 : ChozoStatuePlmRomData.LowerNorfairTriggerY;
        int triggerBlockIndex = collisionBlockIndex ?? level.GetBlockIndex(triggerBlockX, triggerBlockY);
        RoomCollisionBlock triggerBlock = level.GetCollisionBlockByIndex(triggerBlockIndex);

        // Both setup routines clear only the block-type nibble. The parallel BTS byte and
        // visual block number survive until their subsequently spawned hardcoded PLM runs.
        level.SetForegroundEntry(
            triggerBlockIndex,
            unchecked((ushort)(triggerBlock.LevelWord & 0x0fff)));
        statue.Parameter1 = 1;
        SetChozoStatueSamusControls(enabled: false);
        if (statue.Parameter2 != 0)
        {
            // Lower Norfair's $84:D18F trigger records event $0C before waking the actor.
            RequireSetEvent(EventNumber.LowerNorfairChozoLoweredAcid);
            PublishHardcodedChozoPlm(
                ChozoStatuePlmRomData.CrumblePlug,
                blockX: 0x0c,
                blockY: 0x1d);
        }
        else
        {
            // $84:D620 performs two little-endian word stores before handing the statue to
            // $E7AE: scroll bytes 7/8 become green and 13/14 become blue. PLM $D6F8 then
            // queues the authored slope-access terrain transition in bank $84.
            RequireSetRoomScrollState(7, RoomScrollState.Green);
            RequireSetRoomScrollState(8, RoomScrollState.Green);
            RequireSetRoomScrollState(13, RoomScrollState.Blue);
            RequireSetRoomScrollState(14, RoomScrollState.Blue);
            PublishHardcodedChozoPlm(
                ChozoStatuePlmRomData.ClearSlopeAccess,
                blockX: 0x17,
                blockY: 0x1d);
        }
    }

    /// <summary>Runs the statue's trigger-waiting state and starts the corresponding activated instruction list.</summary>
    /// <param name="statue">Common enemy slot containing the statue and its current instruction state.</param>
    /// <param name="state">Typed view of the bank-$AA pre-instruction and movement variables.</param>
    private void RunChozoStatueMain(RoomEnemySlot statue, ChozoStatueState state)
    {
        switch (state.PreInstruction)
        {
            case ChozoStatuePreInstruction.Idle:
            case ChozoStatuePreInstruction.Cleared:
            case ChozoStatuePreInstruction.Walking:
                return;

            case ChozoStatuePreInstruction.WaitForLowerNorfairHandTrigger:
                if (statue.Parameter1 != 0)
                {
                    statue.CurrentInstruction =
                        ChozoStatueInstructionProgramDefinitions.LowerNorfairActivated;
                    statue.InstructionTimer = 1;
                }
                return;

            case ChozoStatuePreInstruction.WaitForWreckedShipHandTrigger:
                if (RequireAreaBossDefeated() && statue.Parameter1 != 0)
                {
                    statue.CurrentInstruction =
                        ChozoStatueInstructionProgramDefinitions.WreckedShipActivated;
                    statue.InstructionTimer = 1;
                    state.VariableA = unchecked((ushort)-256);
                    state.VariableB = 256;
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Chozo statue pre-instruction $AA:{(ushort)state.PreInstruction:X4} is not translated.");
        }
    }

    /// <summary>Dispatches every callback reachable from lists $E39D/$E3A7/$E457/$E461.</summary>
    private bool TryProcessChozoStatueInstruction(
        RoomEnemySlot statue,
        SamusState? samus,
        RoomLevelData? level,
        ushort opcode,
        ref ushort cursor)
    {
        if (statue.EnemyDefinitionPointer != ChozoStatueEnemyDefinitions.EnemyDefinitionPointer)
            return false;

        ChozoStatueState state = RequireChozoStatueState(statue);
        switch (opcode)
        {
            case ChozoStatueInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY:
                state.PreInstruction = (ChozoStatuePreInstruction)ReadChozoInstructionOperand(cursor);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case ChozoStatueInstructionCodes.Instruction_CommonAA3_SetEnemy0FB2ToRTS:
                state.PreInstruction = ChozoStatuePreInstruction.Cleared;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case ChozoStatueInstructionCodes.Instruction_Chozo_StartLoweringAcid:
                ChozoStatueFxTimer = ChozoStatueFxData.LoweringDelay;
                ChozoStatueFxYVelocity = ChozoStatueFxData.LoweringVelocity;
                // Native AI writes the same FX words consumed by the liquid handler.
                // Apply once at instruction execution, not every frame from the retained
                // diagnostic values, which would repeatedly reset the delay.
                _roomFx?.ApplyCartridgeMotionWrites(
                    timer: ChozoStatueFxTimer, packedYVelocity: ChozoStatueFxYVelocity);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case ChozoStatueInstructionCodes.Instruction_Chozo_SetLoweredAcidPosition:
                ChozoStatueFxBaseYPosition = ChozoStatuePlmRomData.LoweredAcidY;
                _roomFx?.ApplyCartridgeMotionWrites(baseYPosition: ChozoStatueFxBaseYPosition);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case ChozoStatueInstructionCodes.Instruction_Chozo_UnlockSamus:
                SetChozoStatueSamusControls(enabled: true);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case ChozoStatueInstructionCodes.Instruction_Chozo_PlayChozoGrabsSamusSFX:
                LastChozoStatueSoundEffect = 0x001c;
                QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2,
                    LastChozoStatueSoundEffect.Value), maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case ChozoStatueInstructionCodes.Instruction_Chozo_PlayChozoFootstepsSFX:
                LastChozoStatueSoundEffect = 0x004b;
                QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2,
                    LastChozoStatueSoundEffect.Value), maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case ChozoStatueInstructionCodes.Instruction_Chozo_SpawnChozoSpikeClearingFootstepProjectile:
                ProcessChozoStatueFootstep(
                    statue,
                    RequireChozoLevel(level),
                    ReadChozoInstructionOperand(cursor));
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case ChozoStatueInstructionCodes.Instruction_Chozo_Movement_IndexInY:
                ProcessChozoStatueMovement(
                    statue,
                    state,
                    RequireChozoSamus(samus),
                    RequireChozoLevel(level),
                    ReadChozoInstructionOperand(cursor));
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case ChozoStatueInstructionCodes.Instruction_Chozo_ReleaseSamus_BlockSlopeAccess:
                FinishWreckedShipChozoSequence();
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            default:
                return false;
        }
    }

    /// <summary>Reads a word operand located after the current two-byte instruction opcode.</summary>
    /// <param name="cursor">Instruction cursor positioned at the opcode.</param>
    private static ushort ReadChozoInstructionOperand(ushort cursor) =>
        ChozoStatueInstructionProgramDefinitions.ReadMechanicsWord(
            unchecked((ushort)(cursor + 2)));

    /// <summary>Applies one authored statue movement-table step and places Samus at its paired joint offset.</summary>
    /// <param name="statue">Statue slot whose collision-aware movement is advanced.</param>
    /// <param name="state">State view receiving the consumed movement-table offset.</param>
    /// <param name="samus">Active Samus actor repositioned relative to the statue after collision movement.</param>
    /// <param name="level">Room collision data used to move and align the statue.</param>
    /// <param name="tableOffset">Byte offset selecting the native statue and Samus motion pair.</param>
    private void ProcessChozoStatueMovement(
        RoomEnemySlot statue,
        ChozoStatueState state,
        SamusState samus,
        RoomLevelData level,
        ushort tableOffset)
    {
        var motion = ChozoCarryMotionDefinitions.Read(tableOffset);
        state.MovementTableOffset = tableOffset;
        short signedVelocity = motion.Velocity;

        // Enemy_MoveRight_IgnoreSlopes receives a signed 8.8 word promoted to 16.16 by
        // INT16_SHL8. Enemy_MoveDown receives its absolute magnitude in the same format.
        _ = MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            statue,
            signedVelocity << 8);
        _ = MoveEnemyVertically(
            level,
            statue,
            Math.Abs((int)signedVelocity) << 8);
        AlignEnemyYWithNonSquareSlope(level, statue);

        // The two 32-word tables are joint offsets indexed by the exact byte offset above.
        // Writing Samus after collision reproduces the native cutscene's absolute ownership.
        samus.XPosition = unchecked((ushort)(statue.XPosition + motion.SamusX));
        samus.YPosition = unchecked((ushort)(statue.YPosition + motion.SamusY));
    }

    /// <summary>Probes beneath a walking footstep and queues a crumble PLM and projectile only over matching terrain.</summary>
    /// <param name="statue">Walking statue slot whose position supplies the probe origin.</param>
    /// <param name="level">Room blocks and collision layer checked by the footstep probe.</param>
    /// <param name="rawXOffset">Signed horizontal offset read from the footstep instruction.</param>
    private void ProcessChozoStatueFootstep(
        RoomEnemySlot statue,
        RoomLevelData level,
        ushort rawXOffset)
    {
        short xOffset = unchecked((short)rawXOffset);
        ushort probeX = unchecked((ushort)(statue.XPosition + xOffset));
        ushort probeY = unchecked((ushort)(statue.YPosition + 28));
        RoomCollisionBlock current = level.GetCollisionBlockAtPixel(probeX, probeY);
        int belowIndex = current.Index + level.WidthInBlocks;
        if ((uint)belowIndex >= (uint)level.ForegroundEntries.Length ||
            (level.GetCollisionBlockByIndex(belowIndex).LevelWord & 0xf000) != 0xa000)
        {
            return;
        }

        _chozoStatuePlmRequests.Add(new ChozoStatuePlmRequest(
            ChozoStatuePlmRomData.CrumblePlug,
            probeX >> 4,
            probeY >> 4,
            IsHardcoded: false));
        SpawnWreckedShipChozoFootstep(statue, rawXOffset);
    }

    /// <summary>Allocates and positions the Wrecked Ship statue's footstep projectile when a projectile slot is available.</summary>
    /// <param name="statue">Statue whose current position anchors the effect.</param>
    /// <param name="xOffset">Signed horizontal offset supplied by the footstep instruction.</param>
    private void SpawnWreckedShipChozoFootstep(RoomEnemySlot statue, ushort xOffset)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep,
            graphicsIndex: 0);
        projectile.XPosition = unchecked((ushort)(statue.XPosition + unchecked((short)xOffset)));
        projectile.YPosition = unchecked((ushort)(statue.YPosition + 28));
    }

    /// <summary>Releases Samus, publishes the final Wrecked Ship scroll states, and requests slope-access terrain.</summary>
    private void FinishWreckedShipChozoSequence()
    {
        SetChozoStatueSamusControls(enabled: true);

        // The assembly performs overlapping 16-bit stores at raw scroll indexes 6, 8, 9,
        // and 13. Publish the resulting bytes (6..10 = 0, 13 = 1, 14 = 0) exactly.
        for (int index = 6; index <= 10; index++)
            RequireSetRoomScrollState(index, RoomScrollState.RedBoundary);
        RequireSetRoomScrollState(13, RoomScrollState.Blue);
        RequireSetRoomScrollState(14, RoomScrollState.RedBoundary);

        PublishHardcodedChozoPlm(
            ChozoStatuePlmRomData.BlockSlopeAccess,
            blockX: 0x17,
            blockY: 0x1d);
    }

    /// <summary>Publishes the current control state and forwards it to the room integration callback.</summary>
    /// <param name="enabled">True to allow Samus control; false to lock control for the statue sequence.</param>
    private void SetChozoStatueSamusControls(bool enabled)
    {
        ChozoStatueSamusControlsEnabled = enabled;
        RequireSetSamusControlsEnabled(enabled);
    }

    /// <summary>Queues a PLM request at an authored room-block coordinate.</summary>
    /// <param name="header">Bank-$84 PLM header to spawn.</param>
    /// <param name="blockX">Horizontal room-block coordinate.</param>
    /// <param name="blockY">Vertical room-block coordinate.</param>
    private void PublishHardcodedChozoPlm(ushort header, int blockX, int blockY) =>
        _chozoStatuePlmRequests.Add(new ChozoStatuePlmRequest(
            header,
            blockX,
            blockY,
            IsHardcoded: true));

    /// <summary>Returns the initialized typed state for a statue slot, or fails when initialization did not create one.</summary>
    /// <param name="statue">Room slot whose state is requested.</param>
    private ChozoStatueState RequireChozoStatueState(RoomEnemySlot statue) =>
        _chozoStatueStates[statue.SlotIndex] ?? throw new InvalidOperationException(
            $"Chozo statue slot {statue.SlotIndex} has no initialized bank-$AA state.");

    /// <summary>Returns the active Samus actor required by movement instructions.</summary>
    /// <param name="samus">Optional active actor supplied by the room scheduler.</param>
    private static SamusState RequireChozoSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Chozo statue movement requires the active Samus actor.");

    /// <summary>Returns the active room level required by statue movement and collision checks.</summary>
    /// <param name="level">Optional room collision data supplied by the room scheduler.</param>
    private static RoomLevelData RequireChozoLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException(
            "Chozo statue movement requires the active room level.");
}
