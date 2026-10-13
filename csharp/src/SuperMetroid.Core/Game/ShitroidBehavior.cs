using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Baby Metroid (Shitroid) species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ShitroidBehavior : IEnemySpecies
{
    private readonly EnemyRoomBinding _room;
    private readonly EnemySlotTable _slotTable;
    private readonly EnemyPresentation _presentation;

    private readonly DeadSidehopperBehavior _deadSidehopperBehavior;

    private readonly ZebetiteBehavior _zebetiteBehavior;

    internal ShitroidBehavior(EnemyRoomBinding room, EnemySlotTable slotTable, EnemyPresentation presentation, DeadSidehopperBehavior deadSidehopperBehavior, ZebetiteBehavior zebetiteBehavior)
    {
        _zebetiteBehavior = zebetiteBehavior;
        _deadSidehopperBehavior = deadSidehopperBehavior;
        _room = room;
        _slotTable = slotTable;
        _presentation = presentation;
    }

    /// <summary>Executes the Tourian Baby Metroid's private animation instructions.</summary>
    private bool TryProcessShitroidInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.BabyMetroid ||
            !Enum.IsDefined((ShitroidInstruction)word))
            return false;

        switch ((ShitroidInstruction)word)
        {
            case ShitroidInstruction.GotoNormal:
                // Instruction 3 returns the calm animation list directly; it does not
                // consume an operand from the calling list.
                cursor = ShitroidInstructionProgramDefinitions.Normal;
                return true;
            case ShitroidInstruction.GotoLatchedOn:
                // Instruction 4 restarts the aggressive/draining loop.
                cursor = ShitroidInstructionProgramDefinitions.LatchedOn;
                return true;
            case ShitroidInstruction.GotoRemorse:
                // Instruction 6 restarts the departure loop.
                cursor = ShitroidInstructionProgramDefinitions.Remorse;
                return true;
            case ShitroidInstruction.GotoYOrPlayRemorseSFX:
                // Instruction 5 samples the existing RNG word; it does not generate a
                // new value. Clear high bit takes the same-bank operand branch. Set high
                // bit plays cry $52 and falls through beyond that operand.
                if ((_room.Random.Read() & 0x8000) == 0)
                {
                    cursor = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)));
                }
                else
                {
                    LastShitroidSoundEffectLibrary2 = ShitroidInstructionSounds.RemorseCry;
                    cursor = unchecked((ushort)(cursor + 4));
                }
                return true;
            default:
                throw new InvalidOperationException(
                    $"Baby Metroid does not own instruction ${word:X4}.");
        }
    }


    // `frameCounter & 6` selects one of these four authored jitter offsets.
    private static readonly Offset16<RoomPixels>[] ShitroidShakeOffsets =
        [new(0, 0), new(-1, 1), new(0, -1), new(1, 1)];

    private readonly List<ShitroidPlmRequest> _shitroidPlmRequests = [];
    internal ShitroidEnemyState? _shitroid;
    private Position16<RoomPixels> _shitroidCamera;

    /// <summary>Frame-local wall mutations requested by the current Shitroid state.</summary>
    public IReadOnlyList<ShitroidPlmRequest> ShitroidPlmRequests => _shitroidPlmRequests;

    /// <summary>Exact layer-one X position requested by state $EFE6, if any.</summary>
    public ushort? RequestedShitroidCameraX { get; private set; }

    /// <summary>Last library-two sound queued during this Shitroid frame.</summary>
    public ushort? LastShitroidSoundEffectLibrary2 { get; private set; }

    /// <summary>Last delayed music command queued during this Shitroid frame.</summary>
    public ShitroidMusicRequest? LastShitroidMusicRequest { get; private set; }

    internal void ResetShitroidRoomState()
    {
        _shitroid = null;
        _shitroidPlmRequests.Clear();
        RequestedShitroidCameraX = null;
        LastShitroidSoundEffectLibrary2 = null;
        LastShitroidMusicRequest = null;
        _shitroidCamera = Position16<RoomPixels>.Zero;
    }

    internal void BeginShitroidFrame()
    {
        _shitroidPlmRequests.Clear();
        RequestedShitroidCameraX = null;
        LastShitroidSoundEffectLibrary2 = null;
        LastShitroidMusicRequest = null;
    }

    /// <summary>Ports <c>Shitroid_Init</c> at <c>$A9:EF37</c>.</summary>
    internal void InitializeShitroid(RoomEnemySlot slot, ushort cameraX)
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
        _room.Scratch.ShitroidScratch.Fill(0);

        // `$2000` is the scheduler's instruction-processing bit; `$1000` blocks plasma
        // from piercing the actor. The upstream C symbol calls `$2000` DisableSamusColl,
        // but the executable scheduler also consumes that same bit for instructions, so
        // the translated property enum retains the directly observed scheduler meaning.
        slot.Properties = slot.Properties.With(
            EnemyProperties.BlocksPlasmaBeam | EnemyProperties.ProcessInstructions);
        slot.PaletteIndex = EnemyPaletteBits.Palette2;
        SetShitroidInstruction(slot, ShitroidInstructionProgramDefinitions.Normal);

        var state = new ShitroidEnemyState(slot)
        {
            Function = ShitroidAiFunction.WaitForCamera,
            Velocity = default,
            PaletteDelay = 10,
        };
        if (unchecked((short)cameraX) < 0)
        {
            // `$0400` removes the actor from ordinary interaction and `$0100` suppresses
            // drawing. Keep the former raw: its historical disassembly name "tangible" is
            // contradicted by the collision walkers that explicitly reject the bit.
            slot.Properties = slot.Properties.With(
                EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
            state.Function = ShitroidAiFunction.Dormant;
        }
        slot.Parameter2 = 0;
        _shitroid = state;

        CopyShitroidTargetPalette(state, ShitroidColorTarget.Sidehopper,
            ShitroidColorRomData.SidehopperDestination);
        CopyShitroidTargetPalette(state, ShitroidColorTarget.Shitroid,
            ShitroidColorRomData.ShitroidDestination);
        CopyShitroidTargetPalette(state, ShitroidColorTarget.DeadSidehopper,
            ShitroidColorRomData.DeadSidehopperDestination);
    }

    private void CopyShitroidTargetPalette(
        ShitroidEnemyState state,
        ShitroidColorTarget target,
        int destinationColor)
    {
        for (int color = 0; color < ShitroidColorRomData.TargetColorCount; color++)
        {
            Bgr555 value = (_room.Artwork?.ShitroidColors ?? throw new InvalidDataException(
                "Shitroid target palette requires installed artwork.")).TargetColor(target, color);
            state.MutableTargetPalette[destinationColor + color] = value;

            // This runtime currently presents completed room fades directly in CGRAM.
            // Keep the separate target copy above, but install the same completed value so
            // the encounter does not wait on an untranslated global palette-fade owner.
            _presentation.BossPalettes.ShitroidTarget(target).Write(color, value);
        }
    }

    /// <summary>Ports <c>Shitroid_Main</c> at <c>$A9:EFC5</c>.</summary>
    internal void RunShitroidMain(
        RoomEnemySlot slot,
        SamusState? samus,
        Position16<RoomPixels> camera,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        ShitroidEnemyState state = RequireShitroidState(slot);
        _shitroidCamera = camera;

        // Shitroid's private shot handler applies recoil but no health loss. The main AI
        // redundantly restores $7FFF every processed frame, exactly as the cartridge does.
        slot.Health = 0x7fff;
        DispatchShitroidState(slot, state, samus, sharedProjectiles);
        DeadTorizoBehavior.MoveBankA9EnemyWithVelocity(slot);
        if (_zebetiteBehavior.PaletteChangeNumber == 0)
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
                state.Velocity = default;
                return;

            case ShitroidAiFunction.WaitForCamera:
                WaitForShitroidCamera(state);
                return;

            case ShitroidAiFunction.BeginEntranceDelay:
                state.Function = ShitroidAiFunction.EntranceDelay;
                state.StateTimer = NativeCountdown.Start(464);
                goto case ShitroidAiFunction.EntranceDelay;

            case ShitroidAiFunction.EntranceDelay:
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    LastShitroidMusicRequest = new ShitroidMusicRequest(
                        MusicCommand.SelectTrack(5),
                        MusicCommandDelay.EightFrames);
                    state.Function = ShitroidAiFunction.FlyToSidehopper;
                    goto case ShitroidAiFunction.FlyToSidehopper;
                }
                return;

            case ShitroidAiFunction.FlyToSidehopper:
                GraduallyAccelerateShitroid(slot, state, 15, new(584, 74), 0x0400);
                if (ShitroidOverlapsRectangle(slot, new(584, 74), new(1, 1)))
                    state.Function = ShitroidAiFunction.ChaseSidehopper;
                return;

            case ShitroidAiFunction.ChaseSidehopper:
            {
                RoomEnemySlot victim = RequireShitroidVictim(slot);
                GraduallyAccelerateShitroid(
                    slot,
                    state,
                    15,
                    victim.Position.Whole + new Offset16<RoomPixels>(0, -32),
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
                _deadSidehopperBehavior.ActivateDeadSidehopperVictim(RequireShitroidVictim(slot), state);
                state.Function = ShitroidAiFunction.RiseAfterFeeding;
                state.StateTimer = NativeCountdown.Start(192);
                goto case ShitroidAiFunction.RiseAfterFeeding;

            case ShitroidAiFunction.RiseAfterFeeding:
                GraduallyAccelerateShitroid(slot, state, 0, new(slot.Position.X.Whole, 104), 0x0400);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Function = ShitroidAiFunction.HoverNearSamus;
                    slot.Parameter2 = 1;
                    SetShitroidScrollPair(RoomScrollState.Blue);
                    QueueShitroidWallPlms(PlmHeaderId.ClearBabyMetroidInvisibleWall);
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
                    target.Position.Whole + new Offset16<RoomPixels>(0, -32),
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
                state.StateTimer = NativeCountdown.Start(120);
                goto case ShitroidAiFunction.PostDrainPause;

            case ShitroidAiFunction.PostDrainPause:
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Function = ShitroidAiFunction.RiseAfterDrainingSamus;
                    state.StateTimer = NativeCountdown.Start(192);
                    goto case ShitroidAiFunction.RiseAfterDrainingSamus;
                }
                return;

            case ShitroidAiFunction.RiseAfterDrainingSamus:
            {
                SamusState target = RequireShitroidSamus(samus);
                GraduallyAccelerateShitroid(slot, state, 0, new(target.Position.X.Whole, 104), 0x0400);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    LastShitroidSoundEffectLibrary2 = 0x007d;
                    state.Function = ShitroidAiFunction.FlyLeft;
                    state.StateTimer = NativeCountdown.Start(88);
                    SetShitroidInstruction(slot, ShitroidInstructionProgramDefinitions.LatchedOn);
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
                    new(unchecked((ushort)(target.Position.X.Whole - 64)), 100),
                    0x0400);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Function = ShitroidAiFunction.FlyRight;
                    state.StateTimer = NativeCountdown.Start(88);
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
                    new(unchecked((ushort)(target.Position.X.Whole + 96)), 104),
                    0x0400);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Function = ShitroidAiFunction.HoldSamusBeforeRelease;
                    state.StateTimer = NativeCountdown.Start(256);
                    SetShitroidInstruction(slot, ShitroidInstructionProgramDefinitions.Remorse);
                }
                return;
            }

            case ShitroidAiFunction.BeginExit:
                LastShitroidSoundEffectLibrary2 = 0x0052;
                state.Function = ShitroidAiFunction.Exit;
                goto case ShitroidAiFunction.Exit;

            case ShitroidAiFunction.Exit:
                GraduallyAccelerateShitroid(slot, state, 0, new(0xff80, 64), 0x0400);
                if (ShitroidOverlapsRectangle(slot, new(0xff80, 64), new(8, 8)))
                {
                    state.Velocity = default;
                    // `$A9:F39A` clears instruction processing and invisibility as Shitroid
                    // reaches the off-screen destination, leaving a dormant non-drawn actor.
                    slot.Properties = slot.Properties.Without(
                        EnemyProperties.ProcessInstructions | EnemyProperties.Invisible);
                    state.Function = ShitroidAiFunction.Dormant;
                }
                return;

            case ShitroidAiFunction.HoldSamusBeforeRelease:
            {
                SamusState target = RequireShitroidSamus(samus);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    target.Drained.Release(_room.Bus!, target);
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
                throw new InvalidDataException(
                    $"Shitroid state $A9:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void WaitForShitroidCamera(ShitroidEnemyState state)
    {
        if (unchecked((short)(_shitroidCamera.X - 513)) >= 0)
            return;

        // Native writes layer1_x_pos directly and zero-extends scroll bytes zero and two
        // into their adjacent bytes. The paired calls below preserve the observable four
        // byte scroll array without pretending that this is ordinary camera tracking.
        RequestedShitroidCameraX = 512;
        SetShitroidScrollPair(RoomScrollState.RedBoundary);
        QueueShitroidWallPlms(PlmHeaderId.CreateBabyMetroidInvisibleWall);
        state.Function = ShitroidAiFunction.BeginEntranceDelay;
        state.CryEnabled = 1;
    }

    private void SetShitroidScrollPair(RoomScrollState highState)
    {
        RoomScrollState scroll0 = _room.Services.RoomScroll.Read(0);
        RoomScrollState scroll2 = _room.Services.RoomScroll.Read(2);
        _room.Services.RoomScroll.Write(0, scroll0);
        _room.Services.RoomScroll.Write(1, highState);
        _room.Services.RoomScroll.Write(2, scroll2);
        _room.Services.RoomScroll.Write(3, highState);
    }

    private void QueueShitroidWallPlms(PlmHeaderId header)
    {
        _shitroidPlmRequests.Add(new ShitroidPlmRequest(new(0x30, 0x03), header));
        _shitroidPlmRequests.Add(new ShitroidPlmRequest(new(0x1f, 0x03), header));
    }

    private void AttachShitroidToSidehopper(RoomEnemySlot slot, ShitroidEnemyState state)
    {
        RoomEnemySlot victim = RequireShitroidVictim(slot);
        Position16<RoomPixels> latch = victim.Position.Whole + new Offset16<RoomPixels>(0, -32);
        if (!AccelerateShitroidExactly(state, 0x0200, latch))
            return;

        state.Velocity = default;
        slot.Position = slot.Position.WithWhole(latch);
        SetShitroidInstruction(slot, ShitroidInstructionProgramDefinitions.LatchedOn);
        state.Function = ShitroidAiFunction.DrainSidehopper;
        state.PaletteDelay = 1;
        slot.Parameter2 = 0;
        state.StateTimer = NativeCountdown.Start(320);
    }

    private void DrainShitroidVictim(RoomEnemySlot slot, ShitroidEnemyState state)
    {
        RoomEnemySlot victim = RequireShitroidVictim(slot);
        int shakeIndex = (slot.FrameCounter & 6) >> 1;
        slot.Position = slot.Position.WithWhole(
            victim.Position.Whole + ShitroidShakeOffsets[shakeIndex] + new Offset16<RoomPixels>(0, -32));
        ushort oldTimer = state.StateTimer.RawValue;
        state.StateTimer = NativeCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer == 1)
        {
            state.Function = ShitroidAiFunction.ActivateSidehopperCorpse;
            SetShitroidInstruction(slot, ShitroidInstructionProgramDefinitions.FinishDraining);
            state.PaletteDelay = 10;
        }
    }

    private void HoverShitroidNearSamus(
        RoomEnemySlot slot,
        ShitroidEnemyState state,
        SamusState samus)
    {
        UpdateShitroidHorizontalLock(state, slot.Position.X.Whole, samus.Position.X.Whole, threshold: 8);
        if (state.HorizontalLockAccumulator >= 0x0100 ||
            unchecked((short)(samus.Position.X.Whole - 512)) < 0)
        {
            state.Function = ShitroidAiFunction.ChaseSamus;
            return;
        }

        ushort targetY = SelectShitroidHoverY(state, samus.Position.Y.Whole);
        GraduallyAccelerateShitroid(
            slot,
            state,
            10,
            new(samus.Position.X.Whole, targetY),
            0x0400);
    }

    private ushort SelectShitroidHoverY(ShitroidEnemyState state, ushort followedY)
    {
        if (!state.HoverTimer.IsZero)
        {
            state.HoverTimer = state.HoverTimer.Decrement().Remaining;
            return followedY;
        }
        if ((_room.Random.Read() & 0x0fff) >= 0x0fe0)
            state.HoverTimer = NativeCountdown.Start(32);
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
            state.Velocity = default;
            SetShitroidInstruction(slot, ShitroidInstructionProgramDefinitions.FinishDraining);
            state.PaletteDelay = 10;
            samus.SpecialSuperPaletteFlags = 0;
            samus.Drained.LetFall(_room.Bus!, samus);
            state.CryEnabled = 0;
            LastShitroidMusicRequest = new ShitroidMusicRequest(
                MusicCommand.SelectTrack(7),
                MusicCommandDelay.EightFrames);
            return;
        }

        sharedProjectiles?.SetSharedCooldown(8);
        sharedProjectiles?.SetSharedBombCounter(5);
        samus.XSpeedDivisor = 2;
        // CMP #4 ($A9:F238) leaves carry = (Y speed >= 4, unsigned) through the clamp and
        // the shake-table loads; $A9:F24D adds Samus's X with no CLC, so that carry is one
        // extra pixel. BMI reads only the sign of the difference.
        ushort ySpeed = samus.Kinematics.YSpeed;
        int xCarry = ySpeed >= 4 ? 1 : 0;
        if ((unchecked((ushort)(ySpeed - 4)) & 0x8000) == 0)
            samus.Kinematics.YSpeed = 2;

        int shakeIndex = (slot.FrameCounter & 6) >> 1;
        slot.Position = slot.Position.WithWhole(
            samus.Position.Whole + ShitroidShakeOffsets[shakeIndex] + new Offset16<RoomPixels>((short)xCarry, -20));

        // `$A9:C560-$C57C` subtracts four without Varia and two with it. The native
        // expression is written as `health + (equipped & 1) + (-4 or -3)`, so reducing
        // those branches to three/two loses one point on both paths and materially shifts
        // the frame on which the Super Metroid forces Samus into drained crouching art.
        int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? 2 : 4;
        samus.Health = damage >= samus.Health
            ? (ushort)1
            : unchecked((ushort)(samus.Health - damage));
    }

    private bool FollowReleasedSamus(
        RoomEnemySlot slot,
        ShitroidEnemyState state,
        SamusState samus)
    {
        UpdateShitroidHorizontalLock(state, slot.Position.X.Whole, samus.Position.X.Whole, threshold: 2);
        ushort targetY = SelectShitroidHoverY(
            state,
            unchecked((ushort)(samus.Position.Y.Whole - 18)));
        GraduallyAccelerateShitroid(
            slot,
            state,
            8,
            new(samus.Position.X.Whole, targetY),
            0x0400);
        return state.HorizontalLockAccumulator >= 0x0400 ||
            unchecked((short)(samus.Position.X.Whole - 128)) < 0;
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
        Position16<RoomPixels> target,
        ushort offscreenReversalKick)
    {
        if ((uint)divisorTableIndex >= 16)
            throw new ArgumentOutOfRangeException(nameof(divisorTableIndex));
        // GradualAccelerationDivisorTable ($A9:F56A) holds $10 down to $01.
        byte divisor = (byte)(16 - divisorTableIndex);
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(BabyMetroidGradualAcceleration.AccelerateHorizontally(
            slot.Position.X.Whole, target.X, state.Velocity.X.RawValue, divisor, offscreenReversalKick,
            () => ShitroidIsOffScreen(slot))));
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(BabyMetroidGradualAcceleration.AccelerateVertically(
            slot.Position.Y.Whole, target.Y, state.Velocity.Y.RawValue, divisor)));
    }

    private bool ShitroidIsOffScreen(RoomEnemySlot slot)
    {
        if (unchecked((short)slot.Position.Y.Whole) < 0)
            return true;
        int screenY = unchecked((short)(slot.Position.Y.Whole + 96 - _shitroidCamera.Y));
        if (screenY is < 0 or >= 416)
            return true;
        if (unchecked((short)slot.Position.X.Whole) < 0)
            return true;
        int screenX = unchecked((short)(slot.Position.X.Whole + 16 - _shitroidCamera.X));
        return screenX is < 0 or >= 288;
    }

    private static bool AccelerateShitroidExactly(
        ShitroidEnemyState state,
        ushort acceleration,
        Position16<RoomPixels> target)
    {
        (bool xArrived, ushort xVelocity) = AccelerateShitroidAxis(
            state.Slot.Position.X.Whole,
            target.X,
            state.Velocity.X.RawValue,
            acceleration);
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(xVelocity));
        (bool yArrived, ushort yVelocity) = AccelerateShitroidAxis(
            state.Slot.Position.Y.Whole,
            target.Y,
            state.Velocity.Y.RawValue,
            acceleration);
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(yVelocity));
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
            current + unchecked((sbyte)(SnesEightEight.WholeOf(velocityWord))) - target));
        bool crossed = delta >= 0 ? predicted <= 0 : predicted >= 0;
        if (crossed)
            velocityWord = 0;
        return (crossed, velocityWord);
    }

    private static bool ShitroidActorsOverlap(RoomEnemySlot left, RoomEnemySlot right) =>
        Math.Abs(unchecked((short)(right.Position.X.Whole - left.Position.X.Whole))) <
            left.Radius.X + right.Radius.X + 1 &&
        Math.Abs(unchecked((short)(right.Position.Y.Whole - left.Position.Y.Whole))) <
            left.Radius.Y + right.Radius.Y + 1;

    private static bool ShitroidOverlapsRectangle(
        RoomEnemySlot slot,
        Position16<RoomPixels> center,
        Extent16<RoomPixels> radius) =>
        Math.Abs(unchecked((short)(center.X - slot.Position.X.Whole))) < slot.Radius.X + radius.X + 1 &&
        Math.Abs(unchecked((short)(center.Y - slot.Position.Y.Whole))) < slot.Radius.Y + radius.Y + 1;

    /// <summary>Ports the private touch callback at <c>$A9:F789</c>.</summary>
    private void ResolveShitroidTouch(RoomEnemySlot slot, SamusState samus)
    {
        ShitroidEnemyState state = RequireShitroidState(slot);
        if (slot.Parameter2 == 0)
            return;

        BreakReleasedShitroidFollow(state);
        if (samus.PoseMetadata.ReadMovementType(_room.Bus!) == SamusMovementType.SpinJumping &&
            unchecked((short)(samus.Position.X.Whole - 512)) >= 0)
        {
            // The native expression `$80-angle+$80`, truncated to a byte, is simply the
            // negated source angle. Magnitude $40 is added to the current 8.8 velocity.
            SnesAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(new(
                unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)),
                unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))));
            SnesAngle angle = SnesAngle.NormalizeRaw(-sourceAngle.RawValue);
            state.Velocity += new EightEightVector<RoomPixels>(
                new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(0x0040, angle.TableIndex)),
                new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(
                    0x0040,
                    angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex)));
            return;
        }

        if (state.Function == ShitroidAiFunction.ChaseSamus)
        {
            if (AccelerateShitroidExactly(
                state,
                0x0200,
                samus.Position.Whole + new Offset16<RoomPixels>(0, -32)))
            {
                SetShitroidInstruction(slot, ShitroidInstructionProgramDefinitions.LatchedOn);
                state.PaletteDelay = 1;
                slot.Parameter2 = 0;
                state.Velocity = default;
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
        ushort collisionDamage,
        SamusProjectileSlot slotZeroProjectile)
    {
        ShitroidEnemyState state = RequireShitroidState(slot);
        if (slot.Parameter2 == 0)
            return;

        BreakReleasedShitroidFollow(state);
        SnesAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(new(
            unchecked((short)(slotZeroProjectile.Position.X.Whole - slot.Position.X.Whole)),
            unchecked((short)(slotZeroProjectile.Position.Y.Whole - slot.Position.Y.Whole))));
        SnesAngle angle = SnesAngle.NormalizeRaw(-sourceAngle.RawValue);
        ushort magnitude = unchecked((ushort)(8 * collisionDamage));
        if (magnitude >= 0x00f0)
            magnitude = 0x00f0;
        state.Velocity += new EightEightVector<RoomPixels>(
            new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(magnitude, angle.TableIndex)),
            new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(
                magnitude,
                angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex)));
    }

    /// <summary>Ports <c>Shitroid_Powerbomb</c> at <c>$A9:EFBA</c>.</summary>
    private void ResolveShitroidPowerBomb(RoomEnemySlot slot, SamusState? samus)
    {
        ShitroidEnemyState state = RequireShitroidState(slot);
        if (slot.Parameter2 != 0)
            BreakReleasedShitroidFollow(state);

        // The private reaction tail-calls main AI. Preserve the extra state/movement/palette
        // tick rather than reducing the power bomb to a state flag.
        RunShitroidMain(slot, samus, _shitroidCamera, null);
    }

    /// <summary>Function 26: shots/contact release function 25 into the exit sequence.</summary>
    private static void BreakReleasedShitroidFollow(ShitroidEnemyState state)
    {
        if (state.Function == ShitroidAiFunction.ReleasedFollow)
            state.Function = ShitroidAiFunction.BeginExit;
    }

    internal void StepShitroidNormalPalette(ShitroidEnemyState state)
    {
        byte timer = SnesLittleEndian.HighByte(state.PaletteTimerAndPhase);
        if (timer != 0)
        {
            state.PaletteTimerAndPhase = unchecked((ushort)(
                state.PaletteTimerAndPhase - 0x0100));
            return;
        }

        byte phase = unchecked((byte)((state.PaletteTimerAndPhase + 1) & 7));
        state.PaletteTimerAndPhase = SnesLittleEndian.Word(phase, unchecked((byte)state.PaletteDelay));
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

        for (int color = 0; color < ShitroidColorRomData.NormalColorsPerFrame; color++)
        {
            Bgr555 value = (_room.Artwork?.ShitroidColors ?? throw new InvalidDataException(
                "Shitroid normal palette requires installed artwork.")).NormalColor(phase, color);
            _presentation.BossPalettes.ShitroidMembrane.Write(color, value);
        }
    }

    private static void SetShitroidInstruction(RoomEnemySlot slot, ushort pointer)
    {
        slot.CurrentInstruction = pointer;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    internal RoomEnemySlot RequireShitroidVictim(RoomEnemySlot slot)
    {
        int victimIndex = slot.SlotIndex + 1;
        if ((uint)victimIndex >= _slotTable._slots.Length || _slotTable._slots[victimIndex].EnemyDefinitionPointer == 0)
            throw new InvalidDataException("Shitroid's physical slot-one victim is missing.");
        return _slotTable._slots[victimIndex];
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
        NativeWordCounterStep timer = state.StateTimer.Decrement();
        state.StateTimer = timer.Remaining;
        return timer.IsNegative;
    }

    internal void RegisterShitroidBombReactions(EnemyAiTable table)
    {
        // `$A9:F842` reads damage from the selected physical projectile slot, but always reads aim coordinates from
        // ordinary slot zero. For a bomb collision that means family-$0500 damage controls recoil strength while
        // whatever stale/live shot occupies slot zero controls direction. It never enters vulnerability damage.
        table.AddBombSpecial(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid &&
                                  c.SelectedShotAi == EnemyInteractionCallback.ShitroidShot,
            c => ResolveShitroidShot(c.Enemy, c.Bomb.Damage, c.OrdinaryProjectiles.Slots[0]));
    }

    internal void RegisterShitroidInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_BabyMetroid, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeShitroid(slot, cameraX);
        });
    }

    internal void RegisterShitroidMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_BabyMetroid, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid, (slot, frame, nmiFrameCounter8) =>
        {
            RunShitroidMain(slot, frame.Samus, frame.Camera, frame.SharedProjectiles);
        });
    }

    internal void RegisterShitroidOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessShitroidInstruction(slot, word, ref cursor);
        });
    }

    internal void RegisterShitroidPowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.ShitroidPowerBomb)
        {
            ReplacesCommon = ResolveShitroidPowerBomb,
        });
    }

    internal void RegisterShitroidShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.ShitroidShot)
        {
            // Like Dead Torizo, Shitroid receives the bank-$A0 projectile prelude but never creates an impact or
            // enters vulnerability damage. Its own callback converts shot strength and slot-zero aim into recoil.
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                ResolveShitroidShot(c.Enemy, c.Projectile.Damage, c.Projectiles.Slots[0]);
                return true;
            },
        });
    }

    internal void RegisterShitroidTouch(EnemyAiTable table)
    {
        // `$A9:F789` replaces normal touch damage entirely. Depending on its current cutscene state, contact either
        // starts pursuit/latching or adds a spin-jump recoil vector to the actor itself; Samus is never hurt here.
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.ShitroidTouch,
            context => ResolveShitroidTouch(context.Slot, context.Samus));
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        BeginShitroidFrame();
        ResetShitroidRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        BeginShitroidFrame();
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastShitroidSoundEffectLibrary2, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(LastShitroidMusicRequest?.Command, LastShitroidMusicRequest?.Delay);
    }
}
