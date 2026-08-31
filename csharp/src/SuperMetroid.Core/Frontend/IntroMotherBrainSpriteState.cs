using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Visible animation core of cinematic sprite definition $8B:CE55 (intro Mother Brain).
/// </summary>
/// <remarks>
/// The definition's initializer fixes the actor at (56,111) with OAM attribute/palette
/// word $0E00. Its $CB05 instruction list cycles four ROM-authored bank-$8C spritemaps at
/// sixteen frames apiece. The actor also retains $B786's hit counter, eight-frame palette
/// flash, and $B80F's 128-frame exploding state and BG1 screen shake.
/// </remarks>
internal sealed class IntroMotherBrainSpriteState
{
    private const ushort GotoInstruction = 0x94bc;
    private const ushort SetPreInstruction = 0x944c;
    private const ushort StartIntroPageTwo = 0xb336;
    private ushort instructionPointer = 0xcb05;
    private ushort instructionTimer = 1;
    private ushort hurtFlashTimer;
    private ushort explodingTimer;
    private bool pageTwoInstructionStarted;
    private bool crossfadingToPageTwo;

    public ushort XPosition => 0x0038;

    public ushort YPosition => 0x006f;

    public ushort PaletteBits => 0x0e00;

    public ushort SpriteMapPointer { get; private set; }

    /// <summary>Number of missiles consumed by <c>$8B:B786</c>, from zero through four.</summary>
    public ushort HitCount { get; private set; }

    /// <summary>True once the fourth hit switches this actor to <c>$8B:B80F</c>.</summary>
    public bool ExplosionStarted { get; private set; }

    /// <summary>BG1 vertical scroll word toggled by $8B:B877 while Mother Brain explodes.</summary>
    public ushort BackgroundVerticalScroll { get; private set; } =
        IntroCinematicState.GameplayFlashbackBg1VerticalScroll;

    /// <summary>True when $B80F switches the actor to its page-two instruction list.</summary>
    public bool PageTwoRequested { get; private set; }

    /// <summary>Whether the actor still contributes its spritemap to cinematic OAM.</summary>
    public bool IsVisible { get; private set; } = true;

    /// <summary>
    /// Runs the actor pre-instruction that precedes its instruction-list timer decrement.
    /// </summary>
    public void RunPreInstruction(
        SnesCgram cgram,
        ReadOnlySpan<ushort> introPalette,
        ushort cinematicFrameCounter,
        ushort introCrossfadeTimer)
    {
        if (crossfadingToPageTwo)
        {
            ApplyScreenShake(cinematicFrameCounter);
            // $8B:B82E observes the timer after the cinematic function has decremented it.
            // At zero it redirects this actor to delete and clears Samus's intro display.
            if (introCrossfadeTimer == 0)
                IsVisible = false;
            return;
        }

        ApplyHurtFlash(cgram, introPalette);
        if (!ExplosionStarted || pageTwoInstructionStarted)
            return;

        ApplyScreenShake(cinematicFrameCounter);

        explodingTimer++;
        if (explodingTimer >= 0x0080)
        {
            // $B80F primes list $CB19. The generic handler below executes its two control
            // instructions during this same cinematic-object call.
            instructionTimer = 1;
            instructionPointer = 0xcb19;
            pageTwoInstructionStarted = true;
        }
    }

    /// <summary>
    /// Eight-frame white/normal alternation loaded into OBJ palette seven by <c>$8B:B846</c>.
    /// </summary>
    public void ApplyHurtFlash(SnesCgram cgram, ReadOnlySpan<ushort> introPalette)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (hurtFlashTimer == 0)
            return;

        bool normalPalette = (hurtFlashTimer & 1) != 0;
        for (int color = 0; color < 16; color++)
        {
            cgram.SetColor(240 + color, normalPalette
                ? introPalette[240 + color]
                : (ushort)0x7fff);
        }
        hurtFlashTimer--;
    }

    /// <summary>Records the synchronous hit side effects after the projectile is killed.</summary>
    public bool RegisterMissileHit()
    {
        hurtFlashTimer = 8;
        HitCount++;
        if (HitCount != 4)
            return false;

        // $8B:B7BD clears the actor timer before selecting pre-instruction $B80F. The
        // explosion pre-instruction does not run until the next cinematic-object frame.
        explodingTimer = 0;
        ExplosionStarted = true;
        return true;
    }

    /// <summary>Ports the timing/control-flow portion of $8B:9409 for definition $CE55.</summary>
    public void Step(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (instructionTimer-- != 1)
            return;

        ushort pointer = instructionPointer;
        while (true)
        {
            ushort instructionOrDuration = ReadWord(bus, pointer);
            if ((instructionOrDuration & 0x8000) == 0)
            {
                instructionTimer = instructionOrDuration;
                SpriteMapPointer = ReadWord(bus, Add(pointer, 2));
                instructionPointer = Add(pointer, 4);
                return;
            }

            if (instructionOrDuration != GotoInstruction)
            {
                if (instructionOrDuration == StartIntroPageTwo)
                {
                    PageTwoRequested = true;
                    pointer = Add(pointer, 2);
                    continue;
                }

                if (instructionOrDuration == SetPreInstruction)
                {
                    ushort preInstruction = ReadWord(bus, Add(pointer, 2));
                    if (preInstruction != 0xb82e)
                    {
                        throw new InvalidDataException(
                            $"Intro Mother Brain names invalid pre-instruction $8B:{preInstruction:X4}.");
                    }

                    crossfadingToPageTwo = true;
                    pointer = Add(pointer, 4);
                    continue;
                }

                throw new InvalidDataException(
                    $"Intro Mother Brain sprite opcode $8B:{instructionOrDuration:X4} at $8B:{pointer:X4} is invalid.");
            }

            pointer = ReadWord(bus, Add(pointer, 2));
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        RomDataReader.ReadWordFixedBank(bus, 0x8b0000 | pointer);

    private static ushort Add(ushort pointer, int byteCount) =>
        unchecked((ushort)(pointer + byteCount));

    private void ApplyScreenShake(ushort cinematicFrameCounter)
    {
        // $8B:B877 adds four on even cinematic frames and subtracts four on odd frames.
        // The inherited $0008 value therefore alternates $000C/$0008; starting this host
        // field at zero would produce $0004/$0000 and shift the whole room down one tile.
        // The arithmetic deliberately wraps as a 16-bit SNES scroll register would.
        BackgroundVerticalScroll = (cinematicFrameCounter & 1) == 0
            ? unchecked((ushort)(BackgroundVerticalScroll + 4))
            : unchecked((ushort)(BackgroundVerticalScroll - 4));
    }
}
