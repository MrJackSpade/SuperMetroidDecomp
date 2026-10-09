using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Cartridge-backed setup and first actor phase of the SR388 baby-Metroid discovery scene.
/// </summary>
internal sealed class IntroBabyDiscoveryState
{
    /// <summary>SNES memory view used for demo input, actor initialization, and cinematic sprite operations.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Optional audio queue receiving the discovery scene's baby and egg sound events.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Reader for demo words, defaulting to the scene's cartridge-backed input definitions.</summary>
    private readonly Func<ushort, ushort> demoWordReader;
    /// <summary>Input-object state that advances Samus through the scene's recorded demo sequence.</summary>
    private readonly DemoInputState demo = new();
    /// <summary>Egg fragments created when the hatch instruction runs.</summary>
    private readonly List<IntroEggParticle> eggParticles = [];
    /// <summary>Slime drops created when the hatched baby reaches its authored scanline.</summary>
    private readonly List<IntroEggSlimeDrop> slimeDrops = [];
    /// <summary>Cinematic sprite object for the egg and its changing instruction lists.</summary>
    private readonly IntroDiscoverySprite egg;
    /// <summary>Cinematic sprite object whose pre-instruction advances the confused baby's phases.</summary>
    private readonly IntroDiscoverySprite confusedBaby;

    /// <summary>Initializes Samus, room collision data, demo input, and the egg and baby actors for discovery.</summary>
    /// <param name="bus">Address space used to read cartridge data and publish the scene's WRAM state.</param>
    /// <param name="audio">Optional queue for scene sound effects; null disables audio publication.</param>
    /// <param name="existingSamus">Samus state carried from the preceding flashback, or a new state for an isolated scene.</param>
    /// <param name="demoWordReader">Optional cartridge-word reader for demo input; the scene definition reader is used when omitted.</param>
    public IntroBabyDiscoveryState(ISnesAddressSpace bus, CartridgeAudioState? audio = null,
        SamusState? existingSamus = null, Func<ushort, ushort>? demoWordReader = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio;
        this.demoWordReader = demoWordReader ?? IntroBabyDiscoveryInputDefinitions.ReadWord;

        egg = CreateActor(IntroBabyActorDefinitions.Egg);
        confusedBaby = CreateActor(IntroBabyActorDefinitions.ConfusedBaby);

        // Native scene setup reuses Samus WRAM, including transition history. Standalone
        // diagnostics may start with a fresh owner, but the full intro carries it forward.
        Samus = existingSamus ?? new SamusState();
        Samus.Pose = SamusPoseIds.FacingLeftNormalPose;
        Samus.XPosition = 0x0178;
        Samus.YPosition = 0x0093;
        Samus.SelectedHudItem = 0;
        Samus.RefreshCollisionRadii(bus);
        // $8B:AF95-$AF99: the flashback leaves Samus in pose two already, so her animation
        // keeps running rather than restarting.
        Samus.SetAnimationFrameIfPoseChanged(bus);
        Samus.CommitPoseHistory(bus);
        Samus.PrimeGraphics(bus);

        // $8B:AFDF copies exactly $300 bytes into a room declared 32x16 blocks. The final
        // 128 foreground words and all BTS bytes retain the earlier zero initialization.
        Level = CreateLevel();

        demo.Clear();
        demo.Enable();
        demo.LoadObject(bus, IntroBabyDiscoveryInputDefinitions.HeaderStart,
            definitionWord: this.demoWordReader);
    }

    /// <summary>Samus actor initialized for the flashback-to-discovery handoff and advanced while her handlers remain active.</summary>
    public SamusState Samus { get; }

    /// <summary>Room geometry and collision data used by grounded demo movement.</summary>
    public RoomLevelData Level { get; }

    /// <summary>Becomes true after Samus crosses the egg's hatch trigger and its object list is redirected.</summary>
    public bool EggHatchingStarted { get; private set; }

    /// <summary>Becomes true when the egg instruction requests the outer cinematic to begin page three.</summary>
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
            demo.Step(
                bus,
                preInstruction: (_, pointer) => RunDemoPreInstruction(pointer, introCrossfadeTimer),
                specialInstruction: HandleDemoInstruction,
                instructionWord: demoWordReader);
            // $90:E70D ends the current-state handler with ResetMovementAndPoseChangeVariables,
            // before this dispatch's movement records new distances.
            Samus.ClearPoseTransitionShotDirection();
            SamusProjectileInheritance.ClearMovement(bus);

