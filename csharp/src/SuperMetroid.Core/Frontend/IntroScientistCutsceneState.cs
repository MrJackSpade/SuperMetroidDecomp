using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge-scripted Ceres laboratory vignette between intro pages three and four.</summary>
internal sealed class IntroScientistCutsceneState
{
    /// <summary>Baby actor whose instruction list drives the selected laboratory vignette.</summary>
    private readonly IntroDiscoverySprite baby;
    /// <summary>Optional audio queue for the baby cries emitted by its instruction opcodes.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Selects delivery or examination camera motion and page-transition behavior.</summary>
    private readonly ScientistSceneKind kind;
    /// <summary>Reader for cinematic instruction words, defaulting to the compiled scientist-scene definitions.</summary>
    private readonly Func<ushort, ushort> instructionWord;

    /// <summary>Initializes the baby actor and page-specific tilemap and camera offsets.</summary>
    /// <param name="kind">Delivery or examination vignette to construct.</param>
    /// <param name="audio">Optional queue for baby cries; null disables sound publication.</param>
    /// <param name="instructionWord">Optional instruction-word reader used by the cinematic sprite interpreter.</param>
    private IntroScientistCutsceneState(ScientistSceneKind kind, CartridgeAudioState? audio,
        Func<ushort, ushort>? instructionWord)
    {
        this.kind = kind;
        this.audio = audio;
        this.instructionWord = instructionWord ?? IntroScientistInstructionDefinitions.ReadWord;
        bool delivery = kind == ScientistSceneKind.Delivery;
        IntroBabyActorDefinition definition = delivery
            ? IntroBabyActorDefinitions.DeliveredBaby
            : IntroBabyActorDefinitions.ExaminedBaby;

        // Definitions $CE61/$CE67 use separate laboratory pages and mirrored pan axes.
        baby = new IntroDiscoverySprite(
            definition.X,
            definition.Y,
            definition.PaletteBits,
            definition.InstructionList);
        baby.PreInstructionPointerForDiscovery(definition.PreInstruction);
        TilemapBaseWord = delivery ? (ushort)0x5800 : (ushort)0x5c00;
        BackgroundX = delivery ? (ushort)0x0020 : (ushort)0;
        BackgroundY = delivery ? (ushort)0x0008 : unchecked((ushort)0xffe8);
    }

    /// <summary>Current horizontal laboratory background offset, advanced during the delivery pan.</summary>
    public ushort BackgroundX { get; private set; }

    /// <summary>Current vertical laboratory background offset, advanced during the examination pan.</summary>
    public ushort BackgroundY { get; private set; }

    /// <summary>Tilemap destination word for the delivery or examination laboratory page.</summary>
    public ushort TilemapBaseWord { get; }

    /// <summary>Set when the delivery script requests the outer cinematic to transition to page four.</summary>
    public bool PageFourRequested { get; private set; }

    /// <summary>Set when the examination script requests the outer cinematic to transition to page five.</summary>
    public bool PageFiveRequested { get; private set; }

    /// <summary>Creates the laboratory scene in which the baby is delivered and the background pans horizontally.</summary>
    /// <param name="audio">Optional audio queue for the baby's scripted cries.</param>
    /// <param name="instructionWord">Optional reader for cinematic instruction words; the compiled reader is used when omitted.</param>
    /// <returns>A delivery-scene state ready for stepping and drawing.</returns>
    public static IntroScientistCutsceneState CreateDelivery(CartridgeAudioState? audio = null,
        Func<ushort, ushort>? instructionWord = null) =>
        new(ScientistSceneKind.Delivery, audio, instructionWord);

    /// <summary>Creates the examination scene in which the baby rises as the laboratory background pans vertically.</summary>
    /// <param name="audio">Optional audio queue for the baby's scripted cries.</param>
    /// <param name="instructionWord">Optional reader for cinematic instruction words; the compiled reader is used when omitted.</param>
    /// <returns>An examination-scene state ready for stepping and drawing.</returns>
    public static IntroScientistCutsceneState CreateExamination(CartridgeAudioState? audio = null,
        Func<ushort, ushort>? instructionWord = null) =>
        new(ScientistSceneKind.Examination, audio, instructionWord);

