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
    private readonly RoomEnemySlot _slot;

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
public readonly record struct ChozoStatuePlmRequest(
    ushort HeaderPointer,
    int BlockX,
    int BlockY,
    bool IsHardcoded);

public sealed partial class RoomEnemySystem
{
    private const ushort N00bTubeCracksDefinition = 0xf0bf;
    private const ushort ChozoStatueDefinition = 0xf0ff;

    private const ushort LowerNorfairChozoInstructionList = 0xe39d;
    private const ushort LowerNorfairChozoActiveInstructionList = 0xe3a7;
    private const ushort WreckedShipChozoInstructionList = 0xe457;
    private const ushort WreckedShipChozoActiveInstructionList = 0xe461;
    private const ushort EmptyBankAaSpritemap = 0x804d;

    private const ushort LowerNorfairHandPlm = 0xd6d6;
    private const ushort WreckedShipHandTriggerPlm = 0xd6ee;
    private const ushort WreckedShipWakePlm = 0xd6f8;
    private const ushort WreckedShipSpikeTerrainPlm = 0xd6fc;
    private const ushort ChozoSpikeFootstepTerrainPlm = 0xd113;

    private readonly ChozoStatueState?[] _chozoStatueStates =
        new ChozoStatueState?[MaximumEnemyCount];
    private readonly List<ChozoStatuePlmRequest> _chozoStatuePlmRequests = new();
    private Action<bool>? _setSamusControlsEnabled;
    private Action<int, byte>? _setRoomScrollByte;

    /// <summary>Typed per-slot state; non-statue slots contain null.</summary>
    public IReadOnlyList<ChozoStatueState?> ChozoStatueStates => _chozoStatueStates;

    /// <summary>
    /// Exact bank-$84 requests emitted since the room was loaded. They are publications, not
    /// fabricated terrain edits: the still-independent PLM owner can consume each header.
    /// </summary>
    public IReadOnlyList<ChozoStatuePlmRequest> ChozoStatuePlmRequests =>
        _chozoStatuePlmRequests;

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

