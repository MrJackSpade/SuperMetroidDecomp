using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Cartridge-backed setup and first actor phase of the SR388 baby-Metroid discovery scene.
/// </summary>
internal sealed class IntroBabyDiscoveryState
{
    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState? audio;
    private readonly DemoInputState demo = new();
    private readonly List<IntroEggParticle> eggParticles = [];
    private readonly List<IntroEggSlimeDrop> slimeDrops = [];
    private readonly IntroDiscoverySprite egg = new(
        xPosition: 0x0070,
        yPosition: 0x009b,
        paletteBits: 0x0e00,
        instructionPointer: 0xcb33);
    private readonly IntroDiscoverySprite confusedBaby = new(
        xPosition: 0x0070,
        yPosition: 0x009b,
        paletteBits: 0x0e00,
        instructionPointer: 0xcc2b);

    public IntroBabyDiscoveryState(ISnesAddressSpace bus, CartridgeAudioState? audio = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio;

        Samus = new SamusState
        {
            Pose = SamusState.FacingLeftNormalPose,
            XPosition = 0x0178,
            YPosition = 0x0093,
            SelectedHudItem = 0,
        };
        Samus.RefreshCollisionRadii(bus);
        Samus.InitializeAnimation(bus);
        Samus.PrimeGraphics(bus);

        // $8B:AFDF copies exactly $300 bytes into a room declared 32x16 blocks. The final
        // 128 foreground words and all BTS bytes retain the earlier zero initialization.
        byte[] source = RomDataReader.ReadFixedBank(bus, 0x8cc083, 0x0300);
        Level = CreateLevel(source);

        demo.Clear();
        demo.Enable();
        demo.LoadObject(bus, 0x877e);
    }

    public SamusState Samus { get; }

    public RoomLevelData Level { get; }

    public bool EggHatchingStarted { get; private set; }

    public int ActiveEggParticleCount => eggParticles.Count(particle => particle.IsActive);

    public bool PageThreeRequested { get; private set; }

    public void Step(ushort nmiFrameCounter, ushort introCrossfadeTimer)
    {
        demo.Step(
            bus,
            preInstruction: (_, pointer) => RunDemoPreInstruction(pointer, introCrossfadeTimer),
            specialInstruction: HandleDemoInstruction);

        IntroSamusDemoMovement.StepGroundedLeft(
            bus,
            Level,
            Samus,
            demo.Held,
            demo.NewlyPressed,
            nmiFrameCounter);

        // The egg's $A8E8 pre-instruction tests Samus's post-movement X during the later
        // cinematic-sprite pass. It permanently redirects the list once X is below $A9.
        if (!EggHatchingStarted && unchecked((short)(Samus.XPosition - 0x00a9)) < 0)
        {
            egg.Redirect(0xcb3b);
            EggHatchingStarted = true;
        }

        // $A903 replaces the egg with the shared delete list only when page three's reverse
        // crossfade reaches zero. It is installed by list opcode $944C after $B33E.
        if (egg.PreInstructionPointer == 0xa903 && introCrossfadeTimer == 0)
            egg.Redirect(0xce53);
        egg.Step(bus, HandleEggInstruction);

        // The baby slot follows the egg slot in the native descending actor traversal, so
        // it observes the egg's freshly advanced list pointer in this same frame.
        StepConfusedBaby(introCrossfadeTimer);
        confusedBaby.Step(bus, HandleConfusedBabyInstruction);

        foreach (IntroEggParticle particle in eggParticles)
            particle.Step(bus);
        foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
            slimeDrop.Step(bus);
    }

    public void DrawActors(OamBuffer oam)
    {
        // IntroSamusDisplayFlag=+1 makes cinematic objects enter OAM before Samus. The egg
        // was spawned before the confused-baby object and retains its own list and timer.
        egg.Draw(bus, oam);
        confusedBaby.Draw(bus, oam);
        foreach (IntroEggParticle particle in eggParticles)
            particle.Draw(bus, oam);
        foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
            slimeDrop.Draw(bus, oam);
    }

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

