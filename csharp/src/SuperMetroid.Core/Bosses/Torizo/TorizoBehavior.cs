using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Torizo species owner, Bomb and Golden Torizo alike: their per-room state, behaviour and the interactions they register.</summary>
[RoomLifetime]
internal sealed class TorizoBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. TorizoPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new TorizoEntity(this, place, common);

    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the room's enemy projectiles, which Torizo's attacks spawn.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the statue-break sound with the enemy audio limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Exposes the two live colour sets occupied by Torizo's body.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Provides the frame counter used by Torizo animation choices.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns the orb drops and the death scatter after the authored sequences.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Bomb and Golden Torizo's body: its behaviour flags, colours, animation start and the low-health drool it triggers.</summary>
    internal TorizoBody Body { get; }

    /// <summary>Bomb and Golden Torizo's setup and movement: walking, jumps, falls and air transitions under gravity.</summary>
    internal TorizoMovement Movement { get; }

    /// <summary>Bomb and Golden Torizo's projectiles: drool, swipes, sonic booms, Chozo orbs, eggs, super missiles, eye beams and explosions.</summary>
    internal TorizoAttacks Attacks { get; }

    /// <summary>The branches Bomb and Golden Torizo's animations take on Samus's position, and their walk steps.</summary>
    internal TorizoChoices Choices { get; }

    /// <summary>Bomb and Golden Torizo's hitboxes, touch and hurt, and what Samus's weapons do to them.</summary>
    internal TorizoCombat Combat { get; }



    /// <summary>The room effects the bosses request this update, where the fight's start asks for the belly's glow.</summary>
    private readonly BossFrameResultBuilder _frameResults;

    /// <summary>Shows step <paramref name="step"/> of the Torizo's belly glow over the belly of the room's Torizo (#1512, #1597, #2030).</summary>
    /// <param name="places">The room's enemy places, one of which holds the Torizo.</param>
    /// <param name="step">The glow's step.</param>
    /// <exception cref="InvalidOperationException">The room holds no Torizo to glow, or more than one.</exception>
    internal static void ShowBellyGlow(IReadOnlyList<RoomEnemyPlace> places, ColourEffectFrame step)
    {
        RoomEnemyPlace? glowing = null;
        foreach (RoomEnemyPlace place in Ensure.NotNull(places))
        {
            if (place.SpeciesState<TorizoEnemyState>() is null)
                continue;
            if (glowing is not null)
                throw new InvalidOperationException("The Torizo belly glow shows a step while the room holds two Torizos.");
            glowing = place;
        }
        if (glowing is null)
            throw new InvalidOperationException("The Torizo belly glow shows a step while no Torizo is loaded.");
        TorizoEnemyState state = glowing.RequireSpeciesState<TorizoEnemyState>();
        state.Colours.ShowBellyGlow(step);
        glowing.Shading = state.Colours.Shading;
    }

    /// <summary>Connects the Bomb and Golden Torizo behaviours to their room-scoped services.</summary>
    internal TorizoBehavior(TorizoBehaviorDependencies dependencies)
    {
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _frameResults = dependencies.FrameResults;
        Attacks = new TorizoAttacks(_room, _projectiles, _audio, _deathsAndDrops);
        Body = new TorizoBody(_room, Attacks);
        Movement = new TorizoMovement(_room, dependencies.Terrain, dependencies.Earthquake, Body);
        Choices = new TorizoChoices(_room, dependencies.Terrain);
        Combat = new TorizoCombat(dependencies.Collision, Body);
    }
    /// <summary>Names the colouring the statue's fade runs toward (#1512).</summary>
    private static void SetTorizoFadeTarget(TorizoEnemyState state, TorizoLookKind look) =>
        state.Colours.FadeToward(new TorizoLook(look));

    /// <summary>Runs the main phase of Bomb or Golden Torizo for one update.</summary>
    private void RunTorizoMain(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        switch (state.Phase)
        {
            case TorizoPhase.Idle:
                return;

            case TorizoPhase.Falling:
                Body.MaybeSpawnBombTorizoLowHealthDrool(torizo);
                Movement.ApplyTorizoGravity(torizo, state, level);
                return;

            case TorizoPhase.WaitForHandTrigger:
                // Bomb Torizo stays dormant, solid, while its statue's hand still stands in the room.
                torizo.Contact = torizo.Contact.With(EnemyContact.SolidToSamus);
                if (_room.Services.RoomObjects.BombTorizoHandRemains)
                    return;

                RequestTorizoMusic(MusicRequest.SelectTrack(MusicTrack.TorizoAwakening));
                torizo.Contact = torizo.Contact.Without(EnemyContact.SolidToSamus);
                // The dormant statue holds until its hand is gone, then awakens. Bomb Torizo always sits facing left: the
                // original's facing-right statue, which no room starts, is not kept (#1941).
                TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoAnimation.BombTorizoAwakening));
                return;

            case TorizoPhase.Active:
                Movement.RunBombTorizoActiveState(torizo, state, level);
                return;

            case TorizoPhase.GoldenWaitForSamus:
                // The wake-up step advances the animation only after Samus has entered the
                // lower-right trigger rectangle. This is the authored statue wake-up gate.
                if (samus.Position.Y.Whole > TorizoDecisionDefinitions.GoldenWakeCorner.Y &&
                    samus.Position.X.Whole > TorizoDecisionDefinitions.GoldenWakeCorner.X)
                {
                    // The dormant statue holds until Samus steps in, then awakens.
                    TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoAnimation.GoldenTorizoAwakening));
                }
                return;

            case TorizoPhase.GoldenActive:
                Body.MaybeSpawnBombTorizoLowHealthDrool(torizo);
                Movement.MoveBody(torizo, state, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Torizo has no behaviour for phase {state.Phase}.");
        }
    }

    /// <summary>Counts consecutive Samus space-jump poses before running the shared statue step.</summary>
    private void RunGoldenTorizoMain(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // Golden Torizo's main behaviour counts consecutive space-jump poses for its anti-stall leap
        // decision, then runs the same main phases as Bomb Torizo.
        if (samus.Pose is SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose)
            state.CountSamusSpaceJumpFrame();
        else
            state.SamusSpaceJumpFrames = 0;
        RunTorizoMain(torizo, state, samus, level);
    }

    /// <summary>Records the area boss's defeat, scatters sixteen drops in the variant's arena, and requests post-fight music.</summary>
    private void FinishTorizoDeath(TorizoEnemyState state)
    {
        _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaTorizo);

        // The two statues roll one chance table over their own arenas.
        TorizoDropArena arena = state.IsGolden ? TorizoDeathDefinitions.GoldenTorizoArena : TorizoDeathDefinitions.BombTorizoArena;
        _deathsAndDrops.SpawnEnemyDropScatter(
            TorizoDeathDefinitions.DeathDropChances,
            count: TorizoDeathDefinitions.DeathDrops,
            xBase: arena.Corner.X,
            xScatter: arena.HorizontalScatter,
            yBase: arena.Corner.Y,
            yScatter: arena.VerticalScatter);
        RequestTorizoMusic(MusicRequest.SelectTrack(MusicTrack.Elevator));
    }

    /// <summary>Replaces Torizo's posture, keeping the rest of its state.</summary>
    private static void SetPosture(RoomEnemyPlace torizo, TorizoPostureFlags posture) =>
        torizo.RequireSpeciesState<TorizoEnemyState>().Posture = posture;

    /// <summary>Clears <paramref name="flags"/>.</summary>
    private static void ClearBehavior(RoomEnemyPlace torizo, TorizoBehaviorFlags flags) => torizo.RequireSpeciesState<TorizoEnemyState>().Behavior &= ~flags;

    /// <summary>Queues <paramref name="sound"/> through the shared enemy queue.</summary>
    private void PlayTorizoSound(SoundEffectId sound)
    {
        _audio.QueueEnemySound(sound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs Bomb Torizo's main AI.</summary>
    internal void RunBombTorizoMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunTorizoMain(place, place.RequireSpeciesState<TorizoEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Runs Golden Torizo's main AI.</summary>
    internal void RunGoldenTorizoMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunGoldenTorizoMain(
            place,
            place.RequireSpeciesState<TorizoEnemyState>(),
            frame.Samus,
            frame.Level);
    }

    /// <summary>Advances either Torizo's typed animation.</summary>
    internal void AdvanceTorizoTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context)
    {
        AdvanceTorizoAnimation(place, new TorizoCueInputs { Samus = context.Samus, Level = context.Level, ControllerInput = context.ControllerInput, FrameCounter = _frameContext.FrameCounter });
    }
    /// <summary>Runs Bomb or Golden Torizo's body animation where the shared enemy animation would.</summary>
    internal void AdvanceTorizoAnimation(RoomEnemyPlace torizo, TorizoCueInputs inputs)
    {
        if (!torizo.AnimationClock.Expire())
            return;
        TorizoEnemyState state = torizo.RequireSpeciesState<TorizoEnemyState>();
        AnimationPlayback<TorizoAnimation> playback = state.BodyPlayback;
        // The death and wake repeats count the body's frame timer, as the animations' counted loops do.
        EnemyAnimationPlayback.Play(torizo, TorizoAnimations.Segments, ref playback,
            (cue, continuation) => ApplyTorizoCue(torizo, state, inputs, cue, continuation));
        state.BodyPlayback = playback;
    }

    /// <summary>
    /// Shows artwork look <paramref name="look"/> on the Torizo at this update (#1406, #1538, #1761): from now on its frames
    /// show their finished pictures of that look.
    /// </summary>
    private void ShowTorizoArtwork(TorizoEnemyState state, TorizoArtworkLook look) =>
        _presentation.ShowLook(look.Of(state.IsGolden ? EncounterArtwork.GoldenTorizo : EncounterArtwork.BombTorizo));

    /// <summary>
    /// Carries out one cue of Bomb or Golden Torizo's body animation, returning where a decision, an interruption or the end
    /// of one continues the play, or null to play on. <paramref name="continuation"/> is where the play stands just past the
    /// cue: the place in the walk an interruption keeps (#2028). <see cref="TorizoCueInputs.FrameCounter"/> is the frame
    /// counter, which both attack choosers add; only whether it lies in the second half of each sixteen-frame cycle survives.
    /// </summary>
    internal AnimationContinuation<TorizoAnimation>? ApplyTorizoCue(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        TorizoCueInputs inputs,
        TorizoCue cue,
        AnimationPlayback<TorizoAnimation> continuation)
    {
        switch (cue)
        {
            case TorizoCue.Signal { Kind: TorizoCueKind.StartWalking }:
                state.Motion = TorizoBodyMotion.Walking;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.StartStanding }:
                state.Motion = TorizoBodyMotion.Standing;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.StartLeaping }:
                state.Motion = TorizoBodyMotion.Leaping;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.StopMoving }:
                state.Motion = null;
                return null;

            case TorizoCue.SetPhase setPhase:
                state.Phase = setPhase.Phase;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.BlowUpGut }:
                TorizoCombat.SetBehavior(torizo, TorizoBehaviorFlags.GutBlownUp);
                for (int piece = 0; piece < TorizoDeathDefinitions.GutDroolPieces; piece++)
                    Attacks.SpawnBombTorizoLowHealthDrool(torizo);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.MarkFaceBlownUp }:
                TorizoCombat.SetBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.MakeVisible }:
                torizo.Presence = torizo.Presence.Without(EnemyPresence.Hidden);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.MakeInvisible }:
                torizo.Presence = torizo.Presence.With(EnemyPresence.Hidden);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.FadeToBlack }:
                SetTorizoFadeTarget(state, TorizoLookKind.Black);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.FinishDeath }:
                FinishTorizoDeath(state);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.AdvanceColourFade }:
                state.Colours.Step(_presentation.FadeStepCount);
                torizo.Shading = state.Colours.Shading;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.FadeToNormalColors }:
                SetTorizoFadeTarget(state, TorizoLookKind.Normal);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.StartBombTorizoFight }:
                RequestTorizoMusic(MusicRequest.SelectTrack(MusicTrack.RidleyAndDraygonBattle));
                // The fight starts the belly's glow, which watches this Torizo and ends once it dies.
                _frameResults.TorizoBellyGlow = new(ColourEffectId.BombTorizoBelly, LitTorizo(torizo));
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.LockAnimation }:
                state.AnimationLocked = true;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.UnlockAnimation }:
                state.AnimationLocked = false;
                return null;

            case TorizoCue.Choice { Kind: TorizoChoiceKind.LandFacelessOrGolden } choice:
            {
                if (TorizoBody.HasBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp))
                    return TorizoAnimations.ContinueWith(choice.Target);
                // The Bomb variant runs on past both targets.
                return state.IsGolden ? TorizoAnimations.ContinueWith(choice.Alternate) : null;
            }

            case TorizoCue.LandIn landIn:
                state.Landing = landIn.Landing;
                return null;

            case TorizoCue.WalkOn walkOn:
                return TorizoAnimations.ContinueWith(walkOn.Walking);

            case TorizoCue.Turn turn:
                return TorizoAnimations.ContinueWith(TorizoFacing.FacesRight(torizo) ? turn.WhenFacingRight : turn.WhenFacingLeft);

            case TorizoCue.Signal { Kind: TorizoCueKind.ResumeWalk }:
                return TorizoAnimations.ContinueAt(state.InterruptedWalk
                    ?? throw new InvalidOperationException("A Torizo's attack resumes a walk no attack interrupted."));

            case TorizoCue.Signal { Kind: TorizoCueKind.ResumeInterrupted }:
                return TorizoAnimations.ContinueAt(state.InterruptedPlayback);

            case TorizoCue.LowHealthExplosions explosions:
                for (int explosion = 0; explosion < TorizoDeathDefinitions.LowHealthExplosions; explosion++)
                    Attacks.SpawnBombTorizoLowHealthExplosion(torizo, explosions.Site);
                torizo.FlashTimer = TorizoDeathDefinitions.LowHealthExplosionFlash;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnDeathExplosion }:
                Attacks.SpawnBombTorizoDeathExplosion(torizo);
                torizo.FlashTimer = TorizoDeathDefinitions.DeathExplosionFlash;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnLandingDust }:
                Attacks.SpawnBombTorizoLandingDust(torizo, foot: HorizontalDirection.Right);
                Attacks.SpawnBombTorizoLandingDust(torizo, foot: HorizontalDirection.Left);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnLowHealthDrool }:
                if (torizo.Health < TorizoDecisionDefinitions.GutBlowUpHealth)
                    Body.SpawnBombTorizoInitialDrool(torizo);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SetTurningAround }:
                SetPosture(torizo, TorizoFacing.Posture(torizo) | TorizoPostureFlags.TurningAround);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SteppedLeftWithLeftFoot }:
                SetPosture(torizo, TorizoPostureFlags.None);
                state.CountDecisionStep();
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SteppedRightWithRightFoot }:
                SetPosture(torizo, TorizoPostureFlags.FacingRight);
                state.CountDecisionStep();
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SteppedLeftWithRightFoot }:
                SetPosture(torizo, TorizoPostureFlags.OppositeLegForward);
                state.CountDecisionStep();
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SteppedRightWithLeftFoot }:
                SetPosture(torizo, TorizoPostureFlags.FacingRight | TorizoPostureFlags.OppositeLegForward);
                state.CountDecisionStep();
                return null;

            case TorizoCue.Posture { Kind: TorizoPostureKind.StandUp } posture:
                ApplyTorizoPosture(torizo, posture.Displacement, move: TorizoPostureMove.StandUp);
                return null;

            case TorizoCue.Posture { Kind: TorizoPostureKind.SitDown } posture:
                ApplyTorizoPosture(torizo, posture.Displacement, move: TorizoPostureMove.SitDown);
                return null;

            case TorizoCue.Walk { Kind: TorizoWalkKind.WalkBombTorizo } walk:
                return Choices.ProcessBombTorizoWalkAnimation(torizo, state, inputs.Samus, inputs.Level, new TorizoWalkStep { Velocity = walk.Velocity, CollisionFacingRight = TorizoAnimation.TurningLeft, CollisionFacingLeft = TorizoAnimation.TurningRight });

            case TorizoCue.Walk { Kind: TorizoWalkKind.WalkFacelessBombTorizo } walk:
                return Choices.ProcessBombTorizoWalkAnimation(torizo, state, inputs.Samus, inputs.Level, new TorizoWalkStep { Velocity = walk.Velocity, CollisionFacingRight = TorizoAnimation.FacelessTurningLeft, CollisionFacingLeft = TorizoAnimation.FacelessTurningRight });

            case TorizoCue.Replay { Kind: TorizoReplayKind.WhileRising }:
                return state.Velocity.Y.IsNegative ? TorizoAnimations.ReplayPhase(continuation) : null;

            case TorizoCue.Interrupt { Kind: TorizoInterruptKind.BombSwipeAtSamusBehind } interrupt:
                return TorizoChoices.SelectBombTorizoCloseBehindBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    continuation,
                    interrupt.Animation);

            case TorizoCue.Branch { Kind: TorizoBranchKind.BombJumpBackFromSamusInFront } branch:
                return TorizoChoices.SelectBombTorizoCloseFrontJump(
                    torizo,
                    state,
                    inputs.Samus,
                    branch.Target);

            case TorizoCue.Choice { Kind: TorizoChoiceKind.BombAttack } choice:
            {
                bool chooseFirst = inputs.Samus.Missiles < TorizoDecisionDefinitions.BombFirstAttackMissiles ||
                    TorizoChoices.InFirstAttackPhase(inputs.FrameCounter, inputs.Samus.Position.X.Whole);
                return TorizoAnimations.ContinueWith(chooseFirst ? choice.Target : choice.Alternate);
            }

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnBombTorizoChozoOrbs }:
                Attacks.SpawnBombTorizoChozoOrb(torizo);
                Attacks.SpawnBombTorizoChozoOrb(torizo);
                Attacks.SpawnBombTorizoChozoOrb(torizo);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnBombTorizoSonicBoom }:
                Attacks.SpawnBombTorizoSonicBoom(torizo);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnGoldenTorizoSonicBoom }:
                Attacks.SpawnGoldenTorizoSonicBoom(torizo);
                return null;

            case TorizoCue.Swipe swipe:
                Attacks.SpawnBombTorizoExplosiveSwipe(torizo, swipe.Hand);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.PlayShotSound }:
                PlayTorizoSound(EnemySound.ShotTorizo);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.PlayFootstepSound }:
                PlayTorizoSound(EnemySound.ChozoFootsteps);
                return null;

            case TorizoCue.Replay { Kind: TorizoReplayKind.WhileAirborne }:
                return torizo.Position.Y.Whole == TorizoDecisionDefinitions.GoldenFloorY ? null : TorizoAnimations.ReplayPhase(continuation);

            case TorizoCue.Signal { Kind: TorizoCueKind.FadeToGoldenColors }:
                SetTorizoFadeTarget(state, TorizoLookKind.Golden);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.StartGoldenTorizoFight }:
                RequestTorizoMusic(MusicRequest.SelectTrack(MusicTrack.RidleyAndDraygonBattle));
                torizo.Radius = TorizoDecisionDefinitions.GoldenFightRadius;
                _frameResults.TorizoBellyGlow = new(ColourEffectId.GoldenTorizoBelly, LitTorizo(torizo));
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.ClearCaughtSuperMissile }:
                ClearBehavior(torizo, TorizoBehaviorFlags.CaughtSuperMissile);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnEgg }:
                Attacks.SpawnGoldenTorizoEgg(torizo);
                return null;

            case TorizoCue.Replay { Kind: TorizoReplayKind.WhileEggsRemain }:
                return _projectiles.EnemyProjectiles.Any(projectile =>
                        projectile.Kind == RoomEnemyProjectileKind.GoldenTorizoEgg)
                    ? TorizoAnimations.ReplayPhase(continuation)
                    : null;

            case TorizoCue.Signal { Kind: TorizoCueKind.DisableEyeBeamExplosions }:
                state.EyeBeamExplosionsEnabled = false;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.EnableEyeBeamExplosions }:
                state.EyeBeamExplosionsEnabled = true;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.UnmarkStunned }:
                ClearBehavior(torizo, TorizoBehaviorFlags.Stunned);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.PlayEggReleasedSound }:
                PlayTorizoSound(EnemySound.CacatacSpikes);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.PlayLaserSound }:
                PlayTorizoSound(EnemySound.Laser);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.PlaySonicBoomSound }:
                PlayTorizoSound(EnemySound.TorizoSonicBoom);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnSuperMissile }:
                Attacks.SpawnGoldenTorizoSuperMissile(torizo);
                return null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.GoldenSitOnSamusMorphedBehind } branch:
                return TorizoChoices.SelectGoldenTorizoMorphBallBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    branch.Target);

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnEyeBeam }:
                Attacks.SpawnGoldenTorizoEyeBeam(torizo);
                return null;

            case TorizoCue.Interrupt { Kind: TorizoInterruptKind.GoldenEyeBeamAtMediumRange } interrupt:
                return Choices.SelectGoldenTorizoMediumRangeBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    continuation,
                    interrupt.Animation);

            case TorizoCue.Interrupt { Kind: TorizoInterruptKind.GoldenEggsAtLowHealth } interrupt:
                if (torizo.Health > TorizoDecisionDefinitions.GoldenEggsHealth || !TorizoRandomDefinitions.LowHealthEggs.IsTakenBy(_room.Random.Generate()))
                    return null;
                state.DecisionCounter = 0;
                state.InterruptedWalk = continuation;
                return TorizoAnimations.ContinueWith(interrupt.Animation);

            case TorizoCue.Interrupt { Kind: TorizoInterruptKind.GoldenStaggerWhenStunned } interrupt:
                if (torizo.Health <= TorizoDecisionDefinitions.GoldenStaggerHealth ||
                    !TorizoBody.HasBehavior(torizo, TorizoBehaviorFlags.Stunned))
                    return null;
                state.InterruptedWalk = continuation;
                return TorizoAnimations.ContinueWith(interrupt.Animation);

            case TorizoCue.Branch { Kind: TorizoBranchKind.GoldenLeapAtDistantSamus } branch:
                return Choices.SelectGoldenTorizoSpaceJumpCounter(
                    torizo,
                    state,
                    inputs.Samus,
                    inputs.ControllerInput,
                    branch.Target);

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnGoldenTorizoChozoOrbs }:
                Attacks.SpawnGoldenTorizoChozoOrb(torizo);
                return null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.GoldenJumpBackFromSamusInFront } branch:
                return TorizoChoices.SelectGoldenTorizoRepeatedJump(
                    torizo,
                    state,
                    inputs.Samus,
                    branch.Target);

            case TorizoCue.Choice { Kind: TorizoChoiceKind.GoldenAttack } choice:
            {
                SamusState activeSamus = inputs.Samus;
                bool chooseFirst = activeSamus.Missiles < TorizoDecisionDefinitions.GoldenFirstAttackMissiles ||
                    TorizoChoices.InFirstAttackPhase(inputs.FrameCounter, activeSamus.Position.X.Whole);
                return TorizoAnimations.ContinueWith(chooseFirst ? choice.Target : choice.Alternate);
            }

            case TorizoCue.Walk { Kind: TorizoWalkKind.WalkGoldenTorizo } walk:
                return Choices.ProcessGoldenTorizoWalkAnimation(
                    torizo,
                    state,
                    inputs.Samus,
                    inputs.Level,
                    walk.Velocity);

            case TorizoCue.ShowArtwork showArtwork:
                ShowTorizoArtwork(state, showArtwork.Look);
                return null;

            default:
                throw new InvalidOperationException($"Undefined {nameof(TorizoCueKind)} {cue}.");
        }
    }

    /// <summary>The Torizo occupying <paramref name="torizo"/>, whose belly the glow its fight starts lights and watches.</summary>
    /// <exception cref="InvalidOperationException">The place holds no Torizo.</exception>
    private static ILitTorizo LitTorizo(RoomEnemyPlace torizo) =>
        torizo.Entity as TorizoEntity ?? throw new InvalidOperationException($"Enemy place {torizo.Ordinal} starts a Torizo's fight but holds no Torizo.");

    /// <summary>Moves a Torizo by a posture change's displacement standing up, or back by it sitting down.</summary>
    internal static void ApplyTorizoPosture(
        RoomEnemyPlace torizo,
        Offset16<RoomPixels> displacement,
        TorizoPostureMove move) =>
        torizo.Position += move == TorizoPostureMove.SitDown ? -displacement : displacement;

    /// <summary>Queues the Torizo's music change as the Torizo makes it, after the update's earlier music requests.</summary>
    private void RequestTorizoMusic(MusicRequest request) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, MusicQueueDelay.Shortest));
}