    private void ResetChozoStatueRoomState(
        Action<bool>? setSamusControlsEnabled,
        Action<int, byte>? setRoomScrollByte)
    {
        Array.Clear(_chozoStatueStates);
        _chozoStatuePlmRequests.Clear();
        _setSamusControlsEnabled = setSamusControlsEnabled;
        _setRoomScrollByte = setRoomScrollByte;
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
        for (int color = 0; color < 32; color++)
        {
            _cgram!.SetColor(
                144 + color,
                ReadWord(_bus!, EnemyRomTablePointers.ChozoStatue.PaletteWords + color * 2));
        }
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
        statue.Properties = unchecked((ushort)(statue.Properties | 0xa800));
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
            statue.CurrentInstruction = WreckedShipChozoInstructionList;
            LoadChozoStatuePalette(rowNineSource: 0xaae31d, rowTenSource: 0xaae33d);
            PublishHardcodedChozoPlm(WreckedShipHandTriggerPlm, blockX: 0x4a, blockY: 0x17);
            PublishHardcodedChozoPlm(WreckedShipSpikeTerrainPlm, blockX: 0x17, blockY: 0x1d);
        }
        else
        {
            statue.CurrentInstruction = LowerNorfairChozoInstructionList;
            LoadChozoStatuePalette(rowNineSource: 0xaae35d, rowTenSource: 0xaae37d);
            PublishHardcodedChozoPlm(LowerNorfairHandPlm, blockX: 0x0c, blockY: 0x1d);
        }
    }

    private void LoadChozoStatuePalette(int rowNineSource, int rowTenSource)
    {
        for (int color = 0; color < 16; color++)
        {
            _cgram!.SetColor(144 + color, ReadWord(_bus!, rowNineSource + color * 2));
            _cgram.SetColor(160 + color, ReadWord(_bus!, rowTenSource + color * 2));
        }
    }

    /// <summary>
    /// Represents the exact enemy-slot side effect produced by the bank-$84 Chozo hand
    /// trigger. Collision/pose/item admission remains the PLM owner's responsibility; once
    /// admitted, both native trigger setups write parameter one and disable Samus controls.
    /// </summary>
    public void ActivateChozoStatueHandTrigger(RoomLevelData level)
    {
        ArgumentNullException.ThrowIfNull(level);
        EnsureLoaded();
        RoomEnemySlot statue = _slots[0];
        if (statue.EnemyDefinitionPointer != ChozoStatueDefinition)
            throw new InvalidOperationException("Chozo hand trigger requires statue $F0FF in enemy slot zero.");

        int triggerBlockX = statue.Parameter2 == 0 ? 0x4a : 0x0c;
        int triggerBlockY = statue.Parameter2 == 0 ? 0x17 : 0x1d;
        int triggerBlockIndex = level.GetBlockIndex(triggerBlockX, triggerBlockY);
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
                ChozoSpikeFootstepTerrainPlm,
                blockX: 0x0c,
                blockY: 0x1d);
        }
        else
        {
            // $84:D620 performs two little-endian word stores before handing the statue to
            // $E7AE: scroll bytes 7/8 become green and 13/14 become blue. PLM $D6F8 then
            // queues the authored music/terrain transition in bank $84.
            RequireSetRoomScrollByte(7, 2);
            RequireSetRoomScrollByte(8, 2);
            RequireSetRoomScrollByte(13, 1);
            RequireSetRoomScrollByte(14, 1);
            PublishHardcodedChozoPlm(
                WreckedShipWakePlm,
                blockX: 0x17,
                blockY: 0x1d);
        }
    }

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
                    statue.CurrentInstruction = LowerNorfairChozoActiveInstructionList;
                    statue.InstructionTimer = 1;
                }
                return;

            case ChozoStatuePreInstruction.WaitForWreckedShipHandTrigger:
                if (RequireAreaBossDefeated() && statue.Parameter1 != 0)
                {
                    statue.CurrentInstruction = WreckedShipChozoActiveInstructionList;
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
        if (statue.EnemyDefinitionPointer != ChozoStatueDefinition)
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
                ChozoStatueFxTimer = 32;
                ChozoStatueFxYVelocity = 64;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case ChozoStatueInstructionCodes.Instruction_Chozo_SetLoweredAcidPosition:
                ChozoStatueFxBaseYPosition = 722;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case ChozoStatueInstructionCodes.Instruction_Chozo_UnlockSamus:
                SetChozoStatueSamusControls(enabled: true);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case ChozoStatueInstructionCodes.Instruction_Chozo_PlayChozoGrabsSamusSFX:
                LastChozoStatueSoundEffect = 0x001c;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case ChozoStatueInstructionCodes.Instruction_Chozo_PlayChozoFootstepsSFX:
                LastChozoStatueSoundEffect = 0x004b;
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

    private ushort ReadChozoInstructionOperand(ushort cursor) =>
        ReadWord(_bus!, 0xaa0000 | unchecked((ushort)(cursor + 2)));

    private void ProcessChozoStatueMovement(
        RoomEnemySlot statue,
        ChozoStatueState state,
        SamusState samus,
        RoomLevelData level,
        ushort tableOffset)
    {
        if ((tableOffset & 1) != 0 || tableOffset > 0x003e)
        {
            throw new InvalidDataException(
                $"Chozo statue movement offset ${tableOffset:X4} is outside $AA:E630's 32-word table.");
        }

        state.MovementTableOffset = tableOffset;
        short signedVelocity = unchecked((short)ReadWord(
            _bus!, EnemyRomTablePointers.ChozoStatue.ProjectileVelocityWords + tableOffset));

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
        samus.XPosition = unchecked((ushort)(
            statue.XPosition + unchecked((short)ReadWord(
                _bus!, EnemyRomTablePointers.ChozoStatue.ProjectileXOffsetWords + tableOffset))));
        samus.YPosition = unchecked((ushort)(
            statue.YPosition + unchecked((short)ReadWord(
                _bus!, EnemyRomTablePointers.ChozoStatue.ProjectileYOffsetWords + tableOffset))));
    }

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
            ChozoSpikeFootstepTerrainPlm,
            probeX >> 4,
            probeY >> 4,
            IsHardcoded: false));
        SpawnWreckedShipChozoFootstep(statue, rawXOffset);
    }

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

    private void FinishWreckedShipChozoSequence()
    {
        SetChozoStatueSamusControls(enabled: true);

        // The assembly performs overlapping 16-bit stores at raw scroll indexes 6, 8, 9,
        // and 13. Publish the resulting bytes (6..10 = 0, 13 = 1, 14 = 0) exactly.
        for (int index = 6; index <= 10; index++)
            RequireSetRoomScrollByte(index, 0);
        RequireSetRoomScrollByte(13, 1);
        RequireSetRoomScrollByte(14, 0);

        PublishHardcodedChozoPlm(
            WreckedShipSpikeTerrainPlm,
            blockX: 0x17,
            blockY: 0x1d);
    }

    private void SetChozoStatueSamusControls(bool enabled)
    {
        ChozoStatueSamusControlsEnabled = enabled;
        RequireSetSamusControlsEnabled(enabled);
    }

    private void PublishHardcodedChozoPlm(ushort header, int blockX, int blockY) =>
        _chozoStatuePlmRequests.Add(new ChozoStatuePlmRequest(
            header,
            blockX,
            blockY,
            IsHardcoded: true));

    private ChozoStatueState RequireChozoStatueState(RoomEnemySlot statue) =>
        _chozoStatueStates[statue.SlotIndex] ?? throw new InvalidOperationException(
            $"Chozo statue slot {statue.SlotIndex} has no initialized bank-$AA state.");

    private static SamusState RequireChozoSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Chozo statue movement requires the active Samus actor.");

    private static RoomLevelData RequireChozoLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException(
            "Chozo statue movement requires the active room level.");
}