    /// <summary>Advances the active baby script and applies the vignette's crossfade-gated camera motion.</summary>
    /// <param name="bus">Address space used by the cinematic sprite interpreter.</param>
    /// <param name="cinematicFunctionTimer">Outer cinematic timer used to cadence background and actor movement.</param>
    /// <param name="introCrossfadeTimer">Reverse-crossfade timer whose zero value deletes the baby actor.</param>
    public void Step(
        ISnesAddressSpace bus,
        ushort cinematicFunctionTimer,
        ushort introCrossfadeTimer)
    {
        if (!baby.IsActive)
            return;

        // AD68 deletes only when the *alternate* intro counter reaches zero during the
        // later page-four reverse crossfade. The forward scientist fade decrements the
        // cinematic-function counter while IntroCrossFadeTimer remains $007F.
        if (introCrossfadeTimer == 0)
        {
            baby.Redirect(CinematicCodePointers.Lists.Delete);
        }
        else if ((cinematicFunctionTimer & 3) == 0)
        {
            if (kind == ScientistSceneKind.Delivery && BackgroundX != 0)
            {
                // AD68 moves the camera left while keeping the delivered baby fixed
                // relative to the laboratory subjects.
                BackgroundX--;
                baby.XPosition++;
            }
            else if (kind == ScientistSceneKind.Examination &&
                unchecked((short)(BackgroundY - 0x0008)) < 0)
            {
                // ADA6 moves the second laboratory page down from -24 to +8 while the
                // examined baby rises by the same 32 pixels.
                BackgroundY++;
                baby.YPosition--;
            }
        }

        baby.Step(bus, HandleInstruction, instructionWord);
    }

    /// <summary>Draws the baby actor with optional installed scientist-scene sprite art.</summary>
    /// <param name="bus">Address space used to resolve the actor's sprite instructions.</param>
    /// <param name="oam">OAM buffer receiving the baby's sprites.</param>
    /// <param name="installedArt">Optional art source for the actor's spritemap operands.</param>
    public void Draw(ISnesAddressSpace bus, OamBuffer oam,
        IntroScientistSpritePresentation? installedArt = null) =>
        baby.Draw(bus, oam, installedArt: installedArt);

    /// <summary>Queues baby cries and records page-change requests for opcodes handled by the outer cinematic.</summary>
    /// <param name="opcode">Cinematic instruction opcode currently being interpreted.</param>
    /// <param name="argumentPointer">Instruction cursor after the opcode.</param>
    /// <returns>The next cursor for handled sound/page opcodes, or null to delegate to the shared interpreter.</returns>
    private ushort? HandleInstruction(ushort opcode, ushort argumentPointer)
    {
        switch (opcode)
        {
            case CinematicCodePointers.Instruction_PlayBabyMetroid_Cry1:
                audio?.QueueSound(
                    IntroCinematicRomData.Objects.BabyCry1,
                    maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
                return argumentPointer;
            case CinematicCodePointers.Instruction_PlayBabyMetroid_Cry2:
                audio?.QueueSound(
                    IntroCinematicRomData.Objects.BabyCry2,
                    maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
                return argumentPointer;
            case CinematicCodePointers.Instruction_PlayBabyMetroid_Cry3:
                audio?.QueueSound(
                    IntroCinematicRomData.Objects.BabyCry3,
                    maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
                return argumentPointer;
            case CinematicCodePointers.Instruction_StartIntroPage4:
                // The delivery loop ends by selecting page four.
                PageFourRequested = true;
                return argumentPointer;
            case CinematicCodePointers.Instruction_StartIntroPage5:
                // The examination loop ends by selecting page five.
                PageFiveRequested = true;
                return argumentPointer;
            default:
                return null;
        }
    }

    /// <summary>Identifies which scientist vignette supplies the scene's pan direction and page handoff.</summary>
    private enum ScientistSceneKind
    {
        /// <summary>Delivery vignette with a horizontal background pan and page-four handoff.</summary>
        Delivery,
        /// <summary>Examination vignette with a vertical background pan and page-five handoff.</summary>
        Examination,
    }
}
