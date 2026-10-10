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
    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState? audio;
    private readonly Func<ushort, ushort> demoWordReader;
    private readonly DemoInputState demo = new();
    private readonly List<IntroEggParticle> eggParticles = [];
    private readonly List<IntroEggSlimeDrop> slimeDrops = [];
    private readonly IntroDiscoverySprite egg;
    private readonly IntroDiscoverySprite confusedBaby;

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
        Samus.Pose = SamusPoseId.FacingLeftNormalPose;
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

    public SamusState Samus { get; }

    public RoomLevelData Level { get; }

    public bool EggHatchingStarted { get; private set; }

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
        egg.Step<IntroEggInstruction>(HandleEggInstruction,
            IntroBabyDiscoveryInstructionDefinitions.ReadWord);

        // The baby slot follows the egg slot in the native descending actor traversal, so
        // it observes the egg's freshly advanced list pointer in this same frame.
        StepConfusedBaby(introCrossfadeTimer);
        confusedBaby.Step<ConfusedBabyInstruction>(HandleConfusedBabyInstruction,
            IntroBabyDiscoveryInstructionDefinitions.ReadWord);

        foreach (IntroEggParticle particle in eggParticles)
            particle.Step();
        foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
            slimeDrop.Step();
    }

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

    private void RunDemoPreInstruction(ushort pointer, ushort introCrossfadeTimer)
    {
        // The Baby-discovery object references only these four routines. Another
        // pointer means the object definition/list pairing is corrupt.
        IntroBabyDiscoveryPreInstruction preInstruction =
            ClosedNativeWords.Decode<IntroBabyDiscoveryPreInstruction>(pointer, "Baby-discovery demo pre-instruction");
        switch (preInstruction)
        {
            case IntroBabyDiscoveryPreInstruction.RunningLeft:
                if (Samus.XPosition < 0x00b2)
                    demo.Redirect(
                        (ushort)IntroBabyDiscoveryPreInstruction.StopAndLook,
                        IntroBabyDiscoveryRomData.StopAndLookInputList);
                return;

            case IntroBabyDiscoveryPreInstruction.StopAndLook:
                if (introCrossfadeTimer == 0)
                    demo.Redirect(
                        (ushort)IntroBabyDiscoveryPreInstruction.Inert,
                        IntroBabyDiscoveryRomData.EndInputList);
                return;

            case IntroBabyDiscoveryPreInstruction.Inert:
            case IntroBabyDiscoveryPreInstruction.InertAlternate:
                return;

            default:
                throw new InvalidOperationException($"Undefined IntroBabyDiscoveryPreInstruction {preInstruction}.");
        }
    }

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

    private ushort HandleEggInstruction(IntroEggInstruction opcode, ushort argumentPointer)
    {
        switch (opcode)
        {
            case IntroEggInstruction.SpawnParticles:
                // The six JSR Spawn calls at $A918..A94C use definitions CECD through CEEB
                // and init parameters zero through five, in this exact order.
                for (byte index = 0; index < IntroEggEffectDefinitions.ParticleCount; index++)
                    eggParticles.Add(new IntroEggParticle(index));
                audio?.QueueSound(
                    IntroCinematicRomData.Objects.EggHatch,
                    maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
                return argumentPointer;

            case IntroEggInstruction.StartIntroPage3:
                // This opcode switches the outer cinematic function to page three and then
                // returns without consuming operands. The state owner performs the palette
                // transition; the actor interpreter merely reports the native request.
                PageThreeRequested = true;
                return argumentPointer;

            default:
                throw new InvalidOperationException($"Undefined IntroEggInstruction {opcode}.");
        }
    }

    private ushort HandleConfusedBabyInstruction(ConfusedBabyInstruction opcode, ushort argumentPointer)
    {
        byte soundId = opcode switch
        {
            ConfusedBabyInstruction.PlayCry1 => IntroCinematicRomData.Objects.BabyCry1.Value,
            ConfusedBabyInstruction.PlayCry2 => IntroCinematicRomData.Objects.BabyCry2.Value,
            ConfusedBabyInstruction.PlayCry3 => IntroCinematicRomData.Objects.BabyCry3.Value,
            _ => throw new InvalidOperationException($"Undefined ConfusedBabyInstruction {opcode}."),
        };

        audio?.QueueSound(
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, soundId),
            maximumQueued: 6);
        return argumentPointer;
    }

    private void StepConfusedBaby(ushort introCrossfadeTimer)
    {
        switch (CinematicInstructionWords.Decode<ConfusedBabyPreInstruction>(
                    confusedBaby.PreInstructionPointer, confusedBaby.InstructionPointer))
        {
            case ConfusedBabyPreInstruction.WaitingForHatch:
                // $BA5E watches the egg's *next* instruction pointer. CB79 is the first
                // fully-hatched frame list, so the baby starts moving on that exact handoff.
                if (egg.InstructionPointer >= CinematicCodePointers.Lists.MetroidEggHatchedFrame2)
                {
                    confusedBaby.PreInstructionPointerForDiscovery(
                        (ushort)ConfusedBabyPreInstruction.Hatched);
                    BabyYVelocity = 0;
                }
                return;

            case ConfusedBabyPreInstruction.Hatched:
                StepHatchedBaby();
                return;

            case ConfusedBabyPreInstruction.Idling:
                BabyIdleTimer = unchecked((ushort)(BabyIdleTimer - 1));
                if (unchecked((short)BabyIdleTimer) <= 0)
                {
                    confusedBaby.PreInstructionPointerForDiscovery(
                        (ushort)ConfusedBabyPreInstruction.Dancing);
                    BabyIdleTimer = 0;
                    BabyYVelocity = 0;
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

    private ushort BabyXVelocity { get; set; }

    private ushort BabyYVelocity { get; set; }

    private ushort BabyIdleTimer { get; set; }

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
                (ushort)ConfusedBabyPreInstruction.Idling);
        }
    }

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
