using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Scripted Ceres laboratory vignette between intro pages three and four.</summary>
internal sealed class IntroScientistCutsceneState
{
    /// <summary>Delivered or examined Baby actor whose animation drives scene completion.</summary>
    internal readonly CinematicActor baby;

    /// <summary>Initializes the selected laboratory page, mirrored camera pan, and Baby actor animation.</summary>
    private IntroScientistCutsceneState(IntroLaboratoryScene scene)
    {
        Scene = scene;
        bool delivery = scene == IntroLaboratoryScene.Delivery;
        IntroBabyActorDefinition definition = delivery
            ? IntroBabyActorDefinitions.DeliveredBaby
            : IntroBabyActorDefinitions.ExaminedBaby;

        // The delivered and examined babies' definitions use separate laboratory pages and mirrored pan axes.
        baby = new CinematicActor(
            definition.Position,
            IntroShades.Drawn(definition.Colouring),
            definition.Animation);
        Background = delivery ? IntroLaboratoryDefinitions.DeliveryPageScroll : IntroLaboratoryDefinitions.ExaminationPageStartScroll;
    }

    /// <summary>The laboratory page's scroll.</summary>
    public Offset16<PlanePixels> Background { get; private set; }

    /// <summary>
    /// Which laboratory scene this is: it selects delivery's horizontal pan or examination's vertical pan, the page
    /// that follows, and the background page the scene shows.
    /// </summary>
    public IntroLaboratoryScene Scene { get; }

    /// <summary>Set when delivery's actor cue hands the intro to page four.</summary>
    public bool PageFourRequested { get; private set; }

    /// <summary>Set when examination's actor cue hands the intro to page five.</summary>
    public bool PageFiveRequested { get; private set; }

    /// <summary>Starts the delivered-Baby page with a leftward camera pan and page-four handoff.</summary>
    public static IntroScientistCutsceneState CreateDelivery() => new(IntroLaboratoryScene.Delivery);

    /// <summary>Starts the examined-Baby page with a downward camera pan and page-five handoff.</summary>
    public static IntroScientistCutsceneState CreateExamination() => new(IntroLaboratoryScene.Examination);

    /// <summary>
    /// Advances the Baby script and pans the laboratory with it on every fourth update counted down by the crossfade into
    /// the laboratory; the crossfade back out of the laboratory ends the Baby as it reaches its final update (#1980).
    /// </summary>
    /// <param name="crossfade">What the running crossfade did on this update, or null when none runs.</param>
    public void Step(IntroCrossfadeTick? crossfade)
    {
        if (!baby.IsActive)
            return;

        if (crossfade is { Finishing: true, From: IntroCrossfadeScene.Laboratory })
        {
            baby.Delete();
        }
        else if (crossfade is { To: IntroCrossfadeScene.Laboratory } intoLaboratory
            && IntroCadenceDefinitions.LaboratoryStepEveryFourthUpdate.IsDue(intoLaboratory.UpdatesLeft))
        {
            if (Scene == IntroLaboratoryScene.Delivery && Background.X != 0)
            {
                // The delivery scene moves the camera left while keeping the delivered baby fixed
                // relative to the laboratory subjects.
                Background += new Offset16<PlanePixels>(-1, 0);
                baby.ScreenPlace += new Offset16<ScreenPixels>(1, 0);
            }
            else if (Scene == IntroLaboratoryScene.Examination &&
                Background.Y < IntroLaboratoryDefinitions.ExaminationPageEndY)
            {
                // The examination scene moves the second laboratory page down to its end scroll while the
                // examined baby rises by the same distance.
                Background += new Offset16<PlanePixels>(0, 1);
                baby.ScreenPlace += new Offset16<ScreenPixels>(0, -1);
            }
        }

        baby.Step(HandleCue);
    }

    /// <summary>Draws the current Baby actor frame through the installed scientist-scene sprite presentation.</summary>
    public void Draw(ScreenSprites screenSprites,
        IIntroCinematicSpritePresentation? installedArt = null) =>
        baby.Draw(screenSprites, installedArt: installedArt);

    /// <summary>Requests the next intro page when the Baby's animation reaches its page cue.</summary>
    private void HandleCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.StartIntroPage4:
                // The delivery loop ends by selecting page four.
                PageFourRequested = true;
                return;
            case CinematicSpriteCueKind.StartIntroPage5:
                // The examination loop ends by selecting page five.
                PageFiveRequested = true;
                return;
            case CinematicSpriteCueKind.StartMotherBrainCrossFade:
            case CinematicSpriteCueKind.DeleteEggAfterCrossFade:
            case CinematicSpriteCueKind.StartRepeatingExplosions:
            case CinematicSpriteCueKind.WaitForFlyaway:
            case CinematicSpriteCueKind.StartBodyFlight:
            case CinematicSpriteCueKind.StartHeadFlight:
            case CinematicSpriteCueKind.EndZebesExplosion:
            case CinematicSpriteCueKind.ExplosionFinale:
            case CinematicSpriteCueKind.FadeExplosionColours:
            case CinematicSpriteCueKind.FadeInZebesText:
            case CinematicSpriteCueKind.FadeOutZebesText:
            case CinematicSpriteCueKind.GreyOut:
            case CinematicSpriteCueKind.Launch:
            case CinematicSpriteCueKind.LaunchHead:
            case CinematicSpriteCueKind.PrepareHead:
            case CinematicSpriteCueKind.Shoot:
            case CinematicSpriteCueKind.SpawnClearTime:
            case CinematicSpriteCueKind.SpawnColon:
            case CinematicSpriteCueKind.SpawnCompletedText:
            case CinematicSpriteCueKind.SpawnExplosionSilhouette:
            case CinematicSpriteCueKind.SpawnFinalExplosionWave:
            case CinematicSpriteCueKind.SpawnHoursTens:
            case CinematicSpriteCueKind.SpawnHoursUnits:
            case CinematicSpriteCueKind.SpawnInitialExplosionWave:
            case CinematicSpriteCueKind.SpawnJapaneseZebesTextIfNeeded:
            case CinematicSpriteCueKind.SpawnMinutesTens:
            case CinematicSpriteCueKind.SpawnMinutesUnits:
            case CinematicSpriteCueKind.SpawnParticles:
            case CinematicSpriteCueKind.SpawnStrikingRinkaPair:
            case CinematicSpriteCueKind.SpawnMissingRinkaPair:
            case CinematicSpriteCueKind.SpawnSuitedJump:
            case CinematicSpriteCueKind.SpawnSuitlessJump:
            case CinematicSpriteCueKind.StartFlyingToZebes:
            case CinematicSpriteCueKind.StartIntroPage2:
            case CinematicSpriteCueKind.StartIntroPage3:
            case CinematicSpriteCueKind.StartRinkaMoving:
            case CinematicSpriteCueKind.StartZebesExplosion:
            case CinematicSpriteCueKind.TransitionToCredits:
            default:
                throw new InvalidOperationException($"The scientist scene Baby's animations publish no {cue} cue.");
        }
    }
}
