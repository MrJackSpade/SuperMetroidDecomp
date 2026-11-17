using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Visible animation core of the intro Mother Brain's cinematic sprite definition.
/// </summary>
/// <remarks>
/// The actor starts at (56,111) in her own colouring,
/// in front of the scene. Its animation cycles four authored frames at
/// sixteen frames apiece. The actor also retains the hit counter, eight-frame colour
/// flash, and the 128-frame exploding state and the room picture's shake.
/// </remarks>
internal sealed class IntroMotherBrainSpriteState
{
    /// <summary>The cinematic actor running the Mother Brain's typed animation (#641).</summary>
    private readonly CinematicActor<IntroMotherBrainAnimation, CinematicSpritePose, IntroMotherBrainCue> actor =
        new(IntroMotherBrainDefinitions.MotherBrainOrigin, Colouring, IntroMotherBrainAnimations.MotherBrain,
            IntroMotherBrainDefinitions.MotherBrain.Animation);
    /// <summary>Remaining colour alternations after a missile hit.</summary>
    private int hurtFlashTimer;
    /// <summary>
    /// Updates since the fourth hit began the explosion phase: zero at the hit, counted up once per update to the
    /// 128 that starts the page-two animation, after which it is no longer advanced.
    /// </summary>
    private int explodingTimer;
    /// <summary>Whether explosion timing has entered the page-two animation.</summary>
    private bool pageTwoAnimationStarted;
    /// <summary>Whether the page-two animation has started the cross-fade, which replaces hit flashing.</summary>
    private bool crossfadingToPageTwo;

    /// <summary>Fixed cinematic screen origin from Mother Brain's definition.</summary>
    public static Offset16<ScreenPixels> Position => IntroMotherBrainDefinitions.MotherBrainOrigin;

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
    public CinematicSpritePose? Pose => actor.Shown;

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
    /// Runs the actor's behaviour, which precedes its animation's timer decrement.
    /// </summary>
    /// <param name="cinematicFrameCounter">The cinematic's frame counter, which paces the room's shake.</param>
    /// <param name="crossfadeFinishing">Whether page two's crossfade reached its final update on this update (#1980).</param>
    public void RunBehavior(
        int cinematicFrameCounter,
        bool crossfadeFinishing)
    {
        if (crossfadingToPageTwo)
        {
            ApplyScreenShake(cinematicFrameCounter);
            // Page two's crossfade reaching its final update deletes this actor and clears Samus's intro display.
            if (crossfadeFinishing)
                IsVisible = false;
            return;
        }

        ApplyHurtFlash();
        if (!ExplosionStarted || pageTwoAnimationStarted)
            return;

        ApplyScreenShake(cinematicFrameCounter);

        explodingTimer++;
        if (explodingTimer >= IntroMotherBrainDefinitions.ExplodingUpdates)
        {
            // The exploding behaviour starts the page-two animation. The shared step below runs its page request and
            // starts the cross-fade during this same cinematic-actor update.
            actor.Start(IntroMotherBrainAnimation.StartingPageTwo);
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
    /// The fourth hit starts her explosion, which tests no missiles, so from then on every missile passes through.
    /// </summary>
    /// <param name="impacts">The flashback projectile system's enemy impacts, which kill the missile.</param>
    /// <param name="sharedProjectiles">The flashback's bombs and power-bomb explosion, which the kill consults.</param>
    /// <returns>Whether this hit was the fourth, which starts the explosion.</returns>
    public bool TakeMissileHit(ProjectileEnemyImpacts impacts, SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(impacts);
        if (ExplosionStarted)
            return false;
        return impacts.TryImpactIntroMotherBrainMissile(sharedProjectiles, IntroMotherBrainDefinitions.MissileImpactLine) && RecordMissileHit();
    }

    /// <summary>Records the synchronous hit side effects after the projectile is killed.</summary>
    public bool RecordMissileHit()
    {
        hurtFlashTimer = IntroMotherBrainDefinitions.HurtFlashUpdates;
        HitCount++;
        if (HitCount != IntroMotherBrainDefinitions.MissileHitsToExplode)
            return false;

        // The fourth hit clears the actor timer before selecting the exploding behaviour. The
        // explosion behaviour does not run until the next cinematic-actor frame.
        explodingTimer = 0;
        ExplosionStarted = true;
        return true;
    }

    /// <summary>Runs one cinematic sprite update for Mother Brain's definition.</summary>
    public void Step() => actor.Step(cue =>
    {
        switch (cue)
        {
            case IntroMotherBrainCue.StartPageTwo:
                PageTwoRequested = true;
                return;
            case IntroMotherBrainCue.StartCrossFade:
                crossfadingToPageTwo = true;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(IntroMotherBrainCue)} {cue}.");
        }
    });

    /// <summary>Alternates the room picture's scroll by four pixels from the inherited flashback offset.</summary>
    private void ApplyScreenShake(int cinematicFrameCounter)
    {
        // The shake adds four on even cinematic frames and subtracts four on odd ones, so the room's scroll alternates
        // between twelve and the inherited eight and never passes the room's top.
        Background = Background.WithY(Background.Y +
            (IntroCadenceDefinitions.MotherBrainRoomShakesUp.IsSetIn(cinematicFrameCounter)
                ? -IntroMotherBrainDefinitions.RoomShakePixels
                : IntroMotherBrainDefinitions.RoomShakePixels));
    }
}
