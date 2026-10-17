using SuperMetroid.Core.Input;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Cartridge-backed setup and first actor phase of the SR388 baby-Metroid discovery scene.
/// </summary>
internal sealed class IntroBabyDiscoveryState
{

    /// <summary>Receives egg-hatch and baby-cry sound requests when audio is available.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Feeds the scripted pose and movement input to Samus.</summary>
    internal readonly DemoInputState demo = new();
    /// <summary>Particles emitted when the egg begins hatching.</summary>
    internal readonly List<IntroEggParticle> eggParticles = [];
    /// <summary>Drops emitted as the baby rises past the hatch scanline.</summary>
    private readonly List<IntroEggSlimeDrop> slimeDrops = [];
    /// <summary>The egg actor whose list pointer controls the hatch sequence.</summary>
    internal readonly IntroDiscoverySprite egg;
    /// <summary>The baby actor that waits for the egg's completed frame before moving.</summary>
    internal readonly IntroDiscoverySprite confusedBaby;

    /// <summary>Sets up the native scene's Samus state, partial room collision map, actors, and demo input list.</summary>
    public IntroBabyDiscoveryState(CartridgeAudioState? audio = null,
        SamusState? existingSamus = null)
    {

        this.audio = audio;

        egg = CreateActor(IntroBabyActorDefinitions.Egg);
        confusedBaby = CreateActor(IntroBabyActorDefinitions.ConfusedBaby);

        // Native scene setup reuses Samus WRAM, including transition history. Standalone
        // diagnostics may start with a fresh owner, but the full intro carries it forward.
        Samus = existingSamus ?? new SamusState();
        Samus.Pose = SamusPoseId.FacingLeftNormalPose;
        Samus.Position = Samus.Position.WithWhole(new(376, 147));
        Samus.SelectedHudItem = HudItem.Beam;
        Samus.PoseMetadata.RefreshCollisionRadii();
        // $8B:AF95-$AF99: the flashback leaves Samus in pose two already, so her animation
        // keeps running rather than restarting.
        Samus.Presentation.SetAnimationFrameIfPoseChanged();
        Samus.CommitPoseHistory();
        Samus.Presentation.PrimeGraphics();

        // $8B:AFDF copies exactly $300 bytes into a room declared 32x16 blocks. The final
        // 128 foreground words and all BTS bytes retain the earlier zero initialization.
        Level = CreateLevel();

        demo.Clear();
        demo.Enable();
        demo.Load(IntroDemoInputObjectDefinitions.IntroBabyDiscovery, initializationParameter: 0);
    }

    /// <summary>Samus' scene state, carried from the earlier intro when supplied.</summary>
    public SamusState Samus { get; }

    /// <summary>The 32 by 16 discovery room with the cartridge's partial foreground copy.</summary>
    public RoomLevelData Level { get; }

    /// <summary>Signals the egg actor's permanent switch to its hatching list.</summary>
    public bool EggHatchingStarted { get; private set; }

    /// <summary>Signals the outer cinematic owner to start page three's palette transition.</summary>
    public bool PageThreeRequested { get; private set; }

    /// <summary>
    /// $1A57 for this scene. $8B:B00F sets it positive; the egg and dancing-baby
    /// pre-instructions later reorder or clear it.
    /// </summary>
    public IntroSamusDisplay SamusDisplay { get; private set; } = IntroSamusDisplay.ObjectsFirst;

    /// <summary>True once $91:8682 has pointed both Samus state handlers at an RTL.</summary>
    private bool samusHandlersEnded;

