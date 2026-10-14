using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Cartridge-scripted Ceres laboratory vignette between intro pages three and four.</summary>
internal sealed class IntroScientistCutsceneState
{
    /// <summary>Delivered or examined Baby actor whose instruction list drives scene completion.</summary>
    internal readonly IntroDiscoverySprite baby;
    /// <summary>Optional intro sound queue for the three Baby cry opcodes.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Selects delivery's horizontal pan or examination's vertical pan and page transition.</summary>
    private readonly ScientistSceneKind kind;
    /// <summary>Instruction source, defaulting to the compiled scientist-scene program words.</summary>
    private readonly Func<ushort, ushort> instructionWord;

    /// <summary>Initializes the selected laboratory page, mirrored camera pan, and Baby actor program.</summary>
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
            definition.Position,
            definition.PaletteBits,
            definition.InstructionList);
        baby.PreInstructionPointerForDiscovery(definition.PreInstruction);
        TilemapBaseWord = delivery ? (ushort)0x5800 : (ushort)0x5c00;
        Background = delivery ? new(0x0020, 0x0008) : new(0, 0xffe8);
    }

    /// <summary>The laboratory page's BG1 scroll.</summary>
    public Position16<RoomPixels> Background { get; private set; }

    /// <summary>BG1 tilemap base $5800 for delivery or $5C00 for examination.</summary>
    public ushort TilemapBaseWord { get; }

    /// <summary>Set when delivery's actor instruction hands the intro to page four.</summary>
    public bool PageFourRequested { get; private set; }

    /// <summary>Set when examination's actor instruction hands the intro to page five.</summary>
    public bool PageFiveRequested { get; private set; }

    /// <summary>Starts the delivered-Baby page with a leftward camera pan and page-four handoff.</summary>
    public static IntroScientistCutsceneState CreateDelivery(CartridgeAudioState? audio = null,
        Func<ushort, ushort>? instructionWord = null) =>
        new(ScientistSceneKind.Delivery, audio, instructionWord);

    /// <summary>Starts the examined-Baby page with a downward camera pan and page-five handoff.</summary>
    public static IntroScientistCutsceneState CreateExamination(CartridgeAudioState? audio = null,
        Func<ushort, ushort>? instructionWord = null) =>
        new(ScientistSceneKind.Examination, audio, instructionWord);

    /// <summary>Advances the Baby script and pans on every fourth function tick until the alternate crossfade timer redirects it to deletion.</summary>
    public void Step(
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
            if (kind == ScientistSceneKind.Delivery && Background.X != 0)
            {
                // AD68 moves the camera left while keeping the delivered baby fixed
                // relative to the laboratory subjects.
                Background += new Offset16<RoomPixels>(-1, 0);
                baby.WholePosition += new Offset16<ScreenPixels>(1, 0);
            }
            else if (kind == ScientistSceneKind.Examination &&
                unchecked((short)(Background.Y - 0x0008)) < 0)
            {
                // ADA6 moves the second laboratory page down from -24 to +8 while the
                // examined baby rises by the same 32 pixels.
                Background += new Offset16<RoomPixels>(0, 1);
                baby.WholePosition += new Offset16<ScreenPixels>(0, -1);
            }
        }

        baby.Step<IntroScientistBabyInstruction>(HandleInstruction, instructionWord);
    }

    /// <summary>Draws the current Baby actor frame through the installed scientist-scene sprite presentation.</summary>
    public void Draw(OamBuffer oam,
        IntroScientistSpritePresentation? installedArt = null) =>
        baby.Draw(oam, installedArt: installedArt);

    /// <summary>Queues authored Baby cries or requests the next intro page, leaving instruction advancement with the actor.</summary>
    private ushort HandleInstruction(IntroScientistBabyInstruction opcode, ushort argumentPointer)
    {
        switch (opcode)
        {
            case IntroScientistBabyInstruction.PlayCry1:
                audio?.QueueSound(
                    IntroCinematicRomData.Objects.BabyCry1,
                    maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
                return argumentPointer;
            case IntroScientistBabyInstruction.PlayCry2:
                audio?.QueueSound(
                    IntroCinematicRomData.Objects.BabyCry2,
                    maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
                return argumentPointer;
            case IntroScientistBabyInstruction.PlayCry3:
                audio?.QueueSound(
                    IntroCinematicRomData.Objects.BabyCry3,
                    maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
                return argumentPointer;
            case IntroScientistBabyInstruction.StartIntroPage4:
                // The delivery loop ends by selecting page four.
                PageFourRequested = true;
                return argumentPointer;
            case IntroScientistBabyInstruction.StartIntroPage5:
                // The examination loop ends by selecting page five.
                PageFiveRequested = true;
                return argumentPointer;
            default:
                throw new InvalidOperationException($"Undefined IntroScientistBabyInstruction {opcode}.");
        }
    }

    /// <summary>Distinct laboratory pages with opposite pan axes and later page destinations.</summary>
    private enum ScientistSceneKind
    {
        /// <summary>Delivered Baby remains fixed against the subjects as the camera pans left.</summary>
        Delivery,
        /// <summary>Examined Baby rises against the laboratory as the camera pans down.</summary>
        Examination,
    }
}
