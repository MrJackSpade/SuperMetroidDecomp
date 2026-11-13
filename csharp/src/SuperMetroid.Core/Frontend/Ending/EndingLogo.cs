using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Logo actors followed by sixteen palette loads.</summary>
internal sealed class EndingLogo
{
    /// <summary>Scene cue fired when the top logo half reaches its landing point.</summary>
    private readonly Action landed;
    /// <summary>Two moving halves and two supporting logo sprites.</summary>
    private readonly IntroDiscoverySprite[] actors = new IntroDiscoverySprite[4];
    /// <summary>Accelerating per-frame speeds of the two moving halves.</summary>
    private readonly int[] speeds = [EndingLogoDefinitions.InitialSpeed, EndingLogoDefinitions.InitialSpeed];
    /// <summary>Stops each half at its respective landing point.</summary>
    private readonly bool[] settled = new bool[2];
    /// <summary>Becomes true when the grey-out cue begins paired background and sprite changes.</summary>
    public bool CrossfadeStarted { get; internal set; }
    /// <summary>Number of crossfade steps already shown.</summary>
    public int PaletteStep { get; internal set; }
    /// <summary>The logo hands off to percentage text after its sixteenth palette step.</summary>
    public bool Completed => PaletteStep == EndingLogoDefinitions.PaletteSteps;

    /// <summary>Creates four logo actors, blacks out the logo's background colours and shows its first sprite colours (#1512).</summary>
    /// <param name="stage">The stage whose colours the logo shows.</param>
    /// <param name="landed">Called once the top half lands.</param>
    public EndingLogo(EndingStage stage, Action landed)
    {
        this.landed = landed;
        for (int i = 0; i < actors.Length; i++)
        {
            CinematicActorDefinition definition = EndingLogoActors.Actor(i);
            actors[i] = new IntroDiscoverySprite(
                EndingLogoActors.Origin(i),
                EndingShades.Drawn(EndingLogoDefinitions.Colouring),
                definition.Animation);
            actors[i].InstallFrameBehavior(definition.FrameBehavior);
        }
        Ensure.NotNull(stage).Colours.Flash(EndingFlash.LogoBackground, null);
        stage.LoadColours(EndingColourLoad.LogoInitial);
    }

    /// <summary>Shows the pending crossfade step before moving actors and running their next cues.</summary>
    /// <param name="colours">The ending's colours, which show the crossfade's step.</param>
    public void Step(EndingColours colours)
    {
        if (Completed) throw new InvalidOperationException("Completed logo must hand off to percentage text.");
        // The cinematic function runs before sprite cues, so the first palette
        // pair is copied on the call after the request, not during the call that requests it.
        if (CrossfadeStarted)
        {
            Ensure.NotNull(colours).LogoCrossfade(PaletteStep);
            if (++PaletteStep == EndingLogoDefinitions.PaletteSteps) return;
        }
        for (int i = 0; i < actors.Length; i++)
        {
            if (i < 2 && !settled[i]) MoveHalf(i);
            actors[i].Step(HandleCue);
        }
    }

    /// <summary>Accelerates one logo half toward its landing point and signals arrival for the top half.</summary>
    private void MoveHalf(int index)
    {
        var actor = actors[index];
        int direction = index == 0 ? -1 : 1;
        Position16<ScreenPixels> moved = actor.WholePosition +
            new Offset16<ScreenPixels>(direction * speeds[index], -direction * speeds[index]);
        actor.WholePosition = moved;
        var target = index == 0 ? EndingLogoDefinitions.TopLanding : EndingLogoDefinitions.BottomLanding;
        if (index == 0 ? CoordinateAxis.Difference(moved.X - target.X, 1) < 0 : CoordinateAxis.Difference(moved.X, target.X) >= 0)
        {
            actor.WholePosition = target;
            settled[index] = true;
            if (index == 0) landed();
        }
        else speeds[index] += EndingLogoDefinitions.Acceleration;
    }

    /// <summary>Starts the crossfade on the step after the grey-out cue.</summary>
    private void HandleCue(CinematicSpriteCueKind cue)
    {
        if (cue != CinematicSpriteCueKind.GreyOut)
            throw new InvalidOperationException($"The logo programs publish no {cue} cue.");
        CrossfadeStarted = true;
    }

    /// <summary>Places active logo actors as the supplied sprite entries.</summary>
    public void Draw(ScreenSprites destination, IIntroCinematicSpritePresentation installedArt)
    {
        Ensure.NotNull(destination);
        if (!Completed)
            foreach (var actor in actors)
                actor.Draw(destination, EndingLogoDefinitions.Camera, installedArt);
    }
}
