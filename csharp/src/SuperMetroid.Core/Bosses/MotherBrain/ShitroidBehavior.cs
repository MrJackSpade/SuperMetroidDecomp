using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby Metroid (Shitroid) species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ShitroidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.BabyMetroid];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new ShitroidEntity(this, place, common);

    /// <summary>Provides the room's enemy session, room scroll values, RNG, and installed encounter colors.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemies, among which the Baby Metroid's victim is placed directly after it (<see cref="BabyMetroidVictim"/>).</summary>
    private readonly EnemyPlaces _places;

    /// <summary>The fade step count every gradual fade shares, past whose start the membrane pulse holds.</summary>
    private readonly SharedFadeStepCount _fadeStepCount;
    /// <summary>Receives the Baby Metroid's cries and music changes.</summary>
    private readonly EnemyAudioQueue _audio;


    /// <summary>Connects the Tourian Baby Metroid to room, victim, colour, audio and Zebetite owners.</summary>
    internal ShitroidBehavior(EnemyRoomBinding room, EnemyPlaces places, SharedFadeStepCount fadeStepCount, BossFrameResultBuilder frameResults, EnemyAudioQueue audio)
    {
        _audio = audio;
        _frameResults = frameResults;
        _fadeStepCount = fadeStepCount;
        _room = room;
        _places = places;
    }

    /// <summary>The speed a spin-jumping Samus adds to the Shitroid's velocity along the negated bearing to her: a quarter pixel an update.</summary>
    private static readonly SignedEightEight ContactPushSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);

    /// <summary>The acceleration of the Shitroid's approach to a latch, on each axis.</summary>
    private static readonly SignedEightEight LatchAcceleration = SignedEightEight.FromPixels(2);

    /// <summary>The fastest the Shitroid approaches a latch, on each axis, in either direction.</summary>
    private static readonly SignedEightEight LatchMaximumSpeed = SignedEightEight.FromPixels(5);

    /// <summary>Queues one of the Baby Metroid's cries as it cries, after the update's earlier requests.</summary>
    private void RequestShitroidSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues the Baby Metroid's music change as it makes it.</summary>
    private void RequestShitroidMusic(MusicRequest request) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, MusicQueueDelay.Shortest));

    /// <summary>Sets the Shitroid up when the room spawns it.</summary>
    internal static void InitializeShitroid(RoomEnemyPlace place, int cameraX)
    {
        // The processing property is the scheduler's animation-processing flag; another flag blocks plasma
        // from piercing the actor. The game's name for the processing flag says it disables Samus
        // collision, but the scheduler also consumes that same flag for animation, so
        // the property enum retains the directly observed scheduler meaning.
        place.Contact = place.Contact.With(EnemyContact.BlocksPlasmaBeam);
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.AnimationClock.Restart();

        // A Shitroid entered with the camera left of the room's start lies dormant; any other waits for the camera.
        bool dormant = CoordinateAxis.AsSigned(cameraX) < 0;
        var state = new ShitroidEnemyState(dormant ? ShitroidAction.Dormant : ShitroidAction.WaitForCamera)
        {
            Playback = ShitroidAnimations.Starting(ShitroidSegment.Normal),
            Velocity = default,
            ColourDelay = 10,
        };
        if (dormant)
        {
            // The intangible property removes the actor from ordinary interaction and the hide-drawing property suppresses
            // drawing. Keep the former raw: although it is traditionally called "tangible", the
            // collision walkers explicitly reject actors that have it.
            place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
            place.Presence = place.Presence.With(EnemyPresence.Hidden);
        }
        state.ReactsToSamus = false;
        place.SetSpeciesState(state);
        // The entry's target colours are where the room's colours fade to; this runtime shows the completed fade: the
        // Shitroid and its victim in the entry's colours (#1512), and the drained corpses in theirs, which the corpses'
        // own initializers show.
        place.Shading = state.Shading;
    }

    /// <summary>
    /// The health the Shitroid's main AI restores every update, kept exact (#1627): a hit later in the update takes its
    /// damage from this count, and what remains is the health the rest of that update reads.
    /// </summary>
    private const int RestoredHealth = 32767;

    /// <summary>Runs the Shitroid's main AI for one update.</summary>
    /// <param name="place">The Shitroid's place.</param>
    /// <param name="samus">Samus, when the update has one.</param>
    /// <param name="camera">
    /// The camera's position as this step runs, which the wait for the camera and the off-screen test read: the enemy
    /// pass's camera for the main AI, the power-bomb pass's for the Power Bomb reaction.
    /// </param>
    /// <param name="sharedProjectiles">Samus's bomb and projectile counters, which draining Samus locks; none for the Power Bomb reaction.</param>
    internal void RunShitroidMain(
        RoomEnemyPlace place,
        SamusState samus,
        Position16<RoomPixels> camera,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        ShitroidEnemyState state = place.RequireSpeciesState<ShitroidEnemyState>();

        // Shitroid's own shot response applies recoil but no health loss. The main AI
        // redundantly restores its health every processed frame, exactly as the game does.
        place.Health = RestoredHealth;
        RunShitroidState(place, state, samus, camera, sharedProjectiles);
        place.Position = place.Position.MovedBy(state.Velocity);
        // The room's enemies run between a door fade's steps; the membrane pulse waits for the fade.
        if (!_fadeStepCount.IsPastStart)
            StepShitroidMembranePulse(place, state);
    }

    /// <summary>Advances entrance, feeding, Samus chase and drain, remorse, or exit cutscene states.</summary>
    private void RunShitroidState(
        RoomEnemyPlace place,
        ShitroidEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        switch (state.Action)
        {
            case ShitroidAction.Dormant:
                state.Velocity = default;
                return;

            case ShitroidAction.WaitForCamera:
                WaitForShitroidCamera(state, camera);
                return;

            case ShitroidAction.BeginEntranceDelay:
                state.Action = ShitroidAction.EntranceDelay;
                state.StateTimer = 464;
                goto case ShitroidAction.EntranceDelay;

            case ShitroidAction.EntranceDelay:
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    RequestShitroidMusic(MusicRequest.SelectTrack(MusicTrack.KraidAndPhantoonBattle));
                    state.Action = ShitroidAction.FlyToSidehopper;
                    goto case ShitroidAction.FlyToSidehopper;
                }
                return;

            case ShitroidAction.FlyToSidehopper:
                GraduallyAccelerateShitroid(place, state, ShitroidFlightDefinitions.RushDivisor, new(584, 74), camera);
                if (ShitroidOverlapsRectangle(place, new(584, 74), new(1, 1)))
                    state.Action = ShitroidAction.ChaseSidehopper;
                return;

            case ShitroidAction.ChaseSidehopper:
            {
                RoomEnemyPlace victim = RequireShitroidVictim(place);
                GraduallyAccelerateShitroid(
                    place,
                    state,
                    ShitroidFlightDefinitions.RushDivisor,
                    victim.Position.Whole + new Offset16<RoomPixels>(0, -32),
                    camera);
                if (ShitroidActorsOverlap(place, victim))
                    state.Action = ShitroidAction.AttachToSidehopper;
                return;
            }

            case ShitroidAction.AttachToSidehopper:
                AttachShitroidToSidehopper(place, state);
                return;

            case ShitroidAction.DrainSidehopper:
                DrainShitroidVictim(place, state);
                return;

            case ShitroidAction.ActivateSidehopperCorpse:
                DeadSidehopperBehavior.ActivateDeadSidehopperVictim(RequireShitroidVictim(place));
                state.Action = ShitroidAction.RiseAfterFeeding;
                state.StateTimer = 192;
                goto case ShitroidAction.RiseAfterFeeding;

            case ShitroidAction.RiseAfterFeeding:
                GraduallyAccelerateShitroid(place, state, ShitroidFlightDefinitions.DriftDivisor, new(place.Position.X.Whole, 104), camera);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Action = ShitroidAction.HoverNearSamus;
                    state.ReactsToSamus = true;
                    SetShitroidScrollPair(RoomScrollState.OpenAlignedToTop);
                    QueueShitroidWallObjects(ShitroidWallMutation.Clear);
                }
                return;

            case ShitroidAction.HoverNearSamus:
                HoverShitroidNearSamus(place, state, samus, camera);
                return;

            case ShitroidAction.ChaseSamus:
            {
                SamusState target = samus;
                GraduallyAccelerateShitroid(
                    place,
                    state,
                    ShitroidFlightDefinitions.RushDivisor,
                    target.Position.Whole + new Offset16<RoomPixels>(0, -32),
                    camera);
                return;
            }

            case ShitroidAction.BeginDrainingSamus:
                samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.SpeedBoostNext;
                state.Action = ShitroidAction.DrainSamus;
                goto case ShitroidAction.DrainSamus;

            case ShitroidAction.DrainSamus:
                DrainSamusWithShitroid(
                    place,
                    state,
                    samus,
                    sharedProjectiles);
                return;

            case ShitroidAction.BeginPostDrainPause:
                state.Action = ShitroidAction.PostDrainPause;
                state.StateTimer = 120;
                goto case ShitroidAction.PostDrainPause;

            case ShitroidAction.PostDrainPause:
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    state.Action = ShitroidAction.RiseAfterDrainingSamus;
                    state.StateTimer = 192;
                    goto case ShitroidAction.RiseAfterDrainingSamus;
                }
                return;

            case ShitroidAction.RiseAfterDrainingSamus:
            {
                SamusState target = samus;
                GraduallyAccelerateShitroid(place, state, ShitroidFlightDefinitions.DriftDivisor, new(target.Position.X.Whole, 104), camera);
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    RequestShitroidSound(ShitroidSoundDefinitions.FlyAwayCry);
                    state.Action = ShitroidAction.FlyBesideSamus;
                    state.DepartureSide = HorizontalDirection.Left;
                    state.StateTimer = 88;
                    SetShitroidAnimation(place, ShitroidSegment.LatchedOn);
                    goto case ShitroidAction.FlyBesideSamus;
                }
                return;
            }

            case ShitroidAction.FlyBesideSamus:
            {
                SamusState target = samus;
                bool toHerLeft = state.DepartureSide == HorizontalDirection.Left;
                GraduallyAccelerateShitroid(
                    place,
                    state,
                    ShitroidFlightDefinitions.DriftDivisor,
                    toHerLeft
                        ? new(CoordinateAxis.Wrap(target.Position.X.Whole - 64), 100)
                        : new(CoordinateAxis.Wrap(target.Position.X.Whole + 96), 104),
                    camera);
                if (!PredecrementShitroidTimerBecameNegative(state))
                    return;
                // Her left first: that flight's end turns to her right on the same update.
                if (toHerLeft)
                {
                    state.DepartureSide = HorizontalDirection.Right;
                    state.StateTimer = 88;
                    goto case ShitroidAction.FlyBesideSamus;
                }
                state.Action = ShitroidAction.HoldSamusBeforeRelease;
                state.StateTimer = 256;
                SetShitroidAnimation(place, ShitroidSegment.Remorse);
                return;
            }

            case ShitroidAction.BeginExit:
                RequestShitroidSound(ShitroidSoundDefinitions.RemorseCry);
                state.Action = ShitroidAction.Exit;
                goto case ShitroidAction.Exit;

            case ShitroidAction.Exit:
                GraduallyAccelerateShitroid(place, state, ShitroidFlightDefinitions.DriftDivisor, Position16<RoomPixels>.Wrapped(-128, 64), camera);
                if (ShitroidOverlapsRectangle(place, Position16<RoomPixels>.Wrapped(-128, 64), new(8, 8)))
                {
                    state.Velocity = default;
                    // The leave step clears animation processing and invisibility as Shitroid
                    // reaches the off-screen destination, leaving a dormant non-drawn actor.
                    place.Processing = place.Processing.Without(EnemyProcessing.Animates);
                    place.Presence = place.Presence.Without(EnemyPresence.Hidden);
                    state.Action = ShitroidAction.Dormant;
                }
                return;

            case ShitroidAction.HoldSamusBeforeRelease:
            {
                SamusState target = samus;
                if (PredecrementShitroidTimerBecameNegative(state))
                {
                    target.Drained.Release(target);
                    state.ReactsToSamus = true;
                    state.Action = ShitroidAction.ReleasedFollow;
                    FollowReleasedSamus(place, state, target, camera);
                }
                else
                {
                    FollowReleasedSamus(place, state, target, camera);
                }
                return;
            }

            case ShitroidAction.ReleasedFollow:
                if (FollowReleasedSamus(place, state, samus, camera))
                    state.Action = ShitroidAction.BeginExit;
                return;

            default:
                throw new InvalidDataException(
                    $"Shitroid has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>
    /// Starts the entrance once <paramref name="camera"/>, the camera as the step runs, is left of column 513, locking
    /// scroll and creating invisible walls.
    /// </summary>
    private void WaitForShitroidCamera(ShitroidEnemyState state, Position16<RoomPixels> camera)
    {
        if (CoordinateAxis.Difference(camera.X, 513) >= 0)
            return;

        // Puts the camera at X 512 directly, then rescrolls the room's four screens as the wall entrance does: the second
        // and fourth are closed, the first and third keep the scroll they have. This is not ordinary camera
        // tracking.
        _frameResults.CameraXOverride = 512;
        SetShitroidScrollPair(RoomScrollState.Closed);
        QueueShitroidWallObjects(ShitroidWallMutation.Create);
        state.Action = ShitroidAction.BeginEntranceDelay;
        state.CryEnabled = true;
    }

    /// <summary>
    /// Gives the second and fourth screens of the Big Boy room's row of four <paramref name="highState"/>, rewriting the
    /// first and third with the scroll they already have.
    /// </summary>
    private void SetShitroidScrollPair(RoomScrollState highState)
    {
        var first = new Position16<RoomScreens>(0, 0);
        var third = new Position16<RoomScreens>(2, 0);
        RoomScrollState firstState = _room.Services.RoomScroll.Read(first);
        RoomScrollState thirdState = _room.Services.RoomScroll.Read(third);
        _room.Services.RoomScroll.Write(first, firstState);
        _room.Services.RoomScroll.Write(new Position16<RoomScreens>(1, 0), highState);
        _room.Services.RoomScroll.Write(third, thirdState);
        _room.Services.RoomScroll.Write(new Position16<RoomScreens>(3, 0), highState);
    }

    /// <summary>Requests the two room-column wall objects at the authored Tourian positions.</summary>
    private void QueueShitroidWallObjects(ShitroidWallMutation mutation)
    {
        _frameResults.ShitroidObjects.Add(new RoomObjectSpawnRequest<ShitroidWallMutation>(new(48, 3), mutation));
        _frameResults.ShitroidObjects.Add(new RoomObjectSpawnRequest<ShitroidWallMutation>(new(31, 3), mutation));
    }

    /// <summary>Accelerates to the Sidehopper's head and enters the latched feeding animation on arrival.</summary>
    private void AttachShitroidToSidehopper(RoomEnemyPlace place, ShitroidEnemyState state)
    {
        RoomEnemyPlace victim = RequireShitroidVictim(place);
        Position16<RoomPixels> latch = victim.Position.Whole + new Offset16<RoomPixels>(0, -32);
        if (!AccelerateShitroidExactly(place, state, LatchAcceleration, latch))
            return;

        state.Velocity = default;
        place.Position = place.Position.WithWhole(latch);
        SetShitroidAnimation(place, ShitroidSegment.LatchedOn);
        state.Action = ShitroidAction.DrainSidehopper;
        state.ColourDelay = 1;
        state.ReactsToSamus = false;
        state.StateTimer = 320;
    }

    /// <summary>Shakes above the victim for 320 ticks before activating its corpse sequence.</summary>
    private void DrainShitroidVictim(RoomEnemyPlace place, ShitroidEnemyState state)
    {
        RoomEnemyPlace victim = RequireShitroidVictim(place);
        int shakeIndex = MotherBrainCadenceDefinitions.ShitroidShakeStep.Of(place.FrameCounter);
        place.Position = place.Position.WithWhole(
            victim.Position.Whole + MotherBrainShakeDefinitions.Offsets[shakeIndex] + new Offset16<RoomPixels>(0, -32));
        if (--state.StateTimer == 0)
        {
            state.Action = ShitroidAction.ActivateSidehopperCorpse;
            SetShitroidAnimation(place, ShitroidSegment.FinishDraining);
            state.ColourDelay = 10;
        }
    }

    /// <summary>Steers toward Samus with intermittent high hovering until horizontal lock enters pursuit.</summary>
    private void HoverShitroidNearSamus(
        RoomEnemyPlace place,
        ShitroidEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        UpdateShitroidHorizontalLock(state, place.Position.X.Whole, samus.Position.X.Whole, threshold: 8);
        if (state.HorizontalLockAccumulator >= 256 ||
            CoordinateAxis.Difference(samus.Position.X.Whole, 512) < 0)
        {
            state.Action = ShitroidAction.ChaseSamus;
            return;
        }

        GraduallyAccelerateShitroid(
            place,
            state,
            ShitroidFlightDefinitions.StareDownDivisor,
            SelectShitroidHoverTarget(state, samus.Position.Whole),
            camera);
    }

    /// <summary>Alternates between a timed followed point and the followed X at authored height 80, using the room RNG.</summary>
    private Position16<RoomPixels> SelectShitroidHoverTarget(ShitroidEnemyState state, Position16<RoomPixels> followed)
    {
        if (state.HoverTimer != 0)
        {
            state.HoverTimer--;
            return followed;
        }
        if (MotherBrainRandomDefinitions.ShitroidHoverRestarts.IsTakenBy(_room.Random.Read()))
            state.HoverTimer = 32;
        return followed.WithY(80);
    }

    /// <summary>Locks bombs and movement, shakes over Samus, and drains energy until the scripted release at one health.</summary>
    private void DrainSamusWithShitroid(
        RoomEnemyPlace place,
        ShitroidEnemyState state,
        SamusState samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        if (samus.Health < 2)
        {
            samus.XSpeedDivisor = 0;
            sharedProjectiles?.SetSharedBombCounter(0);
            state.Action = ShitroidAction.BeginPostDrainPause;
            state.Velocity = default;
            SetShitroidAnimation(place, ShitroidSegment.FinishDraining);
            state.ColourDelay = 10;
            samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.Inactive;
            samus.Drained.LetFall(samus);
            state.CryEnabled = false;
            RequestShitroidMusic(MusicRequest.SelectTrack(MusicTrack.SilentSong));
            return;
        }

        sharedProjectiles?.SetSharedCooldown(8);
        sharedProjectiles?.SetSharedBombCounter(5);
        samus.XSpeedDivisor = 2;
        // A speed slowed past zero at the apex reads as below four in the clamp's comparison, so it keeps its value
        // (#1562); the Shitroid sits a pixel right while the speed reads four or more (DrainingShitroidFallShift).
        int ySpeed = samus.Kinematics.YSpeed.Whole;
        int rightOfShake = DrainingShitroidFallShift.PixelsRightOfShake(ySpeed);
        if (SamusSpeedMagnitude.IsAtLeast(ySpeed, 4))
            samus.Kinematics.YSpeed = samus.Kinematics.YSpeed.WithWhole(2);

        int shakeIndex = MotherBrainCadenceDefinitions.ShitroidShakeStep.Of(place.FrameCounter);
        place.Position = place.Position.WithWhole(
            samus.Position.Whole + MotherBrainShakeDefinitions.Offsets[shakeIndex] + new Offset16<RoomPixels>(rightOfShake, -20));

        // The drain step subtracts four without Varia and two with it. The game's
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
        RoomEnemyPlace place,
        ShitroidEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        UpdateShitroidHorizontalLock(state, place.Position.X.Whole, samus.Position.X.Whole, threshold: 2);
        GraduallyAccelerateShitroid(
            place,
            state,
            ShitroidFlightDefinitions.RemorseDivisor,
            SelectShitroidHoverTarget(state, samus.Position.Whole - new Offset16<RoomPixels>(0, 18)),
            camera);
        return state.HorizontalLockAccumulator >= 1024 ||
            CoordinateAxis.Difference(samus.Position.X.Whole, 128) < 0;
    }

    /// <summary>Accumulates two lock ticks within a narrow X threshold and decays by one when separated.</summary>
    private static void UpdateShitroidHorizontalLock(
        ShitroidEnemyState state,
        int currentX,
        int targetX,
        int threshold)
    {
        int distance = CoordinateAxis.Distance(currentX, targetX);
        if (distance < threshold)
        {
            state.HorizontalLockAccumulator += 2;
        }
        else if (state.HorizontalLockAccumulator != 0)
        {
            state.HorizontalLockAccumulator--;
        }
    }

    /// <summary>
    /// Steers both axes toward <paramref name="target"/> with the pull <paramref name="divisor"/> names
    /// (<see cref="ShitroidFlightDefinitions"/>), with the off-screen horizontal reversal kick.
    /// </summary>
    /// <param name="place">The Shitroid's place.</param>
    /// <param name="state">The Shitroid's state, whose velocity changes.</param>
    /// <param name="divisor">The distance divisor, 1 through 16: a larger divisor is a gentler pull.</param>
    /// <param name="target">The point the Shitroid flies to.</param>
    /// <param name="camera">The camera's position as the step runs, against which the off-screen test places the Shitroid.</param>
    private static void GraduallyAccelerateShitroid(
        RoomEnemyPlace place,
        ShitroidEnemyState state,
        int divisor,
        Position16<RoomPixels> target,
        Position16<RoomPixels> camera)
    {
        state.Velocity = state.Velocity.WithX(BabyMetroidGradualAcceleration.AccelerateHorizontally(
            place.Position.X.Whole, target.X, state.Velocity.X, divisor, ShitroidFlightDefinitions.OffScreenReversalKick,
            () => ShitroidIsOffScreen(place, camera)));
        state.Velocity = state.Velocity.WithY(BabyMetroidGradualAcceleration.AccelerateVertically(
            place.Position.Y.Whole, target.Y, state.Velocity.Y, divisor));
    }

    /// <summary>Tests the Baby Metroid's asymmetric flight window against <paramref name="camera"/>, the camera as the step runs.</summary>
    private static bool ShitroidIsOffScreen(RoomEnemyPlace place, Position16<RoomPixels> camera)
    {
        if (CoordinateAxis.AsSigned(place.Position.Y.Whole) < 0)
            return true;
        int screenY = CoordinateAxis.AsSigned(place.Position.Y.Whole + 96 - camera.Y);
        if (screenY is < 0 or >= 416)
            return true;
        if (CoordinateAxis.AsSigned(place.Position.X.Whole) < 0)
            return true;
        int screenX = CoordinateAxis.AsSigned(place.Position.X.Whole + 16 - camera.X);
        return screenX is < 0 or >= 288;
    }

    /// <summary>Accelerates both axes toward a latch target and reports arrival only when both cross it.</summary>
    private static bool AccelerateShitroidExactly(
        RoomEnemyPlace place,
        ShitroidEnemyState state,
        SignedEightEight acceleration,
        Position16<RoomPixels> target)
    {
        (bool xArrived, SignedEightEight xVelocity) = AccelerateShitroidAxis(
            place.Position.X.Whole,
            target.X,
            state.Velocity.X,
            acceleration);
        state.Velocity = state.Velocity.WithX(xVelocity);
        (bool yArrived, SignedEightEight yVelocity) = AccelerateShitroidAxis(
            place.Position.Y.Whole,
            target.Y,
            state.Velocity.Y,
            acceleration);
        state.Velocity = state.Velocity.WithY(yVelocity);
        return xArrived && yArrived;
    }

    /// <summary>Clamps signed 1/256-pixel speed to 1280 and zeroes it on predicted target crossing.</summary>
    private static ShitroidAxisStep AccelerateShitroidAxis(
        int current,
        int target,
        SignedEightEight velocity,
        SignedEightEight acceleration)
    {
        int delta = CoordinateAxis.Difference(current, target);
        if (delta == 0)
            return new ShitroidAxisStep(true, velocity);

        // Every approach clamps the speed to five pixels an update and a hit's recoil adds less than one, so the step
        // stays far inside the velocity's range.
        SignedEightEight speed = delta >= 0
            ? SignedEightEight.Max(-LatchMaximumSpeed, velocity - acceleration)
            : SignedEightEight.Min(LatchMaximumSpeed, velocity + acceleration);
        int predicted = CoordinateAxis.Difference(current + speed.WholePixels, target);
        bool crossed = delta >= 0 ? predicted <= 0 : predicted >= 0;
        return new ShitroidAxisStep(crossed, crossed ? default : speed);
    }

    /// <summary>Tests inclusive-radius overlap between two enemy rectangles using signed coordinate deltas.</summary>
    private static bool ShitroidActorsOverlap(RoomEnemyPlace left, RoomEnemyPlace right) =>
        CoordinateAxis.Distance(right.Position.X.Whole, left.Position.X.Whole) <
            left.Radius.X + right.Radius.X + 1 &&
        CoordinateAxis.Distance(right.Position.Y.Whole, left.Position.Y.Whole) <
            left.Radius.Y + right.Radius.Y + 1;

    /// <summary>Tests Shitroid against a scripted rectangle without allocating another enemy place.</summary>
    private static bool ShitroidOverlapsRectangle(
        RoomEnemyPlace place,
        Position16<RoomPixels> center,
        Extent16<RoomPixels> radius) =>
        CoordinateAxis.Distance(center.X, place.Position.X.Whole) < place.Radius.X + radius.X + 1 &&
        CoordinateAxis.Distance(center.Y, place.Position.Y.Whole) < place.Radius.Y + radius.Y + 1;

    /// <summary>Resolves the Baby's private touch reaction.</summary>
    private static void ResolveShitroidTouch(RoomEnemyPlace place, SamusState samus)
    {
        ShitroidEnemyState state = place.RequireSpeciesState<ShitroidEnemyState>();
        if (!state.ReactsToSamus)
            return;

        BreakReleasedShitroidFollow(state);
        if (SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.SpinJumping &&
            CoordinateAxis.Difference(samus.Position.X.Whole, 512) >= 0)
        {
            // The push heading, a half turn less the source angle and then a half turn more, is simply the
            // negated source angle. Magnitude 64 is added to the current 1/256-pixel velocity.
            RotationAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(new(
                CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole),
                CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole)));
            RotationAngle angle = RotationAngle.Zero - sourceAngle;
            state.Velocity += new EightEightVector<RoomPixels>(
                EnemyMath.SineSpeed(ContactPushSpeed, angle.ToTrigonometryAngle()),
                EnemyMath.SineSpeed(
                    ContactPushSpeed,
                    (angle + RotationAngle.QuarterTurn).ToTrigonometryAngle()));
            return;
        }

        if (state.Action == ShitroidAction.ChaseSamus)
        {
            if (AccelerateShitroidExactly(
                place,
                state,
                LatchAcceleration,
                samus.Position.Whole + new Offset16<RoomPixels>(0, -32)))
            {
                SetShitroidAnimation(place, ShitroidSegment.LatchedOn);
                state.ColourDelay = 1;
                state.ReactsToSamus = false;
                state.Velocity = default;
                state.Action = ShitroidAction.BeginDrainingSamus;
            }
        }
        else if (state.Action == ShitroidAction.HoverNearSamus)
        {
            state.Action = ShitroidAction.ChaseSamus;
        }
    }

    /// <summary>Resolves the recoil-only shot reaction: the Baby recoils away from <paramref name="recoilOrigin"/>.</summary>
    private static void ResolveShitroidShot(
        RoomEnemyPlace place,
        int collisionDamage,
        SamusProjectile recoilOrigin)
    {
        ShitroidEnemyState state = place.RequireSpeciesState<ShitroidEnemyState>();
        if (!state.ReactsToSamus)
            return;

        BreakReleasedShitroidFollow(state);
        RotationAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(new(
            CoordinateAxis.Difference(recoilOrigin.Position.X.Whole, place.Position.X.Whole),
            CoordinateAxis.Difference(recoilOrigin.Position.Y.Whole, place.Position.Y.Whole)));
        RotationAngle angle = RotationAngle.Zero - sourceAngle;
        // The recoil strength is eight times the damage, capped at 240; no beam, missile or bomb deals the 8192
        // damage whose multiple would pass 65535.
        SignedEightEight magnitude = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(Math.Min(8 * collisionDamage, 240));
        state.Velocity += new EightEightVector<RoomPixels>(
            EnemyMath.SineSpeed(magnitude, angle.ToTrigonometryAngle()),
            EnemyMath.SineSpeed(
                magnitude,
                (angle + RotationAngle.QuarterTurn).ToTrigonometryAngle()));
    }

    /// <summary>Resolves a power bomb's hit on the Shitroid.</summary>
    private void ResolveShitroidPowerBomb(EnemyPowerBombContext context)
    {
        RoomEnemyPlace place = context.Enemy;
        ShitroidEnemyState state = place.RequireSpeciesState<ShitroidEnemyState>();
        if (state.ReactsToSamus)
            BreakReleasedShitroidFollow(state);

        // The power-bomb reaction goes straight on to run main AI. Preserve the extra state/movement/colour
        // tick rather than reducing the power bomb to a state flag. That step reads the camera as the power-bomb pass
        // sees it, after the enemy pass and before the main scrolling.
        RunShitroidMain(place, context.Samus, context.Camera, null);
    }

    /// <summary>Shots or contact break the released follow into the exit sequence.</summary>
    private static void BreakReleasedShitroidFollow(ShitroidEnemyState state)
    {
        if (state.Action == ShitroidAction.ReleasedFollow)
            state.Action = ShitroidAction.BeginExit;
    }

    /// <summary>
    /// Advances the eight-phase membrane colors of the Shitroid in <paramref name="place"/> and paces cries when the
    /// Zebetites' pulse allows it.
    /// </summary>
    internal void StepShitroidMembranePulse(RoomEnemyPlace place, ShitroidEnemyState state)
    {
        if (state.ColourTimer != 0)
        {
            state.ColourTimer--;
            return;
        }

        int phase = (state.MembranePulsePhase + 1) % 8;
        state.MembranePulsePhase = phase;
        state.ColourTimer = state.ColourDelay;
        if (phase == 5 && state.CryEnabled)
        {
            state.CryCounter++;
            if (state.CryCounter >= 4)
            {
                state.CryCounter = 0;
                RequestShitroidSound(state.ColourDelay < 10
                    ? ShitroidSoundDefinitions.QuickDrainCry
                    : ShitroidSoundDefinitions.DrainCry);
            }
        }

        state.MembraneShown = phase;
        place.Shading = state.Shading;
    }

    /// <summary>Starts <paramref name="segment"/> with both animation timers reset.</summary>
    private static void SetShitroidAnimation(RoomEnemyPlace place, ShitroidSegment segment)
    {
        place.RequireSpeciesState<ShitroidEnemyState>().Playback = ShitroidAnimations.Starting(segment);
        place.AnimationClock.Restart();
    }

    /// <summary>Requires the Baby Metroid's victim: the enemy placed directly after it (<see cref="BabyMetroidVictim"/>).</summary>
    internal RoomEnemyPlace RequireShitroidVictim(RoomEnemyPlace place)
    {
        RoomEnemyPlace victim = BabyMetroidVictim.Of(_places.InUpdateOrder, place.Ordinal);
        if (victim.IsFree)
            throw new InvalidDataException("The Baby Metroid's victim, the enemy placed after it, is missing.");
        return victim;
    }

    /// <summary>Counts the state timer down and reports whether it ran past zero.</summary>
    private static bool PredecrementShitroidTimerBecameNegative(ShitroidEnemyState state) => --state.StateTimer < 0;

    /// <summary>Uses bomb damage for recoil strength but Samus's first shot for its direction, without health loss.</summary>
    internal static void ReactToShitroidBomb(EnemyBombContext c) =>
        // The Baby's shot response reads damage from the projectile that hit it, but recoils from Samus's first shot
        // (MetroidRecoilOrigin). For a bomb collision that means the bomb's damage controls recoil strength while
        // whatever live or left-over shot is first controls direction. It never enters vulnerability damage.
        ResolveShitroidShot(c.Enemy, c.Bomb.Damage, MetroidRecoilOrigin.Of(c.OrdinaryProjectiles.Shots));

    /// <summary>Runs Shitroid's initialization AI.</summary>
    internal static void RunShitroidInitializationAi(RoomEnemyPlace place, EnemySpawnSurroundings surroundings)
    {
        InitializeShitroid(place, surroundings.Camera.X);
    }

    /// <summary>Runs Shitroid's main AI.</summary>
    internal void RunShitroidMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunShitroidMain(place, frame.Samus, frame.Camera, frame.SharedProjectiles);
    }

    /// <summary>
    /// Plays the Shitroid's animation where the shared enemy animation runs: the frame expires on the update its
    /// timer would, and a newly shown frame raises the new-frame flag. Its animations only cycle.
    /// </summary>
    internal void AdvanceShitroidAnimation(RoomEnemyPlace place)
    {
        ShitroidEnemyState state = place.RequireSpeciesState<ShitroidEnemyState>();
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<ShitroidSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, ShitroidAnimations.Segments, ref playback, BabyMetroidVisualDefinitions.ShownFrames.Of, ApplyShitroidCue);
        state.Playback = playback;
    }

    /// <summary>Carries out one event of a Shitroid animation.</summary>
    /// <returns>The remorse animation from its start when the cry does not sound; otherwise null.</returns>
    private AnimationContinuation<ShitroidSegment>? ApplyShitroidCue(ShitroidCue cue)
    {
        switch (cue)
        {
            case ShitroidCue.MaybeRemorseCry:
                // The cue samples the existing random number; it does not generate a new one.
                if (BossRandomRolls.InLowerHalf(_room.Random.Read()))
                    return new(ShitroidAnimations.StartingAt[ShitroidSegment.Remorse], AnimationPlace.StartOfAnimation);
                RequestShitroidSound(ShitroidSoundDefinitions.RemorseCry);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(ShitroidCue)} {cue}.");
        }
    }

    /// <summary>Runs the private power-bomb reaction in place of common enemy damage.</summary>
    internal EnemyPowerBombReaction ShitroidPowerBombReaction => field ??= new()
    {
        ReplacesCommon = ResolveShitroidPowerBomb,
    };

    /// <summary>Marks projectile collision and converts shot damage into recoil without vulnerability damage.</summary>
    internal EnemyShotReaction ShitroidShotReaction => field ??= new()
    {
        // Like Dead Torizo, Shitroid receives the projectile prelude but never creates an impact or
        // enters vulnerability damage. Its own reaction converts shot strength and the first shot's aim into recoil.
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
            ResolveShitroidShot(c.Enemy, c.Projectile.Damage, MetroidRecoilOrigin.Of(c.Projectiles.Shots));
            return true;
        },
    };

    /// <summary>Uses cutscene latching or recoil on contact instead of ordinary Samus damage.</summary>
    internal static void ReactToShitroidTouch(EnemyTouchContext context) =>
        // The Shitroid's touch replaces normal touch damage entirely. Depending on its current cutscene state, contact either
        // starts pursuit/latching or adds a spin-jump recoil vector to the actor itself; Samus is never hurt here.
        ResolveShitroidTouch(context.Place, context.Samus);
}