    private DemoInputInstructionResult HandleDemoInstruction(
        DemoInputState state,
        ushort pointer,
        ushort argumentPointer)
    {
        if (pointer != IntroBabyDiscoveryRomData.EndDemoInputInstruction)
            return DemoInputInstructionResult.NotHandled(argumentPointer);

        // $91:8682 replaces both Samus handlers with the locked cinematic RTS, disables
        // demo input, and returns into the following shared delete opcode.
        state.Disable();
        Samus.InputLocked = true;
        return DemoInputInstructionResult.ContinueAt(argumentPointer);
    }

    private ushort? HandleEggInstruction(ushort opcode, ushort argumentPointer)
    {
        switch (opcode)
        {
            case CinematicCodePointers.Instruction_SpawnMetroidEggParticles:
                // The six JSR Spawn calls at $A918..A94C use definitions CECD through CEEB
                // and init parameters zero through five, in this exact order.
                for (byte index = 0; index < 6; index++)
                    eggParticles.Add(new IntroEggParticle(bus, index));
                audio?.QueueSound(library: 2, soundId: 0x0b, maximumQueued: 6);
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

    private ushort? HandleConfusedBabyInstruction(ushort opcode, ushort argumentPointer)
    {
        byte soundId = opcode switch
        {
            0xa25b => 0x23,
            0xa263 => 0x26,
            0xa26b => 0x27,
            _ => 0,
        };
        if (soundId == 0)
            return null;

        audio?.QueueSound(library: 3, soundId, maximumQueued: 6);
        return argumentPointer;
    }

    private void StepConfusedBaby(ushort introCrossfadeTimer)
    {
        switch (confusedBaby.PreInstructionPointer)
        {
            case 0:
                // $BA5E watches the egg's *next* instruction pointer. CB79 is the first
                // fully-hatched frame list, so the baby starts moving on that exact handoff.
                if (egg.InstructionPointer >= 0xcb79)
                {
                    confusedBaby.PreInstructionPointerForDiscovery(0xba73);
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
                    confusedBaby.PreInstructionPointerForDiscovery(0xbb24);
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

    private ushort BabyXVelocity { get; set; }

    private ushort BabyYVelocity { get; set; }

    private ushort BabyIdleTimer { get; set; }

    private void StepHatchedBaby()
    {
        // Equality is intentional: $BA76 uses BNE, and the actor's subpixel integration
        // reaches this scanline once. Four init parameters select four distinct arcs.
        if (confusedBaby.YPosition == 0x0091 && slimeDrops.Count == 0)
        {
            for (byte index = 0; index < 4; index++)
            {
                slimeDrops.Add(new IntroEggSlimeDrop(
                    confusedBaby.XPosition,
                    confusedBaby.YPosition,
                    index));
            }
            // `$8B:BA73` publishes the first confused cry at the same equality edge
            // that creates the four egg-slime drops.
            audio?.QueueSound(library: 3, soundId: 0x23, maximumQueued: 6);
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
            confusedBaby.PreInstructionPointerForDiscovery(0xbb0d);
        }
    }

    private void StepDancingBaby(ushort introCrossfadeTimer)
    {
        if (introCrossfadeTimer == 0)
        {
            confusedBaby.Delete();
            return;
        }

        if (confusedBaby.GeneralTimer < 0x0080)
        {
            confusedBaby.GeneralTimer++;
            // `$8B:BB24` cries when the post-increment timer reaches $40 and $80.
            if ((confusedBaby.GeneralTimer & 0x003f) == 0)
                audio?.QueueSound(library: 3, soundId: 0x23, maximumQueued: 6);
        }

        BabyXVelocity = AccelerateToward(
            confusedBaby.XPosition,
            Samus.XPosition,
            BabyXVelocity,
            positiveLimit: 0x0280,
            negativeLimit: unchecked((short)0xfd80));
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

    private static RoomLevelData CreateLevel(ReadOnlySpan<byte> source)
    {
        const int width = 32;
        const int height = 16;
        var foreground = new ushort[width * height];
        for (int offset = 0; offset < source.Length; offset += 2)
            foreground[offset / 2] = (ushort)(source[offset] | (source[offset + 1] << 8));
        return new RoomLevelData(
            width,
            height,
            foreground,
            new byte[width * height],
            new ushort[width * height],
            Array.Empty<byte>());
    }

}
