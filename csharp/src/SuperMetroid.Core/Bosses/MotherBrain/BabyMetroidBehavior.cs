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

/// <summary>The Baby Metroid (Baby Metroid) species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BabyMetroidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyPassEvents _passEvents;

    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.BabyMetroid], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.FromSurroundings(new BabyMetroidEntity(this, occupant, place, common));

    /// <summary>Provides the room's enemy session, room scroll values, RNG, and installed encounter colors.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemies, among which the Baby Metroid's victim is placed directly after it (<see cref="BabyMetroidVictim"/>).</summary>
    private readonly EnemyPlaces _places;

    /// <summary>The fade step count every gradual fade shares, past whose start the membrane pulse holds.</summary>
    private readonly SharedFadeStepCount _fadeStepCount;
    /// <summary>Receives the Baby Metroid's cries and music changes.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects the Tourian Baby Metroid to room, victim, colour, audio and Zebetite owners.</summary>
    internal BabyMetroidBehavior(EnemyRoomBinding room, EnemyPlaces places, SharedFadeStepCount fadeStepCount, EnemyPassEvents passEvents, EnemyAudioQueue audio)
    {
        _audio = audio;
        _passEvents = passEvents;
        _fadeStepCount = fadeStepCount;
        _room = room;
        _places = places;
    }

    /// <summary>Queues one of the Baby Metroid's cries as it cries, after the update's earlier requests.</summary>
    private void RequestBabyMetroidSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues the Baby Metroid's music change as it makes it.</summary>
    private void RequestBabyMetroidMusic(MusicRequest request) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, MusicQueueDelay.Shortest));

    /// <summary>Sets the Baby Metroid up when the room spawns it.</summary>
    private static void InitializeBabyMetroid(RoomEnemyPlace place, int cameraX)
    {
        // The Baby Metroid stops the Plasma Beam and animates.
        place.Contact = place.Contact.With(EnemyContact.BlocksPlasmaBeam);
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.AnimationClock.Restart();

        // A Baby Metroid entered with the camera left of the room's start lies dormant; any other waits for the camera.
        bool dormant = CameraPastRoomEdge.LiesBeforeRoom(cameraX);
        var state = new BabyMetroidEnemyState(dormant ? BabyMetroidAction.Dormant : BabyMetroidAction.WaitForCamera, BabyMetroidSegment.Normal)
        {
            Velocity = default,
        };
        if (dormant)
        {
            // A dormant Baby Metroid neither touches Samus nor is drawn.
            place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
            place.Presence = place.Presence.With(EnemyPresence.Hidden);
        }
        state.ReactsToSamus = false;
        place.SetSpeciesState(state);
        // The entry's colours are where the room's colours fade to, and they show from the start: the Baby Metroid and
        // its victim in the entry's colours (#1512), and the drained corpses in theirs, which the corpses' own setup
        // shows.
        place.Shading = state.Shading;
    }

    /// <summary>Runs the Baby Metroid's main AI for one update.</summary>
    /// <param name="place">The Baby Metroid's place.</param>
    /// <param name="samus">Samus, when the update has one.</param>
    /// <param name="camera">The enemy pass's camera, which the wait for the camera and the off-screen test read.</param>
    /// <param name="samusBombs">Samus's bomb and projectile counters, which draining Samus locks.</param>
    private void RunBabyMetroidMain(
        RoomEnemyPlace place,
        SamusState samus,
        Position16<RoomPixels> camera,
        SamusBombProjectileSystem samusBombs)
    {
        Ensure.NotNull(samusBombs);
        BabyMetroidEnemyState state = place.RequireSpeciesState<BabyMetroidEnemyState>();
        RunBabyMetroidState(place, state, samus, camera, samusBombs);
        place.Position = place.Position.MovedBy(state.Velocity);
        // The room's enemies run between a door fade's steps; the membrane pulse waits for the fade.
        if (!_fadeStepCount.IsPastStart)
            StepBabyMetroidMembranePulse(place, state);
    }

    /// <summary>Advances entrance, feeding, Samus chase and drain, remorse, or exit cutscene states.</summary>
    private void RunBabyMetroidState(
        RoomEnemyPlace place,
        BabyMetroidEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera,
        SamusBombProjectileSystem samusBombs)
    {
        // Each case ends the update or goes on into the case it has just entered (#2553).
        while (true)
        {
            if (state.Action == BabyMetroidAction.Dormant)
            {
                state.Velocity = default;
                return;
            }
            else if (state.Action == BabyMetroidAction.WaitForCamera)
            {
                WaitForBabyMetroidCamera(state, camera);
                return;
            }
            else if (state.Action == BabyMetroidAction.BeginEntranceDelay)
            {
                state.Action = BabyMetroidAction.EntranceDelay;
                state.StateTimer.LastFor(BabyMetroidFightDefinitions.EntranceDelayUpdates);
                continue;
            }
            else if (state.Action == BabyMetroidAction.EntranceDelay)
            {
                if (state.StateTimer.StepRunsOut())
                {
                    RequestBabyMetroidMusic(MusicRequest.SelectTrack(MusicTrack.KraidAndPhantoonBattle));
                    state.Action = BabyMetroidAction.FlyToSidehopper;
                    continue;
                }
                return;
            }
            else if (state.Action == BabyMetroidAction.FlyToSidehopper)
            {
                GraduallyAccelerateBabyMetroid(place, state, BabyMetroidFlightDefinitions.RushDivisor, BabyMetroidFightDefinitions.SidehopperRendezvous, camera);
                if (BabyMetroidOverlapsRectangle(place, BabyMetroidFightDefinitions.SidehopperRendezvous, BabyMetroidFightDefinitions.RendezvousReach))
                    state.Action = BabyMetroidAction.ChaseSidehopper;
                return;
            }
            else if (state.Action == BabyMetroidAction.ChaseSidehopper)
            {
                RoomEnemyPlace victim = RequireBabyMetroidVictim(place);
                GraduallyAccelerateBabyMetroid(
                    place,
                    state,
                    BabyMetroidFlightDefinitions.RushDivisor,
                    victim.Position.Whole + BabyMetroidFightDefinitions.LatchOffset,
                    camera);
                if (BabyMetroidActorsOverlap(place, victim))
                    state.Action = BabyMetroidAction.AttachToSidehopper;
                return;
            }
            else if (state.Action == BabyMetroidAction.AttachToSidehopper)
            {
                AttachBabyMetroidToSidehopper(place, state);
                return;
            }
            else if (state.Action == BabyMetroidAction.DrainSidehopper)
            {
                DrainBabyMetroidVictim(place, state);
                return;
            }
            else if (state.Action == BabyMetroidAction.ActivateSidehopperCorpse)
            {
                DeadSidehopperBehavior.ActivateDeadSidehopperVictim(RequireBabyMetroidVictim(place));
                state.Action = BabyMetroidAction.RiseAfterFeeding;
                state.StateTimer.LastFor(BabyMetroidFightDefinitions.RiseUpdates);
                continue;
            }
            else if (state.Action == BabyMetroidAction.RiseAfterFeeding)
            {
                GraduallyAccelerateBabyMetroid(place, state, BabyMetroidFlightDefinitions.DriftDivisor, new(place.Position.X.Whole, BabyMetroidFightDefinitions.RiseY), camera);
                if (state.StateTimer.StepRunsOut())
                {
                    state.Action = BabyMetroidAction.HoverNearSamus;
                    state.ReactsToSamus = true;
                    SetBabyMetroidScrollPair(RoomScrollState.OpenAlignedToTop);
                    QueueBabyMetroidWallObjects(BabyMetroidWallMutation.Clear);
                }
                return;
            }
            else if (state.Action == BabyMetroidAction.HoverNearSamus)
            {
                HoverBabyMetroidNearSamus(place, state, samus, camera);
                return;
            }
            else if (state.Action == BabyMetroidAction.ChaseSamus)
            {
                GraduallyAccelerateBabyMetroid(
                    place,
                    state,
                    BabyMetroidFlightDefinitions.RushDivisor,
                    samus.Position.Whole + BabyMetroidFightDefinitions.LatchOffset,
                    camera);
                return;
            }
            else if (state.Action == BabyMetroidAction.BeginDrainingSamus)
            {
                samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.SpeedBoostNext;
                state.Action = BabyMetroidAction.DrainSamus;
                continue;
            }
            else if (state.Action == BabyMetroidAction.DrainSamus)
            {
                DrainSamusWithBabyMetroid(
                    place,
                    state,
                    samus,
                    samusBombs);
                return;
            }
            else if (state.Action == BabyMetroidAction.BeginPostDrainPause)
            {
                state.Action = BabyMetroidAction.PostDrainPause;
                state.StateTimer.LastFor(BabyMetroidFightDefinitions.PostDrainPauseUpdates);
                continue;
            }
            else if (state.Action == BabyMetroidAction.PostDrainPause)
            {
                if (state.StateTimer.StepRunsOut())
                {
                    state.Action = BabyMetroidAction.RiseAfterDrainingSamus;
                    state.StateTimer.LastFor(BabyMetroidFightDefinitions.RiseUpdates);
                    continue;
                }
                return;
            }
            else if (state.Action == BabyMetroidAction.RiseAfterDrainingSamus)
            {
                GraduallyAccelerateBabyMetroid(place, state, BabyMetroidFlightDefinitions.DriftDivisor, new(samus.Position.X.Whole, BabyMetroidFightDefinitions.RiseY), camera);
                if (state.StateTimer.StepRunsOut())
                {
                    RequestBabyMetroidSound(BabyMetroidSoundDefinitions.FlyAwayCry);
                    state.Action = BabyMetroidAction.FlyBesideSamus;
                    state.DepartureSide = HorizontalDirection.Left;
                    state.StateTimer.LastFor(BabyMetroidFightDefinitions.FlyBesideUpdates);
                    SetBabyMetroidAnimation(place, BabyMetroidSegment.LatchedOn);
                    continue;
                }
                return;
            }
            else if (state.Action == BabyMetroidAction.FlyBesideSamus)
            {
                bool toHerLeft = state.DepartureSide == HorizontalDirection.Left;
                GraduallyAccelerateBabyMetroid(
                    place,
                    state,
                    BabyMetroidFlightDefinitions.DriftDivisor,
                    // Samus can stand within 64 pixels of the room's left wall, so the place to her left can lie left of
                    // the room; the place to her right lies inside the room's four screens.
                    toHerLeft
                        ? new(OffRoomPlaces.Place(samus.Position.X.Whole + BabyMetroidFightDefinitions.BesideLeftX), BabyMetroidFightDefinitions.BesideLeftY)
                        : new(samus.Position.X.Whole + BabyMetroidFightDefinitions.BesideRightX, BabyMetroidFightDefinitions.BesideRightY),
                    camera);
                if (!state.StateTimer.StepRunsOut())
                    return;
                // Her left first: that flight's end turns to her right on the same update.
                if (toHerLeft)
                {
                    state.DepartureSide = HorizontalDirection.Right;
                    state.StateTimer.LastFor(BabyMetroidFightDefinitions.FlyBesideUpdates);
                    continue;
                }
                state.Action = BabyMetroidAction.HoldSamusBeforeRelease;
                state.StateTimer.LastFor(BabyMetroidFightDefinitions.HoldBeforeReleaseUpdates);
                SetBabyMetroidAnimation(place, BabyMetroidSegment.Remorse);
                return;
            }
            else if (state.Action == BabyMetroidAction.BeginExit)
            {
                RequestBabyMetroidSound(BabyMetroidSoundDefinitions.RemorseCry);
                state.Action = BabyMetroidAction.Exit;
                continue;
            }
            else if (state.Action == BabyMetroidAction.Exit)
            {
                GraduallyAccelerateBabyMetroid(place, state, BabyMetroidFlightDefinitions.DriftDivisor, BabyMetroidFightDefinitions.ExitPoint, camera);
                if (BabyMetroidOverlapsRectangle(place, BabyMetroidFightDefinitions.ExitPoint, BabyMetroidFightDefinitions.ExitReach))
                {
                    state.Velocity = default;
                    // The leave step clears animation processing and invisibility as Baby Metroid
                    // reaches the off-screen destination, leaving a dormant non-drawn actor.
                    place.Processing = place.Processing.Without(EnemyProcessing.Animates);
                    place.Presence = place.Presence.Without(EnemyPresence.Hidden);
                    state.Action = BabyMetroidAction.Dormant;
                }
                return;
            }
            else if (state.Action == BabyMetroidAction.HoldSamusBeforeRelease)
            {
                if (state.StateTimer.StepRunsOut())
                {
                    samus.Drained.Release(samus);
                    state.ReactsToSamus = true;
                    state.Action = BabyMetroidAction.ReleasedFollow;
                }
                FollowReleasedSamus(place, state, samus, camera);
                return;
            }
            else
            {
                // The remaining member, BabyMetroidAction.ReleasedFollow.
                if (FollowReleasedSamus(place, state, samus, camera))
                    state.Action = BabyMetroidAction.BeginExit;
                return;
            }
        }
    }

    /// <summary>
    /// Starts the entrance once <paramref name="camera"/>, the camera as the step runs, is at or left of its arena's edge, locking
    /// scroll and creating invisible walls.
    /// </summary>
    private void WaitForBabyMetroidCamera(BabyMetroidEnemyState state, Position16<RoomPixels> camera)
    {
        if (CameraPastRoomEdge.CameraPast(camera.X, BabyMetroidFightDefinitions.ArenaLeftX) > 0)
            return;

        // Puts the camera at its arena's edge directly, then rescrolls the room's four screens as the wall entrance does: the second
        // and fourth are closed, the first and third keep the scroll they have. This is not ordinary camera
        // tracking.
        _passEvents.Publish(new EnemyCameraXWrite(BabyMetroidFightDefinitions.ArenaLeftX));
        SetBabyMetroidScrollPair(RoomScrollState.Closed);
        QueueBabyMetroidWallObjects(BabyMetroidWallMutation.Create);
        state.Action = BabyMetroidAction.BeginEntranceDelay;
        state.CryEnabled = true;
    }

    /// <summary>
    /// Gives the second and fourth screens of the Big Boy room's row of four <paramref name="highState"/>; the first and
    /// third keep the scroll they have.
    /// </summary>
    private void SetBabyMetroidScrollPair(RoomScrollState highState)
    {
        foreach (Position16<RoomScreens> screen in BabyMetroidFightDefinitions.ClosingScreens)
            _room.Services.RoomScroll.Write(screen, highState);
    }

    /// <summary>Requests the two room-column wall objects at the authored Tourian positions.</summary>
    private void QueueBabyMetroidWallObjects(BabyMetroidWallMutation mutation)
    {
        foreach (Position16<RoomBlocks> wall in BabyMetroidFightDefinitions.WallBlocks)
            _passEvents.Publish(new BabyMetroidWallChange(new RoomObjectSpawnRequest<BabyMetroidWallMutation>(wall, mutation)));
    }

    /// <summary>Accelerates to the Sidehopper's head and enters the latched feeding animation on arrival.</summary>
    private void AttachBabyMetroidToSidehopper(RoomEnemyPlace place, BabyMetroidEnemyState state)
    {
        RoomEnemyPlace victim = RequireBabyMetroidVictim(place);
        Position16<RoomPixels> latch = victim.Position.Whole + BabyMetroidFightDefinitions.LatchOffset;
        if (!AccelerateBabyMetroidExactly(place, state, BabyMetroidFlightDefinitions.LatchAcceleration, latch))
            return;

        state.Velocity = default;
        place.Position = place.Position.WithWhole(latch);
        SetBabyMetroidAnimation(place, BabyMetroidSegment.LatchedOn);
        state.Action = BabyMetroidAction.DrainSidehopper;
        state.PaceMembrane(BabyMetroidMembranePace.Feeding);
        state.ReactsToSamus = false;
        state.SidehopperDrain.LastFor(BabyMetroidFightDefinitions.SidehopperDrainUpdates);
    }

    /// <summary>Shakes above the victim for its drain before activating its corpse sequence.</summary>
    private void DrainBabyMetroidVictim(RoomEnemyPlace place, BabyMetroidEnemyState state)
    {
        RoomEnemyPlace victim = RequireBabyMetroidVictim(place);
        place.Position = place.Position.WithWhole(victim.Position.Whole
            + MotherBrainShake.OffsetOn(MotherBrainCadenceDefinitions.BabyMetroidShakeStep, place.FrameCounter) + BabyMetroidFightDefinitions.LatchOffset);
        if (state.SidehopperDrain.StepRunsOut())
        {
            state.Action = BabyMetroidAction.ActivateSidehopperCorpse;
            SetBabyMetroidAnimation(place, BabyMetroidSegment.FinishDraining);
            state.PaceMembrane(BabyMetroidMembranePace.Calm);
        }
    }

    /// <summary>Steers toward Samus with intermittent high hovering until horizontal lock enters pursuit.</summary>
    private void HoverBabyMetroidNearSamus(
        RoomEnemyPlace place,
        BabyMetroidEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        UpdateBabyMetroidHorizontalLock(state, place.Position.X.Whole, samus.Position.X.Whole, threshold: BabyMetroidFightDefinitions.HoverLockReach);
        // Samus stays between the room's walls, so her X is compared with the arena's edge plainly.
        if (state.HorizontalLock.HasBuiltTo(BabyMetroidFightDefinitions.HoverLockToChase) ||
            samus.Position.X.Whole < BabyMetroidFightDefinitions.ArenaLeftX)
        {
            state.Action = BabyMetroidAction.ChaseSamus;
            return;
        }

        GraduallyAccelerateBabyMetroid(
            place,
            state,
            BabyMetroidFlightDefinitions.StareDownDivisor,
            SelectBabyMetroidHoverTarget(state, samus.Position.Whole),
            camera);
    }

    /// <summary>
    /// Steers at the followed point itself while it hovers at Samus's height, and otherwise at the followed X at its own
    /// hover height, where the room RNG may restart its hover at Samus's height from the next update.
    /// </summary>
    private Position16<RoomPixels> SelectBabyMetroidHoverTarget(BabyMetroidEnemyState state, Position16<RoomPixels> followed)
    {
        if (state.SamusHeightHover is { } hover)
        {
            if (hover.StepRunsOut())
                state.SamusHeightHover = null;
            return followed;
        }
        if (MotherBrainRandomDefinitions.BabyMetroidHoverRestarts.IsTakenBy(_room.Random.Read()))
            state.SamusHeightHover = BossCountdown.Lasting(BabyMetroidFightDefinitions.HoverUpdates);
        return followed.WithY(BabyMetroidFightDefinitions.HoverY);
    }

    /// <summary>Locks bombs and movement, shakes over Samus, and drains energy until the scripted release at one health.</summary>
    private void DrainSamusWithBabyMetroid(
        RoomEnemyPlace place,
        BabyMetroidEnemyState state,
        SamusState samus,
        SamusBombProjectileSystem samusBombs)
    {
        if (samus.Health < BabyMetroidFightDefinitions.DrainStopHealth)
        {
            samus.SlowdownLevel = 0;
            samusBombs.ReleaseCountAfterDrain();
            state.Action = BabyMetroidAction.BeginPostDrainPause;
            state.Velocity = default;
            SetBabyMetroidAnimation(place, BabyMetroidSegment.FinishDraining);
            state.PaceMembrane(BabyMetroidMembranePace.Calm);
            samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.Inactive;
            samus.Drained.LetFall(samus);
            state.CryEnabled = false;
            RequestBabyMetroidMusic(MusicRequest.SelectTrack(MusicTrack.SilentSong));
            return;
        }

        samusBombs.SetSharedCooldown(BabyMetroidFightDefinitions.DrainShotCooldown);
        samusBombs.HoldCountForDrain();
        samus.SlowdownLevel = BabyMetroidFightDefinitions.DrainXSpeedDivisor;
        // A speed slowed past zero at the apex reads as below four in the clamp's comparison, so it keeps its value
        // (#1562); the Baby Metroid sits a pixel right while the speed reads four or more (DrainingBabyMetroidFallShift).
        UnsignedSixteenSixteen fallSpeed = samus.Kinematics.VerticalVelocity.Speed;
        int rightOfShake = DrainingBabyMetroidFallShift.PixelsRightOfShake(fallSpeed.Whole);
        if (SamusSpeedMagnitude.WholePixelsReach(fallSpeed, BabyMetroidFightDefinitions.DrainFallSpeedLimit))
            samus.Kinematics.VerticalVelocity = samus.Kinematics.VerticalVelocity.WithSpeed(fallSpeed.WithWholePixelsOf(BabyMetroidFightDefinitions.DrainFallSpeed));

        place.Position = place.Position.WithWhole(samus.Position.Whole
            + MotherBrainShake.OffsetOn(MotherBrainCadenceDefinitions.BabyMetroidShakeStep, place.FrameCounter)
            + new Offset16<RoomPixels>(rightOfShake, BabyMetroidFightDefinitions.DrainAboveSamus));

        // The drain takes less energy with Varia than without, leaving Samus at least one.
        int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? BabyMetroidFightDefinitions.DrainWithVaria : BabyMetroidFightDefinitions.DrainWithoutVaria;
        samus.Health = damage >= samus.Health
            ? 1
            : samus.Health - damage;
    }

    /// <summary>Follows the recovering Samus until sustained horizontal lock or the left-screen exit condition.</summary>
    private bool FollowReleasedSamus(
        RoomEnemyPlace place,
        BabyMetroidEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        UpdateBabyMetroidHorizontalLock(state, place.Position.X.Whole, samus.Position.X.Whole, threshold: BabyMetroidFightDefinitions.FollowLockReach);
        GraduallyAccelerateBabyMetroid(
            place,
            state,
            BabyMetroidFlightDefinitions.RemorseDivisor,
            SelectBabyMetroidHoverTarget(state, samus.Position.Whole + BabyMetroidFightDefinitions.FollowOffset),
            camera);
        // Samus stays between the room's walls, so her X is compared with the leaving line plainly.
        return state.HorizontalLock.HasBuiltTo(BabyMetroidFightDefinitions.FollowLockToLeave) ||
            samus.Position.X.Whole < BabyMetroidFightDefinitions.FollowLeaveSamusX;
    }

    /// <summary>Builds the horizontal lock while the Baby Metroid keeps within <paramref name="threshold"/> of its target's X, and decays it otherwise.</summary>
    private static void UpdateBabyMetroidHorizontalLock(
        BabyMetroidEnemyState state,
        int currentX,
        int targetX,
        int threshold) =>
        state.HorizontalLock.Update(withinReach: DistanceAcrossRoomEdge.Between(currentX, targetX) < threshold);

    /// <summary>
    /// Steers both axes toward <paramref name="target"/> with the pull <paramref name="divisor"/> names
    /// (<see cref="BabyMetroidFlightDefinitions"/>), with the off-screen horizontal reversal kick.
    /// </summary>
    /// <param name="place">The Baby Metroid's place.</param>
    /// <param name="state">The Baby Metroid's state, whose velocity changes.</param>
    /// <param name="divisor">The distance divisor, 1 through 16: a larger divisor is a gentler pull.</param>
    /// <param name="target">The point the Baby Metroid flies to.</param>
    /// <param name="camera">The camera's position as the step runs, against which the off-screen test places the Baby Metroid.</param>
    private static void GraduallyAccelerateBabyMetroid(
        RoomEnemyPlace place,
        BabyMetroidEnemyState state,
        int divisor,
        Position16<RoomPixels> target,
        Position16<RoomPixels> camera)
    {
        state.Velocity = state.Velocity.WithX(BabyMetroidGradualAcceleration.AccelerateHorizontally(
            place.Position.X.Whole, target.X, state.Velocity.X, divisor, BabyMetroidFlightDefinitions.OffScreenReversalKick,
            () => BabyMetroidIsOffScreen(place, camera)));
        state.Velocity = state.Velocity.WithY(BabyMetroidGradualAcceleration.AccelerateVertically(
            place.Position.Y.Whole, target.Y, state.Velocity.Y, divisor));
    }

    /// <summary>Tests the Baby Metroid's asymmetric flight window against <paramref name="camera"/>, the camera as the step runs.</summary>
    private static bool BabyMetroidIsOffScreen(RoomEnemyPlace place, Position16<RoomPixels> camera)
    {
        if (OffRoomPlaces.LiesBeforeRoom(place.Position.Y.Whole))
            return true;
        int screenY = CameraPastRoomEdge.FromCamera(place.Position.Y.Whole + BabyMetroidFlightWindowDefinitions.VerticalMargin, camera.Y);
        if (screenY is < 0 or >= BabyMetroidFlightWindowDefinitions.Height)
            return true;
        if (OffRoomPlaces.LiesBeforeRoom(place.Position.X.Whole))
            return true;
        int screenX = CameraPastRoomEdge.FromCamera(place.Position.X.Whole + BabyMetroidFlightWindowDefinitions.HorizontalMargin, camera.X);
        return screenX is < 0 or >= BabyMetroidFlightWindowDefinitions.Width;
    }

    /// <summary>Accelerates both axes toward a latch target and reports arrival only when both cross it.</summary>
    private static bool AccelerateBabyMetroidExactly(
        RoomEnemyPlace place,
        BabyMetroidEnemyState state,
        SignedEightEight acceleration,
        Position16<RoomPixels> target)
    {
        (bool xArrived, SignedEightEight xVelocity) = AccelerateBabyMetroidAxis(
            place.Position.X.Whole,
            target.X,
            state.Velocity.X,
            acceleration);
        state.Velocity = state.Velocity.WithX(xVelocity);
        (bool yArrived, SignedEightEight yVelocity) = AccelerateBabyMetroidAxis(
            place.Position.Y.Whole,
            target.Y,
            state.Velocity.Y,
            acceleration);
        state.Velocity = state.Velocity.WithY(yVelocity);
        return xArrived && yArrived;
    }

    /// <summary>Caps one axis's speed at the latch's fastest speed and stops it where it would pass the target.</summary>
    private static BabyMetroidAxisStep AccelerateBabyMetroidAxis(
        int current,
        int target,
        SignedEightEight velocity,
        SignedEightEight acceleration)
    {
        int delta = DistanceAcrossRoomEdge.Apart(current, target);
        if (delta == 0)
            return new BabyMetroidAxisStep(true, velocity);

        // The speed is capped at BabyMetroidFlightDefinitions.LatchMaximumSpeed.
        SignedEightEight speed = delta >= 0
            ? SignedEightEight.Max(-BabyMetroidFlightDefinitions.LatchMaximumSpeed, velocity - acceleration)
            : SignedEightEight.Min(BabyMetroidFlightDefinitions.LatchMaximumSpeed, velocity + acceleration);
        int predicted = DistanceAcrossRoomEdge.Apart(current + speed.WholePixels, target);
        bool crossed = delta >= 0 ? predicted <= 0 : predicted >= 0;
        return new BabyMetroidAxisStep(crossed, crossed ? default : speed);
    }

    /// <summary>Tests inclusive-radius overlap between two enemy rectangles using signed coordinate deltas.</summary>
    private static bool BabyMetroidActorsOverlap(RoomEnemyPlace left, RoomEnemyPlace right) =>
        DistanceAcrossRoomEdge.Between(right.Position.X.Whole, left.Position.X.Whole) <
            left.Radius.X + right.Radius.X + 1 &&
        DistanceAcrossRoomEdge.Between(right.Position.Y.Whole, left.Position.Y.Whole) <
            left.Radius.Y + right.Radius.Y + 1;

    /// <summary>Tests Baby Metroid against a scripted rectangle without allocating another enemy place.</summary>
    private static bool BabyMetroidOverlapsRectangle(
        RoomEnemyPlace place,
        Position16<RoomPixels> center,
        Extent16<RoomPixels> radius) =>
        DistanceAcrossRoomEdge.Between(center.X, place.Position.X.Whole) < place.Radius.X + radius.X + 1 &&
        DistanceAcrossRoomEdge.Between(center.Y, place.Position.Y.Whole) < place.Radius.Y + radius.Y + 1;

    /// <summary>Resolves the Baby's private touch reaction.</summary>
    private static void ResolveBabyMetroidTouch(RoomEnemyPlace place, SamusState samus)
    {
        BabyMetroidEnemyState state = place.RequireSpeciesState<BabyMetroidEnemyState>();
        if (!state.ReactsToSamus)
            return;

        BreakReleasedBabyMetroidFollow(state);
        // Samus stays between the room's walls, so her X is compared with the arena's edge plainly.
        if (SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.SpinJumping &&
            samus.Position.X.Whole >= BabyMetroidFightDefinitions.ArenaLeftX)
        {
            // The push heads along the negated angle toward Samus and adds the contact push's speed to the
            // velocity each update.
            RotationAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(new(
                DistanceAcrossRoomEdge.Apart(samus.Position.X.Whole, place.Position.X.Whole),
                DistanceAcrossRoomEdge.Apart(samus.Position.Y.Whole, place.Position.Y.Whole)));
            RotationAngle angle = RotationAngle.Zero - sourceAngle;
            state.Velocity += new PixelVelocity<RoomPixels>(
                EnemyMath.SineSpeed(BabyMetroidFlightDefinitions.ContactPushSpeed, angle.ToTrigonometryAngle()),
                EnemyMath.SineSpeed(
                    BabyMetroidFlightDefinitions.ContactPushSpeed,
                    (angle + RotationAngle.QuarterTurn).ToTrigonometryAngle()));
            return;
        }

        if (state.Action == BabyMetroidAction.ChaseSamus)
        {
            if (AccelerateBabyMetroidExactly(
                place,
                state,
                BabyMetroidFlightDefinitions.LatchAcceleration,
                samus.Position.Whole + BabyMetroidFightDefinitions.LatchOffset))
            {
                SetBabyMetroidAnimation(place, BabyMetroidSegment.LatchedOn);
                state.PaceMembrane(BabyMetroidMembranePace.Feeding);
                state.ReactsToSamus = false;
                state.Velocity = default;
                state.Action = BabyMetroidAction.BeginDrainingSamus;
            }
        }
        else if (state.Action == BabyMetroidAction.HoverNearSamus)
        {
            state.Action = BabyMetroidAction.ChaseSamus;
        }
    }

    /// <summary>Resolves the recoil-only shot reaction: the Baby recoils away from <paramref name="recoilOrigin"/>.</summary>
    private static void ResolveBabyMetroidShot(
        RoomEnemyPlace place,
        int collisionDamage,
        SamusProjectile recoilOrigin)
    {
        BabyMetroidEnemyState state = place.RequireSpeciesState<BabyMetroidEnemyState>();
        if (!state.ReactsToSamus)
            return;

        BreakReleasedBabyMetroidFollow(state);
        RotationAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(new(
            DistanceAcrossRoomEdge.Apart(recoilOrigin.Position.X.Whole, place.Position.X.Whole),
            DistanceAcrossRoomEdge.Apart(recoilOrigin.Position.Y.Whole, place.Position.Y.Whole)));
        RotationAngle angle = RotationAngle.Zero - sourceAngle;
        // The recoil strength is a multiple of the damage, capped at its limit.
        SignedEightEight magnitude = SignedEightEight.Min(BabyMetroidFightDefinitions.RecoilPerDamage.Times(collisionDamage), BabyMetroidFightDefinitions.RecoilLimit);
        state.Velocity += new PixelVelocity<RoomPixels>(
            EnemyMath.SineSpeed(magnitude, angle.ToTrigonometryAngle()),
            EnemyMath.SineSpeed(
                magnitude,
                (angle + RotationAngle.QuarterTurn).ToTrigonometryAngle()));
    }

    /// <summary>Shots or contact break the released follow into the exit sequence.</summary>
    private static void BreakReleasedBabyMetroidFollow(BabyMetroidEnemyState state)
    {
        if (state.Action == BabyMetroidAction.ReleasedFollow)
            state.Action = BabyMetroidAction.BeginExit;
    }

    /// <summary>
    /// Plays one update of the eight-phase membrane pulse of the Baby Metroid in <paramref name="place"/>, which paces its cries,
    /// and shows the phase it moves on to.
    /// </summary>
    private void StepBabyMetroidMembranePulse(RoomEnemyPlace place, BabyMetroidEnemyState state)
    {
        if (state.Membrane.StepToNewFrame(_ => CountBabyMetroidCry(state)) is not { } pulse)
            return;

        state.MembraneShown = pulse;
        place.Shading = state.Shading;
    }

    /// <summary>
    /// Counts a pulse reaching its cry phase toward the Baby Metroid's periodic cry while its cry is enabled: one pulse in
    /// each run of <see cref="BabyMetroidFightDefinitions.PulsesPerCry"/> cries, quickly while it feeds.
    /// </summary>
    private void CountBabyMetroidCry(BabyMetroidEnemyState state)
    {
        if (!state.CryEnabled || !state.CryWait.StepRunsOut())
            return;
        state.CryWait.LastFor(BabyMetroidFightDefinitions.PulsesPerCry);
        RequestBabyMetroidSound(state.MembranePace == BabyMetroidMembranePace.Feeding
            ? BabyMetroidSoundDefinitions.QuickDrainCry
            : BabyMetroidSoundDefinitions.DrainCry);
    }

    /// <summary>Starts <paramref name="segment"/> with both animation timers reset.</summary>
    private static void SetBabyMetroidAnimation(RoomEnemyPlace place, BabyMetroidSegment segment)
    {
        place.RequireSpeciesState<BabyMetroidEnemyState>().Playback = BabyMetroidAnimations.Starting(segment);
        place.AnimationClock.Restart();
    }

    /// <summary>Requires the Baby Metroid's victim: the enemy placed directly after it (<see cref="BabyMetroidVictim"/>).</summary>
    private RoomEnemyPlace RequireBabyMetroidVictim(RoomEnemyPlace place)
    {
        RoomEnemyPlace victim = BabyMetroidVictim.Of(_places.InUpdateOrder, place);
        if (victim.IsFree)
            throw new InvalidDataException("The Baby Metroid's victim, the enemy placed after it, is missing.");
        return victim;
    }

    /// <summary>Uses bomb damage for recoil strength but Samus's first shot for its direction, without health loss.</summary>
    internal static void ReactToBabyMetroidBomb(EnemyBombContext c) =>
        // The Baby's shot response reads damage from the projectile that hit it, but recoils from Samus's first shot
        // (MetroidRecoilOrigin). For a bomb collision that means the bomb's damage controls recoil strength while
        // whatever live or left-over shot is first controls direction. It never enters vulnerability damage.
        ResolveBabyMetroidShot(c.Enemy, c.PlacedBomb.Damage, MetroidRecoilOrigin.Of(c.OrdinaryProjectiles.Shots));

    /// <summary>Runs Baby Metroid's initialization AI.</summary>
    internal static void SetUpBabyMetroid(RoomEnemyPlace place, EnemyPopulationSurroundings surroundings)
    {
        InitializeBabyMetroid(place, surroundings.Camera.X);
    }

    /// <summary>Runs Baby Metroid's main AI.</summary>
    internal void RunBabyMetroidMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunBabyMetroidMain(place, frame.Samus, frame.Camera, frame.SamusBombs);
    }

    /// <summary>
    /// Plays the Baby Metroid's animation where the shared enemy animation runs: the frame expires on the update its
    /// timer would, and the clock notes each newly shown frame. Its animations only cycle.
    /// </summary>
    internal void AdvanceBabyMetroidAnimation(RoomEnemyPlace place)
    {
        BabyMetroidEnemyState state = place.RequireSpeciesState<BabyMetroidEnemyState>();
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<BabyMetroidSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, BabyMetroidAnimations.Segments, ref playback, BabyMetroidVisualDefinitions.ShownFrames.Of, ApplyBabyMetroidCue);
        state.Playback = playback;
    }

    /// <summary>What <c>ApplyBabyMetroidCue</c> comes to for each <see cref="BabyMetroidCue"/> member, each member's own arm.</summary>
    private IdentityTable<BabyMetroidCue, Func<AnimationContinuation<BabyMetroidSegment>?>> ApplyBabyMetroidCueForBabyMetroidCue => field ??= new(new Dictionary<BabyMetroidCue, Func<AnimationContinuation<BabyMetroidSegment>?>>
    {
        [BabyMetroidCue.MaybeRemorseCry] = () =>
        {
            // The cue samples the existing random number; it does not generate a new one.
            if (BossRandomRolls.InLowerHalf(_room.Random.Read()))
                return new(BabyMetroidAnimations.StartingAt[BabyMetroidSegment.Remorse], AnimationPlace.StartOfAnimation);
            RequestBabyMetroidSound(BabyMetroidSoundDefinitions.RemorseCry);
            return null;
        },
    });

    /// <summary>Carries out one event of a Baby Metroid animation.</summary>
    /// <returns>The remorse animation from its start when the cry does not sound; otherwise null.</returns>
    private AnimationContinuation<BabyMetroidSegment>? ApplyBabyMetroidCue(BabyMetroidCue cue)
    {
        return ApplyBabyMetroidCueForBabyMetroidCue[cue]();
    }

    /// <summary>Marks projectile collision and converts shot damage into recoil without vulnerability damage.</summary>
    internal EnemyShotReaction BabyMetroidShotReaction => field ??= new()
    {
        // A shot marks its collision and the Baby recoils, without hurting it: the shot's strength and the first shot's
        // aim decide the recoil.
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            ResolveBabyMetroidShot(c.Enemy, c.Shot.Damage, MetroidRecoilOrigin.Of(c.Projectiles.Shots));
            return true;
        },
    };

    /// <summary>Uses cutscene latching or recoil on contact instead of ordinary Samus damage.</summary>
    internal static void ReactToBabyMetroidTouch(EnemyTouchContext context) =>
        // The Baby Metroid's touch replaces normal touch damage entirely. Depending on its current cutscene state, contact either
        // starts pursuit/latching or adds a spin-jump recoil vector to the actor itself; Samus is never hurt here.
        ResolveBabyMetroidTouch(context.Place, context.Samus);
}
