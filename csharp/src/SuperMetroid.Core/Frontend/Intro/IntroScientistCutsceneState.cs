using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Scripted Ceres laboratory vignette between intro pages three and four.</summary>
internal sealed class IntroScientistCutsceneState
{
    /// <summary>Delivered or examined Baby actor whose animation drives scene completion.</summary>
    internal readonly IntroDiscoverySprite baby;

    /// <summary>Initializes the selected laboratory page, mirrored camera pan, and Baby actor animation.</summary>
    private IntroScientistCutsceneState(IntroLaboratoryScene scene)
    {
        Scene = scene;
        bool delivery = scene == IntroLaboratoryScene.Delivery;
        IntroBabyActorDefinition definition = delivery
            ? IntroBabyActorDefinitions.DeliveredBaby
            : IntroBabyActorDefinitions.ExaminedBaby;

        // The delivered and examined babies' definitions use separate laboratory pages and mirrored pan axes.
        baby = new IntroDiscoverySprite(
            definition.Position,
            IntroShades.Drawn(definition.Colouring),
            definition.Animation);
        baby.InstallFrameBehavior(definition.FrameBehavior);
        Background = delivery ? new(32, 8) : new(0, CoordinateAxis.Wrap(-24));
    }

    /// <summary>The laboratory page's scroll.</summary>
    public Position16<RoomPixels> Background { get; private set; }

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

    /// <summary>Advances the Baby script and pans on every fourth cinematic-phase tick until the alternate crossfade timer redirects it to deletion.</summary>
    public void Step(
        int cinematicTimer,
        int introCrossfadeTimer)
    {
        if (!baby.IsActive)
            return;

        // The delivery behaviour deletes only when the *alternate* intro counter reaches zero during the
        // later page-four reverse crossfade. The forward scientist fade decrements the
        // cinematic-phase counter while the intro crossfade counter remains 127.
        if (introCrossfadeTimer == 0)
        {
            baby.Start(CinematicSpriteSegment.Delete);
        }
        else if (IntroCadenceDefinitions.LaboratoryStepEveryFourthUpdate.IsDue(cinematicTimer))
        {
            if (Scene == IntroLaboratoryScene.Delivery && Background.X != 0)
            {
                // The delivery scene moves the camera left while keeping the delivered baby fixed
                // relative to the laboratory subjects.
                Background += new Offset16<RoomPixels>(-1, 0);
                baby.WholePosition += new Offset16<ScreenPixels>(1, 0);
            }
            else if (Scene == IntroLaboratoryScene.Examination &&
                CoordinateAxis.Difference(Background.Y, 8) < 0)
            {
                // The examination scene moves the second laboratory page down from -24 to +8 while the
                // examined baby rises by the same 32 pixels.
                Background += new Offset16<RoomPixels>(0, 1);
                baby.WholePosition += new Offset16<ScreenPixels>(0, -1);
            }
        }

        baby.Step(HandleCue);
    }

    /// <summary>Draws the current Baby actor frame through the installed scientist-scene sprite presentation.</summary>
    public void Draw(ScreenSprites objects,
        IIntroCinematicSpritePresentation? installedArt = null) =>
        baby.Draw(objects, installedArt: installedArt);

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
            case CinematicSpriteCueKind.SetFrameBehavior:
            case CinematicSpriteCueKind.EndZebesExplosion:
            case CinematicSpriteCueKind.ExplosionFinale:
            case CinematicSpriteCueKind.FadeExplosionPalette:
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
            case CinematicSpriteCueKind.SpawnRinkas0And1:
            case CinematicSpriteCueKind.SpawnRinkas2And3:
            case CinematicSpriteCueKind.SpawnSuitedJump:
            case CinematicSpriteCueKind.SpawnSuitlessJump:
            case CinematicSpriteCueKind.StartFlyingToZebes:
            case CinematicSpriteCueKind.StartIntroPage2:
            case CinematicSpriteCueKind.StartIntroPage3:
            case CinematicSpriteCueKind.StartRinkaMoving:
            case CinematicSpriteCueKind.StartZebesExplosion:
            case CinematicSpriteCueKind.TransitionToCredits:
            default:
                throw new InvalidOperationException($"The scientist-scene Baby programs publish no {cue} cue.");
        }
    }
}
