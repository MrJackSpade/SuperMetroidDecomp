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

    /// <summary>Receives egg-hatch and baby-cry sound requests when audio is available.</summary>
    private readonly AudioQueues? audio;
    /// <summary>Feeds the scripted pose and movement input to Samus.</summary>
    internal readonly DemoInputState demo = new();
    /// <summary>What the scripted input watches for before its next step.</summary>
    internal IntroBabyDiscoveryDemoPhase DemoPhase { get; private set; } = IntroBabyDiscoveryDemoPhase.RunningLeft;
    /// <summary>Particles emitted when the egg begins hatching.</summary>
    internal readonly List<IntroEggParticle> eggParticles = [];
    /// <summary>Drops emitted as the baby rises past the hatch row.</summary>
    private readonly List<IntroEggSlimeDrop> slimeDrops = [];
    /// <summary>The session's random number generator, which the demo's Samus animation draws from.</summary>
    private readonly SharedRandom random;
    /// <summary>The installed steps the egg's shell fragments fall by past their gravity curve.</summary>
    private readonly ShellFragmentFallPastCurve shellFallPastCurve;
    /// <summary>The egg actor whose animation controls the hatch sequence.</summary>
    internal readonly IntroDiscoverySprite egg;
    /// <summary>The baby actor that waits for the egg's completed frame before moving.</summary>
    internal readonly IntroDiscoverySprite confusedBaby;

    /// <summary>Sets up the scene's Samus state, partial room collision map, actors, and demo input list.</summary>
    /// <param name="random">The session's random number generator, which the demo's Samus animation draws from.</param>
    /// <param name="audio">The audio owner the scene's sounds publish to, or null for none.</param>
    /// <param name="existingSamus">The flashback's Samus the scene carries on with.</param>
    /// <param name="shellFallPastCurve">The installed steps the egg's shell fragments fall by past their gravity curve.</param>
    public IntroBabyDiscoveryState(SharedRandom random, SamusState existingSamus, ShellFragmentFallPastCurve shellFallPastCurve,
        AudioQueues? audio = null)
    {
        this.random = Ensure.NotNull(random);
        this.shellFallPastCurve = Ensure.NotNull(shellFallPastCurve);

        this.audio = audio;

        egg = CreateActor(IntroBabyActorDefinitions.Egg);
        confusedBaby = CreateActor(IntroBabyActorDefinitions.ConfusedBaby);

        // Scene setup reuses Samus's state, including transition history: the intro carries the flashback's Samus forward.
        Samus = Ensure.NotNull(existingSamus);
        Samus.Pose = SamusPoseId.FacingLeftNormalPose;
        Samus.Position = Samus.Position.WithWhole(new(376, 147));
        Samus.SelectedHudItem = HudItem.Beam;
        SamusPoseMetadata.RefreshCollisionRadii(Samus);
        // The flashback leaves Samus in pose two already, so her animation
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

    /// <summary>The 32 by 16 discovery room with the game's partial foreground copy.</summary>
    public RoomLevelData Level { get; }

    /// <summary>Signals the outer cinematic owner to start page three's palette transition.</summary>
    public bool PageThreeRequested { get; private set; }

    /// <summary>
    /// The Samus display flag for this scene. Scene setup sets it positive; the egg and dancing-baby
    /// frame behaviours later reorder or clear it.
    /// </summary>
    public IntroSamusDisplay SamusDisplay { get; private set; } = IntroSamusDisplay.ObjectsFirst;

    /// <summary>True once the scene's end has pointed both Samus state handlers at handlers that do nothing.</summary>
    private bool samusHandlersEnded;

    /// <summary>
    /// Samus, her demo input and the cinematic sprite objects for one dispatch. All run after
    /// the cinematic phase, so the setup dispatch already plays the demo's first
    /// entry, moves Samus and steps the egg and baby.
    /// </summary>
    public void Step(int frameCounter, int introCrossfadeTimer)
    {
        // The cinematic step runs Samus's handlers, ahead of the sprite pass, only while the display flag is set.
        if (SamusDisplay != IntroSamusDisplay.Hidden && !samusHandlersEnded)
        {
            // The demo pose-input handler runs the demo input object inside Samus's
            // current-state handler, ahead of her pose input and movement.
            if (demo.IsPlaying)
                CheckDemoPhase(introCrossfadeTimer);
            demo.Step(HandleDemoCue);
            // The current-state handler ends by resetting the movement and pose-change state,
            // before this dispatch's movement records new distances.
            SamusAerialPoseTransitions.DropPendingTransitionShot(Samus);
            Samus.ProjectileInheritance.ClearMovement();

            // An end cue read above already replaced the new-state handler, so this
            // dispatch skips Samus's movement and animation.
            if (!samusHandlersEnded)
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

        // The egg's frame behaviour tests Samus's post-movement X during the later
        // cinematic-sprite pass. It permanently redirects the animation once X is below 169.
        if (egg.FrameBehavior == IntroBabyActorDefinitions.Egg.FrameBehavior &&
            CoordinateAxis.Difference(Samus.Position.X.Whole, 169) < 0)
        {
            egg.Start(CinematicSpriteSegment.MetroidEggHatching);
            egg.InstallFrameBehavior(
                CinematicSpriteFrameBehavior.NoOp);
        }

        // The egg's delete behaviour replaces the egg with the shared delete animation only when page three's reverse
        // crossfade reaches zero. The egg's animation installs it after starting page three.
        if (egg.ListFrameBehavior == CinematicSpriteFrameBehavior.MetroidEggDeleteAfterCrossFade &&
            introCrossfadeTimer == 0)
        {
            egg.Start(CinematicSpriteSegment.Delete);
            // Page three's text hides Samus.
            SamusDisplay = IntroSamusDisplay.Hidden;
        }
        egg.Step(HandleEggCue);

        // The baby slot follows the egg slot in the descending actor traversal, so
        // it observes the egg's freshly advanced animation position in this same frame.
        StepConfusedBaby(introCrossfadeTimer);
        confusedBaby.Step();

        foreach (IntroEggParticle particle in eggParticles)
            particle.Step();
        foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
            slimeDrop.Step();
    }

    /// <summary>Draws egg, baby, and hatch effects in the cinematic object order used by this scene.</summary>
    public void DrawActors(ScreenSprites objects,
        IIntroCinematicSpritePresentation? eggEffectArt = null,
        IIntroCinematicSpritePresentation? actorArt = null)
    {
        // An intro Samus display of +1 makes cinematic objects draw before Samus. The egg
        // was spawned before the confused-baby object and retains its own animation and timer.
        egg.Draw(objects, installedArt: actorArt);
        confusedBaby.Draw(objects, installedArt: actorArt);
        foreach (IntroEggParticle particle in eggParticles)
            particle.Draw(objects, eggEffectArt);
        foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
            slimeDrop.Draw(objects, eggEffectArt);
    }

    /// <summary>
    /// Moves the scripted input from running to stopping and looking once Samus passes her scripted place, and from
    /// looking to its ending once the crossfade finishes. Runs before the input's step, in the same step.
    /// </summary>
    private void CheckDemoPhase(int introCrossfadeTimer)
    {
        switch (DemoPhase)
        {
            case IntroBabyDiscoveryDemoPhase.RunningLeft:
                if (Samus.Position.X.Whole < 178)
                {
                    DemoPhase = IntroBabyDiscoveryDemoPhase.StoppedAndLooking;
                    demo.SwitchTo(DemoInputRecordingId.IntroBabyDiscoveryStopAndLook);
                }
                return;

            case IntroBabyDiscoveryDemoPhase.StoppedAndLooking:
                if (introCrossfadeTimer == 0)
                {
                    DemoPhase = IntroBabyDiscoveryDemoPhase.Ending;
                    demo.SwitchTo(DemoInputRecordingId.IntroBabyDiscoveryEnd);
                }
                return;

            case IntroBabyDiscoveryDemoPhase.Ending:
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(IntroBabyDiscoveryDemoPhase)} {DemoPhase}.");
        }
    }

    /// <summary>Runs the discovery demo recording's end cue, disabling input and both Samus handlers for subsequent dispatches.</summary>
    private void HandleDemoCue(DemoInputCue cue)
    {
        if (cue != DemoInputCue.EndIntroBabyDiscovery)
            throw new InvalidOperationException($"The Baby-discovery demo handles no {cue} cue.");

        // The scene's end replaces both Samus handlers with ones that do nothing, disables demo input, and
        // returns into the following shared delete step. Samus is never processed again.
        demo.Disable();
        Samus.Control.LockHandlers();
        samusHandlersEnded = true;
    }

    /// <summary>Spawns hatch particles or requests the page-three transition from the egg's animation.</summary>
    private void HandleEggCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.SpawnParticles:
                // The six pieces of the shell spawn in their spawn order, which is the order they update and draw in.
                foreach (IntroEggShellFragment fragment in IntroEggEffectDefinitions.ShellFragments)
                    eggParticles.Add(new IntroEggParticle(fragment, shellFallPastCurve));
                audio?.QueueSound(
                    IntroCinematicDefinitions.Objects.EggHatch,
                    priority: IntroCinematicDefinitions.Objects.MaximumQueuedSounds);
                return;

            case CinematicSpriteCueKind.StartIntroPage3:
                // This cue switches the outer cinematic phase to page three. The state
                // owner performs the palette transition; the actor merely reports the request.
                PageThreeRequested = true;
                return;

            case CinematicSpriteCueKind.SetFrameBehavior:
            case CinematicSpriteCueKind.EndZebesExplosion:
            case CinematicSpriteCueKind.ExplosionFinale:
            case CinematicSpriteCueKind.FadeExplosionPalette:
            case CinematicSpriteCueKind.FadeInZebesText:
            case CinematicSpriteCueKind.FadeOutZebesText:
            case CinematicSpriteCueKind.GreyOut:
            case CinematicSpriteCueKind.Launch:
            case CinematicSpriteCueKind.LaunchHead:
            case CinematicSpriteCueKind.PrepareHead:
            case CinematicSpriteCueKind.Shoot:
            case CinematicSpriteCueKind.SpawnClearTime:
            case CinematicSpriteCueKind.SpawnColon:
            case CinematicSpriteCueKind.SpawnCompletedText:
            case CinematicSpriteCueKind.SpawnExplosionSilhouette:
            case CinematicSpriteCueKind.SpawnFinalExplosionWave:
            case CinematicSpriteCueKind.SpawnHoursTens:
            case CinematicSpriteCueKind.SpawnHoursUnits:
            case CinematicSpriteCueKind.SpawnInitialExplosionWave:
            case CinematicSpriteCueKind.SpawnJapaneseZebesTextIfNeeded:
            case CinematicSpriteCueKind.SpawnMinutesTens:
            case CinematicSpriteCueKind.SpawnMinutesUnits:
            case CinematicSpriteCueKind.SpawnStrikingRinkaPair:
            case CinematicSpriteCueKind.SpawnMissingRinkaPair:
            case CinematicSpriteCueKind.SpawnSuitedJump:
            case CinematicSpriteCueKind.SpawnSuitlessJump:
            case CinematicSpriteCueKind.StartFlyingToZebes:
            case CinematicSpriteCueKind.StartIntroPage2:
            case CinematicSpriteCueKind.StartIntroPage4:
            case CinematicSpriteCueKind.StartIntroPage5:
            case CinematicSpriteCueKind.StartRinkaMoving:
            case CinematicSpriteCueKind.StartZebesExplosion:
            case CinematicSpriteCueKind.TransitionToCredits:
            default:
                throw new InvalidOperationException($"The Metroid egg programs publish no {cue} cue.");
        }
    }

    /// <summary>Advances the baby from hatch wait through rise, idle, and crossfade dance phases.</summary>
    private void StepConfusedBaby(int introCrossfadeTimer)
    {
        switch (confusedBaby.FrameBehavior)
        {
            case CinematicSpriteFrameBehavior.ConfusedBabyWaitingForHatch:
                // The confused baby watches the egg's *next* animation step reach the first fully-hatched frame:
                // the egg's animation is then MetroidEggRemnantsSettling or a later one, so the baby starts moving on that
                // exact handoff.
                if (egg.CurrentAnimation is CinematicSpriteSegment.MetroidEggRemnantsSettling or
                    CinematicSpriteSegment.MetroidEggHatched or CinematicSpriteSegment.Delete)
                {
                    confusedBaby.InstallFrameBehavior(
                        CinematicSpriteFrameBehavior.ConfusedBabyHatched);
                    BabyVelocity = BabyVelocity.WithY(default);
                }
                return;

            case CinematicSpriteFrameBehavior.ConfusedBabyHatched:
                StepHatchedBaby();
                return;

            case CinematicSpriteFrameBehavior.ConfusedBabyIdling:
                if (--BabyIdleTimer <= 0)
                {
                    confusedBaby.InstallFrameBehavior(
                        CinematicSpriteFrameBehavior.ConfusedBabyDancing);
                    BabyIdleTimer = 0;
                    BabyVelocity = BabyVelocity.WithY(default);
                    BabyDanceFrames = 0;
                }
                return;

            case CinematicSpriteFrameBehavior.ConfusedBabyDancing:
                StepDancingBaby(introCrossfadeTimer);
                return;

            case CinematicSpriteFrameBehavior.NoOp:
            case CinematicSpriteFrameBehavior.MetroidEgg:
            case CinematicSpriteFrameBehavior.MetroidEggDeleteAfterCrossFade:
            case CinematicSpriteFrameBehavior.MetroidEggParticle:
            case CinematicSpriteFrameBehavior.MetroidEggSlimeDrops:
            case CinematicSpriteFrameBehavior.BabyMetroidBeingDelivered:
            case CinematicSpriteFrameBehavior.BabyMetroidBeingExamined:
            case CinematicSpriteFrameBehavior.IntroMotherBrainCrossFading:
            case CinematicSpriteFrameBehavior.IntroRinkaMovingHitsSamus:
            case CinematicSpriteFrameBehavior.IntroRinkaMovingMissesSamus:
            case CinematicSpriteFrameBehavior.CeresStars:
            case CinematicSpriteFrameBehavior.CeresLargeAsteroids:
            case CinematicSpriteFrameBehavior.CeresUnderAttack:
            case CinematicSpriteFrameBehavior.CeresSmallAsteroid:
            case CinematicSpriteFrameBehavior.CeresPurpleSpaceVortex:
            case CinematicSpriteFrameBehavior.CeresStationaryVortexNoOp:
            case CinematicSpriteFrameBehavior.CeresExplosionSpawnerRepeating:
            case CinematicSpriteFrameBehavior.CeresExplosion:
            case CinematicSpriteFrameBehavior.Zebes:
            case CinematicSpriteFrameBehavior.ZebesSlideSceneAway:
            case CinematicSpriteFrameBehavior.ZebesStarsCompletionWait:
            case CinematicSpriteFrameBehavior.ZebesStarsCompletionSlideSceneAway:
            case CinematicSpriteFrameBehavior.ZebesStarsWait:
            case CinematicSpriteFrameBehavior.ZebesStarsSlideSceneAway:
            case CinematicSpriteFrameBehavior.SuperMetroidIconTopHalf:
            case CinematicSpriteFrameBehavior.SuperMetroidIconBottomHalf:
            case CinematicSpriteFrameBehavior.ZebesExplosionStarsWaitForFlyaway:
            case CinematicSpriteFrameBehavior.ZebesExplosionStarsMoving:
            case CinematicSpriteFrameBehavior.RewardSuitlessUpperNoOp:
            case CinematicSpriteFrameBehavior.RewardBodyFlight:
            case CinematicSpriteFrameBehavior.RewardHeadFlight:
            case CinematicSpriteFrameBehavior.RewardLanding:
            default:
                throw new InvalidOperationException($"The confused baby does not run {confusedBaby.FrameBehavior}.");
        }
    }

    /// <summary>Subpixel velocity used by the baby's rise and two-axis dance.</summary>
    private EightEightVector<ScreenPixels> BabyVelocity { get; set; }

    /// <summary>Countdown from the apex of the baby's rise to the start of its dance.</summary>
    private int BabyIdleTimer { get; set; }

    /// <summary>The frames the baby has danced, counted up to 128; it cries as the count reaches 64 and 128.</summary>
    private int BabyDanceFrames { get; set; }

    /// <summary>Spawns slime at the authored row and accelerates the baby toward its hover point.</summary>
    private void StepHatchedBaby()
    {
        // Equality is intentional: the hatched baby tests for equality, and the actor's subpixel integration
        // reaches this row once. Four init parameters select four distinct arcs.
        if (confusedBaby.WholePosition.Y == 145 && slimeDrops.Count == 0)
        {
            for (int index = 0; index < IntroEggEffectDefinitions.SlimeDropCount; index++)
            {
                slimeDrops.Add(new IntroEggSlimeDrop(confusedBaby.WholePosition, index));
            }
            // The hatched baby publishes the first confused cry at the same equality edge
            // that creates the four egg-slime drops.
            audio?.QueueSound(
                IntroCinematicDefinitions.Objects.BabyCry1,
                priority: IntroCinematicDefinitions.Objects.MaximumQueuedSounds);
        }

        // The hatched baby accelerates toward 32 pixels above Samus, clamping to +/-544/256 in 1/256 pixels.
        int targetY = CoordinateAxis.Wrap(Samus.Position.Y.Whole - 32);
        BabyVelocity = BabyVelocity.WithY(AccelerateToward(
            confusedBaby.WholePosition.Y,
            targetY,
            BabyVelocity.Y,
            limit: VerticalSpeedLimit));
        AddEightEightVelocity(confusedBaby, axis: Axis.Vertical, BabyVelocity.Y);

        // The first non-negative velocity marks the top of the initial arc. The game
        // then holds for 128 frames before enabling the two-axis dance around Samus.
        if (!BabyVelocity.Y.IsNegative)
        {
            BabyIdleTimer = 128;
            confusedBaby.InstallFrameBehavior(
                CinematicSpriteFrameBehavior.ConfusedBabyIdling);
        }
    }

    /// <summary>Moves the baby around Samus, switches their draw order, and ends on crossfade completion.</summary>
    private void StepDancingBaby(int introCrossfadeTimer)
    {
        if (introCrossfadeTimer == 0)
        {
            confusedBaby.Delete();
            // The dancing baby's drift.
            SamusDisplay = IntroSamusDisplay.Hidden;
            return;
        }

        if (BabyDanceFrames < 128)
        {
            BabyDanceFrames++;
            // The dancing baby cries when its count of frames danced reaches 64 and 128.
            if (IntroCadenceDefinitions.BabyCriesEverySixtyFourthDanceUpdate.IsDue(BabyDanceFrames))
                audio?.QueueSound(
                    IntroCinematicDefinitions.Objects.BabyCry1,
                    priority: IntroCinematicDefinitions.Objects.MaximumQueuedSounds);
        }

        BabyVelocity = BabyVelocity.WithX(AccelerateToward(
            confusedBaby.WholePosition.X,
            Samus.Position.X.Whole,
            BabyVelocity.X,
            limit: HorizontalSpeedLimit));
        // A baby moving left draws over Samus; otherwise Samus draws first.
        SamusDisplay = BabyVelocity.X.IsNegative
            ? IntroSamusDisplay.ObjectsFirst
            : IntroSamusDisplay.SamusFirst;
        AddEightEightVelocity(confusedBaby, axis: Axis.Horizontal, BabyVelocity.X);

        int targetY = CoordinateAxis.Wrap(Samus.Position.Y.Whole - 8);
        BabyVelocity = BabyVelocity.WithY(AccelerateToward(
            confusedBaby.WholePosition.Y,
            targetY,
            BabyVelocity.Y,
            limit: VerticalSpeedLimit));
        AddEightEightVelocity(confusedBaby, axis: Axis.Vertical, BabyVelocity.Y);
    }

    /// <summary>The hatched baby's fastest vertical speed either way.</summary>
    private static readonly SignedEightEight VerticalSpeedLimit = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(544);

    /// <summary>The dancing baby's fastest horizontal speed either way.</summary>
    private static readonly SignedEightEight HorizontalSpeedLimit = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(640);

    /// <summary>How much the baby's speed changes toward its target on each update.</summary>
    private static readonly SignedEightEight BabyAcceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(32);

    /// <summary>
    /// Adjusts a 1/256-pixel velocity toward a target by one acceleration step without passing <paramref name="limit"/> either
    /// way; every step keeps the speed within the limit, so the step stays far inside the velocity's range.
    /// </summary>
    private static SignedEightEight AccelerateToward(
        int position,
        int target,
        SignedEightEight current,
        SignedEightEight limit) =>
        CoordinateAxis.Difference(target, position) >= 0
            ? SignedEightEight.Min(limit, current + BabyAcceleration)
            : SignedEightEight.Max(-limit, current - BabyAcceleration);

    /// <summary>Integrates one 1/256-pixel velocity component into the actor's subpixel position.</summary>
    private static void AddEightEightVelocity(
        IntroDiscoverySprite sprite,
        Axis axis,
        SignedEightEight velocity)
    {
        sprite.Position = axis == Axis.Horizontal
            ? sprite.Position.WithX(IntroCinematicMotion.AddEightEight(sprite.Position.X, velocity))
            : sprite.Position.WithY(IntroCinematicMotion.AddEightEight(sprite.Position.Y, velocity));
    }

    /// <summary>Instantiates a cinematic actor with its authored position, palette, animation, and frame behaviour.</summary>
    private static IntroDiscoverySprite CreateActor(IntroBabyActorDefinition definition)
    {
        var actor = new IntroDiscoverySprite(
            definition.Position,
            IntroShades.Drawn(definition.Colouring),
            definition.Animation);
        actor.InstallFrameBehavior(definition.FrameBehavior);
        return actor;
    }

    /// <summary>Creates the discovery collision room: its terrain, with every block's behaviour cleared.</summary>
    private static RoomLevelData CreateLevel()
    {
        const int width = IntroBabyDiscoveryCollisionDefinitions.Columns;
        const int height = IntroBabyDiscoveryCollisionDefinitions.RoomRows;
        RoomTerrainBlock[] foreground = IntroBabyDiscoveryCollision.CreateForeground();
        var behavior = new RoomBlockBehavior[width * height];
        behavior.AsSpan().Fill(RoomBlockBehavior.None);
        return new RoomLevelData(new Extent16<RoomBlocks>(width, height),
            new RoomLevelPlanes(foreground, behavior, new RoomBlockAppearance[width * height]), RoomLevelData.NothingBeyondGrid());
    }
}
