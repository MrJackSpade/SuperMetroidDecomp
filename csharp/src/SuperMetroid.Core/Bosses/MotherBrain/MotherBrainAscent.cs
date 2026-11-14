using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain rising into her second form: its graphics, brain setup, the rise and the menacing head shake.</summary>
[RoomLifetime]
internal sealed class MotherBrainAscent
{
    /// <summary>Provides room services, pictures, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Draws encounter artwork and applies installed palette data.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies Samus and camera state for the current enemy frame.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainAscent(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, EnemyPresentation presentation, EnemyFrameContext frameContext, MotherBrainEncounter motherBrainEncounter)
    {
        _room = room;
        _projectiles = projectiles;
        _earthquake = earthquake;
        _presentation = presentation;
        _frameContext = frameContext;
        _motherBrainEncounter = motherBrainEncounter;
    }

    /// <summary>Reveals her body and requests her first artwork.</summary>
    internal void SetupMotherBrainPhaseTwoGraphics(MotherBrainEnemyState state)
    {
        // The ascent installs the attack and back-leg palettes' colours, all but the backdrop colour,
        // which belongs to the room and must remain untouched.
        state.Colours.ShowPhaseTwo();
        state.Phase = MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Brain;
        // The transition reveals her body, its frame as her body last showed it, replacing the room's pipe background
        // before the articulated body becomes visible (#1356, #1466, #1515).
        _presentation.RequireBossBody().Reveal();
    }

    /// <summary>Attaches the phase-two head and prepares its first animation and position.</summary>
    internal static void SetupMotherBrainPhaseTwoBrain(
        MotherBrainEnemyState state)
    {
        state.BrainAction = MotherBrainBrainAction.SetupBrainAndNeckToBeDrawn;

        // The body and brain both become tangible before the suspense pause, but remain invisible for
        // their separate reasons: the body waits for its leg pictures, while the head's hide-drawing
        // bit deliberately selects her brain and articulated neck's own draw.
        state.Body.Contact = state.Body.Contact.Without(EnemyContact.IgnoresSamus);
        state.Head.Contact = state.Head.Contact.Without(EnemyContact.IgnoresSamus);
        state.Head.Health = 18000;
        state.Phase = MotherBrainBodyPhase.FakeDeathAscentPauseForSuspense;
        state.PhaseTimer = 128;

        // The setup falls directly into the suspense wait; the freshly written timer is observed as
        // 127 before another enemy slot can run.
        PauseBeforeMotherBrainRises(state);
    }

    /// <summary>Holds the phase-two body before its scripted ascent begins.</summary>
    internal static void PauseBeforeMotherBrainRises(
        MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.Phase = MotherBrainBodyPhase.FakeDeathAscentPrepareForRising;
        state.PhaseTimer = 32;
        PrepareMotherBrainForRising(state);
    }

    /// <summary>Stages the body and head state used by the rise from the collapsed room.</summary>
    internal static void PrepareMotherBrainForRising(
        MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        // The display hides the newly loaded leg sprites below a fixed line so they do not
        // appear over the floor. The renderer consumes this
        // typed lifetime rather than pretending it is a body-visibility flag.
        state.RisingFloorClipActive = true;
        state.Head.Presence = state.Head.Presence.With(EnemyPresence.Hidden);
        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.Initial);
        state.Phase = MotherBrainBodyPhase.FakeDeathAscentReadyStandingBody;
        state.PhaseTimer = 256;

