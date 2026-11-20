using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Scripted Ceres laboratory vignette between intro pages three and four.</summary>
internal sealed class IntroScientistCutsceneState
{
    /// <summary>Delivered or examined Baby actor whose animation drives scene completion.</summary>
    private readonly CinematicActor<IntroBabyAnimation, CinematicSpritePose, IntroLaboratoryCue> baby;

    /// <summary>Initializes the selected laboratory page, mirrored camera pan, and Baby actor animation.</summary>
    private IntroScientistCutsceneState(IntroLaboratoryScene scene)
    {
        Scene = scene;
        bool delivery = scene == IntroLaboratoryScene.Delivery;
        IntroBabyActorDefinition definition = delivery
            ? IntroBabyActorDefinitions.DeliveredBaby
            : IntroBabyActorDefinitions.ExaminedBaby;

        // The delivered and examined babies' definitions use separate laboratory pages and mirrored pan axes.
        baby = new(definition.Position, IntroShades.Drawn(definition.Colouring), IntroBabyAnimations.Laboratory, definition.Animation);
        Background = delivery ? IntroLaboratoryDefinitions.DeliveryPageScroll : IntroLaboratoryDefinitions.ExaminationPageStartScroll;
    }

    /// <summary>The laboratory page's scroll.</summary>
    public Offset16<PlanePixels> Background { get; private set; }

    /// <summary>
    /// Which laboratory scene this is: it selects delivery's horizontal pan or examination's vertical pan, the page
    /// that follows, and the background page the scene shows.
    /// </summary>
    public IntroLaboratoryScene Scene { get; }

    /// <summary>Set when the Baby's cue hands the intro to the page the scene leads into.</summary>
    /// <remarks>
    /// The scene's end, kept once its cue reaches it (#2562): the cinematic's phase step, which runs before the actors'
    /// step on each update, starts the next page on the update after the cue, so the end lasts across that boundary; it is
    /// set once and never cleared, and the next page's setup discards the scene.
    /// </remarks>
    public bool NextPageRequested { get; private set; }

    /// <summary>Starts the delivered-Baby page with a leftward camera pan and page-four handoff.</summary>
    public static IntroScientistCutsceneState CreateDelivery() => new(IntroLaboratoryScene.Delivery);

    /// <summary>Starts the examined-Baby page with a downward camera pan and page-five handoff.</summary>
    public static IntroScientistCutsceneState CreateExamination() => new(IntroLaboratoryScene.Examination);

    /// <summary>
    /// Advances the Baby script and pans the laboratory with it on each crossfade step counted down by the crossfade into
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
            && IntroCadenceDefinitions.LaboratoryStep.IsDue(intoLaboratory.UpdatesLeft))
        {
            if (Scene == IntroLaboratoryScene.Delivery && Background.X != 0)
            {
                // The delivery scene moves the camera left while keeping the delivered baby fixed
                // relative to the laboratory subjects.
                Background += IntroLaboratoryDefinitions.DeliveryPagePan;
                baby.ScreenPlace += IntroLaboratoryDefinitions.DeliveredBabyCounterPan;
            }
            else if (Scene == IntroLaboratoryScene.Examination &&
                Background.Y < IntroLaboratoryDefinitions.ExaminationPageEndY)
            {
                // The examination scene moves the second laboratory page down to its end scroll while the
                // examined baby rises by the same distance.
                Background += IntroLaboratoryDefinitions.ExaminationPageScroll;
                baby.ScreenPlace += IntroLaboratoryDefinitions.ExaminedBabyRise;
            }
        }

        baby.Step(HandleCue);
    }

    /// <summary>Draws the current Baby actor frame through the installed scientist-scene sprite presentation.</summary>
    public void Draw(ScreenSprites screenSprites, IIntroCinematicSpritePresentation installedArt) =>
        baby.Draw(screenSprites, installedArt: installedArt);

    /// <summary>Requests the next intro page when the Baby's animation reaches its page cue.</summary>
    private void HandleCue(IntroLaboratoryCue cue)
    {
        switch (cue)
        {
            case IntroLaboratoryCue.StartNextPage:
                // The Baby's last pulse ends by selecting the next page.
                NextPageRequested = true;
                return;
        }
    }
}
