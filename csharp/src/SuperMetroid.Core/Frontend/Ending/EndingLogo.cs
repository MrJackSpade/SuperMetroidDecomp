using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's assembling logo, followed by its colour loads (<see cref="EndingLogoDefinitions.CrossfadeSteps"/>): its two S halves move into place while its two
/// circles draw in (#1979).
/// </summary>
internal sealed class EndingLogo
{
    /// <summary>The upper S half, which signals the scene as it lands.</summary>
    private readonly MovingHalf upperS;
    /// <summary>The lower S half.</summary>
    private readonly MovingHalf lowerS;
    /// <summary>The upper circle.</summary>
    private readonly CinematicActor<EndingLogoAnimation, CinematicSpritePose, EndingLogoCue> upperCircle;
    /// <summary>The lower circle.</summary>
    private readonly CinematicActor<EndingLogoAnimation, CinematicSpritePose, EndingLogoCue> lowerCircle;
    /// <summary>Becomes true when the grey-out cue begins paired background and sprite changes.</summary>
    public bool CrossfadeStarted { get; private set; }
    /// <summary>The crossfade step the logo showed last, or null before its crossfade shows a step.</summary>
    private EndingLogoCrossfadeStep? ShownCrossfade { get; set; }
    /// <summary>Whether the logo has shown its crossfade's last step and hands off to the percentage text.</summary>
    public bool Completed { get; private set; }

    /// <summary>Creates the four logo actors, blacks out the logo's background colours and shows the assembling logo's first colours (#1512).</summary>
    /// <param name="stage">The stage whose colours the logo shows.</param>
    /// <param name="landed">Called once the upper S half lands.</param>
    public EndingLogo(EndingStage stage, Action landed)
    {
        Ensure.NotNull(landed);
        upperS = new MovingHalf(Create(EndingLogoDefinitions.UpperS), EndingLogoDefinitions.TopLanding, HorizontalDirection.Left, landed);
        lowerS = new MovingHalf(Create(EndingLogoDefinitions.LowerS), EndingLogoDefinitions.BottomLanding, HorizontalDirection.Right, Landed: null);
        upperCircle = Create(EndingLogoDefinitions.UpperCircle);
        lowerCircle = Create(EndingLogoDefinitions.LowerCircle);
        Ensure.NotNull(stage).Colours.Flash(EndingFlash.LogoBackground, null);
        stage.LoadColours(EndingColourLoad.LogoInitial);
    }

    /// <summary>The actors in the order they spawned, which is the order they step and draw in.</summary>
    private IEnumerable<CinematicActor<EndingLogoAnimation, CinematicSpritePose, EndingLogoCue>> Actors => [upperS.Actor, lowerS.Actor, upperCircle, lowerCircle];

    /// <summary>Shows the pending crossfade step before moving actors and running their next cues.</summary>
    /// <param name="colours">The ending's colours, which show the crossfade's step.</param>
    public void Step(EndingColours colours)
    {
        if (Completed) throw new InvalidOperationException("Completed logo must hand off to percentage text.");
        // The crossfade's first step shows on the update after the grey-out cue, not on the update that requests it.
        if (CrossfadeStarted)
        {
            EndingLogoCrossfadeStep step = ShownCrossfade is { } shown
                ? shown.Following ?? throw new InvalidOperationException("The logo's crossfade showed its last step.")
                : EndingLogoCrossfadeStep.First;
            Ensure.NotNull(colours).LogoCrossfade(step);
            ShownCrossfade = step;
            if (step.Following is null)
            {
                Completed = true;
                return;
            }
        }
        // Each S half moves just before its own animation steps.
        upperS.Move();
        upperS.Actor.Step(HandleCue);
        lowerS.Move();
        lowerS.Actor.Step(HandleCue);
        upperCircle.Step(HandleCue);
        lowerCircle.Step(HandleCue);
    }

    /// <summary>Starts the crossfade on the step after the grey-out cue.</summary>
    private void HandleCue(EndingLogoCue cue)
    {
        switch (cue)
        {
            case EndingLogoCue.GreyOut:
                CrossfadeStarted = true;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EndingLogoCue)} {cue}.");
        }
    }

    /// <summary>Places active logo actors as the supplied sprites.</summary>
    public void Draw(ScreenSprites destination, IIntroCinematicSpritePresentation installedArt)
    {
        Ensure.NotNull(destination);
        if (!Completed)
            foreach (CinematicActor<EndingLogoAnimation, CinematicSpritePose, EndingLogoCue> actor in Actors)
                actor.Draw(destination, installedArt);
    }

    /// <summary>Creates the actor <paramref name="definition"/> describes, in the logo's colouring.</summary>
    private static CinematicActor<EndingLogoAnimation, CinematicSpritePose, EndingLogoCue> Create(EndingLogoActorDefinition definition) =>
        new(definition.Origin, EndingShades.Drawn(EndingLogoDefinitions.Colouring), EndingLogoAnimations.Logo, definition.Animation);

    /// <summary>
    /// An S half accelerating diagonally toward its landing point: the upper half down and left, the lower half up and
    /// right, each stopping where it reaches or passes its landing point's X.
    /// </summary>
    /// <param name="Actor">The half's actor.</param>
    /// <param name="Landing">Where the half comes to rest.</param>
    /// <param name="Direction">The way the half moves across: left, which also moves it down, or right, which also moves it up.</param>
    /// <param name="Landed">Called as the half lands, or null for none.</param>
    private sealed record MovingHalf(CinematicActor<EndingLogoAnimation, CinematicSpritePose, EndingLogoCue> Actor, Offset16<ScreenPixels> Landing,
        HorizontalDirection Direction, Action? Landed)
    {
        /// <summary>The half's speed on each axis, in pixels per update.</summary>
        private int speed = EndingLogoDefinitions.InitialSpeed;

        /// <summary>Whether the half has landed and moves no more.</summary>
        private bool settled;

        /// <summary>Moves the half one update toward its landing point, landing it there once it reaches or passes it.</summary>
        internal void Move()
        {
            if (settled)
                return;
            int across = Direction switch
            {
                HorizontalDirection.Left => -speed,
                HorizontalDirection.Right => speed,
                _ => throw new InvalidOperationException($"Undefined {nameof(HorizontalDirection)} {Direction}."),
            };
            Offset16<ScreenPixels> moved = Actor.ScreenPlace + new Offset16<ScreenPixels>(across, -across);
            Actor.ScreenPlace = moved;
            // The half lands once it reaches or passes its landing point's X in its own direction.
            bool reached = Direction == HorizontalDirection.Left ? moved.X <= Landing.X : moved.X >= Landing.X;
            if (reached)
            {
                Actor.ScreenPlace = Landing;
                settled = true;
                Landed?.Invoke();
            }
            else speed += EndingLogoDefinitions.Acceleration;
        }
    }
}
