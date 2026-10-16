using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby Metroid (Shitroid) species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ShitroidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.BabyMetroid];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ShitroidEntity(this, slot, common);

    /// <summary>Provides scratch WRAM, room scroll words, RNG, and installed encounter colors.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Locates the physical slot-one Sidehopper victim beside Shitroid's slot zero.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Receives the membrane cycle and the encounter's target palettes.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Activates the Sidehopper corpse after Shitroid finishes feeding.</summary>
    private readonly DeadSidehopperBehavior _deadSidehopperBehavior;

    /// <summary>Controls whether the normal membrane palette can advance during the Zebetite sequence.</summary>
    private readonly ZebetiteBehavior _zebetiteBehavior;

    /// <summary>Connects the Tourian Baby Metroid to room, victim, palette, and Zebetite owners.</summary>
    internal ShitroidBehavior(EnemyRoomBinding room, EnemySlotTable slotTable, EnemyPresentation presentation, DeadSidehopperBehavior deadSidehopperBehavior, ZebetiteBehavior zebetiteBehavior, EnemyFrameResultBuilder frameResults)
    {
        _frameResults = frameResults;
        _zebetiteBehavior = zebetiteBehavior;
        _deadSidehopperBehavior = deadSidehopperBehavior;
        _room = room;
        _slotTable = slotTable;
        _presentation = presentation;
    }

    /// <summary>Four authored feeding jitter offsets indexed by <c>frameCounter &amp; 6</c>.</summary>
    private static readonly Offset16<RoomPixels>[] ShitroidShakeOffsets =
        [new(0, 0), new(-1, 1), new(0, -1), new(1, 1)];

    /// <summary>Single slot-zero actor's private velocity, palette, and cutscene state.</summary>
    internal ShitroidEnemyState? _shitroid;
    /// <summary>Layer-one camera used by the offscreen acceleration and wall sequence.</summary>
    private Position16<RoomPixels> _shitroidCamera;

    /// <summary>Last library-two sound queued during this Shitroid frame.</summary>
    public ushort? LastShitroidSoundEffectLibrary2 { get; private set; }

    /// <summary>Last delayed music command queued during this Shitroid frame.</summary>
    public ShitroidMusicRequest? LastShitroidMusicRequest { get; private set; }

    /// <summary>Discards the prior room's actor state, camera, and pending cutscene outputs.</summary>
    internal void ResetShitroidRoomState()
    {
        _shitroid = null;
        LastShitroidSoundEffectLibrary2 = null;
        LastShitroidMusicRequest = null;
        _shitroidCamera = Position16<RoomPixels>.Zero;
    }

    /// <summary>Clears frame-local wall, camera, sound, and music requests before AI dispatch.</summary>
    internal void BeginShitroidFrame()
    {
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
        _room.Staging.ShitroidScratch.Fill(0);

        // `$2000` is the scheduler's instruction-processing bit; `$1000` blocks plasma
        // from piercing the actor. The upstream C symbol calls `$2000` DisableSamusColl,
        // but the executable scheduler also consumes that same bit for instructions, so
        // the translated property enum retains the directly observed scheduler meaning.
        slot.Properties = slot.Properties.With(
            EnemyProperties.BlocksPlasmaBeam | EnemyProperties.ProcessInstructions);
        slot.PaletteIndex = EnemyPaletteBits.Palette2;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        var state = new ShitroidEnemyState(slot)
        {
            Cursor = AnimationProgramCursor<ShitroidProgram>.Start(ShitroidProgram.Normal),
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

    /// <summary>Copies an installed target palette into mutable encounter state and the visible boss palette.</summary>
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
        slot.Health = (ushort)short.MaxValue;
        DispatchShitroidState(slot, state, samus, sharedProjectiles);
        DeadTorizoBehavior.MoveBankA9EnemyWithVelocity(slot);
        if (_zebetiteBehavior.PaletteChangeNumber == 0)
            StepShitroidNormalPalette(state);
    }

    /// <summary>Advances entrance, feeding, Samus chase and drain, remorse, or exit cutscene states.</summary>
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
                GraduallyAccelerateShitroid(slot, state, 15, new(584, 74), 1024);
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
                    1024);
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
                DeadSidehopperBehavior.ActivateDeadSidehopperVictim(RequireShitroidVictim(slot), state);
                state.Function = ShitroidAiFunction.RiseAfterFeeding;
                state.StateTimer = NativeCountdown.Start(192);
                goto case ShitroidAiFunction.RiseAfterFeeding;

            case ShitroidAiFunction.RiseAfterFeeding:
                GraduallyAccelerateShitroid(slot, state, 0, new(slot.Position.X.Whole, 104), 1024);
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
                    1024);
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
                GraduallyAccelerateShitroid(slot, state, 0, new(target.Position.X.Whole, 104), 1024);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    LastShitroidSoundEffectLibrary2 = ShitroidSoundDefinitions.FlyAwayCry;
                    state.Function = ShitroidAiFunction.FlyLeft;
                    state.StateTimer = NativeCountdown.Start(88);
                    SetShitroidProgram(slot, ShitroidProgram.LatchedOn);
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
                    1024);
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
                    1024);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Function = ShitroidAiFunction.HoldSamusBeforeRelease;
                    state.StateTimer = NativeCountdown.Start(256);
                    SetShitroidProgram(slot, ShitroidProgram.Remorse);
                }
                return;
            }

            case ShitroidAiFunction.BeginExit:
                LastShitroidSoundEffectLibrary2 = ShitroidSoundDefinitions.RemorseCry;
                state.Function = ShitroidAiFunction.Exit;
                goto case ShitroidAiFunction.Exit;

            case ShitroidAiFunction.Exit:
                GraduallyAccelerateShitroid(slot, state, 0, new(unchecked((ushort)-128), 64), 1024);
                if (ShitroidOverlapsRectangle(slot, new(unchecked((ushort)-128), 64), new(8, 8)))
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
                    target.Drained.Release(target);
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

    /// <summary>Starts the entrance after layer-one X crosses 513, locking scroll and creating invisible walls.</summary>
    private void WaitForShitroidCamera(ShitroidEnemyState state)
    {
        if (unchecked((short)(_shitroidCamera.X - 513)) >= 0)
            return;

        // Native writes layer1_x_pos directly and zero-extends scroll bytes zero and two
        // into their adjacent bytes. The paired calls below preserve the observable four
        // byte scroll array without pretending that this is ordinary camera tracking.
        _frameResults.LayerOneXOverride = 512;
        SetShitroidScrollPair(RoomScrollState.RedBoundary);
        QueueShitroidWallPlms(PlmHeaderId.CreateBabyMetroidInvisibleWall);
        state.Function = ShitroidAiFunction.BeginEntranceDelay;
        state.CryEnabled = 1;
    }

    /// <summary>Writes high scroll bytes one and three while retaining and zero-extending the existing low bytes.</summary>
    private void SetShitroidScrollPair(RoomScrollState highState)
    {
        RoomScrollState scroll0 = _room.Services.RoomScroll.Read(0);
        RoomScrollState scroll2 = _room.Services.RoomScroll.Read(2);
        _room.Services.RoomScroll.Write(0, scroll0);
        _room.Services.RoomScroll.Write(1, highState);
        _room.Services.RoomScroll.Write(2, scroll2);
        _room.Services.RoomScroll.Write(3, highState);
    }

    /// <summary>Requests the two room-column wall PLMs at the authored Tourian positions.</summary>
    private void QueueShitroidWallPlms(PlmHeaderId header)
    {
        _frameResults.ShitroidPlms.Add(new ShitroidPlmRequest(new(48, 3), header));
        _frameResults.ShitroidPlms.Add(new ShitroidPlmRequest(new(31, 3), header));
    }

    /// <summary>Accelerates to the Sidehopper's head and enters the latched feeding animation on arrival.</summary>
    private void AttachShitroidToSidehopper(RoomEnemySlot slot, ShitroidEnemyState state)
    {
        RoomEnemySlot victim = RequireShitroidVictim(slot);
        Position16<RoomPixels> latch = victim.Position.Whole + new Offset16<RoomPixels>(0, -32);
        if (!AccelerateShitroidExactly(state, 512, latch))
            return;

        state.Velocity = default;
        slot.Position = slot.Position.WithWhole(latch);
        SetShitroidProgram(slot, ShitroidProgram.LatchedOn);
        state.Function = ShitroidAiFunction.DrainSidehopper;
        state.PaletteDelay = 1;
        slot.Parameter2 = 0;
        state.StateTimer = NativeCountdown.Start(320);
    }

    /// <summary>Shakes above the victim for 320 ticks before activating its corpse sequence.</summary>
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
            SetShitroidProgram(slot, ShitroidProgram.FinishDraining);
            state.PaletteDelay = 10;
        }
    }

    /// <summary>Steers toward Samus with intermittent high hovering until horizontal lock enters pursuit.</summary>
    private void HoverShitroidNearSamus(
        RoomEnemySlot slot,
        ShitroidEnemyState state,
        SamusState samus)
    {
        UpdateShitroidHorizontalLock(state, slot.Position.X.Whole, samus.Position.X.Whole, threshold: 8);
        if (state.HorizontalLockAccumulator >= 256 ||
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
            1024);
    }

    /// <summary>Alternates between a timed followed Y and authored height 80 using the room RNG.</summary>
    private ushort SelectShitroidHoverY(ShitroidEnemyState state, ushort followedY)
    {
        if (!state.HoverTimer.IsZero)
        {
            state.HoverTimer = state.HoverTimer.Decrement().Remaining;
            return followedY;
        }
        if ((_room.Random.Read() & 4095) >= 4064)
            state.HoverTimer = NativeCountdown.Start(32);
        return 80;
    }

    /// <summary>Locks bombs and movement, shakes over Samus, and drains energy until the scripted release at one health.</summary>
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
            SetShitroidProgram(slot, ShitroidProgram.FinishDraining);
            state.PaletteDelay = 10;
            samus.SpecialSuperPaletteFlags = 0;
            samus.Drained.LetFall(samus);
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
        if (unchecked((short)unchecked((ushort)(ySpeed - 4))) >= 0)
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
            ? 1
            : samus.Health - damage;
    }

    /// <summary>Follows the recovering Samus until sustained horizontal lock or the left-screen exit condition.</summary>
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
            1024);
        return state.HorizontalLockAccumulator >= 1024 ||
            unchecked((short)(samus.Position.X.Whole - 128)) < 0;
    }

    /// <summary>Accumulates two lock ticks within a narrow X threshold and decays by one when separated.</summary>
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
            state.HorizontalLockAccumulator = NativeWordCounter.Decrement(state.HorizontalLockAccumulator).Value;
        }
    }

    /// <summary>Uses the bank-$A9 divisor table to steer both axes, with an offscreen horizontal reversal kick.</summary>
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

    /// <summary>Tests the Baby Metroid's asymmetric flight window against the current layer-one camera.</summary>
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

    /// <summary>Accelerates both axes toward a latch target and reports arrival only when both cross it.</summary>
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

    /// <summary>Clamps signed 8.8 speed to 1280 and zeroes it on predicted target crossing.</summary>
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

    /// <summary>Tests inclusive-radius overlap between two enemy rectangles using signed native coordinate deltas.</summary>
    private static bool ShitroidActorsOverlap(RoomEnemySlot left, RoomEnemySlot right) =>
        Math.Abs(unchecked((short)(right.Position.X.Whole - left.Position.X.Whole))) <
            left.Radius.X + right.Radius.X + 1 &&
        Math.Abs(unchecked((short)(right.Position.Y.Whole - left.Position.Y.Whole))) <
            left.Radius.Y + right.Radius.Y + 1;

    /// <summary>Tests Shitroid against a scripted rectangle without allocating another enemy slot.</summary>
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
        if (samus.PoseMetadata.ReadMovementType() == SamusMovementType.SpinJumping &&
            unchecked((short)(samus.Position.X.Whole - 512)) >= 0)
        {
            // The native expression `$80-angle+$80`, truncated to a byte, is simply the
            // negated source angle. Magnitude $40 is added to the current 8.8 velocity.
            SnesAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(new(
                unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)),
                unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))));
            SnesAngle angle = SnesAngle.NormalizeRaw(-sourceAngle.RawValue);
            state.Velocity += new EightEightVector<RoomPixels>(
                new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(64, angle.TableIndex)),
                new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(
                    64,
                    angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex)));
            return;
        }

        if (state.Function == ShitroidAiFunction.ChaseSamus)
        {
            if (AccelerateShitroidExactly(
                state,
                512,
                samus.Position.Whole + new Offset16<RoomPixels>(0, -32)))
            {
                SetShitroidProgram(slot, ShitroidProgram.LatchedOn);
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
        if (magnitude >= 240)
            magnitude = 240;
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

    /// <summary>Advances the eight-phase membrane colors and paces cries when the Zebetite palette allows it.</summary>
    internal void StepShitroidNormalPalette(ShitroidEnemyState state)
    {
        byte timer = SnesLittleEndian.HighByte(state.PaletteTimerAndPhase);
        if (timer != 0)
        {
            state.PaletteTimerAndPhase = unchecked((ushort)(
                state.PaletteTimerAndPhase - 256));
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
                    ? ShitroidSoundDefinitions.QuickDrainCry
                    : ShitroidSoundDefinitions.DrainCry;
            }
        }

        for (int color = 0; color < ShitroidColorRomData.NormalColorsPerFrame; color++)
        {
            Bgr555 value = (_room.Artwork?.ShitroidColors ?? throw new InvalidDataException(
                "Shitroid normal palette requires installed artwork.")).NormalColor(phase, color);
            _presentation.BossPalettes.ShitroidMembrane.Write(color, value);
        }
    }

    /// <summary>Starts <paramref name="program"/> with both instruction timers reset.</summary>
    private void SetShitroidProgram(RoomEnemySlot slot, ShitroidProgram program)
    {
        RequireShitroidState(slot).Cursor = AnimationProgramCursor<ShitroidProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Requires the Sidehopper victim in the physical enemy slot immediately after Shitroid.</summary>
    internal RoomEnemySlot RequireShitroidVictim(RoomEnemySlot slot)
    {
        int victimIndex = slot.SlotIndex + 1;
        if ((uint)victimIndex >= _slotTable._slots.Length || _slotTable._slots[victimIndex].EnemyDefinitionPointer == 0)
            throw new InvalidDataException("Shitroid's physical slot-one victim is missing.");
        return _slotTable._slots[victimIndex];
    }

    /// <summary>Rejects missing state or dispatch from an enemy slot other than the initialized Baby Metroid.</summary>
    private ShitroidEnemyState RequireShitroidState(RoomEnemySlot slot)
    {
        ShitroidEnemyState state = _shitroid ??
            throw new InvalidDataException("Shitroid has no initialized extended state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("A non-Shitroid slot entered the Shitroid dispatcher.");
        return state;
    }

    /// <summary>Requires the live Samus actor for pursuit or draining states.</summary>
    private static SamusState RequireShitroidSamus(SamusState? samus) => samus ??
        throw new InvalidOperationException("The active Shitroid state requires Samus.");

    /// <summary>Applies the native wrapping countdown and reports its signed-negative transition.</summary>
    private static bool PredecrementShitroidTimerBecameNegative(ShitroidEnemyState state)
    {
        NativeWordCounterStep timer = state.StateTimer.Decrement();
        state.StateTimer = timer.Remaining;
        return timer.IsNegative;
    }

    /// <summary>Uses bomb damage for recoil strength but ordinary slot zero for its direction, without health loss.</summary>
    internal void RegisterShitroidBombReactions(EnemySharedReactions table)
    {
        // `$A9:F842` reads damage from the selected physical projectile slot, but always reads aim coordinates from
        // ordinary slot zero. For a bomb collision that means family-$0500 damage controls recoil strength while
        // whatever stale/live shot occupies slot zero controls direction. It never enters vulnerability damage.
        table.AddBombSpecial(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid &&
                                  c.SelectedShotAi == EnemyInteractionCallback.ShitroidShot,
            c => ResolveShitroidShot(c.Enemy, c.Bomb.Damage, c.OrdinaryProjectiles.Slots[0]));
    }

    /// <summary>Ports Shitroid's initialization AI.</summary>
    internal void RunShitroidInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort cameraX)
    {
        InitializeShitroid(slot, cameraX);
    }

    /// <summary>Ports Shitroid's main AI.</summary>
    internal void RunShitroidMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunShitroidMain(slot, frame.Samus, frame.Camera, frame.SharedProjectiles);
    }

    /// <summary>Advances Shitroid's typed animation.</summary>
    internal void AdvanceShitroidTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        AdvanceShitroidAnimation(slot);
    }

    /// <summary>
    /// Runs the Shitroid's program where the enemy instruction interpreter would: the frame expires on the update its
    /// native timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvanceShitroidAnimation(RoomEnemySlot slot)
    {
        ShitroidEnemyState state = RequireShitroidState(slot);
        ushort oldTimer = slot.InstructionTimer.RawValue;
        slot.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer != 1)
        {
            slot.ExtraProperties = slot.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        AnimationProgramCursor<ShitroidProgram> cursor = state.Cursor;
        NativeCountdown counter = slot.Timer;
        AnimationProgramAdvance<BabyMetroidFrame> advance = ShitroidAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, continuation) => ApplyShitroidCue(cue));
        slot.Timer = counter;
        state.Cursor = cursor;
        if (advance.Kind != AnimationProgramAdvanceKind.Show)
            throw new InvalidOperationException($"The Shitroid's programs have no {advance.Kind} step.");
        slot.InstructionTimer = NativeCountdown.FromRaw((ushort)advance.Duration);
        slot.SpritemapPointer = advance.Frame.Spritemap;
        slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
    }

    /// <summary>Carries out one cue of a Shitroid program.</summary>
    /// <returns>The remorse program when the cry does not sound; otherwise null.</returns>
    private ShitroidProgram? ApplyShitroidCue(ShitroidCue cue)
    {
        switch (cue)
        {
            case ShitroidCue.MaybeRemorseCry:
                // The cue samples the existing random word; it does not generate a new one.
                if (unchecked((short)_room.Random.Read()) >= 0)
                    return ShitroidProgram.Remorse;
                LastShitroidSoundEffectLibrary2 = ShitroidSoundDefinitions.RemorseCry;
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(ShitroidCue)} {cue}.");
        }
    }

    /// <summary>Runs the private power-bomb callback in place of common enemy damage.</summary>
    internal void RegisterShitroidPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.ShitroidPowerBomb)
        {
            ReplacesCommon = ResolveShitroidPowerBomb,
        });
    }

    /// <summary>Marks projectile collision and converts shot damage into recoil without vulnerability damage.</summary>
    internal void RegisterShitroidShotReactions(EnemySharedReactions table)
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

    /// <summary>Uses cutscene latching or recoil on contact instead of ordinary Samus damage.</summary>
    internal void RegisterShitroidTouch(EnemySharedReactions table)
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
