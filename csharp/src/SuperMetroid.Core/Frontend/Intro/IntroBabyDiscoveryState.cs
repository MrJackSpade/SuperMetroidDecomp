using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Setup and first actor phase of the SR388 baby-Metroid discovery scene.
/// </summary>
[RoomScopeBoundary("A cinematic scene that owns its own level for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class IntroBabyDiscoveryState
{

    /// <summary>Receives the egg-hatch and baby-cry sound requests.</summary>
    private readonly AudioQueues audio;
    /// <summary>Feeds the scripted pose and movement input to Samus.</summary>
    private readonly DemoInputState demo = new();
    /// <summary>What the scripted input watches for before its next step.</summary>
    internal IntroBabyDiscoveryDemoPhase DemoPhase { get; private set; } = IntroBabyDiscoveryDemoPhase.RunningLeft;
    /// <summary>Particles emitted when the egg begins hatching.</summary>
    private readonly List<IntroEggParticle> eggParticles = [];
    /// <summary>Drops emitted as the baby rises past the hatch row.</summary>
    private readonly List<IntroEggSlimeDrop> slimeDrops = [];
    /// <summary>The session's random number generator, which the demo's Samus animation draws from.</summary>
    private readonly SharedRandom random;
    /// <summary>The installed steps the egg's shell fragments fall by past their gravity curve.</summary>
    private readonly ShellFragmentFallPastCurve shellFallPastCurve;
    /// <summary>The egg actor whose animation controls the hatch sequence.</summary>
    private readonly CinematicActor<IntroBabyAnimation, CinematicSpritePose, IntroDiscoveryCue> egg;
    /// <summary>What the egg waits for: Samus's approach, then nothing while it hatches, then page three's cross-fade.</summary>
    private MetroidEggBehavior eggBehavior = MetroidEggBehavior.WaitsForSamus;
    /// <summary>How the confused baby moves: waiting for the hatch, rising out of the egg, idling, then dancing.</summary>
    private ConfusedBabyMotion babyMotion = ConfusedBabyMotion.WaitingForHatch;
    /// <summary>The baby actor that waits for the egg's completed frame before moving.</summary>
    private readonly CinematicActor<IntroBabyAnimation, CinematicSpritePose, IntroDiscoveryCue> confusedBaby;

    /// <summary>Sets up the scene's Samus state, partial room collision map, actors, and demo input list.</summary>
    /// <param name="random">The session's random number generator, which the demo's Samus animation draws from.</param>
    /// <param name="audio">The audio owner the scene's sounds publish to.</param>
    /// <param name="existingSamus">The flashback's Samus the scene carries on with.</param>
    /// <param name="shellFallPastCurve">The installed steps the egg's shell fragments fall by past their gravity curve.</param>
    public IntroBabyDiscoveryState(SharedRandom random, SamusState existingSamus, ShellFragmentFallPastCurve shellFallPastCurve,
        AudioQueues audio)
    {
        this.random = Ensure.NotNull(random);
        this.shellFallPastCurve = Ensure.NotNull(shellFallPastCurve);

        this.audio = Ensure.NotNull(audio);

        egg = CreateActor(IntroBabyActorDefinitions.Egg);
        confusedBaby = CreateActor(IntroBabyActorDefinitions.ConfusedBaby);

        // Scene setup reuses Samus's state, including transition history: the intro carries the flashback's Samus forward.
        Samus = Ensure.NotNull(existingSamus);
        Samus.Pose = SamusPoseId.FacingLeftNormalPose;
        Samus.Position = Samus.Position.WithWhole(IntroBabyDiscoveryDefinitions.SamusStart);
        Samus.SelectedHudItem = HudItem.Beam;
        SamusPoseMetadata.RefreshCollisionRadii(Samus);
        // The flashback leaves Samus standing facing left already, so her animation
        // keeps running rather than restarting.
        SamusPresentation.SetAnimationFrameIfPoseChanged(Samus);
        Samus.CommitPoseHistory();

        // The discovery room is 32x16 blocks, of which the authored terrain fills twelve rows.
        // The final four rows are empty, and no block has a behaviour.
        Level = CreateLevel();

        demo.Clear();
        demo.Enable();
        demo.Load(DemoInputRecordingId.IntroBabyDiscoveryRunningLeft);
    }

    /// <summary>Samus' scene state, carried from the earlier intro when supplied.</summary>
    public SamusState Samus { get; }

    /// <summary>The 32 by 16 discovery room's collision: its two authored strips and air elsewhere.</summary>
    private RoomLevelData Level { get; }

    /// <summary>Signals the outer cinematic owner to start the next page's colour transition.</summary>
    /// <remarks>
    /// The scene's end, kept once its cue reaches it (#2562): the cinematic's phase step, which runs before the actors'
    /// step on each update, starts the next page on the update after the cue, so the end lasts across that boundary; it is
    /// set once and never cleared, and the next page's setup discards the scene.
    /// </remarks>
    public bool NextPageRequested { get; private set; }

    /// <summary>
    /// How this scene shows Samus. Scene setup shows her under the cinematic actors; the egg's and the dancing
    /// baby's behaviours later reorder or clear it.
    /// </summary>
    public IntroSamusDisplay SamusDisplay { get; private set; } = IntroSamusDisplay.BehindActors;

    /// <summary>True once the scene's end has stopped both phases of Samus's update.</summary>
    private bool samusUpdateStopped;

    /// <summary>
    /// Samus, her demo input and the cinematic actors for one update. All run after
    /// the cinematic phase, so the setup update already plays the demo's first
    /// entry, moves Samus and steps the egg and baby.
    /// </summary>
    /// <param name="frameCounter">The cinematic's frame counter, which Samus's demo movement reads.</param>
    /// <param name="crossfadeFinishing">Whether the running crossfade reached its final update on this update (#1980).</param>
    public void Step(int frameCounter, bool crossfadeFinishing)
    {
        // The cinematic step runs Samus's input and movement phases, ahead of the sprite pass, only while the intro shows her.
        if (SamusDisplay != IntroSamusDisplay.Hidden && !samusUpdateStopped)
        {
            // The demo's pose input runs the demo input object inside Samus's
            // input phase, ahead of her pose input and movement.
            if (demo.IsPlaying)
                CheckDemoPhase(crossfadeFinishing);
            demo.Step(HandleDemoCue);
            // The input phase ends by resetting the movement and pose-change state,
            // before this update's movement records new distances.
            SamusAerialPoseTransitions.DropTransitionShot(Samus);
            Samus.ProjectileInheritance.ClearMovement();

            // An end cue read above already stopped Samus's update, so this
            // update skips Samus's movement and animation.
            if (!samusUpdateStopped)
            {
                // The intro-demo input phase publishes the current pose's collision radius
                // before input/movement, rather than requiring pose setters to publish early.
                SamusPoseMetadata.RefreshCollisionRadii(Samus);
                IntroSamusDemoMovement.StepGroundedLeft(

                    Level,
                    Samus,
                    demo.Held,
                    demo.NewlyPressed,
                    frameCounter,
                    random);
            }
        }

        // The egg tests Samus's post-movement X during the later cinematic-sprite pass.
        // It permanently redirects the animation once Samus is left of the hatching line.
        if (eggBehavior == MetroidEggBehavior.WaitsForSamus &&
            Samus.Position.X.Whole < IntroBabyDiscoveryDefinitions.EggHatchX)
        {
            egg.Start(IntroBabyAnimation.EggHatching);
            eggBehavior = MetroidEggBehavior.Hatches;
        }

        // The egg is gone from the update page three's crossfade reaches its final update. The egg's animation starts the wait
        // after starting page three.
        if (eggBehavior == MetroidEggBehavior.WaitsForCrossFadeEnd && crossfadeFinishing)
        {
            egg.Delete();
            // Page three's text hides Samus.
            SamusDisplay = IntroSamusDisplay.Hidden;
        }
        egg.Step(HandleEggCue);

        // The baby updates after the egg, so it sees the egg's animation as this
        // update has already advanced it.
        StepConfusedBaby(crossfadeFinishing);
        confusedBaby.Step();

        foreach (IntroEggParticle particle in eggParticles)
            particle.Step();
        foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
            slimeDrop.Step();
    }

    /// <summary>Draws egg, baby, and hatch effects in the cinematic actor order used by this scene.</summary>
    public void DrawActors(ScreenSprites screenSprites,
        IIntroCinematicSpritePresentation eggEffectArt,
        IIntroCinematicSpritePresentation actorArt)
    {
        // With objects drawn first, the cinematic actors draw before Samus. The egg
        // was spawned before the confused-baby object and retains its own animation and timer.
        egg.Draw(screenSprites, installedArt: actorArt);
        confusedBaby.Draw(screenSprites, installedArt: actorArt);
        foreach (IntroEggParticle particle in eggParticles)
            particle.Draw(screenSprites, eggEffectArt);
        foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
            slimeDrop.Draw(screenSprites, eggEffectArt);
    }

    /// <summary>
    /// Moves the scripted input from running to stopping and looking once Samus passes her scripted place, and from
    /// looking to its ending once the crossfade finishes. Runs before the input's step, in the same step.
    /// </summary>
    private void CheckDemoPhase(bool crossfadeFinishing)
    {
        switch (DemoPhase)
        {
            case IntroBabyDiscoveryDemoPhase.RunningLeft:
                if (Samus.Position.X.Whole < IntroBabyDiscoveryDefinitions.DemoRunStopX)
                {
                    DemoPhase = IntroBabyDiscoveryDemoPhase.StoppedAndLooking;
                    demo.Load(DemoInputRecordingId.IntroBabyDiscoveryStopAndLook);
                }
                return;

            case IntroBabyDiscoveryDemoPhase.StoppedAndLooking:
                if (crossfadeFinishing)
                {
                    DemoPhase = IntroBabyDiscoveryDemoPhase.Ending;
                    demo.Load(DemoInputRecordingId.IntroBabyDiscoveryEnd);
                }
                return;

            case IntroBabyDiscoveryDemoPhase.Ending:
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(IntroBabyDiscoveryDemoPhase)} {DemoPhase}.");
        }
    }

    /// <summary>Runs the discovery demo recording's end cue, disabling input and both phases of Samus's frame for the frames that follow.</summary>
    private void HandleDemoCue(DemoInputCue cue)
    {
        if (cue != DemoInputCue.EndIntroBabyDiscovery)
            throw new InvalidOperationException($"The Baby-discovery demo handles no {cue} cue.");

        // The scene's end makes both phases of Samus's frame do nothing, disables demo input, and
        // returns into the following shared delete step. Samus is never processed again.
        demo.Disable();
        Samus.Control.LockOutPlayer();
        samusUpdateStopped = true;
    }

    /// <summary>Spawns hatch particles or requests the page-three transition from the egg's animation.</summary>
    private void HandleEggCue(IntroDiscoveryCue cue)
    {
        switch (cue)
        {
            case IntroDiscoveryCue.SpawnShellPieces:
                // The six pieces of the shell spawn in their spawn order, which is the order they update and draw in.
                foreach (IntroEggShellFragment fragment in IntroEggEffectDefinitions.ShellFragments)
                    eggParticles.Add(new IntroEggParticle(fragment, shellFallPastCurve));
                audio.QueueSound(
                    IntroCinematicDefinitions.Sounds.EggHatch,
                    priority: IntroCinematicDefinitions.Sounds.SoundPriority);
                return;

            case IntroDiscoveryCue.StartNextPage:
                // This cue switches the outer cinematic phase to page three. The state
                // owner performs the colour transition; the actor merely reports the request.
                NextPageRequested = true;
                return;

            case IntroDiscoveryCue.DeleteEggAfterCrossFade:
                // Page three's cross-fade deletes the egg once it ends.
                eggBehavior = MetroidEggBehavior.WaitsForCrossFadeEnd;
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(IntroDiscoveryCue)} {cue}.");
        }
    }

    /// <summary>Advances the baby from hatch wait through rise, idle, and crossfade dance phases.</summary>
    private void StepConfusedBaby(bool crossfadeFinishing)
    {
        switch (babyMotion)
        {
            case ConfusedBabyMotion.WaitingForHatch:
                // The confused baby starts rising on the update the egg first shows its burst shell, after this
                // update's egg step.
                if (egg.Shown is { } eggPose && IntroEggPoses.BurstShell.Contains(eggPose))
                {
                    babyMotion = ConfusedBabyMotion.Rising;
                    BabyVelocity = BabyVelocity.WithY(default);
                }
                return;

            case ConfusedBabyMotion.Rising:
                StepHatchedBaby();
                return;

            case ConfusedBabyMotion.Idling:
                if (BabyIdleTimer.Step())
                {
                    babyMotion = ConfusedBabyMotion.Dancing;
                    BabyVelocity = BabyVelocity.WithY(default);
                    BabyCryingDance = UpdateCountdown.Lasting(IntroBabyDiscoveryDefinitions.CryingDanceUpdates);
                }
                return;

            case ConfusedBabyMotion.Dancing:
                StepDancingBaby(crossfadeFinishing);
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(ConfusedBabyMotion)} {babyMotion}.");
        }
    }

    /// <summary>The velocity of the baby's rise and two-axis dance.</summary>
    private PixelVelocity<ScreenPixels> BabyVelocity { get; set; }

    /// <summary>The hold from the apex of the baby's rise to the start of its dance.</summary>
    private UpdateCountdown BabyIdleTimer { get; set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The crying stretch of the baby's dance, its first 128 updates, counted once each: it cries as its age reaches 64 and
    /// 128, and dances on silently once it is over.
    /// </summary>
    private UpdateCountdown BabyCryingDance { get; set; } = UpdateCountdown.Ended();

    /// <summary>Spawns slime at the authored row and accelerates the baby toward its hover point.</summary>
    private void StepHatchedBaby()
    {
        // The hatched baby sheds its slime only on the update it stands exactly on this row, which its
        // motion reaches once. Each of the four drops flies its own arc.
        if (confusedBaby.ScreenPlace.Y == IntroEggMotionDefinitions.SlimeShedRow && slimeDrops.Count == 0)
        {
            foreach (IntroEggSlimeLaunch launch in IntroEggMotionDefinitions.SlimeLaunches)
                slimeDrops.Add(new IntroEggSlimeDrop(confusedBaby.ScreenPlace, launch));
            // The hatched baby publishes the first confused cry at the same equality edge
            // that creates the four egg-slime drops.
            audio.QueueSound(
                IntroCinematicDefinitions.Sounds.BabyCry1,
                priority: IntroCinematicDefinitions.Sounds.SoundPriority);
        }

        // The hatched baby accelerates toward a point above Samus, clamping to its vertical speed limit.
        int targetY = Samus.Position.Y.Whole - IntroBabyDiscoveryDefinitions.HatchedBabyHeightAboveSamus;
        BabyVelocity = BabyVelocity.WithY(AccelerateToward(
            confusedBaby.ScreenPlace.Y,
            targetY,
            BabyVelocity.Y,
            limit: IntroBabyDiscoveryDefinitions.VerticalSpeedLimit));
        confusedBaby.Position = WithEightEightVelocity(confusedBaby.Position, Axis.Vertical, BabyVelocity.Y);

        // The first non-negative velocity marks the top of the initial arc. The baby
        // then holds before enabling the two-axis dance around Samus.
        if (!BabyVelocity.Y.IsNegative)
        {
            BabyIdleTimer = UpdateCountdown.For(IntroBabyDiscoveryDefinitions.ArcTopHold);
            babyMotion = ConfusedBabyMotion.Idling;
        }
    }

    /// <summary>Moves the baby around Samus, switches their draw order, and ends on crossfade completion.</summary>
    private void StepDancingBaby(bool crossfadeFinishing)
    {
        if (crossfadeFinishing)
        {
            confusedBaby.Delete();
            // The dancing baby's drift.
            SamusDisplay = IntroSamusDisplay.Hidden;
            return;
        }

        if (BabyCryingDance.IsRunning)
        {
            BabyCryingDance.Count();
            // The dancing baby cries when its count of frames danced reaches 64 and 128.
            if (IntroCadenceDefinitions.BabyCriesEverySixtyFourthDanceUpdate.IsDue(BabyCryingDance.Age))
                audio.QueueSound(
                    IntroCinematicDefinitions.Sounds.BabyCry1,
                    priority: IntroCinematicDefinitions.Sounds.SoundPriority);
        }

        BabyVelocity = BabyVelocity.WithX(AccelerateToward(
            confusedBaby.ScreenPlace.X,
            Samus.Position.X.Whole,
            BabyVelocity.X,
            limit: IntroBabyDiscoveryDefinitions.HorizontalSpeedLimit));
        // A baby moving left draws over Samus; otherwise Samus draws first.
        SamusDisplay = BabyVelocity.X.IsNegative
            ? IntroSamusDisplay.BehindActors
            : IntroSamusDisplay.InFrontOfActors;
        confusedBaby.Position = WithEightEightVelocity(confusedBaby.Position, Axis.Horizontal, BabyVelocity.X);

        int targetY = Samus.Position.Y.Whole - IntroBabyDiscoveryDefinitions.DanceHeightAboveSamus;
        BabyVelocity = BabyVelocity.WithY(AccelerateToward(
            confusedBaby.ScreenPlace.Y,
            targetY,
            BabyVelocity.Y,
            limit: IntroBabyDiscoveryDefinitions.VerticalSpeedLimit));
        confusedBaby.Position = WithEightEightVelocity(confusedBaby.Position, Axis.Vertical, BabyVelocity.Y);
    }


    /// <summary>
    /// Adjusts a velocity toward a target by one acceleration step without passing <paramref name="limit"/> either
    /// way; every step keeps the speed within the limit, so the step stays far inside the velocity's range. The baby and its
    /// targets lie on the screen near Samus, so their places compare directly (#1974).
    /// </summary>
    private static SignedEightEight AccelerateToward(
        int position,
        int target,
        SignedEightEight current,
        SignedEightEight limit) =>
        target >= position
            ? SignedEightEight.Min(limit, current + IntroBabyDiscoveryDefinitions.BabyAcceleration)
            : SignedEightEight.Max(-limit, current - IntroBabyDiscoveryDefinitions.BabyAcceleration);

    /// <summary><paramref name="position"/> moved by one axis of a velocity, fraction included.</summary>
    private static FixedVector<ScreenPixels> WithEightEightVelocity(FixedVector<ScreenPixels> position, Axis axis, SignedEightEight velocity) =>
        axis == Axis.Horizontal
            ? position.WithX(position.X + velocity.ToSixteenSixteenDelta())
            : position.WithY(position.Y + velocity.ToSixteenSixteenDelta());

    /// <summary>Instantiates a cinematic actor with its authored position, colouring and animation.</summary>
    private static CinematicActor<IntroBabyAnimation, CinematicSpritePose, IntroDiscoveryCue> CreateActor(IntroBabyActorDefinition definition) =>
        new(definition.Position, IntroShades.Drawn(definition.Colouring), IntroBabyAnimations.Discovery, definition.Animation);

    /// <summary>Creates the discovery room: collision only, which the scene's own pictures show.</summary>
    private static RoomLevelData CreateLevel() => RoomLevelData.CollisionOnly(
        new Extent16<RoomBlocks>(IntroBabyDiscoveryCollisionDefinitions.Columns, IntroBabyDiscoveryCollisionDefinitions.RoomRows),
        IntroBabyDiscoveryCollision.CreateCollision());
}
