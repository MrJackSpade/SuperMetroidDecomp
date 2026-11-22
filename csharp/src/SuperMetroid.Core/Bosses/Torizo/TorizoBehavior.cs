using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Torizo species owner, Bomb and Golden Torizo alike: their per-room state, behaviour and the interactions they register.</summary>
internal sealed partial class TorizoBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>
    /// The Bomb Torizo and the Golden Torizo, which only their rooms' population loads place: each reads the load's
    /// surroundings as it is set up.
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    [field: NonSerialized]
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Loads<EnemySpawnSettings>([EnemyDefinitionId.BombTorizo], CreateBombTorizo);
        registrar.Loads<EnemySpawnSettings>([EnemyDefinitionId.GoldenTorizo], CreateGoldenTorizo);
    }

    /// <summary>The Bomb Torizo, set up in the population load's surroundings.</summary>
    private EnemyOccupant CreateBombTorizo(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings) =>
        EnemyOccupant.Running(new BombTorizoEntity(this, occupant, place, common, surroundings));

    /// <summary>The Golden Torizo, set up in the population load's surroundings.</summary>
    private EnemyOccupant CreateGoldenTorizo(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings) =>
        EnemyOccupant.Running(new GoldenTorizoEntity(this, occupant, place, common, surroundings));

    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the room's enemy projectiles, which Torizo's attacks spawn.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the statue-break sound with the enemy audio limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Shows the colours of Torizo's body.</summary>
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
    private readonly EnemyPassEvents _passEvents;

    /// <summary>Shows step <paramref name="step"/> of the belly glow <paramref name="torizo"/> lit over its belly (#1512, #1597, #2240).</summary>
    /// <param name="torizo">The Torizo whose fight start lit the glow.</param>
    /// <param name="state">The enemy's state, which it holds from its setup.</param>
    /// <param name="step">The glow's step.</param>
    internal static void ShowBellyGlow(RoomEnemyPlace torizo, TorizoEnemyState state, ColourEffectFrame step)
    {
        state.Colours.ShowBellyGlow(step);
        torizo.Shading = state.Colours.Shading;
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
        _passEvents = dependencies.PassEvents;
        Attacks = new TorizoAttacks(_room, _projectiles, _audio, _deathsAndDrops);
        Body = new TorizoBody(_room, Attacks);
        Movement = new TorizoMovement(_room, dependencies.Terrain, dependencies.Earthquake, Body);
        Choices = new TorizoChoices(_room, dependencies.Terrain);
        Combat = new TorizoCombat(dependencies.Collision, Body);
    }
    /// <summary>Names the colouring the statue's fade runs toward (#1512).</summary>
    private static void SetTorizoFadeTarget(TorizoEnemyState state, TorizoLookKind look) =>
        state.Colours.FadeToward(new TorizoLook(look));

    /// <summary>What <see cref="RunTorizoMain"/> does for each <see cref="TorizoPhase"/> member, each member's own arm.</summary>
    private IdentityTable<TorizoPhase, Action<RoomEnemyOccupant, RoomEnemyPlace, TorizoEnemyState, SamusState, RoomLevelData>> RunTorizoMainForTorizoPhase => field ??= new(new Dictionary<TorizoPhase, Action<RoomEnemyOccupant, RoomEnemyPlace, TorizoEnemyState, SamusState, RoomLevelData>>
    {
        [TorizoPhase.Idle] = (_, torizo, state, samus, level) => { },
        [TorizoPhase.Falling] = (occupant, torizo, state, samus, level) =>
        {
            Body.MaybeSpawnLowHealthDrool(occupant, torizo, state);
            Movement.ApplyTorizoGravity(torizo, state, level);
        },
        [TorizoPhase.WaitForHandTrigger] = (_, torizo, state, samus, level) =>
        {
            // Bomb Torizo stays dormant, solid, while its statue's hand still stands in the room.
            torizo.Contact = torizo.Contact.With(EnemyContact.SolidToSamus);
            if (_room.Services.RoomObjects.BombTorizoHandRemains)
                return;

            RequestTorizoMusic(MusicRequest.SelectTrack(MusicTrack.TorizoAwakening));
            torizo.Contact = torizo.Contact.Without(EnemyContact.SolidToSamus);
            // The dormant statue holds until its hand is gone, then awakens. Bomb Torizo always sits facing left; no
            // room has him facing right (#1941).
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoAnimation.BombTorizoAwakening));
        },
        [TorizoPhase.Active] = (occupant, torizo, state, samus, level) => Movement.RunBombTorizoActiveState(occupant, torizo, state, level),
        [TorizoPhase.GoldenWaitForSamus] = (_, torizo, state, samus, level) =>
        {
            // The wake-up step advances the animation only after Samus has entered the
            // lower-right trigger rectangle. This is the authored statue wake-up gate.
            if (samus.Position.Y.Whole > TorizoDecisionDefinitions.GoldenWakeCorner.Y &&
                samus.Position.X.Whole > TorizoDecisionDefinitions.GoldenWakeCorner.X)
            {
                // The dormant statue holds until Samus steps in, then awakens.
                TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoAnimation.GoldenTorizoAwakening));
            }
        },
        [TorizoPhase.GoldenActive] = (occupant, torizo, state, samus, level) =>
        {
            Body.MaybeSpawnLowHealthDrool(occupant, torizo, state);
            Movement.MoveBody(torizo, state, level);
        },
    });

    /// <summary>Runs the main phase of Bomb or Golden Torizo for one update.</summary>
    private void RunTorizoMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace torizo,
        TorizoEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        RunTorizoMainForTorizoPhase[state.Phase](occupant, torizo, state, samus, level);
    }

    /// <summary>Counts consecutive Samus space-jump poses before running the shared statue step.</summary>
    private void RunGoldenTorizoMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace torizo,
        TorizoEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // Golden Torizo's main behaviour counts consecutive space-jump poses for its anti-stall leap
        // decision, then runs the same main phases as Bomb Torizo.
        if (SamusPoseMetadata.IsSpaceJumpPose(samus.Pose))
            state.CountSamusSpaceJumpFrame();
        else
            state.SamusSpaceJumpFrames = 0;
        RunTorizoMain(occupant, torizo, state, samus, level);
    }

    /// <summary>Records the area boss's defeat, scatters its death drops (<see cref="TorizoDeathDefinitions.DeathDrops"/>) in the variant's arena, and requests post-fight music.</summary>
    private void FinishTorizoDeath(TorizoEnemyState state, SamusState samus)
    {
        _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaTorizo);

        // The two statues roll one chance table over their own arenas.
        TorizoDropArena arena = state.Variant.DropArena;
        _deathsAndDrops.SpawnEnemyDropScatter(
            TorizoDeathDefinitions.DeathDropChances,
            count: TorizoDeathDefinitions.DeathDrops,
            xBase: arena.Corner.X,
            xScatter: arena.HorizontalScatter,
            yBase: arena.Corner.Y,
            yScatter: arena.VerticalScatter,
            samus: samus);
        RequestTorizoMusic(MusicRequest.SelectTrack(MusicTrack.Elevator));
    }

    /// <summary>Replaces Torizo's posture, keeping the rest of its state.</summary>
    private static void SetPosture(TorizoEnemyState torizoEnemyState, TorizoPosture posture) =>
        torizoEnemyState.Posture = posture;

    /// <summary>Queues <paramref name="sound"/> through the shared enemy queue.</summary>
    private void PlayTorizoSound(SoundEffectId sound)
    {
        _audio.QueueEnemySound(sound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs Bomb Torizo's main AI.</summary>
    internal void RunBombTorizoMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, TorizoEnemyState torizoEnemyState, IEnemyAiFrame frame)
    {
        RunTorizoMain(occupant, place, torizoEnemyState, frame.Samus, frame.Level);
    }

    /// <summary>Runs Golden Torizo's main AI.</summary>
    internal void RunGoldenTorizoMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, TorizoEnemyState torizoEnemyState, IEnemyAiFrame frame)
    {
        RunGoldenTorizoMain(
            occupant, place,
            torizoEnemyState,
            frame.Samus,
            frame.Level);
    }

    /// <summary>Advances the typed animation of <paramref name="torizo"/>, either Torizo, in <paramref name="place"/>.</summary>
    internal void AdvanceTorizoTypedAnimation(RoomEnemyOccupant occupant, RoomEnemyPlace place, TorizoEnemyState torizoEnemyState, ILitTorizo torizo, EnemyAnimationCueContext context)
    {
        AdvanceTorizoAnimation(occupant, place, torizoEnemyState, new TorizoCueInputs { Torizo = torizo, Samus = context.Samus, Level = context.Level, ControllerInput = context.ControllerInput, FrameCounter = _frameContext.FrameCounter });
    }
    /// <summary>Runs Bomb or Golden Torizo's body animation where the shared enemy animation would.</summary>
    internal void AdvanceTorizoAnimation(RoomEnemyOccupant occupant, RoomEnemyPlace torizo, TorizoEnemyState state, TorizoCueInputs inputs)
    {
        if (!torizo.AnimationClock.Expire())
            return;
        AnimationPlayback<TorizoBodySegment> playback = state.BodyPlayback;
        // The death and wake repeats count the body's frame timer, as the animations' counted loops do.
        EnemyAnimationPlayback.Play(torizo, TorizoAnimations.Segments, ref playback,
            (cue, continuation) => ApplyTorizoCue(occupant, torizo, state, inputs, cue, continuation));
        state.BodyPlayback = playback;
    }

    /// <summary>
    /// Shows artwork look <paramref name="look"/> on the Torizo at this update (#1406, #1538, #1761): from now on its frames
    /// show their finished pictures of that look.
    /// </summary>
    private void ShowTorizoArtwork(TorizoEnemyState state, TorizoArtworkLook look) =>
        _presentation.ShowLook(look.Of(state.Variant.Artwork));

    /// <summary>What <see cref="ApplyTorizoCue"/> comes to for each <see cref="TorizoCueKind"/> member, each member's own arm.</summary>
    private IdentityTable<TorizoCueKind, Func<RoomEnemyOccupant, RoomEnemyPlace, TorizoEnemyState, TorizoCueInputs, AnimationContinuation<TorizoBodySegment>?>> ApplyTorizoCueForTorizoCueKind => field ??= new(new Dictionary<TorizoCueKind, Func<RoomEnemyOccupant, RoomEnemyPlace, TorizoEnemyState, TorizoCueInputs, AnimationContinuation<TorizoBodySegment>?>>
    {
        [TorizoCueKind.StartWalking] = (_, torizo, state, inputs) =>
        {
            state.Motion = TorizoBodyMotion.Walking;
            return null;
        },
        [TorizoCueKind.StartStanding] = (_, torizo, state, inputs) =>
        {
            state.Motion = TorizoBodyMotion.Standing;
            return null;
        },
        [TorizoCueKind.StartLeaping] = (_, torizo, state, inputs) =>
        {
            state.Motion = TorizoBodyMotion.Leaping;
            return null;
        },
        [TorizoCueKind.StopMoving] = (_, torizo, state, inputs) =>
        {
            state.Motion = TorizoBodyMotion.HoldingStill;
            return null;
        },
        [TorizoCueKind.BlowUpGut] = (occupant, torizo, state, inputs) =>
        {
            if (state.Breakage == TorizoBreakage.Intact)
                state.Breakage = TorizoBreakage.GutBlownUp;
            for (int piece = 0; piece < TorizoDeathDefinitions.GutDroolPieces; piece++)
                Attacks.SpawnBombTorizoLowHealthDrool(occupant, torizo, state);
            return null;
        },
        [TorizoCueKind.MarkFaceBlownUp] = (_, torizo, state, inputs) =>
        {
            state.Breakage = TorizoBreakage.GutAndFaceBlownUp;
            return null;
        },
        [TorizoCueKind.MakeVisible] = (_, torizo, state, inputs) =>
        {
            torizo.Presence = torizo.Presence.Without(EnemyPresence.Hidden);
            return null;
        },
        [TorizoCueKind.MakeInvisible] = (_, torizo, state, inputs) =>
        {
            torizo.Presence = torizo.Presence.With(EnemyPresence.Hidden);
            return null;
        },
        [TorizoCueKind.FadeToBlack] = (_, torizo, state, inputs) =>
        {
            SetTorizoFadeTarget(state, TorizoLookKind.Black);
            return null;
        },
        [TorizoCueKind.FinishDeath] = (_, torizo, state, inputs) =>
        {
            FinishTorizoDeath(state, inputs.Samus);
            return null;
        },
        [TorizoCueKind.AdvanceColourFade] = (_, torizo, state, inputs) =>
        {
            state.Colours.Step(_presentation.FadeStepCount);
            torizo.Shading = state.Colours.Shading;
            return null;
        },
        [TorizoCueKind.FadeToNormalColors] = (_, torizo, state, inputs) =>
        {
            SetTorizoFadeTarget(state, TorizoLookKind.Normal);
            return null;
        },
        [TorizoCueKind.StartBombTorizoFight] = (_, torizo, state, inputs) =>
        {
            RequestTorizoMusic(MusicRequest.SelectTrack(MusicTrack.RidleyAndDraygonBattle));
            // The fight starts the belly's glow, which watches this Torizo and ends once it dies.
            _passEvents.Publish(new TorizoBellyGlowRequest(TorizoBellyGlow.BombTorizo, inputs.Torizo));
            return null;
        },
        [TorizoCueKind.LockAnimation] = (_, torizo, state, inputs) =>
        {
            state.AnimationLocked = true;
            return null;
        },
        [TorizoCueKind.UnlockAnimation] = (_, torizo, state, inputs) =>
        {
            state.AnimationLocked = false;
            return null;
        },
        [TorizoCueKind.ResumeWalk] = (_, torizo, state, inputs) => TorizoAnimations.ContinueAt(state.InterruptedWalk),
        [TorizoCueKind.ResumeInterrupted] = (_, torizo, state, inputs) => TorizoAnimations.ContinueAt(state.InterruptedPlayback),
        [TorizoCueKind.SpawnDeathExplosion] = (_, torizo, state, inputs) =>
        {
            Attacks.SpawnDeathExplosion(torizo);
            torizo.HurtFlash.Restart(TorizoDeathDefinitions.DeathExplosionFlash);
            return null;
        },
        [TorizoCueKind.SpawnLandingDust] = (_, torizo, state, inputs) =>
        {
            Attacks.SpawnLandingDust(torizo, foot: HorizontalDirection.Right);
            Attacks.SpawnLandingDust(torizo, foot: HorizontalDirection.Left);
            return null;
        },
        [TorizoCueKind.SpawnLowHealthDrool] = (occupant, torizo, state, inputs) =>
        {
            if (torizo.Health < TorizoDecisionDefinitions.GutBlowUpHealth)
                Body.SpawnInitialDrool(occupant, torizo, state);
            return null;
        },
        [TorizoCueKind.SetTurningAround] = (_, torizo, state, inputs) =>
        {
            SetPosture(state, TorizoFacing.Posture(state).TurningAround);
            return null;
        },
        [TorizoCueKind.LeapingPoseShown] = (_, torizo, state, inputs) =>
        {
            // The jump holds its leaping pose while the Torizo rises; once it no longer rises, it falls in the fall its
            // jump named.
            return state.Velocity.Y.IsNegative ? null : TorizoAnimations.ContinueWith(state.Landing);
        },
        [TorizoCueKind.SpawnBombTorizoChozoOrbs] = (occupant, torizo, state, inputs) =>
        {
            for (int orb = 0; orb < TorizoChozoOrbDefinitions.BombTorizoOrbsPerBurst; orb++)
                Attacks.SpawnBombTorizoChozoOrb(occupant, torizo, state);
            return null;
        },
        [TorizoCueKind.SpawnBombTorizoSonicBoom] = (occupant, torizo, state, inputs) =>
        {
            Attacks.SpawnBombTorizoSonicBoom(occupant, torizo, state);
            return null;
        },
        [TorizoCueKind.SpawnGoldenTorizoSonicBoom] = (occupant, torizo, state, inputs) =>
        {
            Attacks.SpawnGoldenTorizoSonicBoom(occupant, torizo, state);
            return null;
        },
        [TorizoCueKind.PlayShotSound] = (_, torizo, state, inputs) =>
        {
            PlayTorizoSound(EnemySound.ShotTorizo);
            return null;
        },
        [TorizoCueKind.PlayFootstepSound] = (_, torizo, state, inputs) =>
        {
            PlayTorizoSound(EnemySound.ChozoFootsteps);
            return null;
        },
        [TorizoCueKind.DroppingPoseShown] = (_, torizo, state, inputs) =>
        {
            // The waking Golden Torizo holds its half-risen pose while it drops; on its floor, it settles.
            return torizo.Position.Y.Whole == TorizoDecisionDefinitions.GoldenFloorY ? TorizoAnimations.ContinueWith(TorizoAnimation.GoldenTorizoSettling) : null;
        },
        [TorizoCueKind.FadeToGoldenColors] = (_, torizo, state, inputs) =>
        {
            SetTorizoFadeTarget(state, TorizoLookKind.Golden);
            return null;
        },
        [TorizoCueKind.StartGoldenTorizoFight] = (_, torizo, state, inputs) =>
        {
            RequestTorizoMusic(MusicRequest.SelectTrack(MusicTrack.RidleyAndDraygonBattle));
            torizo.Radius = TorizoDecisionDefinitions.GoldenFightRadius;
            _passEvents.Publish(new TorizoBellyGlowRequest(TorizoBellyGlow.GoldenTorizo, inputs.Torizo));
            return null;
        },
        [TorizoCueKind.ClearCaughtSuperMissile] = (_, torizo, state, inputs) =>
        {
            state.CaughtSuperMissile = false;
            return null;
        },
        [TorizoCueKind.SpawnEgg] = (occupant, torizo, state, inputs) =>
        {
            Attacks.SpawnGoldenTorizoEgg(occupant, torizo, state);
            return null;
        },
        [TorizoCueKind.ChestHeldOpen] = (_, torizo, state, inputs) =>
        {
            // Golden Torizo holds its chest open while any of its eggs remains; once they are gone, it closes it.
            return state.EggsRemain() ? null : TorizoAnimations.ContinueWith(TorizoAnimation.GoldenChestClosing);
        },
        [TorizoCueKind.DisableEyeBeamExplosions] = (_, torizo, state, inputs) =>
        {
            state.EyeBeamExplosionsEnabled = false;
            return null;
        },
        [TorizoCueKind.EnableEyeBeamExplosions] = (_, torizo, state, inputs) =>
        {
            state.EyeBeamExplosionsEnabled = true;
            return null;
        },
        [TorizoCueKind.UnmarkStunned] = (_, torizo, state, inputs) =>
        {
            state.Stunned = false;
            return null;
        },
        [TorizoCueKind.PlayEggReleasedSound] = (_, torizo, state, inputs) =>
        {
            PlayTorizoSound(EnemySound.LowThump);
            return null;
        },
        [TorizoCueKind.PlayLaserSound] = (_, torizo, state, inputs) =>
        {
            PlayTorizoSound(EnemySound.Laser);
            return null;
        },
        [TorizoCueKind.PlaySonicBoomSound] = (_, torizo, state, inputs) =>
        {
            PlayTorizoSound(EnemySound.TorizoSonicBoom);
            return null;
        },
        [TorizoCueKind.SpawnSuperMissile] = (occupant, torizo, state, inputs) =>
        {
            Attacks.SpawnGoldenTorizoSuperMissile(occupant, torizo, state);
            return null;
        },
        [TorizoCueKind.SpawnEyeBeam] = (occupant, torizo, state, inputs) =>
        {
            Attacks.SpawnGoldenTorizoEyeBeam(occupant, torizo, state);
            return null;
        },
        [TorizoCueKind.SpawnGoldenTorizoChozoOrbs] = (occupant, torizo, state, inputs) =>
        {
            Attacks.SpawnGoldenTorizoChozoOrb(occupant, torizo, state);
            return null;
        },
    });

    /// <summary>What <see cref="ApplyTorizoCue"/> comes to for each <see cref="TorizoChoiceKind"/> member, each member's own arm.</summary>
    private static IdentityTable<TorizoChoiceKind, Func<TorizoCue.Choice, TorizoEnemyState, TorizoCueInputs, AnimationContinuation<TorizoBodySegment>?>> ApplyTorizoCueForTorizoChoiceKind => field ??= new(new Dictionary<TorizoChoiceKind, Func<TorizoCue.Choice, TorizoEnemyState, TorizoCueInputs, AnimationContinuation<TorizoBodySegment>?>>
    {
        [TorizoChoiceKind.LandFacelessOrGolden] = static (choice, state, inputs) =>
        {
            if (state.FaceBlownUp)
                return TorizoAnimations.ContinueWith(choice.Target);
            return state.Variant.WalkOnFromLanding(choice.Alternate);
        },
        [TorizoChoiceKind.BombAttack] = ChooseTorizoAttack,
        [TorizoChoiceKind.GoldenAttack] = ChooseTorizoAttack,
    });

    /// <summary>
    /// A Torizo's attack choice: the first attack while Samus carries fewer missiles than the variant's threshold or while
    /// the first-attack phase lasts, the alternate otherwise.
    /// </summary>
    private static AnimationContinuation<TorizoBodySegment>? ChooseTorizoAttack(TorizoCue.Choice choice, TorizoEnemyState state, TorizoCueInputs inputs)
    {
        bool chooseFirst = inputs.Samus.Missiles < state.Variant.FirstAttackMissiles ||
            TorizoChoices.InFirstAttackPhase(inputs.FrameCounter, inputs.Samus.Position.X.Whole);
        return TorizoAnimations.ContinueWith(chooseFirst ? choice.Target : choice.Alternate);
    }

    /// <summary>What <see cref="ApplyTorizoCue"/> comes to for each <see cref="TorizoInterruptKind"/> member, each member's own arm.</summary>
    private IdentityTable<TorizoInterruptKind, Func<RoomEnemyPlace, TorizoCue.Interrupt, TorizoEnemyState, TorizoCueInputs, AnimationPlayback<TorizoBodySegment>, AnimationContinuation<TorizoBodySegment>?>> ApplyTorizoCueForTorizoInterruptKind => field ??= new(new Dictionary<TorizoInterruptKind, Func<RoomEnemyPlace, TorizoCue.Interrupt, TorizoEnemyState, TorizoCueInputs, AnimationPlayback<TorizoBodySegment>, AnimationContinuation<TorizoBodySegment>?>>
    {
        [TorizoInterruptKind.BombSwipeAtSamusBehind] = (torizo, interrupt, state, inputs, continuation) => TorizoChoices.SelectBombTorizoCloseBehindBranch(
                            torizo,
                            state,
                            inputs.Samus,
                            continuation,
                            interrupt.Animation),
        [TorizoInterruptKind.GoldenEyeBeamAtMediumRange] = (torizo, interrupt, state, inputs, continuation) => Choices.SelectGoldenTorizoMediumRangeBranch(
                            torizo,
                            state,
                            inputs.Samus,
                            continuation,
                            interrupt.Animation),
        [TorizoInterruptKind.GoldenEggsAtLowHealth] = (torizo, interrupt, state, inputs, continuation) =>
        {
            if (torizo.Health > TorizoDecisionDefinitions.GoldenEggsHealth || !TorizoRandomRolls.LowHealthEggs.IsTakenBy(_room.Random.Generate()))
                return null;
            state.DecisionCounter = 0;
            state.InterruptedWalk = continuation;
            return TorizoAnimations.ContinueWith(interrupt.Animation);
        },
        [TorizoInterruptKind.GoldenStaggerWhenStunned] = (torizo, interrupt, state, inputs, continuation) =>
        {
            if (torizo.Health <= TorizoDecisionDefinitions.GoldenStaggerHealth ||
                !state.Stunned)
                return null;
            state.InterruptedWalk = continuation;
            return TorizoAnimations.ContinueWith(interrupt.Animation);
        },
    });

    /// <summary>What <see cref="ApplyTorizoCue"/> comes to for each <see cref="TorizoBranchKind"/> member, each member's own arm.</summary>
    private IdentityTable<TorizoBranchKind, Func<RoomEnemyPlace, TorizoCue.Branch, TorizoEnemyState, TorizoCueInputs, AnimationContinuation<TorizoBodySegment>?>> ApplyTorizoCueForTorizoBranchKind => field ??= new(new Dictionary<TorizoBranchKind, Func<RoomEnemyPlace, TorizoCue.Branch, TorizoEnemyState, TorizoCueInputs, AnimationContinuation<TorizoBodySegment>?>>
    {
        [TorizoBranchKind.BombJumpBackFromSamusInFront] = (torizo, branch, state, inputs) => TorizoChoices.SelectBombTorizoCloseFrontJump(
                            torizo,
                            state,
                            inputs.Samus,
                            TorizoBranches.Target(branch.Kind, TorizoFacing.Posture(state))),
        [TorizoBranchKind.GoldenSitOnSamusMorphedBehind] = (torizo, branch, state, inputs) => TorizoChoices.SelectGoldenTorizoMorphBallBranch(
                            torizo,
                            state,
                            inputs.Samus,
                            TorizoBranches.Target(branch.Kind, TorizoFacing.Posture(state))),
        [TorizoBranchKind.GoldenLeapAtDistantSamus] = (torizo, branch, state, inputs) => Choices.SelectGoldenTorizoSpaceJumpCounter(
                            torizo,
                            state,
                            inputs.Samus,
                            inputs.ControllerInput,
                            TorizoBranches.Target(branch.Kind, TorizoFacing.Posture(state))),
        [TorizoBranchKind.GoldenJumpBackFromSamusInFront] = (torizo, branch, state, inputs) => TorizoChoices.SelectGoldenTorizoRepeatedJump(
                            torizo,
                            state,
                            inputs.Samus,
                            TorizoBranches.Target(branch.Kind, TorizoFacing.Posture(state))),
    });

    /// <summary>
    /// Carries out one cue of Bomb or Golden Torizo's body animation, returning where a decision, an interruption or the end
    /// of one continues the play, or null to play on. <paramref name="continuation"/> is where the play stands just past the
    /// cue: the place in the walk an interruption keeps (#2028). The attack choice alternates with the time
    /// (<see cref="TorizoCueInputs.FrameCounter"/>), as the attack phase's definition states.
    /// </summary>
    internal AnimationContinuation<TorizoBodySegment>? ApplyTorizoCue(
        RoomEnemyOccupant occupant, RoomEnemyPlace torizo,
        TorizoEnemyState state,
        TorizoCueInputs inputs,
        TorizoCue cue,
        AnimationPlayback<TorizoBodySegment> continuation)
    {
        return cue.Match(ApplyTorizoCueCases, new ApplyTorizoCueContext(occupant, torizo, state, inputs, continuation, this));
    }

    /// <summary>What <see cref="ApplyTorizoCue"/> does on each <see cref="TorizoCue"/>, built once (#2553).</summary>
    private static TorizoCueCases<ApplyTorizoCueContext, AnimationContinuation<TorizoBodySegment>?> ApplyTorizoCueCases { get; } = new(
        Branch: static (branch, context) =>
        {
            return context.Owner.ApplyTorizoCueForTorizoBranchKind[branch.Kind](context.Torizo, branch, context.State, context.Inputs);
        },
        Interrupt: static (interrupt, context) =>
        {
            return context.Owner.ApplyTorizoCueForTorizoInterruptKind[interrupt.Kind](context.Torizo, interrupt, context.State, context.Inputs, context.Continuation);
        },
        Choice: static (choice, context) =>
        {
            return ApplyTorizoCueForTorizoChoiceKind[choice.Kind](choice, context.State, context.Inputs);
        },
        LandIn: static (landIn, context) =>
        {
            context.State.Landing = landIn.Landing;
            return null;
        },
        WalkOn: static (walkOn, _) => TorizoAnimations.ContinueWith(walkOn.Walking),
        Turn: static (turn, context) => TorizoAnimations.ContinueWith(TorizoFacing.FacesRight(context.State) ? turn.WhenFacingRight : turn.WhenFacingLeft),
        Posture: static (posture, context) =>
        {
            ApplyTorizoPosture(context.Torizo, posture.Displacement, posture.Kind);
            return null;
        },
        Walk: static (walk, context) => context.Owner.Choices.ProcessTorizoWalkStep(context.Torizo, context.State, context.Inputs.Samus, context.Inputs.Level, walk.Walker, walk.Velocity),
        Swipe: static (swipe, context) =>
        {
            context.Owner.Attacks.SpawnBombTorizoExplosiveSwipe(context.Torizo, context.State, swipe.Hand);
            return null;
        },
        LowHealthExplosions: static (explosions, context) =>
        {
            for (int explosion = 0; explosion < TorizoDeathDefinitions.LowHealthExplosions; explosion++)
                context.Owner.Attacks.SpawnBombTorizoLowHealthExplosion(context.Torizo, context.State, explosions.Site);
            context.Torizo.HurtFlash.Restart(TorizoDeathDefinitions.LowHealthExplosionFlash);
            return null;
        },
        SetPhase: static (setPhase, context) =>
        {
            context.State.Phase = setPhase.Phase;
            return null;
        },
        ShowArtwork: static (showArtwork, context) =>
        {
            context.Owner.ShowTorizoArtwork(context.State, showArtwork.Look);
            return null;
        },
        Stepped: static (stepped, context) =>
        {
            // The step replaces the whole posture: the way it faces, the foot it leads with, and any turn.
            SetPosture(context.State, TorizoPosture.AfterStep(stepped.Direction, stepped.Foot));
            context.State.CountDecisionStep();
            return null;
        },
        Signal: static (signal, context) =>
        {
            return context.Owner.ApplyTorizoCueForTorizoCueKind[signal.Kind](context.Occupant, context.Torizo, context.State, context.Inputs);
        });

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Occupant">The occupant whose cue is carried out.</param>
    /// <param name="Torizo">The torizo the cases read.</param>
    /// <param name="State">The state the cases read.</param>
    /// <param name="Inputs">The inputs the cases read.</param>
    /// <param name="Continuation">The continuation the cases read.</param>
    /// <param name="Owner">The <see cref="TorizoBehavior"/> reacting.</param>
    private readonly partial record struct ApplyTorizoCueContext(RoomEnemyOccupant Occupant, RoomEnemyPlace Torizo, TorizoEnemyState State, TorizoCueInputs Inputs, AnimationPlayback<TorizoBodySegment> Continuation, TorizoBehavior Owner);

    /// <summary>Moves a Torizo by a posture change's displacement standing up, or back by it sitting down.</summary>
    internal static void ApplyTorizoPosture(
        RoomEnemyPlace torizo,
        Offset16<RoomPixels> displacement,
        TorizoPostureKind kind) =>
        torizo.Position += kind switch
        {
            TorizoPostureKind.StandUp => displacement,
            TorizoPostureKind.SitDown => -displacement,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, $"Undefined {nameof(TorizoPostureKind)}."),
        };

    /// <summary>Queues the Torizo's music change as the Torizo makes it, after the update's earlier music requests.</summary>
    private void RequestTorizoMusic(MusicRequest request) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, MusicQueueDelay.Shortest));
}
