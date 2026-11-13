using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Bomb Torizo species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BombTorizoBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. BombTorizoPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BombTorizoEntity(this, slot, common);

    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the eighteen-slot enemy projectile pool used by Torizo attacks.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the statue-break sound with the enemy audio limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Exposes the two live colour sets occupied by Torizo's body.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Provides the frame counter used by Torizo animation choices.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns the orb drops and the death scatter after the authored sequences.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Bomb and Golden Torizo's body: its behaviour flags, palette, animation start and the low-health drool it triggers.</summary>
    internal TorizoBody Body { get; }

    /// <summary>Bomb and Golden Torizo's setup and movement: walking, jumps, falls and air transitions under gravity.</summary>
    internal TorizoMovement Movement { get; }

    /// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the function their branches test.</summary>
    internal TorizoEncounterAccess Access { get; }

    /// <summary>Bomb and Golden Torizo's projectiles: drool, swipes, sonic booms, Chozo orbs, eggs, super missiles, eye beams and explosions.</summary>
    internal TorizoAttacks Attacks { get; }

    /// <summary>The branches Bomb and Golden Torizo's animations take on Samus's position, and their walk steps.</summary>
    internal TorizoChoices Choices { get; }

    /// <summary>Bomb and Golden Torizo's hitboxes, touch and hurt, and what Samus's weapons do to them.</summary>
    internal TorizoCombat Combat { get; }



    /// <summary>The room effects the bosses request this update, where the fight's start asks for the belly's glow.</summary>
    private readonly BossFrameResultBuilder _frameResults;

    /// <summary>Shows step <paramref name="step"/> of the Torizo's belly glow over his belly (#1512, #1597).</summary>
    /// <exception cref="InvalidOperationException">No Torizo is loaded to glow.</exception>
    internal void ShowBellyGlow(PaletteFxFrame step)
    {
        TorizoEnemyState state = Access.TorizoState
            ?? throw new InvalidOperationException("The Torizo belly glow shows a step while no Torizo is loaded.");
        state.Colours.ShowBellyGlow(step);
        state.Slot.Shading = state.Colours.Shading;
    }

    /// <summary>Connects the Bomb and Golden Torizo callbacks to their room-scoped services.</summary>
    internal BombTorizoBehavior(BombTorizoBehaviorDependencies dependencies)
    {
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _frameResults = dependencies.FrameResults;
        Access = new TorizoEncounterAccess();
        Attacks = new TorizoAttacks(_room, _projectiles, _audio, _deathsAndDrops, Access);
        Body = new TorizoBody(_room, Access, Attacks);
        Movement = new TorizoMovement(_room, dependencies.Terrain, dependencies.Earthquake, Body, Access);
        Choices = new TorizoChoices(_room, dependencies.Terrain, Access);
        Combat = new TorizoCombat(dependencies.Collision, Body, Access);
    }
    /// <summary>Health below which the head-break animation interrupts active Bomb Torizo AI.</summary>
    internal const int BombTorizoHeadExplosionHealth = 350;
    /// <summary>Health below which the later core-break animation interrupts active AI.</summary>
    internal const int BombTorizoCoreExplosionHealth = 100;

    /// <summary>Names the colouring the statue's fade runs toward (#1512).</summary>
    private static void SetTorizoPaletteTarget(TorizoEnemyState state, TorizoLookKind look) =>
        state.Colours.FadeToward(new TorizoLook(look));

    /// <summary>Runs Bomb Torizo's per-frame behaviour and its main-function dispatcher.</summary>
    private void RunBombTorizoMain(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        switch (state.Function)
        {
            case TorizoFunction.Idle:
                return;

            case TorizoFunction.Falling:
                Body.MaybeSpawnBombTorizoLowHealthDrool(torizo);
                Movement.ApplyBombTorizoGravity(torizo, state, TorizoMovement.RequireBombTorizoLevel(level));
                return;

            case TorizoFunction.WaitForHandTrigger:
                // Bomb Torizo sleeps, solid, while its statue's hand still stands in the room.
                torizo.Contact = torizo.Contact.With(EnemyContact.SolidToSamus);
                if (_room.Services.RoomObjects.BombTorizoHandRemains)
                    return;

                RequestBombTorizoMusic(MusicRequest.SelectTrack(MusicTrack.SecondSetSong));
                torizo.Contact = torizo.Contact.Without(EnemyContact.SolidToSamus);
                TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoAnimations.AfterSleep(TorizoAnimations.SegmentOf(state.BodyPlayback))));
                return;

            case TorizoFunction.Active:
                Movement.RunBombTorizoActiveState(torizo, state, level);
                return;

            case TorizoFunction.GoldenWaitForSamus:
                // The wake-up step advances the animation only after Samus has entered the
                // lower-right trigger rectangle. This is the authored statue wake-up gate.
                if (samus is not null && samus.Position.Y.Whole > 320 && samus.Position.X.Whole > 368)
                {
                    TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoAnimations.AfterSleep(TorizoAnimations.SegmentOf(state.BodyPlayback))));
                }
                return;

            case TorizoFunction.GoldenGravity:
                Body.MaybeSpawnBombTorizoLowHealthDrool(torizo);
                Movement.ApplyBombTorizoGravity(torizo, state, TorizoMovement.RequireBombTorizoLevel(level));
                return;

            case TorizoFunction.GoldenFrameBehavior:
                Body.MaybeSpawnBombTorizoLowHealthDrool(torizo);
                Movement.RunBombTorizoFrameBehavior(torizo, state, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Torizo main function {state.Function} is not translated.");
        }
    }

    /// <summary>Counts consecutive Samus space-jump poses before running the shared statue dispatcher.</summary>
    private void RunGoldenTorizoMain(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        // Golden Torizo's main behaviour counts consecutive space-jump poses for its anti-stall leap
        // decision, then calls the same main-function dispatcher as Bomb Torizo.
        if (samus?.Pose is SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose)
            state.CountSamusSpaceJumpFrame();
        else
            state.SamusSpaceJumpFrames = 0;
        RunBombTorizoMain(torizo, state, samus, level);
    }

    /// <summary>Records the area boss bit, scatters sixteen drops in the variant's arena, and requests post-fight music.</summary>
    private void FinishBombTorizoDeath(TorizoEnemyState state)
    {
        _room.Services.Bosses.SetDefeated(BossBits.AreaTorizo);

        // The two statues share one chance table but use different
        // arena rectangles: Bomb Torizo uses the 128x64 upper room region, while Golden
        // Torizo uses a 256x64 strip much lower in its room.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.BombTorizo,
            count: 16,
            xBase: state.IsGolden ? 128 : 64,
            xScatter: state.IsGolden ? EnemyDropScatterDefinitions.WideArenaHorizontalScatter : EnemyDropScatterDefinitions.NarrowArenaHorizontalScatter,
            yBase: state.IsGolden ? 288 : 96,
            yScatter: EnemyDropScatterDefinitions.ArenaBandVerticalScatter);
        RequestBombTorizoMusic(MusicRequest.SelectTrack(MusicTrack.Elevator));
    }

    /// <summary>Replaces Torizo's posture, keeping the rest of its state.</summary>
    private void SetPosture(RoomEnemySlot torizo, TorizoPostureFlags posture) =>
        Access.RequireBombTorizoState(torizo).Posture = posture;

    /// <summary>Clears <paramref name="flags"/>.</summary>
    private void ClearBehavior(RoomEnemySlot torizo, TorizoBehaviorFlags flags) => Access.RequireBombTorizoState(torizo).Behavior &= ~flags;

    /// <summary>Queues <paramref name="sound"/> through the shared enemy queue.</summary>
    private void PlayTorizoSound(SoundEffectId sound)
    {
        _audio.QueueEnemySound(sound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs BombTorizo's main AI.</summary>
    internal void RunBombTorizoMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBombTorizoMain(slot, Access.RequireBombTorizoState(slot), frame.Samus, frame.Level);
    }

    /// <summary>Runs GoldenTorizo's main AI.</summary>
    internal void RunGoldenTorizoMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunGoldenTorizoMain(
            slot,
            Access.RequireBombTorizoState(slot),
            frame.Samus,
            frame.Level);
    }

    /// <summary>Advances BombTorizo's typed animation.</summary>
    internal void AdvanceBombTorizoTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext context)
    {
        AdvanceTorizoAnimation(slot, new TorizoCueInputs { Samus = context.Samus, Level = context.Level, ControllerInput = context.ControllerInput, FrameCounter = _frameContext.FrameCounter });
    }
    /// <summary>Runs Bomb or Golden Torizo's body animation where the shared enemy animation would.</summary>
    internal void AdvanceTorizoAnimation(RoomEnemySlot torizo, TorizoCueInputs inputs)
    {
        if (!torizo.AnimationClock.Expire())
            return;
        TorizoEnemyState state = Access.RequireBombTorizoState(torizo);
        AnimationPlayback<TorizoBodySegment> playback = state.BodyPlayback;
        // The death and wake repeats count the body's frame timer, as the timer commands do.
        EnemyAnimationPlayback.Play(torizo, TorizoAnimations.Segments, ref playback,
            (cue, continuation) => ApplyTorizoCue(torizo, state, inputs, cue, continuation));
        state.BodyPlayback = playback;
    }

    /// <summary>
    /// Shows the look artwork swap <paramref name="swap"/> gives the Torizo at this update (#1406, #1538): from now on its
    /// frames show their finished pictures of that look.
    /// </summary>
    private void ShowTorizoArtwork(TorizoEnemyState state, TorizoArtworkSwap swap) =>
        _presentation.ShowLook(swap.Look(state.IsGolden ? EncounterArtwork.GoldenTorizo : EncounterArtwork.BombTorizo));

    /// <summary>
    /// Carries out one cue of Bomb or Golden Torizo's body animation, returning where a branch, call or return
    /// continues the play, or null to play on. <paramref name="continuation"/> is where the play stands just past the
    /// cue: the return point a call saves. <see cref="TorizoCueInputs.FrameCounter"/> is the frame
    /// counter, which both attack choosers add; only whether it lies in the second half of each sixteen-frame cycle survives.
    /// </summary>
    internal AnimationContinuation<TorizoBodySegment>? ApplyTorizoCue(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        TorizoCueInputs inputs,
        TorizoCue cue,
        AnimationPlayback<TorizoBodySegment> continuation)
    {
        switch (cue)
        {
            case TorizoCue.SetFrameBehavior setFrameBehavior:
                state.FrameBehavior = setFrameBehavior.FrameBehavior;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.ClearFrameBehavior }:
                state.FrameBehavior = TorizoFrameBehavior.Idle;
                return null;

            case TorizoCue.SetFunction setFunction:
                state.Function = setFunction.Function;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.BlowUpGut }:
                Combat.SetBehavior(torizo, TorizoBehaviorFlags.GutBlownUp);
                for (int piece = 0; piece < 6; piece++)
                    Attacks.SpawnBombTorizoLowHealthDrool(torizo);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.MarkFaceBlownUp }:
                Combat.SetBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.MakeVisible }:
                torizo.Presence = torizo.Presence.Without(EnemyPresence.Hidden);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.MakeInvisible }:
                torizo.Presence = torizo.Presence.With(EnemyPresence.Hidden);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.FadeToBlack }:
                SetTorizoPaletteTarget(state, TorizoLookKind.Black);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.FinishDeath }:
                FinishBombTorizoDeath(state);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.AdvanceColourFade }:
                state.Colours.Step(_presentation.FadeStepCount);
                torizo.Shading = state.Colours.Shading;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.FadeToNormalColors }:
                SetTorizoPaletteTarget(state, TorizoLookKind.Normal);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.StartBombTorizoFight }:
                RequestBombTorizoMusic(MusicRequest.SelectTrack(MusicTrack.FirstSetSong));
                // The fight starts the belly's glow, which watches this Torizo and ends once it dies.
                _frameResults.TorizoBellyGlow = new(PaletteFxObject.BombTorizoBelly, state);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.NoOperation }:
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.LockAnimation }:
                state.AnimationLocked = true;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.UnlockAnimation }:
                state.AnimationLocked = false;
                return null;

            case TorizoCue.Choice { Kind: TorizoChoiceKind.GoToIfFaceBlownUpElseIfGolden } choice:
            {
                if (Body.HasBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp))
                    return TorizoAnimations.ContinueWith(choice.Target);
                // The Bomb variant runs on past both targets.
                return state.IsGolden ? TorizoAnimations.ContinueWith(choice.Alternate) : null;
            }

            case TorizoCue.Branch { Kind: TorizoBranchKind.Link } branch:
                state.ReturnPlayback = TorizoAnimations.Start(branch.Target);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.Return }:
                return TorizoAnimations.ContinueAt(state.ReturnPlayback);

            case TorizoCue.Signal { Kind: TorizoCueKind.ResumeInterrupted }:
                return TorizoAnimations.ContinueAt(state.InterruptedPlayback);

            case TorizoCue.LowHealthExplosions explosions:
                for (int explosion = 0; explosion < 6; explosion++)
                    Attacks.SpawnBombTorizoLowHealthExplosion(torizo, explosions.Site);
                // The animation then waits forty updates, as the callback's sleep does.
                torizo.FlashTimer = 40;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnDeathExplosion }:
                Attacks.SpawnBombTorizoDeathExplosion(torizo);
                // The animation then waits one update, as the callback's sleep does.
                torizo.FlashTimer = 1;
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnLandingDust }:
                Attacks.SpawnBombTorizoLandingDust(torizo, foot: HorizontalDirection.Right);
                Attacks.SpawnBombTorizoLandingDust(torizo, foot: HorizontalDirection.Left);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnLowHealthDrool }:
                if (torizo.Health < BombTorizoHeadExplosionHealth)
                    Body.SpawnBombTorizoInitialDrool(torizo);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SetTurningAround }:
                SetPosture(torizo, Access.Posture(torizo) | TorizoPostureFlags.TurningAround);
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
                ApplyBombTorizoPosture(torizo, posture.Step, move: BombTorizoPostureMove.StandUp);
                return null;

            case TorizoCue.Posture { Kind: TorizoPostureKind.SitDown } posture:
                ApplyBombTorizoPosture(torizo, posture.Step, move: BombTorizoPostureMove.SitDown);
                return null;

            case TorizoCue.Walk { Kind: TorizoWalkKind.WalkBombTorizo } walk:
                return Choices.ProcessBombTorizoWalkAnimation(torizo, state, inputs.Samus, TorizoMovement.RequireBombTorizoLevel(inputs.Level), new TorizoWalkStep { Step = walk.Step, CollisionFacingRight = TorizoBodySegment.FacingLeftTurningLeft, CollisionFacingLeft = TorizoBodySegment.FacingRightTurningRight });

            case TorizoCue.Walk { Kind: TorizoWalkKind.WalkFacelessBombTorizo } walk:
                return Choices.ProcessBombTorizoWalkAnimation(torizo, state, inputs.Samus, TorizoMovement.RequireBombTorizoLevel(inputs.Level), new TorizoWalkStep { Step = walk.Step, CollisionFacingRight = TorizoBodySegment.FacingLeftFacelessTurningLeft, CollisionFacingLeft = TorizoBodySegment.FacingRightFacelessTurningRight });

            case TorizoCue.Branch { Kind: TorizoBranchKind.GoToIfRising } branch:
                return state.Velocity.Y.IsNegative ? TorizoAnimations.ContinueWith(branch.Target) : null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.CallIfSamusCloseBehind } branch:
                return Choices.SelectBombTorizoCloseBehindBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    continuation,
                    branch.Target);

            case TorizoCue.Branch { Kind: TorizoBranchKind.JumpBackIfSamusCloseInFront } branch:
                return Choices.SelectBombTorizoCloseFrontJump(
                    torizo,
                    state,
                    inputs.Samus,
                    branch.Target);

            case TorizoCue.Choice { Kind: TorizoChoiceKind.CallBombTorizoAttack } choice:
            {
                if (inputs.Samus is null)
                    throw new InvalidOperationException("Bomb Torizo attack selection requires Samus state.");
                state.ReturnPlayback = continuation;
                bool chooseFirst = inputs.Samus.Missiles < 5 ||
                    TorizoChoices.InFirstAttackPhase(inputs.FrameCounter, inputs.Samus.Position.X.Whole);
                return TorizoAnimations.ContinueWith(chooseFirst ? choice.Target : choice.Alternate);
            }

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnBombTorizoChozoOrbs }:
                Attacks.SpawnBombTorizoChozoOrb(torizo);
                Attacks.SpawnBombTorizoChozoOrb(torizo);
                Attacks.SpawnBombTorizoChozoOrb(torizo);
                return null;

            case TorizoCue.ParameterizedSpawn { Kind: TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom }:
                Attacks.SpawnBombTorizoSonicBoom(torizo);
                return null;

            case TorizoCue.ParameterizedSpawn { Kind: TorizoParameterizedSpawnKind.SpawnGoldenTorizoSonicBoom }:
                Attacks.SpawnGoldenTorizoSonicBoom(torizo);
                return null;

            case TorizoCue.ParameterizedSpawn { Kind: TorizoParameterizedSpawnKind.SpawnExplosiveSwipe } parameterizedSpawn:
                Attacks.SpawnBombTorizoExplosiveSwipe(torizo, parameterizedSpawn.Parameter);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.PlayShotSound }:
                PlayTorizoSound(EnemySound.ShotTorizo);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.PlayFootstepSound }:
                PlayTorizoSound(EnemySound.ChozoFootsteps);
                return null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.GoToIfAirborne } branch:
                return torizo.Position.Y.Whole == 375 ? null : TorizoAnimations.ContinueWith(branch.Target);

            case TorizoCue.Signal { Kind: TorizoCueKind.LoadGoldenTorizoPalettes }:
                SetTorizoPaletteTarget(state, TorizoLookKind.Golden);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.StartGoldenTorizoFight }:
                RequestBombTorizoMusic(MusicRequest.SelectTrack(MusicTrack.FirstSetSong));
                torizo.Radius = new(18, 48);
                _frameResults.TorizoBellyGlow = new(PaletteFxObject.GoldenTorizoBelly, state);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.ClearCaughtSuperMissile }:
                ClearBehavior(torizo, TorizoBehaviorFlags.CaughtSuperMissile);
                return null;

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnEgg }:
                Attacks.SpawnGoldenTorizoEgg(torizo);
                return null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.GoToIfEyeBeamActive } branch:
                return _projectiles.EnemyProjectiles.Any(projectile =>
                        projectile.Kind == RoomEnemyProjectileKind.GoldenTorizoEgg)
                    ? TorizoAnimations.ContinueWith(branch.Target)
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

            case TorizoCue.Branch { Kind: TorizoBranchKind.GoToIfSamusMorphedBehind } branch:
                return Choices.SelectGoldenTorizoMorphBallBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    branch.Target);

            case TorizoCue.ParameterizedSpawn { Kind: TorizoParameterizedSpawnKind.SpawnEyeBeam }:
                Attacks.SpawnGoldenTorizoEyeBeam(torizo);
                return null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.CallSometimesIfSamusMorphedInFront } branch:
                return Choices.SelectGoldenTorizoMediumRangeBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    continuation,
                    branch.Target);

            case TorizoCue.Branch { Kind: TorizoBranchKind.CallSometimesIfHealthLow } branch:
                if (torizo.Health > 1928 || !TorizoRandomDefinitions.LowHealthCall.IsTakenBy(_room.Random.Generate()))
                    return null;
                state.DecisionCounter = 0;
                state.ReturnPlayback = continuation;
                return TorizoAnimations.ContinueWith(branch.Target);

            case TorizoCue.Branch { Kind: TorizoBranchKind.CallIfStunHealthHigh } branch:
                if (torizo.Health <= 10800 ||
                    !Body.HasBehavior(torizo, TorizoBehaviorFlags.Stunned))
                    return null;
                state.ReturnPlayback = continuation;
                return TorizoAnimations.ContinueWith(branch.Target);

            case TorizoCue.Branch { Kind: TorizoBranchKind.JumpForwardIfSamusFar } branch:
                return Choices.SelectGoldenTorizoSpaceJumpCounter(
                    torizo,
                    state,
                    inputs.Samus,
                    inputs.ControllerInput,
                    branch.Target);

            case TorizoCue.Signal { Kind: TorizoCueKind.SpawnGoldenTorizoChozoOrbs }:
                Attacks.SpawnGoldenTorizoChozoOrb(torizo);
                return null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden } branch:
                return Choices.SelectGoldenTorizoRepeatedJump(
                    torizo,
                    state,
                    inputs.Samus,
                    branch.Target);

            case TorizoCue.Choice { Kind: TorizoChoiceKind.CallGoldenTorizoAttack } choice:
            {
                SamusState activeSamus = RequireGoldenTorizoSamus(inputs.Samus);
                state.ReturnPlayback = continuation;
                bool chooseFirst = activeSamus.Missiles < 32 ||
                    TorizoChoices.InFirstAttackPhase(inputs.FrameCounter, activeSamus.Position.X.Whole);
                return TorizoAnimations.ContinueWith(chooseFirst ? choice.Target : choice.Alternate);
            }

            case TorizoCue.Walk { Kind: TorizoWalkKind.WalkGoldenTorizo } walk:
                return Choices.ProcessGoldenTorizoWalkAnimation(
                    torizo,
                    state,
                    inputs.Samus,
                    TorizoMovement.RequireBombTorizoLevel(inputs.Level),
                    walk.Step);

            case TorizoCue.ShowArtwork showArtwork:
                ShowTorizoArtwork(state, showArtwork.Swap);
                return null;

            default:
                throw new InvalidOperationException($"Undefined {nameof(TorizoCueKind)} {cue}.");
        }
    }

    /// <summary>Requires the active Samus actor for distance and attack decisions in Golden Torizo's animation.</summary>
    private static SamusState RequireGoldenTorizoSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Golden Torizo instruction selection requires the active Samus actor.");

    /// <summary>Applies or reverses the displacement of a Bomb Torizo posture step.</summary>
    internal static void ApplyBombTorizoPosture(
        RoomEnemySlot torizo,
        int step,
        BombTorizoPostureMove move)
    {
        Offset16<RoomPixels> displacement = BombTorizoMovement.Posture(step);
        torizo.Position += move == BombTorizoPostureMove.SitDown ? -displacement : displacement;
    }

    /// <summary>Queues the Torizo's music change as the Torizo makes it, after the update's earlier music requests.</summary>
    private void RequestBombTorizoMusic(MusicRequest request) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, MusicQueueDelay.Shortest));
}
