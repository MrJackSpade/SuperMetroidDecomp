namespace SuperMetroid.Core.Frontend.Intro;

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
    /// <summary>The cinematic sprite object running the Mother Brain's typed program (#641).</summary>
    private readonly IntroDiscoverySprite actor =
        new(IntroMotherBrainDefinitions.MotherBrainOrigin, IntroCinematicRomData.Objects.DiscoveryPalette.Raw,
            IntroMotherBrainDefinitions.MotherBrain.Program);
    /// <summary>Remaining palette alternations after a missile hit.</summary>
    private NativeCountdown hurtFlashTimer;
    /// <summary>Frames since the fourth hit began the explosion phase.</summary>
    private ushort explodingTimer;
    /// <summary>Whether explosion timing has entered the page-two instruction list.</summary>
    private bool pageTwoInstructionStarted;
    /// <summary>Whether the page-two program's pre-instruction has replaced hit flashing.</summary>
    private bool CrossfadingToPageTwo =>
        actor.ListPreInstruction == CinematicSpritePreInstruction.IntroMotherBrainCrossFading;

    /// <summary>Fixed cinematic screen origin from definition $8B:CE55.</summary>
    public static Position16<ScreenPixels> Position => IntroMotherBrainDefinitions.MotherBrainOrigin;

    /// <summary>OAM palette and attribute bits selected by the discovery cinematic object.</summary>
    public static ushort PaletteBits => IntroCinematicRomData.Objects.DiscoveryPalette.Raw;

    /// <summary>Bank-$8C spritemap selected by the latest timed instruction entry.</summary>
    public ushort SpriteMapPointer => actor.SpriteMapPointer;

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
        ReadOnlySpan<Bgr555> introPalette,
        ushort cinematicFrameCounter,
        ushort introCrossfadeTimer)
    {
        if (CrossfadingToPageTwo)
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
            // $B80F starts the page-two program. The generic handler below runs its page request and
            // pre-instruction install during this same cinematic-object call.
            actor.Start(CinematicSpriteProgram.IntroMotherBrainStartPage2);
            pageTwoInstructionStarted = true;
        }
    }

    /// <summary>
    /// Eight-frame white/normal alternation loaded into OBJ palette seven by <c>$8B:B846</c>.
    /// </summary>
    public void ApplyHurtFlash(SnesCgram cgram, ReadOnlySpan<Bgr555> introPalette)
    {
        Ensure.NotNull(cgram);
        if (hurtFlashTimer.IsZero)
            return;

        bool normalPalette = (hurtFlashTimer.RawValue & 1) != 0;
        new FrontendPalettes(cgram).WriteIntroMotherBrain(introPalette, flashWhite: !normalPalette);
        hurtFlashTimer = hurtFlashTimer.Decrement().Remaining;
    }

    /// <summary>Records the synchronous hit side effects after the projectile is killed.</summary>
    public bool RegisterMissileHit()
    {
        hurtFlashTimer = NativeCountdown.Start(8);
        HitCount++;
        if (HitCount != 4)
            return false;

        // $8B:B7BD clears the actor timer before selecting pre-instruction $B80F. The
        // explosion pre-instruction does not run until the next cinematic-object frame.
        explodingTimer = 0;
        ExplosionStarted = true;
        return true;
    }

    /// <summary>Runs the generic cinematic sprite handler ($8B:9409) for definition $CE55.</summary>
    public void Step() => actor.Step(cue =>
    {
        if (cue != CinematicSpriteCueKind.StartIntroPage2)
            throw new InvalidOperationException($"The intro Mother Brain programs publish no {cue} cue.");
        PageTwoRequested = true;
    });

    /// <summary>Alternates BG1 scroll by four pixels from the inherited flashback offset.</summary>
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