    /// <summary>
    /// Samus, her demo input and the cinematic sprite objects for one dispatch. All run after
    /// the cinematic function, so $8B:AF6C's setup dispatch already plays the demo's first
    /// entry, moves Samus and steps the egg and baby.
    /// </summary>
    public void Step(ushort nmiFrameCounter, ushort introCrossfadeTimer)
    {
        // $8B:8E0D runs Samus's handlers, ahead of the sprite pass, only while $1A57 is set.
        if (SamusDisplay != IntroSamusDisplay.Hidden && !samusHandlersEnded)
        {
            // $90:E91D: the demo pose-input handler runs DemoInputObjectHandler inside Samus's
            // current-state handler, ahead of her pose input and movement.
            demo.Step((_, preInstruction) => RunDemoPreInstruction(preInstruction, introCrossfadeTimer), HandleDemoCue);
            // $90:E70D ends the current-state handler with ResetMovementAndPoseChangeVariables,
            // before this dispatch's movement records new distances.
            Samus.PoseTransitions.ClearPoseTransitionShotDirection();
            Samus.ProjectileInheritance.ClearMovement();

            // An end instruction read above already replaced the new-state handler, so this
            // dispatch skips Samus's movement and animation.
            if (!samusHandlersEnded)
            {
                // The native intro-demo alpha publishes the current pose's collision radius
                // before input/movement, rather than requiring pose setters to publish early.
                Samus.PoseMetadata.RefreshCollisionRadii();
                IntroSamusDemoMovement.StepGroundedLeft(

                    Level,
                    Samus,
                    demo.Held,
                    demo.NewlyPressed,
                    nmiFrameCounter);
            }
        }

        // The egg's $A8E8 pre-instruction tests Samus's post-movement X during the later
        // cinematic-sprite pass. It permanently redirects the list once X is below $A9.
        if (egg.PreInstructionPointer == IntroBabyActorDefinitions.Egg.PreInstruction &&
            unchecked((short)(Samus.Position.X.Whole - 169)) < 0)
        {
            egg.Start(CinematicSpriteProgram.MetroidEggHatching);
            egg.PreInstructionPointerForDiscovery(
                CinematicCodePointers.CinematicSpriteObject_PreInstruction_NoOp);
            EggHatchingStarted = true;
        }

        // $A903 replaces the egg with the shared delete list only when page three's reverse
        // crossfade reaches zero. The egg's program installs it after starting page three.
        if (egg.ListPreInstruction == CinematicSpritePreInstruction.MetroidEggDeleteAfterCrossFade &&
            introCrossfadeTimer == 0)
        {
            egg.Start(CinematicSpriteProgram.Delete);
            // $8B:A914: page three's text hides Samus.
            SamusDisplay = IntroSamusDisplay.Hidden;
        }
        egg.Step(HandleEggCue);

        // The baby slot follows the egg slot in the native descending actor traversal, so
        // it observes the egg's freshly advanced list pointer in this same frame.
        StepConfusedBaby(introCrossfadeTimer);
        confusedBaby.Step();

        foreach (IntroEggParticle particle in eggParticles)
            particle.Step();
        foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
            slimeDrop.Step();
    }

    /// <summary>Draws egg, baby, and hatch effects in the cinematic object order used by this scene.</summary>
    public void DrawActors(OamBuffer oam,
        IntroEggEffectSpritePresentation? eggEffectArt = null,
        IntroDiscoveryActorSpritePresentation? actorArt = null)
    {
        // IntroSamusDisplayFlag=+1 makes cinematic objects enter OAM before Samus. The egg
        // was spawned before the confused-baby object and retains its own list and timer.
        egg.Draw(oam, installedArt: actorArt);
        confusedBaby.Draw(oam, installedArt: actorArt);
        foreach (IntroEggParticle particle in eggParticles)
            particle.Draw(oam, eggEffectArt);
        foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
            slimeDrop.Draw(oam, eggEffectArt);
    }

    /// <summary>Changes the demo from running to waiting or ending at its scripted position and crossfade edges.</summary>
    private void RunDemoPreInstruction(DemoInputPreInstruction preInstruction, ushort introCrossfadeTimer)
    {
        // The Baby-discovery object holds only its own two routines or the return-only one.
        switch (preInstruction)
        {
            case DemoInputPreInstruction.IntroBabyRunningLeft:
                if (Samus.Position.X.Whole < 178)
                    demo.Redirect(
                        DemoInputPreInstruction.IntroBabyStopAndLook,
                        DemoInputProgram.IntroBabyDiscoveryStopAndLook);
                return;

            case DemoInputPreInstruction.IntroBabyStopAndLook:
                if (introCrossfadeTimer == 0)
                    demo.Redirect(
                        DemoInputPreInstruction.ReturnOnly,
                        DemoInputProgram.IntroBabyDiscoveryEnd);
                return;

            case DemoInputPreInstruction.ReturnOnly:
                return;

            case DemoInputPreInstruction.None:
            case DemoInputPreInstruction.AttractCheckLeave:
            case DemoInputPreInstruction.AttractShinespark:
                throw new InvalidOperationException($"The Baby-discovery demo object cannot hold the {preInstruction} pre-instruction.");

            default:
                throw new InvalidOperationException($"Undefined {nameof(DemoInputPreInstruction)} {preInstruction}.");
        }
    }

    /// <summary>Runs the discovery list's end routine, disabling input and both Samus handlers for subsequent dispatches.</summary>
    private void HandleDemoCue(DemoInputCue cue)
    {
        if (cue != DemoInputCue.EndIntroBabyDiscovery)
            throw new InvalidOperationException($"The Baby-discovery demo calls no {cue} routine.");

        // $91:8682 replaces both Samus handlers with RTL $90:E8CD, disables demo input, and
        // returns into the following shared delete opcode. Samus is never processed again.
        demo.Disable();
        Samus.InputLocked = true;
        samusHandlersEnded = true;
    }