        // The first readying update runs on the same frame the floor-clipping effect starts.
        ReadyMotherBrainStandingBody(state);
    }

    /// <summary>Runs one update of readying her standing body before the phase-two body becomes visible.</summary>
    internal static void ReadyMotherBrainStandingBody(
        MotherBrainEnemyState state)
    {
        if (!ReadyStandingBodyOnce(state))
        {
            return;
        }

        state.Phase = MotherBrainBodyPhase.FakeDeathAscentContinuePausing;

        // The final artwork step runs straight into the next step; the still-256 timer
        // loses its first tick on the final artwork frame.
        ContinueMotherBrainAscentPause(state);
    }

    /// <summary>
    /// Runs one update of readying her standing body, one per call, and reports whether it was the last of
    /// <see cref="MotherBrainReadyingDefinitions.StandingBodyDuration"/>: the last completes in the same call and starts
    /// the count over.
    /// </summary>
    internal static bool ReadyStandingBodyOnce(MotherBrainEnemyState state)
    {
        int elapsed = state.StandingBodyReadyingElapsed;
        if (elapsed + 1 == MotherBrainReadyingDefinitions.StandingBodyDuration)
        {
            state.StandingBodyReadyingElapsed = 0;
            return true;
        }

        state.StandingBodyReadyingElapsed = elapsed + 1;
        return false;
    }

    /// <summary>Where her crouched body's area lies in the room as her ascent begins, with her at (59, 279) (#1802).</summary>
    private static readonly Position16<RoomPixels> CrouchedBodyOrigin = new(27, 217);

    /// <summary>Consumes the pre-rise wait and advances to body ascent.</summary>
    internal static void ContinueMotherBrainAscentPause(MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.Body.Position = state.Body.Position.WithWhole(new(59, 279));
        MotherBrainBody.PlaceMotherBrainBody(state, CrouchedBodyOrigin);
        state.HitboxesEnabled = MotherBrainHitboxParts.Body | MotherBrainHitboxParts.Brain | MotherBrainHitboxParts.Neck;
        state.Phase = MotherBrainBodyPhase.FakeDeathAscentStartMusicAndEarthquake;
    }

    /// <summary>Switches the body into the rising state after its setup pause.</summary>
    internal void StartMotherBrainAscent(MotherBrainEnemyState state)
    {
        MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.Crouched);
        state.Head.AnimationClock.StepOnNextUpdate();
        state.Body.Presence = state.Body.Presence.Without(EnemyPresence.Hidden);
        state.Body.Position = state.Body.Position.WithWhole(new(59, 279));
        MotherBrainBody.PlaceMotherBrainBody(state, CrouchedBodyOrigin);
        _motherBrainEncounter.RequestMusic(MusicRequest.SelectTrack(MusicTrack.MotherBrainBattle), MusicQueueDelay.Shortest);
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Diagonal, EarthquakeReach.Foreground), 256);
        state.NeckAngleDelta = 80;
        state.NeckMovementEnabled = true;
        state.LowerNeckMovement = MotherBrainNeckMovement.Raise;
        state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
        state.Phase = MotherBrainBodyPhase.FakeDeathAscentRaiseMotherBrain;
    }

    /// <summary>Moves the body upward through the authored rise while updating its head attachment.</summary>
    internal void RaiseMotherBrain(MotherBrainEnemyState state)
    {
        // The rise gates on the frame counter, not the separate 256-step frame counter; the
        // two advance together but are not aligned, so their places in the cadence differ.
        if (!MotherBrainCadenceDefinitions.AscentStepEveryFourthUpdate.IsDue(_frameContext.FrameCounter))
            return;

        SpawnMotherBrainAscentDust(state);
        state.BodyOrigin += new Offset16<RoomPixels>(0, -2);
        state.Body.Position = state.Body.Position.WithWrappedWholeY(state.Body.Position.Y.Whole - 2);
        MotherBrainBody.PlaceMotherBrainBody(state, state.BodyOrigin);
        if (state.Body.Position.Y.Whole >= 189)
            return;

        state.Body.Position = state.Body.Position.WithWholeY(188);
        _earthquake.Stop();
        state.RisingFloorClipActive = false;
        MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.StandAfterCrouchSlow);
        state.Phase = MotherBrainBodyPhase.FakeDeathAscentWaitUntilUncrouched;
    }

    /// <summary>Emits dust at the feet as the body rises from the floor.</summary>
    private void SpawnMotherBrainAscentDust(MotherBrainEnemyState state)
    {
        if (--state.BodySubPhaseTimer < 0)
            state.BodySubPhaseTimer = 7;

        int random = _room.Random.Read();
        MiscDustAnimation animation = MotherBrainRandomDefinitions.AscentDustBig.IsSetIn(random) ? MiscDustAnimation.ShortBigDustCloud : MiscDustAnimation.SmallDustCloud;
        _projectiles.SpawnRoomGraphicsDustExplosion(
            MotherBrainAscentDustDefinitions.DustPositions[state.BodySubPhaseTimer], animation);
        _motherBrainEncounter.RequestBodySound(MotherBrainSoundDefinitions.AscentDust);
    }

    /// <summary>Holds the body phase until its slow uncrouch animation ends.</summary>
    internal static void WaitForMotherBrainToFinishUncrouching(MotherBrainEnemyState state)
    {
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;

        state.GrayTransitionCounter = 0;
        state.Phase = MotherBrainBodyPhase.FakeDeathAscentTransitionFromGray;
        state.PhaseTimer = 0;
    }

    /// <summary>Applies the phase-two head shake before normal combat begins.</summary>
    internal static void ShakeMotherBrainHeadMenacingly(
        MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.StretchingPhase2);
        state.Phase = MotherBrainBodyPhase.SecondPhaseStretchingBringHeadUp;
        state.NeckAngleDelta = 64;
        state.PhaseTimer = 256;
        BringMotherBrainHeadBackUp(state);
    }

    /// <summary>Returns the head from its lowered pose after the stretch.</summary>
    internal static void BringMotherBrainHeadBackUp(
        MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
        state.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
        state.Phase = MotherBrainBodyPhase.SecondPhaseStretchingFinish;
        state.PhaseTimer = 64;
        FinishMotherBrainStretching(state);
    }

    /// <summary>Switches the head to its neutral phase-two animation.</summary>
    internal static void FinishMotherBrainStretching(MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.SmallPurpleBreathGenerationEnabled = true;
        state.Phase = MotherBrainBodyPhase.SecondPhaseThinking;
    }
}
