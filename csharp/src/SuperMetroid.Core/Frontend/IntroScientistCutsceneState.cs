using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge-scripted Ceres laboratory vignette between intro pages three and four.</summary>
internal sealed class IntroScientistCutsceneState
{
    private readonly IntroDiscoverySprite baby;
    private readonly CartridgeAudioState? audio;
    private readonly ScientistSceneKind kind;

    private IntroScientistCutsceneState(ScientistSceneKind kind, CartridgeAudioState? audio)
    {
        this.kind = kind;
        this.audio = audio;
        bool delivery = kind == ScientistSceneKind.Delivery;

        // Definitions $CE61/$CE67 use separate laboratory pages and mirrored pan axes.
        baby = new IntroDiscoverySprite(
            xPosition: delivery ? (ushort)0x0054 : (ushort)0x0070,
            yPosition: delivery ? (ushort)0x008b : (ushort)0x006f,
            paletteBits: 0x0c00,
            instructionPointer: delivery ? (ushort)0xcb9f : (ushort)0xcbcd);
        TilemapBaseWord = delivery ? (ushort)0x5800 : (ushort)0x5c00;
        BackgroundX = delivery ? (ushort)0x0020 : (ushort)0;
        BackgroundY = delivery ? (ushort)0x0008 : unchecked((ushort)0xffe8);
    }

    public ushort BackgroundX { get; private set; }

    public ushort BackgroundY { get; private set; }

    public ushort TilemapBaseWord { get; }

    public bool PageFourRequested { get; private set; }

    public bool PageFiveRequested { get; private set; }

    public static IntroScientistCutsceneState CreateDelivery(CartridgeAudioState? audio = null) =>
        new(ScientistSceneKind.Delivery, audio);

    public static IntroScientistCutsceneState CreateExamination(CartridgeAudioState? audio = null) =>
        new(ScientistSceneKind.Examination, audio);

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
            baby.Redirect(0xce53);
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

        baby.Step(bus, HandleInstruction);
    }

    public void Draw(ISnesAddressSpace bus, OamBuffer oam) => baby.Draw(bus, oam);

    private ushort? HandleInstruction(ushort opcode, ushort argumentPointer)
    {
        switch (opcode)
        {
            case 0xa25b:
                audio?.QueueSound(library: 3, soundId: 0x23, maximumQueued: 6);
                return argumentPointer;
            case 0xa263:
                audio?.QueueSound(library: 3, soundId: 0x26, maximumQueued: 6);
                return argumentPointer;
            case 0xa26b:
                audio?.QueueSound(library: 3, soundId: 0x27, maximumQueued: 6);
                return argumentPointer;
            case 0xb346:
                // The delivery loop ends by selecting page four.
                PageFourRequested = true;
                return argumentPointer;
            case 0xb34e:
                // The examination loop ends by selecting page five.
                PageFiveRequested = true;
                return argumentPointer;
            default:
                return null;
        }
    }

    private enum ScientistSceneKind
    {
        Delivery,
        Examination,
    }
}
