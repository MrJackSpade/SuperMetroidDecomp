namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed names for the bank-$A9 function pointers stored in Shitroid's enemy variable A.
/// The numeric values intentionally remain the cartridge addresses used by the dispatcher.
/// </summary>
public enum ShitroidAiFunction : ushort
{
    Dormant = 0xefdf,
    WaitForCamera = 0xefe6,
    BeginEntranceDelay = 0xf02b,
    EntranceDelay = 0xf037,
    FlyToSidehopper = 0xf049,
    ChaseSidehopper = 0xf06d,
    AttachToSidehopper = 0xf094,
    DrainSidehopper = 0xf0e6,
    ActivateSidehopperCorpse = 0xf125,
    RiseAfterFeeding = 0xf138,
    HoverNearSamus = 0xf180,
    ChaseSamus = 0xf1fa,
    BeginDrainingSamus = 0xf20e,
    DrainSamus = 0xf21b,
    BeginPostDrainPause = 0xf2a2,
    PostDrainPause = 0xf2ae,
    RiseAfterDrainingSamus = 0xf2c0,
    FlyLeft = 0xf2fb,
    FlyRight = 0xf324,
    BeginExit = 0xf360,
    Exit = 0xf36d,
    HoldSamusBeforeRelease = 0xf3a3,
    ReleasedFollow = 0xf3be,
}

/// <summary>One hardcoded bank-$84 wall mutation emitted by Shitroid's room script.</summary>
public readonly record struct ShitroidPlmRequest(byte BlockX, byte BlockY, ushort Header);

/// <summary>One delayed music command emitted by the Shitroid encounter.</summary>
public readonly record struct ShitroidMusicRequest(byte Track, byte DelayFrames);

/// <summary>Typed projection of Shitroid's common and extended enemy WRAM.</summary>
public sealed class ShitroidEnemyState
{
    private readonly ushort[] _targetPalette = new ushort[256];

    internal ShitroidEnemyState(RoomEnemySlot slot) => Slot = slot;

    public RoomEnemySlot Slot { get; }

    public ShitroidAiFunction Function
    {
        get => (ShitroidAiFunction)Slot.VariableA;
        internal set => Slot.VariableA = (ushort)value;
    }

    /// <summary>Signed 8.8 horizontal velocity in native enemy variable B.</summary>
    public ushort XVelocity
    {
        get => Slot.VariableB;
        internal set => Slot.VariableB = value;
    }

    /// <summary>Signed 8.8 vertical velocity in native enemy variable C.</summary>
    public ushort YVelocity
    {
        get => Slot.VariableC;
        internal set => Slot.VariableC = value;
    }

    /// <summary>Low-byte palette phase and high-byte delay timer in variable D.</summary>
    public ushort PaletteTimerAndPhase
    {
        get => Slot.VariableD;
        internal set => Slot.VariableD = value;
    }

    public ushort PaletteDelay
    {
        get => Slot.VariableE;
        internal set => Slot.VariableE = value;
    }

    public ushort StateTimer
    {
        get => Slot.VariableF;
        internal set => Slot.VariableF = value;
    }

    /// <summary>Native extended word 01: randomized vertical-follow duration.</summary>
    public ushort HoverTimer { get; internal set; }

    /// <summary>Native extended word 02: consecutive horizontal-lock accumulator.</summary>
    public ushort HorizontalLockAccumulator { get; internal set; }

    /// <summary>Native extended word 04: enables the periodic Shitroid cry.</summary>
    public ushort CryEnabled { get; internal set; }

    /// <summary>Native extended word 05: counts every fourth palette-cycle cry.</summary>
    public ushort CryCounter { get; internal set; }

    /// <summary>
    /// Native word 08 belongs to physical slot one, the dead sidehopper actor. Shitroid
    /// writes it after feeding; preserving the ownership here lets that actor consume the
    /// signal when its own bank-$A9 translation is installed.
    /// </summary>
    public ushort VictimActivationFlag { get; internal set; }

    /// <summary>The independent 256-color target palette written during initialization.</summary>
    public ReadOnlyMemory<ushort> TargetPalette => _targetPalette;
    internal Span<ushort> MutableTargetPalette => _targetPalette;
}

/// <summary>Retail Shitroid actor at $A9:EED1-$F99A.</summary>
public sealed partial class RoomEnemySystem
{
    public const ushort ShitroidDefinition = 0xeebf;

