namespace SuperMetroid.Core.Game;

/// <summary>
/// Native bank-$A2 function pointer stored in Dragon variable F. Exposing the cartridge
/// addresses keeps debugger state comparable with WRAM while giving callers readable states.
/// </summary>
public enum DragonEnemyFunction : ushort
{
    WaitToRise = 0xe654,
    Rising = 0xe6ad,
    Attacking = 0xe6f1,
    WaitToSink = 0xe734,
    Sinking = 0xe749,
    WingNoOp = 0xe781,
}

/// <summary>
/// Typed view of Dragon's overloaded ordinary and extended enemy words. The original actor
/// uses variable D first as a motion timer and then as its three-shot counter; both names are
/// intentionally retained as aliases so a debugger explains the current function's meaning.
/// </summary>
public sealed class DragonEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _requestedInstructionLists;
    private readonly ushort[] _installedInstructionLists;
    private readonly ushort[] _animationFinishedFlags;

    internal DragonEnemyState(
        RoomEnemySlot slot,
        ushort[] requestedInstructionLists,
        ushort[] installedInstructionLists,
        ushort[] animationFinishedFlags)
    {
        _slot = slot;
        _requestedInstructionLists = requestedInstructionLists;
        _installedInstructionLists = installedInstructionLists;
        _animationFinishedFlags = animationFinishedFlags;
    }

    /// <summary>Population parameter one: zero is a body; nonzero is its cosmetic wing.</summary>
    public bool IsWing => _slot.Parameter1 != 0;

    /// <summary>Variable A; bit 15 is the facing bit consumed by the fireball initializer.</summary>
    public ushort DirectionWord
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Variable D while waiting, rising, or sinking.</summary>
    public ushort FunctionTimer
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Variable D while attacking; initialized to three before the first volley.</summary>
    public ushort AttackCounter
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Variable F, represented by its literal bank-$A2 function pointer.</summary>
    public DragonEnemyFunction Function
    {
        get => (DragonEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Extended enemy word $7E:7800,x: index into the six-entry list table.</summary>
    public ushort RequestedInstructionListIndex
    {
        get => _requestedInstructionLists[_slot.SlotIndex];
        internal set => _requestedInstructionLists[_slot.SlotIndex] = value;
    }

    /// <summary>Extended enemy word $7E:7802,x: list index already installed in the slot.</summary>
    public ushort InstalledInstructionListIndex
    {
        get => _installedInstructionLists[_slot.SlotIndex];
        internal set => _installedInstructionLists[_slot.SlotIndex] = value;
    }

    /// <summary>Extended enemy word $7E:7804,x, set by instruction opcode $A2:E5FB.</summary>
    public bool AnimationFinished
    {
        get => _animationFinishedFlags[_slot.SlotIndex] != 0;
        internal set => _animationFinishedFlags[_slot.SlotIndex] = value ? (ushort)1 : (ushort)0;
    }
}

/// <summary>
/// Literal translation of enemy $D4BF (Dragon). Every retail Dragon is two consecutive
/// physical enemy records: an interactive body followed by a collision-disabled wing actor.
/// The body deliberately moves and reprograms that following slot through the same +$40 WRAM
/// alias used by the 65C816 routine.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort DragonDefinition = 0xd4bf;
    internal const ushort DragonTouchAi = EnemyAiCodePointers.BankA2.DragonTouch;
    internal const ushort DragonShotAi = EnemyAiCodePointers.BankA2.DragonShot;
    internal const ushort DragonPowerBombAi = EnemyAiCodePointers.BankA2.DragonPowerBomb;

    private const ushort DragonIdleFacingLeftInstruction = 0xe59b;
    private const ushort DragonWingsFacingLeftInstruction = 0xe5a1;
    private const int DragonInstructionListPointerTable = 0xa2e5ef;
    private const ushort DragonAnimationFinishedInstruction = 0xe5fb;
    private const ushort DragonRiseOrSinkFrames = 0x0030;
    private const ushort DragonShotCount = 3;
    private const ushort DragonWaitBeforeSinkFrames = 0x0060;
    private const ushort DragonWaitBeforeRiseFrames = 0x0080;
    private const ushort DragonFireballSound = 0x0061;

    private readonly ushort[] _dragonRequestedInstructionLists = new ushort[MaximumEnemyCount];
    private readonly ushort[] _dragonInstalledInstructionLists = new ushort[MaximumEnemyCount];
    private readonly ushort[] _dragonAnimationFinishedFlags = new ushort[MaximumEnemyCount];
    private readonly DragonEnemyState?[] _dragonStates = new DragonEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Dragon state by physical enemy slot, including cosmetic wing slots.</summary>
    public IReadOnlyList<DragonEnemyState?> DragonStates => _dragonStates;

    /// <summary>Last library-two sound queued by a Dragon volley during the current frame.</summary>
    public ushort? LastDragonSoundEffect { get; private set; }

    private void ResetDragonRoomState()
    {
        Array.Clear(_dragonRequestedInstructionLists);
        Array.Clear(_dragonInstalledInstructionLists);
        Array.Clear(_dragonAnimationFinishedFlags);
        Array.Clear(_dragonStates);
        LastDragonSoundEffect = null;
    }

    /// <summary>Ports <c>InitAI_Dragon</c> at $A2:E606.</summary>
    private void InitializeDragon(RoomEnemySlot slot)
    {
        DragonEnemyState state = CreateDragonState(slot);
        state.AnimationFinished = false;

        if (state.IsWing)
        {
            // The wing begins on table index two (left-facing wings) and never runs body AI.
            // Property $0400 keeps the cosmetic record out of touch/shot collision passes.
            state.RequestedInstructionListIndex = 2;
            state.InstalledInstructionListIndex = 2;
            slot.CurrentInstruction = DragonWingsFacingLeftInstruction;
            slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
            state.Function = DragonEnemyFunction.WingNoOp;
            return;
        }

        state.RequestedInstructionListIndex = 0;
        state.InstalledInstructionListIndex = 0;
        slot.CurrentInstruction = DragonIdleFacingLeftInstruction;
        state.Function = DragonEnemyFunction.WaitToRise;
    }

    /// <summary>Ports <c>MainAI_Dragon</c> at $A2:E64E and its five body functions.</summary>
    private void RunDragonMain(RoomEnemySlot slot, DragonEnemyState state, SamusState? samus)
    {
        switch (state.Function)
        {
            case DragonEnemyFunction.WingNoOp:
                return;

            case DragonEnemyFunction.WaitToRise:
                RunDragonWaitToRise(slot, state, samus);
                return;

            case DragonEnemyFunction.Rising:
                RunDragonRising(slot, state);
                return;

            case DragonEnemyFunction.Attacking:
                RunDragonAttacking(slot, state);
                return;

            case DragonEnemyFunction.WaitToSink:
                RunDragonWaitToSink(slot, state);
                return;

            case DragonEnemyFunction.Sinking:
                RunDragonSinking(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Dragon function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports Dragon function $A2:E654.</summary>
    private void RunDragonWaitToRise(
        RoomEnemySlot body,
        DragonEnemyState bodyState,
        SamusState? samus)
    {
        bodyState.FunctionTimer = unchecked((ushort)(bodyState.FunctionTimer - 1));
        if (unchecked((short)bodyState.FunctionTimer) >= 0)
            return;
        if (samus is null)
            throw new InvalidOperationException("Dragon facing selection requires the active Samus actor.");

        RoomEnemySlot wing = GetDragonWing(body);
        DragonEnemyState wingState = RequireDragonState(wing);
        bodyState.FunctionTimer = DragonRiseOrSinkFrames;
        bodyState.Function = DragonEnemyFunction.Rising;

        // $E666-$E672 rotates the sign of (Samus X - body X) into bit 15 of variable A,
        // preserving its other fifteen bits. The following byte-sized list edits use that
        // sign to select even/odd left/right table entries for both physical records.
        bool facingLeft = unchecked((short)(samus.XPosition - body.XPosition)) < 0;
        bodyState.DirectionWord = unchecked((ushort)(
            (bodyState.DirectionWord & 0x7fff) | (facingLeft ? 0x8000 : 0)));
        bodyState.RequestedInstructionListIndex = SetDragonFacingBit(
            bodyState.RequestedInstructionListIndex,
            facingLeft);
        wingState.RequestedInstructionListIndex = SetDragonFacingBit(
            wingState.RequestedInstructionListIndex,
            facingLeft);
        InstallDragonInstructionList(body, bodyState);
        InstallDragonInstructionList(wing, wingState);
    }

    /// <summary>Ports Dragon function $A2:E6AD.</summary>
    private void RunDragonRising(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        bodyState.FunctionTimer = unchecked((ushort)(bodyState.FunctionTimer - 1));
        if (unchecked((short)bodyState.FunctionTimer) < 0)
        {
            bodyState.RequestedInstructionListIndex = unchecked((ushort)(
                bodyState.RequestedInstructionListIndex + 4));
            bodyState.AttackCounter = DragonShotCount;
            bodyState.Function = DragonEnemyFunction.Attacking;
        }

        // The native 16.16 constant is exactly -$0001:0000 in the NTSC build, so both
        // subpositions remain untouched while body and wing rise one whole pixel.
        RoomEnemySlot wing = GetDragonWing(body);
        body.YPosition = unchecked((ushort)(body.YPosition - 1));
        wing.YPosition = unchecked((ushort)(wing.YPosition - 1));
    }

    /// <summary>Ports Dragon function $A2:E6F1.</summary>
    private void RunDragonAttacking(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        InstallDragonInstructionList(body, bodyState);
        if (!bodyState.AnimationFinished)
            return;

        bodyState.AnimationFinished = false;

        // $FFFF forces the same attack list to reinstall after each animation reaches its
        // sleep opcode. That restart is what spaces the three fireballs by a full ROM list.
        bodyState.InstalledInstructionListIndex = 0xffff;
        SpawnDragonFireball(body, bodyState);
        LastDragonSoundEffect = DragonFireballSound;
        bodyState.AttackCounter = unchecked((ushort)(bodyState.AttackCounter - 1));
        if (bodyState.AttackCounter != 0)
            return;

        bodyState.RequestedInstructionListIndex = unchecked((ushort)(
            bodyState.RequestedInstructionListIndex - 4));
        bodyState.FunctionTimer = DragonWaitBeforeSinkFrames;
        bodyState.Function = DragonEnemyFunction.WaitToSink;
    }

    /// <summary>Ports Dragon function $A2:E734.</summary>
    private void RunDragonWaitToSink(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        bodyState.FunctionTimer = unchecked((ushort)(bodyState.FunctionTimer - 1));
        if (bodyState.FunctionTimer != 0)
            return;

        bodyState.FunctionTimer = DragonRiseOrSinkFrames;
        bodyState.Function = DragonEnemyFunction.Sinking;
        InstallDragonInstructionList(body, bodyState);
    }

    /// <summary>Ports Dragon function $A2:E749.</summary>
    private void RunDragonSinking(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        bodyState.FunctionTimer = unchecked((ushort)(bodyState.FunctionTimer - 1));
        if (unchecked((short)bodyState.FunctionTimer) < 0)
        {
            bodyState.FunctionTimer = DragonWaitBeforeRiseFrames;
            bodyState.Function = DragonEnemyFunction.WaitToRise;
        }

        // As with rising, the native regionalized 16.16 displacement is exactly one pixel.
        RoomEnemySlot wing = GetDragonWing(body);
        body.YPosition = unchecked((ushort)(body.YPosition + 1));
        wing.YPosition = unchecked((ushort)(wing.YPosition + 1));
    }

    /// <summary>Ports instruction opcode $A2:E5FB.</summary>
    private void FinishDragonAttackAnimation(RoomEnemySlot body)
    {
        DragonEnemyState state = RequireDragonState(body);
        if (state.IsWing)
        {
            throw new InvalidDataException(
                "Dragon wing reached the body-only attack-finished instruction.");
        }
        state.AnimationFinished = true;
    }

    /// <summary>
    /// Ports the shared tail at $A2:E7DA after touch, shot, or power-bomb common AI. A dead
    /// body deletes its wing; a surviving body mirrors every timer/handler word that affects
    /// visible hurt and freeze rendering into that cosmetic record.
    /// </summary>
    private void ResolveDragonCombatAfterCommon(RoomEnemySlot actor)
    {
        // The callback at $E7DA does not test init0. Normal touch/shot collision excludes
        // wings via property $0400, but bank $A0's power-bomb scan does not. Consequently a
        // power bomb may invoke this on a wing and copy into whatever physical record follows
        // it. Preserve that retail aliasing bug instead of silently normalizing to a body/wing
        // relationship; several dense populations place another live actor in that slot.
        if (actor.SlotIndex >= MaximumEnemyCount - 1)
        {
            throw new InvalidDataException(
                "Dragon combat callback cannot alias beyond the final physical enemy slot.");
        }
        RoomEnemySlot following = _slots[actor.SlotIndex + 1];
        if (actor.Health == 0)
        {
            following.Properties = following.Properties.With(EnemyProperties.Deleted);
            return;
        }

        following.ShakeTimer = actor.ShakeTimer;
        following.InvincibilityTimer = actor.InvincibilityTimer;
        following.FlashTimer = actor.FlashTimer;
        following.FrozenTimer = actor.FrozenTimer;
        following.AiHandlerBits = actor.AiHandlerBits;
    }

    private void InstallDragonInstructionList(RoomEnemySlot slot, DragonEnemyState state)
    {
        if (state.RequestedInstructionListIndex == state.InstalledInstructionListIndex)
            return;
        if (state.RequestedInstructionListIndex >= 6)
        {
            throw new InvalidDataException(
                $"Dragon list index {state.RequestedInstructionListIndex} is outside the six-entry ROM table.");
        }

        state.InstalledInstructionListIndex = state.RequestedInstructionListIndex;
        slot.CurrentInstruction = ReadWord(
            _bus!,
            DragonInstructionListPointerTable + state.RequestedInstructionListIndex * 2);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private RoomEnemySlot GetDragonWing(RoomEnemySlot body)
    {
        if (body.SlotIndex >= MaximumEnemyCount - 1)
            throw new InvalidDataException("Dragon body occupies the final physical enemy slot.");

        RoomEnemySlot wing = _slots[body.SlotIndex + 1];
        if (body.Parameter1 != 0 || wing.EnemyDefinitionPointer != DragonDefinition ||
            wing.Parameter1 == 0)
        {
            throw new InvalidDataException(
                $"Dragon body slot {body.SlotIndex} is not followed by its retail wing record.");
        }
        return wing;
    }

    private DragonEnemyState CreateDragonState(RoomEnemySlot slot)
    {
        var state = new DragonEnemyState(
            slot,
            _dragonRequestedInstructionLists,
            _dragonInstalledInstructionLists,
            _dragonAnimationFinishedFlags);
        _dragonStates[slot.SlotIndex] = state;
        return state;
    }

    private DragonEnemyState RequireDragonState(RoomEnemySlot slot) =>
        _dragonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Dragon state.");

    private static ushort SetDragonFacingBit(ushort listIndex, bool facingLeft) =>
        facingLeft
            ? unchecked((ushort)(listIndex & 0xfffe))
            : unchecked((ushort)(listIndex | 0x0001));
}