            // An end instruction read above already replaced the new-state handler, so this
            // dispatch skips Samus's movement and animation.
            if (!samusHandlersEnded)
            {
                // The native intro-demo alpha publishes the current pose's collision radius
                // before input/movement, rather than requiring pose setters to publish early.
                Samus.RefreshCollisionRadii(bus);
                IntroSamusDemoMovement.StepGroundedLeft(
                    bus,
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
            unchecked((short)(Samus.XPosition - 0x00a9)) < 0)
        {
            egg.Redirect(CinematicCodePointers.Lists.MetroidEggHatching);
            egg.PreInstructionPointerForDiscovery(
                CinematicCodePointers.CinematicSpriteObject_PreInstruction_NoOp);
            EggHatchingStarted = true;
        }

        // $A903 replaces the egg with the shared delete list only when page three's reverse
        // crossfade reaches zero. It is installed by list opcode $944C after $B33E.
        if (egg.PreInstructionPointer ==
                CinematicCodePointers.PreInstruction_MetroidEgg_DeleteAfterCrossFade &&
            introCrossfadeTimer == 0)
        {
            egg.Redirect(CinematicCodePointers.Lists.Delete);
            // $8B:A914: page three's text hides Samus.
            SamusDisplay = IntroSamusDisplay.Hidden;
        }
        egg.Step(bus, HandleEggInstruction,
            IntroBabyDiscoveryInstructionDefinitions.ReadWord);

        // The baby slot follows the egg slot in the native descending actor traversal, so
        // it observes the egg's freshly advanced list pointer in this same frame.
        StepConfusedBaby(introCrossfadeTimer);
        confusedBaby.Step(bus, HandleConfusedBabyInstruction,
            IntroBabyDiscoveryInstructionDefinitions.ReadWord);

        foreach (IntroEggParticle particle in eggParticles)
            particle.Step(bus);
        foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
            slimeDrop.Step(bus);
    }

    /// <summary>Draws the egg, baby, and their spawned effects in cinematic object order.</summary>
    /// <param name="oam">OAM buffer that receives the scene actors and effect sprites.</param>
    /// <param name="eggEffectArt">Optional installed art used for egg particles and slime drops.</param>
    /// <param name="actorArt">Optional installed art used for the egg and baby actors.</param>
    public void DrawActors(OamBuffer oam,
        IntroEggEffectSpritePresentation? eggEffectArt = null,
        IntroDiscoveryActorSpritePresentation? actorArt = null)
    {
        // IntroSamusDisplayFlag=+1 makes cinematic objects enter OAM before Samus. The egg
        // was spawned before the confused-baby object and retains its own list and timer.
        egg.Draw(bus, oam, installedArt: actorArt);
        confusedBaby.Draw(bus, oam, installedArt: actorArt);
        foreach (IntroEggParticle particle in eggParticles)
            particle.Draw(bus, oam, eggEffectArt);
        foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
            slimeDrop.Draw(bus, oam, eggEffectArt);
    }

    /// <summary>Applies the discovery demo's running, stop-and-look, and inert pre-instruction transitions.</summary>
    /// <param name="pointer">Current demo pre-instruction address.</param>
    /// <param name="introCrossfadeTimer">Outer cinematic crossfade countdown used to end the stop-and-look phase.</param>
    private void RunDemoPreInstruction(ushort pointer, ushort introCrossfadeTimer)
    {
        switch (pointer)
        {
            case IntroBabyDiscoveryRomData.RunningLeftPreInstruction:
                if (Samus.XPosition < 0x00b2)
                    demo.Redirect(
                        IntroBabyDiscoveryRomData.StopAndLookPreInstruction,
                        IntroBabyDiscoveryRomData.StopAndLookInputList);
                return;

            case IntroBabyDiscoveryRomData.StopAndLookPreInstruction:
                if (introCrossfadeTimer == 0)
                    demo.Redirect(
                        IntroBabyDiscoveryRomData.InertPreInstruction,
                        IntroBabyDiscoveryRomData.EndInputList);
                return;

            case IntroBabyDiscoveryRomData.InertPreInstruction:
            case IntroBabyDiscoveryRomData.InertPreInstructionAlternate:
                return;

            default:
                // The Baby-discovery object references only the four routines above.
                // Another pointer means the object definition/list pairing is corrupt.
                throw new InvalidDataException(
                    $"Baby-discovery demo names invalid pre-instruction $91:{pointer:X4}.");
        }
    }