    /// <summary>Spawns hatch particles or requests the page-three transition from the egg's list.</summary>
    private void HandleEggCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.SpawnParticles:
                // The six JSR Spawn calls at $A918..A94C use definitions CECD through CEEB
                // and init parameters zero through five, in this exact order.
                for (byte index = 0; index < IntroEggEffectDefinitions.ParticleCount; index++)
                    eggParticles.Add(new IntroEggParticle(index));
                audio?.QueueSound(
                    IntroCinematicRomData.Objects.EggHatch,
                    maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
                return;

            case CinematicSpriteCueKind.StartIntroPage3:
                // This cue switches the outer cinematic function to page three. The state
                // owner performs the palette transition; the actor merely reports the request.
                PageThreeRequested = true;
                return;

            default:
                throw new InvalidOperationException($"The Metroid egg programs publish no {cue} cue.");
        }
    }

    /// <summary>Advances the baby from hatch wait through rise, idle, and crossfade dance phases.</summary>
    private void StepConfusedBaby(ushort introCrossfadeTimer)
    {
        switch (CinematicInstructionWords.DecodePreInstruction<ConfusedBabyPreInstruction>(
                    confusedBaby.PreInstructionPointer, confusedBaby.CurrentProgram))
        {
            case ConfusedBabyPreInstruction.WaitingForHatch:
                // $BA5E watches the egg's *next* instruction pointer reach CB79, the first fully-hatched frame list:
                // the egg's program is then MetroidEggHatchedFrame2 or a later one, so the baby starts moving on that
                // exact handoff.
                if (egg.CurrentProgram is CinematicSpriteProgram.MetroidEggHatchedFrame2 or
                    CinematicSpriteProgram.MetroidEggHatched or CinematicSpriteProgram.Delete)
                {
                    confusedBaby.PreInstructionPointerForDiscovery(
                        (ushort)ConfusedBabyPreInstruction.Hatched);
                    BabyVelocity = BabyVelocity.WithY(default);
                }
                return;

            case ConfusedBabyPreInstruction.Hatched:
                StepHatchedBaby();
                return;

            case ConfusedBabyPreInstruction.Idling:
                BabyIdleTimer = BabyIdleTimer.Decrement().Remaining;
                if (unchecked((short)BabyIdleTimer.RawValue) <= 0)
                {
                    confusedBaby.PreInstructionPointerForDiscovery(
                        (ushort)ConfusedBabyPreInstruction.Dancing);
                    BabyIdleTimer = NativeCountdown.Start(0);
                    BabyVelocity = BabyVelocity.WithY(default);
                    confusedBaby.GeneralTimer = 0;
                }
                return;

            case ConfusedBabyPreInstruction.Dancing:
                StepDancingBaby(introCrossfadeTimer);
                return;

            default:
                throw new InvalidOperationException($"Undefined ConfusedBabyPreInstruction {confusedBaby.PreInstructionPointer}.");
        }
    }

    /// <summary>Subpixel velocity used by the baby's rise and two-axis dance.</summary>
    private EightEightVector<ScreenPixels> BabyVelocity { get; set; }

    /// <summary>Countdown from the apex of the baby's rise to the start of its dance.</summary>
    private NativeCountdown BabyIdleTimer { get; set; }

    /// <summary>Spawns slime at the authored scanline and accelerates the baby toward its hover point.</summary>
    private void StepHatchedBaby()
    {
        // Equality is intentional: $BA76 uses BNE, and the actor's subpixel integration
        // reaches this scanline once. Four init parameters select four distinct arcs.
        if (confusedBaby.WholePosition.Y == 145 && slimeDrops.Count == 0)
        {
            for (byte index = 0; index < IntroEggEffectDefinitions.SlimeDropCount; index++)
            {
                slimeDrops.Add(new IntroEggSlimeDrop(confusedBaby.WholePosition, index));
            }
            // `$8B:BA73` publishes the first confused cry at the same equality edge
            // that creates the four egg-slime drops.
            audio?.QueueSound(
                IntroCinematicRomData.Objects.BabyCry1,
                maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
        }

        // $BA73 accelerates toward 32 pixels above Samus, clamping to +/-$220 in 8.8.
        ushort targetY = unchecked((ushort)(Samus.Position.Y.Whole - 32));
        BabyVelocity = BabyVelocity.WithY(AccelerateToward(
            confusedBaby.WholePosition.Y,
            targetY,
            BabyVelocity.Y,
            positiveLimit: 544,
            negativeLimit: -544));
        AddEightEightVelocity(confusedBaby, horizontal: false, BabyVelocity.Y);

        // The first non-negative velocity marks the top of the initial arc. Native code
        // then holds for $80 frames before enabling the two-axis dance around Samus.
        if (unchecked((short)BabyVelocity.Y.RawValue) >= 0)
        {
            BabyIdleTimer = NativeCountdown.Start(128);
            confusedBaby.PreInstructionPointerForDiscovery(
                (ushort)ConfusedBabyPreInstruction.Idling);
        }
    }

    /// <summary>Moves the baby around Samus, switches their draw order, and ends on crossfade completion.</summary>
    private void StepDancingBaby(ushort introCrossfadeTimer)
    {
        if (introCrossfadeTimer == 0)
        {
            confusedBaby.Delete();
            // $8B:BB35.
            SamusDisplay = IntroSamusDisplay.Hidden;
            return;
        }

        if (confusedBaby.GeneralTimer < 128)
        {
            confusedBaby.GeneralTimer++;
            // `$8B:BB24` cries when the post-increment timer reaches $40 and $80.
            if (confusedBaby.GeneralTimer % 64 == 0)
                audio?.QueueSound(
                    IntroCinematicRomData.Objects.BabyCry1,
                    maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
        }

        BabyVelocity = BabyVelocity.WithX(AccelerateToward(
            confusedBaby.WholePosition.X,
            Samus.Position.X.Whole,
            BabyVelocity.X,
            positiveLimit: 640,
            negativeLimit: -640));
        // $8B:BB79-$BB9E: a baby moving left draws over Samus; otherwise Samus draws first.
        SamusDisplay = unchecked((sbyte)(SnesEightEight.WholeOf(BabyVelocity.X.RawValue))) < 0
            ? IntroSamusDisplay.ObjectsFirst
            : IntroSamusDisplay.SamusFirst;
        AddEightEightVelocity(confusedBaby, horizontal: true, BabyVelocity.X);

        ushort targetY = unchecked((ushort)(Samus.Position.Y.Whole - 8));
        BabyVelocity = BabyVelocity.WithY(AccelerateToward(
            confusedBaby.WholePosition.Y,
            targetY,
            BabyVelocity.Y,
            positiveLimit: 544,
            negativeLimit: -544));
        AddEightEightVelocity(confusedBaby, horizontal: false, BabyVelocity.Y);
    }

    /// <summary>Adjusts an 8.8 velocity toward a target by one $20 step without exceeding its limits.</summary>
    private static SnesSignedEightEight AccelerateToward(
        ushort position,
        ushort target,
        SnesSignedEightEight current,
        short positiveLimit,
        short negativeLimit)
    {
        int velocity = unchecked((short)current.RawValue);
        if (unchecked((short)(target - position)) >= 0)
            velocity = Math.Min(positiveLimit, velocity + 32);
        else
            velocity = Math.Max(negativeLimit, velocity - 32);
        return new(unchecked((ushort)velocity));
    }

    /// <summary>Integrates one 8.8 velocity component into the actor's subpixel position.</summary>
    private static void AddEightEightVelocity(
        IntroDiscoverySprite sprite,
        bool horizontal,
        SnesSignedEightEight velocity)
    {
        sprite.Position = horizontal
            ? sprite.Position.WithX(IntroCinematicMotion.AddEightEight(sprite.Position.X, velocity))
            : sprite.Position.WithY(IntroCinematicMotion.AddEightEight(sprite.Position.Y, velocity));
    }

    /// <summary>Instantiates a cinematic actor with its authored position, palette, list, and pre-instruction.</summary>
    private static IntroDiscoverySprite CreateActor(IntroBabyActorDefinition definition)
    {
        var actor = new IntroDiscoverySprite(
            definition.Position,
            definition.PaletteBits,
            definition.Program);
        actor.PreInstructionPointerForDiscovery(definition.PreInstruction);
        return actor;
    }

    /// <summary>Creates the discovery collision room with zero-filled cells beyond the $300-byte foreground copy.</summary>
    private static RoomLevelData CreateLevel()
    {
        const int width = IntroBabyDiscoveryCollisionDefinitions.Columns;
        const int height = IntroBabyDiscoveryCollisionDefinitions.RoomRows;
        ushort[] foreground = IntroBabyDiscoveryCollisionDefinitions.CreateForeground();
        return new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(foreground, new byte[width * height], new ushort[width * height]), Array.Empty<byte>());
    }
}
