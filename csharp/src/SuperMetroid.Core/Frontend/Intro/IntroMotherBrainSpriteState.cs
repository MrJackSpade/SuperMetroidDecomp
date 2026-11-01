using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Visible animation core of the intro Mother Brain's cinematic sprite definition.
/// </summary>
/// <remarks>
/// The definition's initializer fixes the actor at (56,111) with sprite palette seven and
/// priority three. Its animation cycles four authored spritemaps at
/// sixteen frames apiece. The actor also retains the hit counter, eight-frame palette
/// flash, and the 128-frame exploding state and BG1 screen shake.
/// </remarks>
internal sealed class IntroMotherBrainSpriteState
{
    /// <summary>The cinematic sprite object running the Mother Brain's typed program (#641).</summary>
    private readonly IntroDiscoverySprite actor =
        new(IntroMotherBrainDefinitions.MotherBrainOrigin, ScreenColourGroup.IntroMotherBrain,
            IntroMotherBrainDefinitions.MotherBrain.Program);
    /// <summary>Remaining palette alternations after a missile hit.</summary>
    private WrappingCountdown hurtFlashTimer;
    /// <summary>
    /// Updates since the fourth hit began the explosion phase: zero at the hit, counted up once per update to the
    /// 128 that starts the page-two program, after which it is no longer advanced.
    /// </summary>
    private int explodingTimer;
    /// <summary>Whether explosion timing has entered the page-two instruction list.</summary>
    private bool pageTwoInstructionStarted;
    /// <summary>Whether the page-two program's pre-instruction has replaced hit flashing.</summary>
    private bool CrossfadingToPageTwo =>
        actor.ListFrameBehavior == CinematicSpriteFrameBehavior.IntroMotherBrainCrossFading;

    /// <summary>Fixed cinematic screen origin from Mother Brain's definition.</summary>
    public static Position16<ScreenPixels> Position => IntroMotherBrainDefinitions.MotherBrainOrigin;

    /// <summary>The screen colours Mother Brain draws in, which her hurt flash animates.</summary>
    public static ScreenColourGroup Colouring => ScreenColourGroup.IntroMotherBrain;

    /// <summary>The pose Mother Brain's program shows; null before its first frame and after deletion.</summary>
    public CinematicSpritePose? Pose => actor.Pose;

    /// <summary>Number of missiles consumed by Mother Brain's hit test; the fourth starts the explosion.</summary>
    public int HitCount { get; private set; }

    /// <summary>True once the fourth hit switches this actor to its exploding behaviour.</summary>
    public bool ExplosionStarted { get; private set; }

    /// <summary>The flashback room's BG1 scroll, shaken vertically while Mother Brain explodes.</summary>
    public Position16<RoomPixels> Background { get; private set; } =
        new(0, IntroCinematicState.GameplayFlashbackBg1VerticalScroll);

    /// <summary>True when the exploding behaviour switches the actor to its page-two animation.</summary>
    public bool PageTwoRequested { get; private set; }

    /// <summary>Whether the actor still contributes its spritemap to the cinematic sprites.</summary>
    public bool IsVisible { get; private set; } = true;

    /// <summary>
    /// Runs the actor pre-instruction that precedes its instruction-list timer decrement.
    /// </summary>
    public void RunFrameBehavior(
        ScreenPalette colours,
        ScreenColours introPalette,
        int cinematicFrameCounter,
        int introCrossfadeTimer)
    {
        if (CrossfadingToPageTwo)
        {
            ApplyScreenShake(cinematicFrameCounter);
            // The cross-fade behaviour observes the timer after the cinematic function has decremented it.
            // At zero it redirects this actor to delete and clears Samus's intro display.
            if (introCrossfadeTimer == 0)
                IsVisible = false;
            return;
        }

        ApplyHurtFlash(colours, introPalette);
        if (!ExplosionStarted || pageTwoInstructionStarted)
            return;

        ApplyScreenShake(cinematicFrameCounter);

        explodingTimer++;
        if (explodingTimer >= 128)
        {
            // The exploding behaviour starts the page-two program. The generic handler below runs its page request and
            // frame behaviour install during this same cinematic-object call.
            actor.Start(CinematicSpriteProgram.IntroMotherBrainStartPage2);
            pageTwoInstructionStarted = true;
        }
    }

    /// <summary>
    /// Eight-frame white/normal alternation loaded into sprite palette seven.
    /// </summary>
    public void ApplyHurtFlash(ScreenPalette colours, ScreenColours introPalette)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(introPalette);
        if (hurtFlashTimer.IsZero)
            return;

        bool normalPalette = hurtFlashTimer.IsInAlternatePhase(1);
        ColourRun motherBrainColours = colours.Colours(ScreenColourGroup.IntroMotherBrain);
        if (normalPalette)
            motherBrainColours.CopyFrom(introPalette.Colours(ScreenColourGroup.IntroMotherBrain));
        else
            motherBrainColours.Fill(RgbColor.White);
        hurtFlashTimer = hurtFlashTimer.Decrement().Remaining;
    }

    /// <summary>
    /// Mother Brain's hit test: the highest missile left of X=84 is killed and counted as a hit.
    /// The fourth hit installs the exploding behaviour, which tests no missiles, so from then on every missile passes through.
    /// </summary>
    /// <param name="impacts">The flashback projectile system's enemy impacts, which kill the missile.</param>
    /// <param name="sharedProjectiles">The flashback's bombs and power-bomb explosion, which the kill consults.</param>
    /// <returns>Whether this hit was the fourth, which starts the explosion.</returns>
    public bool TakeMissileHit(ProjectileEnemyImpacts impacts, SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(impacts);
        if (ExplosionStarted)
            return false;
        return impacts.TryImpactIntroMotherBrainMissile(sharedProjectiles) && RecordMissileHit();
    }

    /// <summary>Records the synchronous hit side effects after the projectile is killed.</summary>
    public bool RecordMissileHit()
    {
        hurtFlashTimer = WrappingCountdown.Start(8);
        HitCount++;
        if (HitCount != 4)
            return false;

        // The fourth hit clears the actor timer before selecting the exploding behaviour. The
        // explosion behaviour does not run until the next cinematic-object frame.
        explodingTimer = 0;
        ExplosionStarted = true;
        return true;
    }

    /// <summary>Runs the generic cinematic sprite handler for Mother Brain's definition.</summary>
    public void Step() => actor.Step(cue =>
    {
        if (cue != CinematicSpriteCueKind.StartIntroPage2)
            throw new InvalidOperationException($"The intro Mother Brain programs publish no {cue} cue.");
        PageTwoRequested = true;
    });

    /// <summary>Alternates BG1 scroll by four pixels from the inherited flashback offset.</summary>
    private void ApplyScreenShake(int cinematicFrameCounter)
    {
        // The shake adds four on even cinematic frames and subtracts four on odd frames.
        // The inherited scroll of 8 therefore alternates 12/8; starting this host
        // field at zero would produce 4/0 and shift the whole room down one tile.
        // The scroll wraps at sixteen bits, as every room scroll does.
        Background += (cinematicFrameCounter & 1) == 0
            ? new Offset16<RoomPixels>(0, 4)
            : new Offset16<RoomPixels>(0, -4);
    }
}