    /// <summary>Handles the terminal demo opcode by disabling input and ending Samus's scene handlers.</summary>
    /// <param name="state">Demo input object whose playback is disabled when the end opcode is encountered.</param>
    /// <param name="pointer">Instruction address being dispatched.</param>
    /// <param name="argumentPointer">Cursor after the opcode, returned for continued instruction processing.</param>
    private DemoInputInstructionResult HandleDemoInstruction(
        DemoInputState state,
        ushort pointer,
        ushort argumentPointer)
    {
        if (pointer != IntroBabyDiscoveryRomData.EndDemoInputInstruction)
            return DemoInputInstructionResult.NotHandled(argumentPointer);

        // $91:8682 replaces both Samus handlers with RTL $90:E8CD, disables demo input, and
        // returns into the following shared delete opcode. Samus is never processed again.
        state.Disable();
        Samus.InputLocked = true;
        samusHandlersEnded = true;
        return DemoInputInstructionResult.ContinueAt(argumentPointer);
    }

    /// <summary>Handles egg-specific opcodes that spawn hatch effects or request page three.</summary>
    /// <param name="opcode">Cinematic instruction opcode to inspect.</param>
    /// <param name="argumentPointer">Instruction cursor returned when this handler consumes the opcode.</param>
    /// <returns>The next cursor for handled instructions, or null to defer to the shared interpreter.</returns>
    private ushort? HandleEggInstruction(ushort opcode, ushort argumentPointer)
    {
        switch (opcode)
        {
            case CinematicCodePointers.Instruction_SpawnMetroidEggParticles:
                // The six JSR Spawn calls at $A918..A94C use definitions CECD through CEEB
                // and init parameters zero through five, in this exact order.
                for (byte index = 0; index < IntroEggEffectDefinitions.ParticleCount; index++)
                    eggParticles.Add(new IntroEggParticle(index));
                audio?.QueueSound(
                    IntroCinematicRomData.Objects.EggHatch,
                    maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
                return argumentPointer;

            case CinematicCodePointers.Instruction_StartIntroPage3:
                // This opcode switches the outer cinematic function to page three and then
                // returns without consuming operands. The state owner performs the palette
                // transition; the actor interpreter merely reports the native request.
                PageThreeRequested = true;
                return argumentPointer;

            default:
                return null;
        }
    }

    /// <summary>Queues the cry associated with a baby-metroid cry opcode and leaves unrelated opcodes to the interpreter.</summary>
    /// <param name="opcode">Cinematic instruction opcode to inspect.</param>
    /// <param name="argumentPointer">Instruction cursor returned when a cry opcode is handled.</param>
    /// <returns>The next cursor for a handled cry, or null for an unrelated instruction.</returns>
    private ushort? HandleConfusedBabyInstruction(ushort opcode, ushort argumentPointer)
    {
        byte soundId = opcode switch
        {
            CinematicCodePointers.Instruction_PlayBabyMetroid_Cry1 =>
                IntroCinematicRomData.Objects.BabyCry1.Value,
            CinematicCodePointers.Instruction_PlayBabyMetroid_Cry2 =>
                IntroCinematicRomData.Objects.BabyCry2.Value,
            CinematicCodePointers.Instruction_PlayBabyMetroid_Cry3 =>
                IntroCinematicRomData.Objects.BabyCry3.Value,
            _ => 0,
        };
        if (soundId == 0)
            return null;

        audio?.QueueSound(
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, soundId),
            maximumQueued: 6);
        return argumentPointer;
    }

