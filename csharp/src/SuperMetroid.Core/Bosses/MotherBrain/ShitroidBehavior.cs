using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby Metroid (Shitroid) species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ShitroidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;

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

    /// <summary>Controls whether the normal membrane palette can advance during the Zebetite sequence.</summary>
    private readonly ZebetiteBehavior _zebetiteBehavior;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Connects the Tourian Baby Metroid to room, victim, palette, and Zebetite owners.</summary>
    internal ShitroidBehavior(EnemyRoomBinding room, EnemySlotTable slotTable, EnemyPresentation presentation, ZebetiteBehavior zebetiteBehavior, BossFrameResultBuilder frameResults, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _frameResults = frameResults;
        _zebetiteBehavior = zebetiteBehavior;
        _room = room;
        _slotTable = slotTable;
        _presentation = presentation;
    }

    /// <summary>Four authored feeding jitter offsets indexed by <c>frameCounter &amp; 6</c>.</summary>
    private static readonly Offset16<RoomPixels>[] ShitroidShakeOffsets =
        [new(0, 0), new(-1, 1), new(0, -1), new(1, 1)];

    /// <summary>Single slot-zero actor's private velocity, palette, and cutscene state.</summary>
    internal ShitroidEnemyState? Shitroid { get; private set; }
    /// <summary>Layer-one camera used by the offscreen acceleration and wall sequence.</summary>
    private Position16<RoomPixels> _shitroidCamera;

    /// <summary>Last library-two sound queued during this Shitroid frame.</summary>
    public SoundEffectId? LastShitroidSoundEffectLibrary2 { get; private set; }

    /// <summary>Last delayed music command queued during this Shitroid frame.</summary>
    public DelayedMusicRequest? LastShitroidMusicRequest { get; private set; }

    /// <summary>Ports <c>Shitroid_Init</c> at <c>$A9:EF37</c>.</summary>
    internal void InitializeShitroid(RoomEnemySlot slot, int cameraX)
    {
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Shitroid requires native enemy slot zero, not slot {slot.SlotIndex}.");
        }
        if (Shitroid is not null)
            throw new InvalidDataException("A room population contains more than one Shitroid.");

        // The cutscene's scratch surface is the enemy BG2 staging image Mother Brain stages her body in; its
        // initialization clears all of it.
        _room.Staging.ClearEnemyBg2Tilemap();

        // `$2000` is the scheduler's instruction-processing bit; `$1000` blocks plasma
        // from piercing the actor. The upstream C symbol calls `$2000` DisableSamusColl,
        // but the executable scheduler also consumes that same bit for instructions, so
        // the translated property enum retains the directly observed scheduler meaning.
        slot.Properties = slot.Properties.With(
            EnemyProperties.BlocksPlasmaBeam | EnemyProperties.ProcessInstructions);
        slot.PaletteRow = 2;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

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
        state.ReactsToSamus = false;
        Shitroid = state;
        slot.SetSpeciesState(state, state.ClearWithRecord);

        CopyShitroidTargetColours(state, ShitroidColorTarget.Sidehopper);
        CopyShitroidTargetColours(state, ShitroidColorTarget.Shitroid);
        CopyShitroidTargetColours(state, ShitroidColorTarget.DeadSidehopper);
    }

    /// <summary>Copies an installed target row's colours into mutable encounter state and the visible boss colours.</summary>
    private void CopyShitroidTargetColours(
        ShitroidEnemyState state,
        ShitroidColorTarget target)
    {
        for (int color = 0; color < ShitroidColorDefinitions.TargetColorCount; color++)
        {
            RgbColor value = (_bossArtwork.Artwork?.ShitroidColors ?? throw new InvalidDataException(
                "Shitroid target palette requires installed artwork.")).TargetColor(target, color);
            state.TargetColours(target)[color] = value;

            // This runtime currently presents completed room fades directly in CGRAM.
            // Keep the separate target copy above, but install the same completed value so
            // the encounter does not wait on an untranslated global palette-fade owner.
            _presentation.BossColours().ShitroidTarget(target).Write(color, value);
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
        slot.Health = short.MaxValue;
        DispatchShitroidState(slot, state, samus, sharedProjectiles);
        DeadTorizoBehavior.MoveWithEightEightVelocity(slot, state.Velocity);
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
                state.StateTimer = WrappingCountdown.Start(464);
                goto case ShitroidAiFunction.EntranceDelay;

            case ShitroidAiFunction.EntranceDelay:
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    LastShitroidMusicRequest = new DelayedMusicRequest(
                        MusicRequest.SelectTrack(5),
                        MusicQueueDelay.Shortest);
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
                DeadSidehopperBehavior.ActivateDeadSidehopperVictim(RequireShitroidVictim(slot));
                state.VictimActivationFlag = true;
                state.Function = ShitroidAiFunction.RiseAfterFeeding;
                state.StateTimer = WrappingCountdown.Start(192);
                goto case ShitroidAiFunction.RiseAfterFeeding;

            case ShitroidAiFunction.RiseAfterFeeding:
                GraduallyAccelerateShitroid(slot, state, 0, new(slot.Position.X.Whole, 104), 1024);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Function = ShitroidAiFunction.HoverNearSamus;
                    state.ReactsToSamus = true;
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
                state.StateTimer = WrappingCountdown.Start(120);
                goto case ShitroidAiFunction.PostDrainPause;

            case ShitroidAiFunction.PostDrainPause:
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Function = ShitroidAiFunction.RiseAfterDrainingSamus;
                    state.StateTimer = WrappingCountdown.Start(192);
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
                    state.StateTimer = WrappingCountdown.Start(88);
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
                    state.StateTimer = WrappingCountdown.Start(88);
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
                    state.StateTimer = WrappingCountdown.Start(256);
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
                    state.ReactsToSamus = true;
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
                    $"Shitroid state {state.Function?.ToString() ?? "none"} is not translated.");
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
        state.CryEnabled = true;
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
        _frameResults.ShitroidPlms.Add(new PlmSpawnRequest(new(48, 3), header));
        _frameResults.ShitroidPlms.Add(new PlmSpawnRequest(new(31, 3), header));
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
        state.ReactsToSamus = false;
        state.StateTimer = WrappingCountdown.Start(320);
    }

    /// <summary>Shakes above the victim for 320 ticks before activating its corpse sequence.</summary>
    private void DrainShitroidVictim(RoomEnemySlot slot, ShitroidEnemyState state)
    {
        RoomEnemySlot victim = RequireShitroidVictim(slot);
        int shakeIndex = (slot.FrameCounter & 6) >> 1;
        slot.Position = slot.Position.WithWhole(
            victim.Position.Whole + ShitroidShakeOffsets[shakeIndex] + new Offset16<RoomPixels>(0, -32));
        CountdownStep timer = state.StateTimer.Decrement();
        state.StateTimer = timer.Remaining;
        if (timer.IsZero)
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

        GraduallyAccelerateShitroid(
            slot,
            state,
            10,
            SelectShitroidHoverTarget(state, samus.Position.Whole),
            1024);
    }

    /// <summary>Alternates between a timed followed point and the followed X at authored height 80, using the room RNG.</summary>
    private Position16<RoomPixels> SelectShitroidHoverTarget(ShitroidEnemyState state, Position16<RoomPixels> followed)
    {
        if (!state.HoverTimer.IsZero)
        {
            state.HoverTimer = state.HoverTimer.Decrement().Remaining;
            return followed;
        }
        if ((_room.Random.Read() & 4095) >= 4064)
            state.HoverTimer = WrappingCountdown.Start(32);
        return followed.WithY(80);
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
            state.CryEnabled = false;
            LastShitroidMusicRequest = new DelayedMusicRequest(
                MusicRequest.SelectTrack(7),
                MusicQueueDelay.Shortest);
            return;
        }

        sharedProjectiles?.SetSharedCooldown(8);
        sharedProjectiles?.SetSharedBombCounter(5);
        samus.XSpeedDivisor = 2;
        // CMP #4 ($A9:F238) leaves carry = (Y speed >= 4, unsigned) through the clamp and
        // the shake-table loads; $A9:F24D adds Samus's X with no CLC, so that carry is one
        // extra pixel. BMI reads only the sign of the difference.
        int ySpeed = samus.Kinematics.YSpeed.Whole;
        int xCarry = ySpeed >= 4 ? 1 : 0;
        if (unchecked((short)unchecked((ushort)(ySpeed - 4))) >= 0)
            samus.Kinematics.YSpeed = samus.Kinematics.YSpeed.WithWhole(2);

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
        GraduallyAccelerateShitroid(
            slot,
            state,
            8,
            SelectShitroidHoverTarget(state, samus.Position.Whole - new Offset16<RoomPixels>(0, 18)),
            1024);
        return state.HorizontalLockAccumulator >= 1024 ||
            unchecked((short)(samus.Position.X.Whole - 128)) < 0;
    }

    /// <summary>Accumulates two lock ticks within a narrow X threshold and decays by one when separated.</summary>
    private static void UpdateShitroidHorizontalLock(
        ShitroidEnemyState state,
        int currentX,
        int targetX,
        int threshold)
    {
        int distance = Math.Abs(unchecked((short)(currentX - targetX)));
        if (distance < threshold)
        {
            state.HorizontalLockAccumulator += 2;
        }
        else if (state.HorizontalLockAccumulator != 0)
        {
            state.HorizontalLockAccumulator--;
        }
    }

    /// <summary>Uses the bank-$A9 divisor table to steer both axes, with an offscreen horizontal reversal kick.</summary>
    private void GraduallyAccelerateShitroid(
        RoomEnemySlot slot,
        ShitroidEnemyState state,
        int divisorTableIndex,
        Position16<RoomPixels> target,
        int offscreenReversalKick)
    {
        if ((uint)divisorTableIndex >= 16)
            throw new ArgumentOutOfRangeException(nameof(divisorTableIndex));
        // GradualAccelerationDivisorTable ($A9:F56A) holds $10 down to $01.
        int divisor = 16 - divisorTableIndex;
        state.Velocity = state.Velocity.WithX(BabyMetroidGradualAcceleration.AccelerateHorizontally(
            slot.Position.X.Whole, target.X, state.Velocity.X, divisor, offscreenReversalKick,
            () => ShitroidIsOffScreen(slot)));
        state.Velocity = state.Velocity.WithY(BabyMetroidGradualAcceleration.AccelerateVertically(
            slot.Position.Y.Whole, target.Y, state.Velocity.Y, divisor));
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
        int acceleration,
        Position16<RoomPixels> target)
    {
        (bool xArrived, int xVelocity) = AccelerateShitroidAxis(
            state.Slot.Position.X.Whole,
            target.X,
            state.Velocity.X.RawValue,
            acceleration);
        state.Velocity = state.Velocity.WithX(new SignedEightEight(checked((ushort)xVelocity)));
        (bool yArrived, int yVelocity) = AccelerateShitroidAxis(
            state.Slot.Position.Y.Whole,
            target.Y,
            state.Velocity.Y.RawValue,
            acceleration);
        state.Velocity = state.Velocity.WithY(new SignedEightEight(checked((ushort)yVelocity)));
        return xArrived && yArrived;
    }

    /// <summary>Clamps signed 8.8 speed to 1280 and zeroes it on predicted target crossing.</summary>
    private static ShitroidAxisStep AccelerateShitroidAxis(
        int current,
        int target,
        int velocityWord,
        int acceleration)
    {
        int delta = unchecked((short)(current - target));
        if (delta == 0)
            return new ShitroidAxisStep(true, velocityWord);

        int velocity = unchecked((short)velocityWord);
        velocity = delta >= 0
            ? Math.Max(-1280, velocity - acceleration)
            : Math.Min(1280, velocity + acceleration);
        velocityWord = unchecked((ushort)velocity);

        int predicted = unchecked((short)(
            current + unchecked((sbyte)(EightEight.WholeOf(velocityWord))) - target));
        bool crossed = delta >= 0 ? predicted <= 0 : predicted >= 0;
        if (crossed)
            velocityWord = 0;
        return new ShitroidAxisStep(crossed, velocityWord);
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
        if (!state.ReactsToSamus)
            return;

        BreakReleasedShitroidFollow(state);
        if (SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.SpinJumping &&
            unchecked((short)(samus.Position.X.Whole - 512)) >= 0)
        {
            // The native expression `$80-angle+$80`, truncated to a byte, is simply the
            // negated source angle. Magnitude $40 is added to the current 8.8 velocity.
            RotationAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(new(
                unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)),
                unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))));
            RotationAngle angle = RotationAngle.NormalizeRaw(-sourceAngle.RawValue);
            state.Velocity += new EightEightVector<RoomPixels>(
                new SignedEightEight(EnemyMath.MultiplySineSample(64, angle.ToByteAngle())),
                new SignedEightEight(EnemyMath.MultiplySineSample(
                    64,
                    angle.AddRaw(RotationAngle.QuarterTurn.RawValue).ToByteAngle())));
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
                state.ReactsToSamus = false;
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
        int collisionDamage,
        SamusProjectileSlot slotZeroProjectile)
    {
        ShitroidEnemyState state = RequireShitroidState(slot);
        if (!state.ReactsToSamus)
            return;

        BreakReleasedShitroidFollow(state);
        RotationAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(new(
            unchecked((short)(slotZeroProjectile.Position.X.Whole - slot.Position.X.Whole)),
            unchecked((short)(slotZeroProjectile.Position.Y.Whole - slot.Position.Y.Whole))));
        RotationAngle angle = RotationAngle.NormalizeRaw(-sourceAngle.RawValue);
        // The recoil strength is eight times the damage, kept to sixteen bits as the damage word's
        // multiple is, and capped at 240.
        int magnitude = Math.Min((int)unchecked((ushort)(8 * collisionDamage)), 240);
        state.Velocity += new EightEightVector<RoomPixels>(
            new SignedEightEight(EnemyMath.MultiplySineSample((ushort)magnitude, angle.ToByteAngle())),
            new SignedEightEight(EnemyMath.MultiplySineSample(
                (ushort)magnitude,
                angle.AddRaw(RotationAngle.QuarterTurn.RawValue).ToByteAngle())));
    }

    /// <summary>Ports <c>Shitroid_Powerbomb</c> at <c>$A9:EFBA</c>.</summary>
    private void ResolveShitroidPowerBomb(RoomEnemySlot slot, SamusState? samus)
    {
        ShitroidEnemyState state = RequireShitroidState(slot);
        if (state.ReactsToSamus)
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
        if (state.PaletteTimer != 0)
        {
            state.PaletteTimer--;
            return;
        }

        int phase = (state.PalettePhase + 1) % 8;
        state.PalettePhase = phase;
        state.PaletteTimer = state.PaletteDelay;
        if (phase == 5 && state.CryEnabled)
        {
            state.CryCounter++;
            if (state.CryCounter >= 4)
            {
                state.CryCounter = 0;
                LastShitroidSoundEffectLibrary2 = state.PaletteDelay < 10
                    ? ShitroidSoundDefinitions.QuickDrainCry
                    : ShitroidSoundDefinitions.DrainCry;
            }
        }

        for (int color = 0; color < ShitroidColorDefinitions.NormalColorsPerFrame; color++)
        {
            RgbColor value = (_bossArtwork.Artwork?.ShitroidColors ?? throw new InvalidDataException(
                "Shitroid normal palette requires installed artwork.")).NormalColor(phase, color);
            _presentation.BossColours().ShitroidMembrane.Write(color, value);
        }
    }

    /// <summary>Starts <paramref name="program"/> with both instruction timers reset.</summary>
    private void SetShitroidProgram(RoomEnemySlot slot, ShitroidProgram program)
    {
        RequireShitroidState(slot).Cursor = AnimationProgramCursor<ShitroidProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Requires the Sidehopper victim in the physical enemy slot immediately after Shitroid.</summary>
    internal RoomEnemySlot RequireShitroidVictim(RoomEnemySlot slot)
    {
        int victimIndex = slot.SlotIndex + 1;
        if ((uint)victimIndex >= _slotTable.Slots.Count || _slotTable.Slots[victimIndex].DefinitionId == EnemyDefinitionId.None)
            throw new InvalidDataException("Shitroid's physical slot-one victim is missing.");
        return _slotTable.Slots[victimIndex];
    }

    /// <summary>Rejects missing state or dispatch from an enemy slot other than the initialized Baby Metroid.</summary>
    private ShitroidEnemyState RequireShitroidState(RoomEnemySlot slot)
    {
        ShitroidEnemyState state = Shitroid ??
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
        CountdownStep timer = state.StateTimer.Decrement();
        state.StateTimer = timer.Remaining;
        return timer.IsNegative;
    }

    /// <summary>Uses bomb damage for recoil strength but ordinary slot zero for its direction, without health loss.</summary>
    internal void AddShitroidBombReactions(EnemySharedReactions table)
    {
        // `$A9:F842` reads damage from the selected physical projectile slot, but always reads aim coordinates from
        // ordinary slot zero. For a bomb collision that means family-$0500 damage controls recoil strength while
        // whatever stale/live shot occupies slot zero controls direction. It never enters vulnerability damage.
        table.AddBombSpecial(c => c.Enemy.DefinitionId == EnemyDefinitionId.BabyMetroid &&
                                  c.SelectedShotAi == EnemyInteractionCallback.ShitroidShot,
            c => ResolveShitroidShot(c.Enemy, c.Bomb.Damage, c.OrdinaryProjectiles.Slots[0]));
    }

    /// <summary>Ports Shitroid's initialization AI.</summary>
    internal void RunShitroidInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeShitroid(slot, surroundings.Camera.X);
    }

    /// <summary>Ports Shitroid's main AI.</summary>
    internal void RunShitroidMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunShitroidMain(slot, frame.Samus, frame.Camera, frame.SharedProjectiles);
    }

    /// <summary>
    /// Runs the Shitroid's program where the enemy instruction interpreter would: the frame expires on the update its
    /// native timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvanceShitroidAnimation(RoomEnemySlot slot)
    {
        ShitroidEnemyState state = RequireShitroidState(slot);
        CountdownStep instructionTimer = slot.InstructionTimer.Decrement();
        slot.InstructionTimer = instructionTimer.Remaining;
        if (!instructionTimer.IsZero)
        {
            slot.ExtraProperties = slot.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        AnimationProgramCursor<ShitroidProgram> cursor = state.Cursor;
        WrappingCountdown counter = slot.Timer;
        AnimationProgramAdvance<BabyMetroidFrame> advance = ShitroidAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, continuation) => ApplyShitroidCue(cue));
        slot.Timer = counter;
        state.Cursor = cursor;
        if (advance is not AnimationProgramAdvance<BabyMetroidFrame>.Show shown)
            throw new InvalidOperationException($"The Shitroid's programs have no {advance} step.");
        slot.InstructionTimer = WrappingCountdown.FromRaw((ushort)shown.Duration);
        slot.Frame = BabyMetroidVisualDefinitions.ShownFrames.Of(shown.Frame);
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
    internal void AddShitroidPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.BabyMetroid &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.ShitroidPowerBomb)
        {
            ReplacesCommon = ResolveShitroidPowerBomb,
        });
    }

    /// <summary>Marks projectile collision and converts shot damage into recoil without vulnerability damage.</summary>
    internal void AddShitroidShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.BabyMetroid &&
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
    internal void AddShitroidTouch(EnemySharedReactions table)
    {
        // `$A9:F789` replaces normal touch damage entirely. Depending on its current cutscene state, contact either
        // starts pursuit/latching or adds a spin-jump recoil vector to the actor itself; Samus is never hurt here.
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.BabyMetroid &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.ShitroidTouch,
            context => ResolveShitroidTouch(context.Slot, context.Samus));
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastShitroidSoundEffectLibrary2 = null;
        LastShitroidMusicRequest = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastShitroidSoundEffectLibrary2 is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: SoundQueueLimit.Max6);
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(LastShitroidMusicRequest?.Request, LastShitroidMusicRequest?.Delay);
    }
}