    private const int ShitroidWorkBufferAddress = 0x7e2000;
    private const int ShitroidWorkBufferSize = 0x1000;
    private const ushort ShitroidInitialInstruction = 0xf90e;
    private const ushort ShitroidCalmInstruction = 0xf906;
    private const ushort ShitroidAggressiveInstruction = 0xf924;
    private const ushort ShitroidExitInstruction = 0xf93a;
    private const ushort ShitroidCloseWallPlm = 0xb767;
    private const ushort ShitroidOpenWallPlm = 0xb763;
    private const int ShitroidNormalPaletteSource = 0xa9f6d1;

    private static ReadOnlySpan<short> ShitroidShakeX => [0, -1, 0, 1];
    private static ReadOnlySpan<short> ShitroidShakeY => [0, 1, -1, 1];

    private readonly List<ShitroidPlmRequest> _shitroidPlmRequests = [];
    private ShitroidEnemyState? _shitroid;
    private ushort _shitroidCameraX;
    private ushort _shitroidCameraY;

    public ShitroidEnemyState? Shitroid => _shitroid;

    /// <summary>Frame-local wall mutations requested by the current Shitroid state.</summary>
    public IReadOnlyList<ShitroidPlmRequest> ShitroidPlmRequests => _shitroidPlmRequests;

    /// <summary>Exact layer-one X position requested by state $EFE6, if any.</summary>
    public ushort? RequestedShitroidCameraX { get; private set; }

    /// <summary>Last library-two sound queued during this Shitroid frame.</summary>
    public ushort? LastShitroidSoundEffectLibrary2 { get; private set; }

    /// <summary>Last delayed music command queued during this Shitroid frame.</summary>
    public ShitroidMusicRequest? LastShitroidMusicRequest { get; private set; }

    private void ResetShitroidRoomState()
    {
        _shitroid = null;
        _shitroidPlmRequests.Clear();
        RequestedShitroidCameraX = null;
        LastShitroidSoundEffectLibrary2 = null;
        LastShitroidMusicRequest = null;
        _shitroidCameraX = 0;
        _shitroidCameraY = 0;
    }

    private void BeginShitroidFrame()
    {
        _shitroidPlmRequests.Clear();
        RequestedShitroidCameraX = null;
        LastShitroidSoundEffectLibrary2 = null;
        LastShitroidMusicRequest = null;
    }

    /// <summary>Ports <c>Shitroid_Init</c> at <c>$A9:EF37</c>.</summary>
    private void InitializeShitroid(RoomEnemySlot slot, ushort cameraX)
    {
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Shitroid requires native enemy slot zero, not slot {slot.SlotIndex}.");
        }
        if (_shitroid is not null)
            throw new InvalidDataException("A room population contains more than one Shitroid.");

        // The cutscene shares the same $7E:2000 scratch surface as corpse-rotting code.
        // Initialization clears every byte, descending by words in the original routine.
        for (int offset = 0; offset < ShitroidWorkBufferSize; offset++)
            _bus!.WriteByte(ShitroidWorkBufferAddress + offset, 0);

        // `$2000` is the scheduler's instruction-processing bit; `$1000` blocks plasma
        // from piercing the actor. The upstream C symbol calls `$2000` DisableSamusColl,
        // but the executable scheduler also consumes that same bit for instructions, so
        // the translated property enum retains the directly observed scheduler meaning.
        slot.Properties = unchecked((ushort)(slot.Properties | 0x3000));
        slot.PaletteIndex = 0x0400;
        SetShitroidInstruction(slot, ShitroidInitialInstruction);

        var state = new ShitroidEnemyState(slot)
        {
            Function = ShitroidAiFunction.WaitForCamera,
            XVelocity = 0,
            YVelocity = 0,
            PaletteDelay = 10,
        };
        if (unchecked((short)cameraX) < 0)
        {
            // `$0400` removes the actor from ordinary interaction and `$0100` suppresses
            // drawing. Keep the former raw: its historical disassembly name "tangible" is
            // contradicted by the collision walkers that explicitly reject the bit.
            slot.Properties = unchecked((ushort)(slot.Properties | 0x0500));
            state.Function = ShitroidAiFunction.Dormant;
        }
        slot.Parameter2 = 0;
        _shitroid = state;