    /// <summary>Dispatches the baby's hatch ascent, idle countdown, or dancing behavior by native pre-instruction state.</summary>
    /// <param name="introCrossfadeTimer">Outer page crossfade countdown that ends the dancing phase.</param>
    private void StepConfusedBaby(ushort introCrossfadeTimer)
    {
        switch (confusedBaby.PreInstructionPointer)
        {
            case IntroBabyActorDefinitions.ConfusedBabyInitialPreInstruction:
                // $BA5E watches the egg's *next* instruction pointer. CB79 is the first
                // fully-hatched frame list, so the baby starts moving on that exact handoff.
                if (egg.InstructionPointer >= CinematicCodePointers.Lists.MetroidEggHatchedFrame2)
                {
                    confusedBaby.PreInstructionPointerForDiscovery(
                        CinematicCodePointers.PreInstruction_ConfusedBabyMetroid_Hatched);
                    BabyYVelocity = 0;
                }
                return;

            case CinematicCodePointers.PreInstruction_ConfusedBabyMetroid_Hatched:
                StepHatchedBaby();
                return;

            case CinematicCodePointers.PreInstruction_ConfusedBabyMetroid_Idling:
                BabyIdleTimer = unchecked((ushort)(BabyIdleTimer - 1));
                if (unchecked((short)BabyIdleTimer) <= 0)
                {
                    confusedBaby.PreInstructionPointerForDiscovery(
                        CinematicCodePointers.PreInstruction_ConfusedBabyMetroid_Dancing);
                    BabyIdleTimer = 0;
                    BabyYVelocity = 0;
                    confusedBaby.GeneralTimer = 0;
                }
                return;

            case CinematicCodePointers.PreInstruction_ConfusedBabyMetroid_Dancing:
                StepDancingBaby(introCrossfadeTimer);
                return;

            default:
                throw new InvalidDataException(
                    $"Confused-baby sprite names invalid pre-instruction $8B:{confusedBaby.PreInstructionPointer:X4}.");
        }
    }

    /// <summary>Horizontal 8.8 velocity retained while the baby accelerates toward Samus during its dance.</summary>
    private ushort BabyXVelocity { get; set; }

    /// <summary>Vertical 8.8 velocity retained during the hatch arc and later movement toward Samus.</summary>
    private ushort BabyYVelocity { get; set; }

    /// <summary>Countdown between the initial hatch ascent and the start of the dancing phase.</summary>
    private ushort BabyIdleTimer { get; set; }

