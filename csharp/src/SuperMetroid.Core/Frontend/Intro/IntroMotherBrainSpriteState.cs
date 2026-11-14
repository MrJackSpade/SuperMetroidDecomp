using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Visible animation core of the intro Mother Brain's cinematic sprite definition.
/// </summary>
/// <remarks>
/// The definition's initializer fixes the actor at (56,111) in her own colouring,
/// in front of the scene. Its animation cycles four authored frames at
/// sixteen frames apiece. The actor also retains the hit counter, eight-frame palette
/// flash, and the 128-frame exploding state and the room picture's shake.
/// </remarks>
internal sealed class IntroMotherBrainSpriteState
{
    /// <summary>The cinematic sprite object running the Mother Brain's typed animation (#641).</summary>
    private readonly IntroDiscoverySprite actor =
        new(IntroMotherBrainDefinitions.MotherBrainOrigin, Colouring,
            IntroMotherBrainDefinitions.MotherBrain.Animation);
    /// <summary>Remaining palette alternations after a missile hit.</summary>
    private int hurtFlashTimer;
    /// <summary>
    /// Updates since the fourth hit began the explosion phase: zero at the hit, counted up once per update to the
    /// 128 that starts the page-two animation, after which it is no longer advanced.
    /// </summary>
    private int explodingTimer;
    /// <summary>Whether explosion timing has entered the page-two animation.</summary>
    private bool pageTwoAnimationStarted;
    /// <summary>Whether the page-two animation's frame behaviour has replaced hit flashing.</summary>
    private bool CrossfadingToPageTwo =>
        actor.ListFrameBehavior == CinematicSpriteFrameBehavior.IntroMotherBrainCrossFading;

    /// <summary>Fixed cinematic screen origin from Mother Brain's definition.</summary>
    public static Position16<ScreenPixels> Position => IntroMotherBrainDefinitions.MotherBrainOrigin;

    /// <summary>The drawn shade of the colouring Mother Brain draws in (#1512), which her hurt flash whitens.</summary>
    public static PictureShade Colouring => IntroShades.Drawn(IntroSpriteColouring.MotherBrain);

    /// <summary>
    /// Whether the hurt flash's last alternation whitened Mother Brain (#1512): every colour she draws in shows white
    /// until the next alternation restores her colours. The flash's last alternation always restores them.
    /// </summary>
    public bool FlashesWhite { get; private set; }

    /// <summary>Mother Brain's own colour effect: white while her hurt flash whitens her, none otherwise (#1512).</summary>
    public PictureShading Shading(PictureShading drawn) =>
        FlashesWhite ? Ensure.NotNull(drawn).With(new ColourFlash(RgbColor.White)) : Ensure.NotNull(drawn);

    /// <summary>The pose Mother Brain's animation shows; null before its first frame and after deletion.</summary>
    public CinematicSpritePose? Pose => actor.Pose;

    /// <summary>Number of missiles consumed by Mother Brain's hit test; the fourth starts the explosion.</summary>
    public int HitCount { get; private set; }

    /// <summary>True once the fourth hit switches this actor to its exploding behaviour.</summary>
    public bool ExplosionStarted { get; private set; }

    /// <summary>The flashback room picture's scroll, shaken vertically while Mother Brain explodes.</summary>
    public Position16<RoomPixels> Background { get; private set; } =
        new(0, IntroCinematicState.GameplayFlashbackVerticalScroll);

    /// <summary>True when the exploding behaviour switches the actor to its page-two animation.</summary>
    public bool PageTwoRequested { get; private set; }

    /// <summary>Whether the actor still contributes its frame to the cinematic sprites.</summary>
    public bool IsVisible { get; private set; } = true;

    /// <summary>
    /// Runs the actor's frame behaviour that precedes its animation's timer decrement.
    /// </summary>
    public void RunFrameBehavior(
        int cinematicFrameCounter,
        int introCrossfadeTimer)
    {
        if (CrossfadingToPageTwo)
        {
            ApplyScreenShake(cinematicFrameCounter);
            // The cross-fade behaviour observes the timer after the cinematic phase has decremented it.
            // At zero it redirects this actor to delete and clears Samus's intro display.
            if (introCrossfadeTimer == 0)
                IsVisible = false;
            return;
        }

        ApplyHurtFlash();
        if (!ExplosionStarted || pageTwoAnimationStarted)
            return;

        ApplyScreenShake(cinematicFrameCounter);

        explodingTimer++;
        if (explodingTimer >= 128)
        {
            // The exploding behaviour starts the page-two animation. The generic handler below runs its page request and
            // frame behaviour install during this same cinematic-object call.
            actor.Start(CinematicSpriteSegment.IntroMotherBrainStartPage2);
            pageTwoAnimationStarted = true;
        }
    }

    /// <summary>
    /// Eight-frame white/normal alternation of every colour Mother Brain draws in (#1512).
    /// </summary>
    public void ApplyHurtFlash()
    {
        if (hurtFlashTimer == 0)
            return;

        FlashesWhite = !IntroCadenceDefinitions.MotherBrainHurtShowsOwnColours.IsSetIn(hurtFlashTimer);
        hurtFlashTimer--;
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
        hurtFlashTimer = 8;
        HitCount++;
        if (HitCount != 4)
            return false;

        // The fourth hit clears the actor timer before selecting the exploding behaviour. The
        // explosion behaviour does not run until the next cinematic-object frame.
        explodingTimer = 0;
        ExplosionStarted = true;
        return true;
    }

    /// <summary>Runs one cinematic sprite update for Mother Brain's definition.</summary>
    public void Step() => actor.Step(cue =>
    {
        if (cue != CinematicSpriteCueKind.StartIntroPage2)
            throw new InvalidOperationException($"The intro Mother Brain's animations publish no {cue} cue.");
        PageTwoRequested = true;
    });

    /// <summary>Alternates the room picture's scroll by four pixels from the inherited flashback offset.</summary>
    private void ApplyScreenShake(int cinematicFrameCounter)
    {
        // The shake adds four on even cinematic frames and subtracts four on odd frames.
        // The inherited scroll of 8 therefore alternates 12/8; starting the scroll
        // at zero would produce 4/0 and shift the whole room down eight pixels.
        // The scroll wraps at sixteen bits, as every room scroll does.
        Background += IntroCadenceDefinitions.MotherBrainRoomShakesUp.IsSetIn(cinematicFrameCounter)
            ? new Offset16<RoomPixels>(0, -4)
            : new Offset16<RoomPixels>(0, 4);
    }
}