        CopyShitroidTargetPalette(state, 0xa9f8c6, destinationColor: 0x90);
        CopyShitroidTargetPalette(state, 0xa9f8e6, destinationColor: 0xa0);
        CopyShitroidTargetPalette(state, 0xa9f8a6, destinationColor: 0xf0);
    }

    private void CopyShitroidTargetPalette(
        ShitroidEnemyState state,
        int sourceAddress,
        int destinationColor)
    {
        for (int color = 0; color < 16; color++)
        {
            ushort value = ReadWord(_bus!, sourceAddress + color * 2);
            state.MutableTargetPalette[destinationColor + color] = value;

            // This runtime currently presents completed room fades directly in CGRAM.
            // Keep the separate target copy above, but install the same completed value so
            // the encounter does not wait on an untranslated global palette-fade owner.
            _cgram!.SetColor(destinationColor + color, value);
        }
    }

    /// <summary>Ports <c>Shitroid_Main</c> at <c>$A9:EFC5</c>.</summary>
    private void RunShitroidMain(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        ShitroidEnemyState state = RequireShitroidState(slot);
        _shitroidCameraX = cameraX;
        _shitroidCameraY = cameraY;

        // Shitroid's private shot handler applies recoil but no health loss. The main AI
        // redundantly restores $7FFF every processed frame, exactly as the cartridge does.
        slot.Health = 0x7fff;
        DispatchShitroidState(slot, state, samus, sharedProjectiles);
        MoveBankA9EnemyWithVelocity(slot);
        if (PaletteChangeNumber == 0)
            StepShitroidNormalPalette(state);
    }

    private void DispatchShitroidState(
        RoomEnemySlot slot,
        ShitroidEnemyState state,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        switch (state.Function)
        {
            case ShitroidAiFunction.Dormant:
                state.XVelocity = 0;
                state.YVelocity = 0;
                return;

            case ShitroidAiFunction.WaitForCamera:
                WaitForShitroidCamera(state);
                return;

            case ShitroidAiFunction.BeginEntranceDelay:
                state.Function = ShitroidAiFunction.EntranceDelay;
                state.StateTimer = 464;
                goto case ShitroidAiFunction.EntranceDelay;

            case ShitroidAiFunction.EntranceDelay:
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    LastShitroidMusicRequest = new ShitroidMusicRequest(5, 8);
                    state.Function = ShitroidAiFunction.FlyToSidehopper;
                    goto case ShitroidAiFunction.FlyToSidehopper;
                }
                return;

            case ShitroidAiFunction.FlyToSidehopper:
                GraduallyAccelerateShitroid(slot, state, 15, 584, 74, 0x0400);
                if (ShitroidOverlapsRectangle(slot, 584, 74, 1, 1))
                    state.Function = ShitroidAiFunction.ChaseSidehopper;
                return;

            case ShitroidAiFunction.ChaseSidehopper:
            {
                RoomEnemySlot victim = RequireShitroidVictim(slot);
                GraduallyAccelerateShitroid(
                    slot,
                    state,
                    15,
                    victim.XPosition,
                    unchecked((ushort)(victim.YPosition - 32)),
                    0x0400);
                if (ShitroidActorsOverlap(slot, victim))
                    state.Function = ShitroidAiFunction.AttachToSidehopper;
                return;
            }

            case ShitroidAiFunction.AttachToSidehopper:
                AttachShitroidToSidehopper(slot, state);
                return;

            case ShitroidAiFunction.DrainSidehopper:
                DrainShitroidVictim(slot, state);
                return;

            case ShitroidAiFunction.ActivateSidehopperCorpse:
                state.VictimActivationFlag = 1;
                state.Function = ShitroidAiFunction.RiseAfterFeeding;
                state.StateTimer = 192;
                goto case ShitroidAiFunction.RiseAfterFeeding;

            case ShitroidAiFunction.RiseAfterFeeding:
                GraduallyAccelerateShitroid(slot, state, 0, slot.XPosition, 104, 0x0400);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Function = ShitroidAiFunction.HoverNearSamus;
                    slot.Parameter2 = 1;
                    SetShitroidScrollPair(1);
                    QueueShitroidWallPlms(ShitroidOpenWallPlm);
                }
                return;

            case ShitroidAiFunction.HoverNearSamus:
                HoverShitroidNearSamus(slot, state, RequireShitroidSamus(samus));
                return;

            case ShitroidAiFunction.ChaseSamus:
            {
                SamusState target = RequireShitroidSamus(samus);
                GraduallyAccelerateShitroid(
                    slot,
                    state,
                    15,
                    target.XPosition,
                    unchecked((ushort)(target.YPosition - 32)),
                    0x0400);
                return;
            }

            case ShitroidAiFunction.BeginDrainingSamus:
                RequireShitroidSamus(samus).SpecialSuperPaletteFlags = 1;
                state.Function = ShitroidAiFunction.DrainSamus;
                goto case ShitroidAiFunction.DrainSamus;

            case ShitroidAiFunction.DrainSamus:
                DrainSamusWithShitroid(
                    slot,
                    state,
                    RequireShitroidSamus(samus),
                    sharedProjectiles);
                return;

            case ShitroidAiFunction.BeginPostDrainPause:
                state.Function = ShitroidAiFunction.PostDrainPause;
                state.StateTimer = 120;
                goto case ShitroidAiFunction.PostDrainPause;

            case ShitroidAiFunction.PostDrainPause:
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Function = ShitroidAiFunction.RiseAfterDrainingSamus;
                    state.StateTimer = 192;
                    goto case ShitroidAiFunction.RiseAfterDrainingSamus;
                }
                return;

            case ShitroidAiFunction.RiseAfterDrainingSamus:
            {
                SamusState target = RequireShitroidSamus(samus);
                GraduallyAccelerateShitroid(slot, state, 0, target.XPosition, 104, 0x0400);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    LastShitroidSoundEffectLibrary2 = 0x007d;
                    state.Function = ShitroidAiFunction.FlyLeft;
                    state.StateTimer = 88;
                    SetShitroidInstruction(slot, ShitroidAggressiveInstruction);
                    goto case ShitroidAiFunction.FlyLeft;
                }
                return;
            }

            case ShitroidAiFunction.FlyLeft:
            {
                SamusState target = RequireShitroidSamus(samus);
                GraduallyAccelerateShitroid(
                    slot,
                    state,
                    0,
                    unchecked((ushort)(target.XPosition - 64)),
                    100,
                    0x0400);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Function = ShitroidAiFunction.FlyRight;
                    state.StateTimer = 88;
                    goto case ShitroidAiFunction.FlyRight;
                }
                return;
            }

            case ShitroidAiFunction.FlyRight:
            {
                SamusState target = RequireShitroidSamus(samus);
                GraduallyAccelerateShitroid(
                    slot,
                    state,
                    0,
                    unchecked((ushort)(target.XPosition + 96)),
                    104,
                    0x0400);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Function = ShitroidAiFunction.HoldSamusBeforeRelease;
                    state.StateTimer = 256;
                    SetShitroidInstruction(slot, ShitroidExitInstruction);
                }
                return;
            }

            case ShitroidAiFunction.BeginExit:
                LastShitroidSoundEffectLibrary2 = 0x0052;
                state.Function = ShitroidAiFunction.Exit;
                goto case ShitroidAiFunction.Exit;

            case ShitroidAiFunction.Exit:
                GraduallyAccelerateShitroid(slot, state, 0, 0xff80, 64, 0x0400);
                if (ShitroidOverlapsRectangle(slot, 0xff80, 64, 8, 8))
                {
                    state.XVelocity = 0;
                    state.YVelocity = 0;
                    // `$A9:F39A` clears instruction processing and invisibility as Shitroid
                    // reaches the off-screen destination, leaving a dormant non-drawn actor.
                    slot.Properties = unchecked((ushort)(slot.Properties & ~0x2100));
                    state.Function = ShitroidAiFunction.Dormant;
                }
                return;

            case ShitroidAiFunction.HoldSamusBeforeRelease:
            {
                SamusState target = RequireShitroidSamus(samus);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    target.Drained.Release(_bus!, target);
                    slot.Parameter2 = 1;
                    state.Function = ShitroidAiFunction.ReleasedFollow;
                    FollowReleasedSamus(slot, state, target);
                }
                else
                {
                    FollowReleasedSamus(slot, state, target);
                }
                return;
            }

            case ShitroidAiFunction.ReleasedFollow:
                if (FollowReleasedSamus(slot, state, RequireShitroidSamus(samus)))
                    state.Function = ShitroidAiFunction.BeginExit;
                return;

            default:
                throw new NotSupportedException(
                    $"Shitroid state $A9:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void WaitForShitroidCamera(ShitroidEnemyState state)
    {
        if (unchecked((short)(_shitroidCameraX - 513)) >= 0)
            return;

        // Native writes layer1_x_pos directly and zero-extends scroll bytes zero and two
        // into their adjacent bytes. The paired calls below preserve the observable four
        // byte scroll array without pretending that this is ordinary camera tracking.
        RequestedShitroidCameraX = 512;
        SetShitroidScrollPair(0);
        QueueShitroidWallPlms(ShitroidCloseWallPlm);
        state.Function = ShitroidAiFunction.BeginEntranceDelay;
        state.CryEnabled = 1;
    }

    private void SetShitroidScrollPair(byte highByte)
    {
        if (_setMotherBrainRoomScrollByte is null || _readMotherBrainRoomScrollByte is null)
            return;
        byte scroll0 = _readMotherBrainRoomScrollByte(0);
        byte scroll2 = _readMotherBrainRoomScrollByte(2);
        _setMotherBrainRoomScrollByte(0, scroll0);
        _setMotherBrainRoomScrollByte(1, highByte);
        _setMotherBrainRoomScrollByte(2, scroll2);
        _setMotherBrainRoomScrollByte(3, highByte);
    }

    private void QueueShitroidWallPlms(ushort header)
    {
        _shitroidPlmRequests.Add(new ShitroidPlmRequest(0x30, 0x03, header));
        _shitroidPlmRequests.Add(new ShitroidPlmRequest(0x1f, 0x03, header));
    }

    private void AttachShitroidToSidehopper(RoomEnemySlot slot, ShitroidEnemyState state)
    {
        RoomEnemySlot victim = RequireShitroidVictim(slot);
        if (!AccelerateShitroidExactly(
            state,
            0x0200,
            victim.XPosition,
            unchecked((ushort)(victim.YPosition - 32))))
        {
            return;
        }

        state.XVelocity = 0;
        state.YVelocity = 0;
        slot.XPosition = victim.XPosition;
        slot.YPosition = unchecked((ushort)(victim.YPosition - 32));
        SetShitroidInstruction(slot, ShitroidAggressiveInstruction);
        state.Function = ShitroidAiFunction.DrainSidehopper;
        state.PaletteDelay = 1;
        slot.Parameter2 = 0;
        state.StateTimer = 320;
    }

    private void DrainShitroidVictim(RoomEnemySlot slot, ShitroidEnemyState state)
    {
        RoomEnemySlot victim = RequireShitroidVictim(slot);
        int shakeIndex = (slot.FrameCounter & 6) >> 1;
        slot.XPosition = unchecked((ushort)(victim.XPosition + ShitroidShakeX[shakeIndex]));
        slot.YPosition = unchecked((ushort)(
            victim.YPosition + ShitroidShakeY[shakeIndex] - 32));
        ushort oldTimer = state.StateTimer;
        state.StateTimer = unchecked((ushort)(oldTimer - 1));
        if (oldTimer == 1)
        {
            state.Function = ShitroidAiFunction.ActivateSidehopperCorpse;
            SetShitroidInstruction(slot, ShitroidCalmInstruction);
            state.PaletteDelay = 10;
        }
    }

    private void HoverShitroidNearSamus(
        RoomEnemySlot slot,
        ShitroidEnemyState state,
        SamusState samus)
    {
        UpdateShitroidHorizontalLock(state, slot.XPosition, samus.XPosition, threshold: 8);
        if (state.HorizontalLockAccumulator >= 0x0100 ||
            unchecked((short)(samus.XPosition - 512)) < 0)
        {
            state.Function = ShitroidAiFunction.ChaseSamus;
            return;
        }

        ushort targetY = SelectShitroidHoverY(state, samus.YPosition);
        GraduallyAccelerateShitroid(
            slot,
            state,
            10,
            samus.XPosition,
            targetY,
            0x0400);
    }

    private ushort SelectShitroidHoverY(ShitroidEnemyState state, ushort followedY)
    {
        if (state.HoverTimer != 0)
        {
            state.HoverTimer = unchecked((ushort)(state.HoverTimer - 1));
            return followedY;
        }
        if (((_readRandomNumber?.Invoke() ?? 0) & 0x0fff) >= 0x0fe0)
            state.HoverTimer = 32;
        return 80;
    }

    private void DrainSamusWithShitroid(
        RoomEnemySlot slot,
        ShitroidEnemyState state,
        SamusState samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        if (samus.Health < 2)
        {
            samus.XSpeedDivisor = 0;
            sharedProjectiles?.SetSharedBombCounter(0);
            state.Function = ShitroidAiFunction.BeginPostDrainPause;
            state.XVelocity = 0;
            state.YVelocity = 0;
            SetShitroidInstruction(slot, ShitroidCalmInstruction);
            state.PaletteDelay = 10;
            samus.SpecialSuperPaletteFlags = 0;
            samus.Drained.LetFall(_bus!, samus);
            state.CryEnabled = 0;
            LastShitroidMusicRequest = new ShitroidMusicRequest(7, 8);
            return;
        }

        sharedProjectiles?.SetSharedCooldown(8);
        sharedProjectiles?.SetSharedBombCounter(5);
        samus.XSpeedDivisor = 2;
        if (unchecked((short)(samus.Kinematics.YSpeed - 4)) >= 0)
            samus.Kinematics.YSpeed = 2;

        int shakeIndex = (slot.FrameCounter & 6) >> 1;
        slot.XPosition = unchecked((ushort)(samus.XPosition + ShitroidShakeX[shakeIndex]));
        slot.YPosition = unchecked((ushort)(samus.YPosition + ShitroidShakeY[shakeIndex] - 20));

        int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? 2 : 3;
        samus.Health = damage >= samus.Health
            ? (ushort)1
            : unchecked((ushort)(samus.Health - damage));
    }

    private bool FollowReleasedSamus(
        RoomEnemySlot slot,
        ShitroidEnemyState state,
        SamusState samus)
    {
        UpdateShitroidHorizontalLock(state, slot.XPosition, samus.XPosition, threshold: 2);
        ushort targetY = SelectShitroidHoverY(
            state,
            unchecked((ushort)(samus.YPosition - 18)));
        GraduallyAccelerateShitroid(
            slot,
            state,
            8,
            samus.XPosition,
            targetY,
            0x0400);
        return state.HorizontalLockAccumulator >= 0x0400 ||
            unchecked((short)(samus.XPosition - 128)) < 0;
    }

    private static void UpdateShitroidHorizontalLock(
        ShitroidEnemyState state,
        ushort currentX,
        ushort targetX,
        ushort threshold)
    {
        int distance = Math.Abs(unchecked((short)(currentX - targetX)));
        if (distance < threshold)
        {
            state.HorizontalLockAccumulator = unchecked((ushort)(
                state.HorizontalLockAccumulator + 2));
        }
        else if (state.HorizontalLockAccumulator != 0)
        {
            state.HorizontalLockAccumulator = unchecked((ushort)(
                state.HorizontalLockAccumulator - 1));
        }
    }

    private void GraduallyAccelerateShitroid(
        RoomEnemySlot slot,
        ShitroidEnemyState state,
        int divisorTableIndex,
        ushort targetX,
        ushort targetY,
        ushort offscreenReversalKick)
    {
        if ((uint)divisorTableIndex >= 16)
            throw new ArgumentOutOfRangeException(nameof(divisorTableIndex));
        int divisor = 16 - divisorTableIndex;

        int deltaX = unchecked((short)(slot.XPosition - targetX));
        if (deltaX != 0)
        {
            int step = Math.Max(1, Math.Abs(deltaX) / divisor);
            int velocity = unchecked((short)state.XVelocity);
            if (deltaX > 0)
            {
                if (velocity >= 0)
                {
                    if (ShitroidIsOffScreen(slot))
                        velocity -= offscreenReversalKick;
                    velocity -= 8 + step;
                }
                velocity -= step;
            }
            else
            {
                if (velocity < 0)
                {
                    if (ShitroidIsOffScreen(slot))
                        velocity += offscreenReversalKick;
                    velocity += 8 + step;
                }
                velocity += step;
            }
            state.XVelocity = unchecked((ushort)Math.Clamp(velocity, -2048, 2048));
        }

        int deltaY = unchecked((short)(slot.YPosition - targetY));
        if (deltaY == 0)
            return;
        int verticalStep = Math.Max(1, Math.Abs(deltaY) / divisor);
        int verticalVelocity = unchecked((short)state.YVelocity);
        if (deltaY > 0)
        {
            if (verticalVelocity >= 0)
                verticalVelocity -= 8 + verticalStep;
            verticalVelocity -= verticalStep;
        }
        else
        {
            if (verticalVelocity < 0)
                verticalVelocity += 8 + verticalStep;
            verticalVelocity += verticalStep;
        }
        state.YVelocity = unchecked((ushort)Math.Clamp(verticalVelocity, -1280, 1280));
    }

    private bool ShitroidIsOffScreen(RoomEnemySlot slot)
    {
        if (unchecked((short)slot.YPosition) < 0)
            return true;
        int screenY = unchecked((short)(slot.YPosition + 96 - _shitroidCameraY));
        if (screenY < 0 || screenY >= 416)
            return true;
        if (unchecked((short)slot.XPosition) < 0)
            return true;
        int screenX = unchecked((short)(slot.XPosition + 16 - _shitroidCameraX));
        return screenX < 0 || screenX >= 288;
    }

    private static bool AccelerateShitroidExactly(
        ShitroidEnemyState state,
        ushort acceleration,
        ushort targetX,
        ushort targetY)
    {
        (bool xArrived, ushort xVelocity) = AccelerateShitroidAxis(
            state.Slot.XPosition,
            targetX,
            state.XVelocity,
            acceleration);
        state.XVelocity = xVelocity;
        (bool yArrived, ushort yVelocity) = AccelerateShitroidAxis(
            state.Slot.YPosition,
            targetY,
            state.YVelocity,
            acceleration);
        state.YVelocity = yVelocity;
        return xArrived && yArrived;
    }

    private static (bool Arrived, ushort Velocity) AccelerateShitroidAxis(
        ushort current,
        ushort target,
        ushort velocityWord,
        ushort acceleration)
    {
        int delta = unchecked((short)(current - target));
        if (delta == 0)
            return (true, velocityWord);

        int velocity = unchecked((short)velocityWord);
        velocity = delta >= 0
            ? Math.Max(-1280, velocity - acceleration)
            : Math.Min(1280, velocity + acceleration);
        velocityWord = unchecked((ushort)velocity);

        int predicted = unchecked((short)(
            current + unchecked((sbyte)(velocityWord >> 8)) - target));
        bool crossed = delta >= 0 ? predicted <= 0 : predicted >= 0;
        if (crossed)
            velocityWord = 0;
        return (crossed, velocityWord);
    }

    private static bool ShitroidActorsOverlap(RoomEnemySlot left, RoomEnemySlot right) =>
        Math.Abs(unchecked((short)(right.XPosition - left.XPosition))) <
            left.XRadius + right.XRadius + 1 &&
        Math.Abs(unchecked((short)(right.YPosition - left.YPosition))) <
            left.YRadius + right.YRadius + 1;

    private static bool ShitroidOverlapsRectangle(
        RoomEnemySlot slot,
        ushort x,
        ushort y,
        ushort xRadius,
        ushort yRadius) =>
        Math.Abs(unchecked((short)(x - slot.XPosition))) < slot.XRadius + xRadius + 1 &&
        Math.Abs(unchecked((short)(y - slot.YPosition))) < slot.YRadius + yRadius + 1;

    /// <summary>Ports the private touch callback at <c>$A9:F789</c>.</summary>
    private void ResolveShitroidTouch(RoomEnemySlot slot, SamusState samus)
    {
        ShitroidEnemyState state = RequireShitroidState(slot);
        if (slot.Parameter2 == 0)
            return;

        BreakReleasedShitroidFollow(state);
        if (samus.ReadMovementType(_bus!) == 3 &&
            unchecked((short)(samus.XPosition - 512)) >= 0)
        {
            // The native expression `$80-angle+$80`, truncated to a byte, is simply the
            // negated source angle. Magnitude $40 is added to the current 8.8 velocity.
            byte angle = unchecked((byte)-SamusGrappleMovement.CalculateAngleFromXY(
                unchecked((short)(samus.XPosition - slot.XPosition)),
                unchecked((short)(samus.YPosition - slot.YPosition))));
            state.XVelocity = unchecked((ushort)(
                state.XVelocity + MultiplyCartridgeSinCos(0x0040, angle)));
            state.YVelocity = unchecked((ushort)(
                state.YVelocity + MultiplyCartridgeSinCos(
                    0x0040,
                    unchecked((byte)(angle + 64)))));
            return;
        }

        if (state.Function == ShitroidAiFunction.ChaseSamus)
        {
            if (AccelerateShitroidExactly(
                state,
                0x0200,
                samus.XPosition,
                unchecked((ushort)(samus.YPosition - 32))))
            {
                SetShitroidInstruction(slot, ShitroidAggressiveInstruction);
                state.PaletteDelay = 1;
                slot.Parameter2 = 0;
                state.XVelocity = 0;
                state.YVelocity = 0;
                state.Function = ShitroidAiFunction.BeginDrainingSamus;
            }
        }
        else if (state.Function == ShitroidAiFunction.HoverNearSamus)
        {
            state.Function = ShitroidAiFunction.ChaseSamus;
        }
    }

    /// <summary>Ports the recoil-only shot callback at <c>$A9:F842</c>.</summary>
    private void ResolveShitroidShot(
        RoomEnemySlot slot,
        SamusProjectileSlot collisionProjectile,
        SamusProjectileSlot slotZeroProjectile)
    {
        ShitroidEnemyState state = RequireShitroidState(slot);
        if (slot.Parameter2 == 0)
            return;

        BreakReleasedShitroidFollow(state);
        byte angle = unchecked((byte)-SamusGrappleMovement.CalculateAngleFromXY(
            unchecked((short)(slotZeroProjectile.XPosition - slot.XPosition)),
            unchecked((short)(slotZeroProjectile.YPosition - slot.YPosition))));
        ushort magnitude = unchecked((ushort)(8 * collisionProjectile.Damage));
        if (magnitude >= 0x00f0)
            magnitude = 0x00f0;
        state.XVelocity = unchecked((ushort)(
            state.XVelocity + MultiplyCartridgeSinCos(magnitude, angle)));
        state.YVelocity = unchecked((ushort)(
            state.YVelocity + MultiplyCartridgeSinCos(
                magnitude,
                unchecked((byte)(angle + 64)))));
    }

    /// <summary>Ports <c>Shitroid_Powerbomb</c> at <c>$A9:EFBA</c>.</summary>
    private void ResolveShitroidPowerBomb(RoomEnemySlot slot, SamusState? samus)
    {
        ShitroidEnemyState state = RequireShitroidState(slot);
        if (slot.Parameter2 != 0)
            BreakReleasedShitroidFollow(state);

        // The private reaction tail-calls main AI. Preserve the extra state/movement/palette
        // tick rather than reducing the power bomb to a state flag.
        RunShitroidMain(slot, samus, _shitroidCameraX, _shitroidCameraY, null);
    }

    /// <summary>Function 26: shots/contact release function 25 into the exit sequence.</summary>
    private static void BreakReleasedShitroidFollow(ShitroidEnemyState state)
    {
        if (state.Function == ShitroidAiFunction.ReleasedFollow)
            state.Function = ShitroidAiFunction.BeginExit;
    }

    private void StepShitroidNormalPalette(ShitroidEnemyState state)
    {
        byte timer = unchecked((byte)(state.PaletteTimerAndPhase >> 8));
        if (timer != 0)
        {
            state.PaletteTimerAndPhase = unchecked((ushort)(
                state.PaletteTimerAndPhase - 0x0100));
            return;
        }

        byte phase = unchecked((byte)((state.PaletteTimerAndPhase + 1) & 7));
        state.PaletteTimerAndPhase = unchecked((ushort)((state.PaletteDelay << 8) | phase));
        if (phase == 5 && state.CryEnabled != 0)
        {
            state.CryCounter = unchecked((ushort)(state.CryCounter + 1));
            if (state.CryCounter >= 4)
            {
                state.CryCounter = 0;
                LastShitroidSoundEffectLibrary2 = state.PaletteDelay < 10
                    ? (ushort)0x0078
                    : (ushort)0x0072;
            }
        }

        int source = ShitroidNormalPaletteSource + phase * 8;
        for (int color = 0; color < 4; color++)
            _cgram!.SetColor(165 + color, ReadWord(_bus!, source + color * 2));
    }

    private static void SetShitroidInstruction(RoomEnemySlot slot, ushort pointer)
    {
        slot.CurrentInstruction = pointer;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private RoomEnemySlot RequireShitroidVictim(RoomEnemySlot slot)
    {
        int victimIndex = slot.SlotIndex + 1;
        if ((uint)victimIndex >= _slots.Length || _slots[victimIndex].EnemyDefinitionPointer == 0)
            throw new InvalidDataException("Shitroid's physical slot-one victim is missing.");
        return _slots[victimIndex];
    }

    private ShitroidEnemyState RequireShitroidState(RoomEnemySlot slot)
    {
        ShitroidEnemyState state = _shitroid ??
            throw new InvalidDataException("Shitroid has no initialized extended state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("A non-Shitroid slot entered the Shitroid dispatcher.");
        return state;
    }

    private static SamusState RequireShitroidSamus(SamusState? samus) => samus ??
        throw new InvalidOperationException("The active Shitroid state requires Samus.");

    private static bool PredecrementShitroidTimerBecameNegative(ShitroidEnemyState state)
    {
        state.StateTimer = unchecked((ushort)(state.StateTimer - 1));
        return unchecked((short)state.StateTimer) < 0;
    }
}