    /// <summary>Advances the hatch arc, spawns slime drops at the authored Y position, and starts the idle delay at the apex.</summary>
    private void StepHatchedBaby()
    {
        // Equality is intentional: $BA76 uses BNE, and the actor's subpixel integration
        // reaches this scanline once. Four init parameters select four distinct arcs.
        if (confusedBaby.YPosition == 0x0091 && slimeDrops.Count == 0)
        {
            for (byte index = 0; index < IntroEggEffectDefinitions.SlimeDropCount; index++)
            {
                slimeDrops.Add(new IntroEggSlimeDrop(
                    confusedBaby.XPosition,
                    confusedBaby.YPosition,
                    index));
            }
            // `$8B:BA73` publishes the first confused cry at the same equality edge
            // that creates the four egg-slime drops.
            audio?.QueueSound(
                IntroCinematicRomData.Objects.BabyCry1,
                maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
        }

        // $BA73 accelerates toward 32 pixels above Samus, clamping to +/-$220 in 8.8.
        ushort targetY = unchecked((ushort)(Samus.YPosition - 0x0020));
        BabyYVelocity = AccelerateToward(
            confusedBaby.YPosition,
            targetY,
            BabyYVelocity,
            positiveLimit: 0x0220,
            negativeLimit: unchecked((short)0xfde0));
        AddEightEightVelocity(confusedBaby, horizontal: false, BabyYVelocity);

        // The first non-negative velocity marks the top of the initial arc. Native code
        // then holds for $80 frames before enabling the two-axis dance around Samus.
        if (unchecked((short)BabyYVelocity) >= 0)
        {
            BabyIdleTimer = 0x0080;
            confusedBaby.PreInstructionPointerForDiscovery(
                CinematicCodePointers.PreInstruction_ConfusedBabyMetroid_Idling);
        }
    }

    /// <summary>Moves the baby toward Samus, updates draw ordering, emits periodic cries, and deletes it when the crossfade ends.</summary>
    /// <param name="introCrossfadeTimer">Outer cinematic crossfade countdown; zero terminates the baby actor.</param>
    private void StepDancingBaby(ushort introCrossfadeTimer)
    {
        if (introCrossfadeTimer == 0)
        {
            confusedBaby.Delete();
            // $8B:BB35.
            SamusDisplay = IntroSamusDisplay.Hidden;
            return;
        }

        if (confusedBaby.GeneralTimer < 0x0080)
        {
            confusedBaby.GeneralTimer++;
            // `$8B:BB24` cries when the post-increment timer reaches $40 and $80.
            if ((confusedBaby.GeneralTimer & 0x003f) == 0)
                audio?.QueueSound(
                    IntroCinematicRomData.Objects.BabyCry1,
                    maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
        }

        BabyXVelocity = AccelerateToward(
            confusedBaby.XPosition,
            Samus.XPosition,
            BabyXVelocity,
            positiveLimit: 0x0280,
            negativeLimit: unchecked((short)0xfd80));
        // $8B:BB79-$BB9E: a baby moving left draws over Samus; otherwise Samus draws first.
        SamusDisplay = unchecked((sbyte)(BabyXVelocity >> 8)) < 0
            ? IntroSamusDisplay.ObjectsFirst
            : IntroSamusDisplay.SamusFirst;
        AddEightEightVelocity(confusedBaby, horizontal: true, BabyXVelocity);

        ushort targetY = unchecked((ushort)(Samus.YPosition - 0x0008));
        BabyYVelocity = AccelerateToward(
            confusedBaby.YPosition,
            targetY,
            BabyYVelocity,
            positiveLimit: 0x0220,
            negativeLimit: unchecked((short)0xfde0));
        AddEightEightVelocity(confusedBaby, horizontal: false, BabyYVelocity);
    }

    /// <summary>Changes a signed 8.8 velocity toward a target coordinate, clamping it to the supplied limits.</summary>
    /// <param name="position">Current whole-pixel coordinate.</param>
    /// <param name="target">Coordinate the actor is approaching.</param>
    /// <param name="velocityWord">Current velocity represented as a signed 8.8 word.</param>
    /// <param name="positiveLimit">Maximum positive velocity.</param>
    /// <param name="negativeLimit">Minimum negative velocity.</param>
    private static ushort AccelerateToward(
        ushort position,
        ushort target,
        ushort velocityWord,
        short positiveLimit,
        short negativeLimit)
    {
        int velocity = unchecked((short)velocityWord);
        if (unchecked((short)(target - position)) >= 0)
            velocity = Math.Min(positiveLimit, velocity + 0x20);
        else
            velocity = Math.Max(negativeLimit, velocity - 0x20);
        return unchecked((ushort)velocity);
    }

    /// <summary>Adds an 8.8 velocity to the selected sprite axis while preserving whole and fractional coordinates.</summary>
    /// <param name="sprite">Actor whose position is updated.</param>
    /// <param name="horizontal">True to update X; false to update Y.</param>
    /// <param name="velocity">Signed 8.8 displacement applied this update.</param>
    private static void AddEightEightVelocity(
        IntroDiscoverySprite sprite,
        bool horizontal,
        ushort velocity)
    {
        if (horizontal)
        {
            ushort whole = sprite.XPosition;
            ushort fraction = sprite.XSubPosition;
            IntroCinematicMotion.AddEightEight(ref whole, ref fraction, velocity);
            sprite.XPosition = whole;
            sprite.XSubPosition = fraction;
        }
        else
        {
            ushort whole = sprite.YPosition;
            ushort fraction = sprite.YSubPosition;
            IntroCinematicMotion.AddEightEight(ref whole, ref fraction, velocity);
            sprite.YPosition = whole;
            sprite.YSubPosition = fraction;
        }
    }

    /// <summary>Builds a discovery-scene sprite object from its authored actor definition.</summary>
    /// <param name="definition">Position, palette, instruction list, and pre-instruction for the actor.</param>
    private static IntroDiscoverySprite CreateActor(IntroBabyActorDefinition definition)
    {
        var actor = new IntroDiscoverySprite(
            definition.X,
            definition.Y,
            definition.PaletteBits,
            definition.InstructionList);
        actor.PreInstructionPointerForDiscovery(definition.PreInstruction);
        return actor;
    }

    /// <summary>Creates the scene's room grid from extracted collision definitions and zero-filled auxiliary layers.</summary>
    private static RoomLevelData CreateLevel()
    {
        const int width = IntroBabyDiscoveryCollisionDefinitions.Columns;
        const int height = IntroBabyDiscoveryCollisionDefinitions.RoomRows;
        ushort[] foreground = IntroBabyDiscoveryCollisionDefinitions.CreateForeground();
        return new RoomLevelData(
            width,
            height,
            foreground,
            new byte[width * height],
            new ushort[width * height],
            Array.Empty<byte>());
    }

}
