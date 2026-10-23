using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Native logo actors followed by E58A's sixteen palette transfers.</summary>
internal sealed class EndingLogo
{
    /// <summary>Scene callback fired when the top logo half reaches its landing point.</summary>
    private readonly Action landed;
    /// <summary>Two moving halves and two supporting logo sprites.</summary>
    private readonly IntroDiscoverySprite[] actors = new IntroDiscoverySprite[4];
    /// <summary>Accelerating per-frame speeds of the two moving halves.</summary>
    private readonly int[] speeds = [EndingLogoDefinitions.InitialSpeed, EndingLogoDefinitions.InitialSpeed];
    /// <summary>Stops each half at its respective landing point.</summary>
    private readonly bool[] settled = new bool[2];
    /// <summary>Selected palette source rebound after scene-state restoration.</summary>
    [NonSerialized] private EndingPaletteCatalog? paletteArtwork;
    /// <summary>Becomes true when the grey-out instruction begins paired background and OBJ transfers.</summary>
    public bool CrossfadeStarted { get; internal set; }
    /// <summary>Number of paired palette transfers already applied.</summary>
    public int PaletteStep { get; internal set; }
    /// <summary>The logo hands off to percentage text after its sixteenth palette step.</summary>
    public bool Completed => PaletteStep == EndingLogoDefinitions.PaletteSteps;

    /// <summary>Creates four logo actors and loads the initial artwork palette into CGRAM.</summary>
    public EndingLogo(IPaletteColors colours, Action landed,
        EndingPaletteCatalog? paletteArtwork = null)
    {
        this.landed = landed;
        this.paletteArtwork = paletteArtwork;
        for (int i = 0; i < actors.Length; i++)
        {
            CinematicActorDefinition definition = EndingLogoDefinitions.Actor(i);
            actors[i] = new IntroDiscoverySprite(
                EndingLogoDefinitions.Origin(i),
                SnesObjPalettes.Palette(7).Raw,
                definition.Program);
            actors[i].InstallPreInstruction(definition.PreInstruction);
        }
        new FrontendPalettes(colours).EndingLogoBackground.Fill(Bgr555.Black);
        (paletteArtwork ?? throw new InvalidOperationException(
            "Ending logo requires installed palette artwork."))
            [EndingPaletteId.LogoInitial].LoadTo(colours, 0, 16, 240);
    }

    /// <summary>Restored scene state keeps its current CGRAM; only future transfers change.</summary>
    public void BindPaletteArtwork(EndingPaletteCatalog? value) => paletteArtwork = value;

    /// <summary>Applies the pending crossfade pair before moving actors and interpreting their next instructions.</summary>
    public void Step(IPaletteColors colours)
    {
        if (Completed) throw new InvalidOperationException("Completed logo must hand off to percentage text.");
        // The cinematic function runs before sprite instructions, so the first palette
        // pair is copied on the call after F25E, not during the call that requests it.
        if (CrossfadeStarted)
        {
            for (int palette = 0; palette < 2; palette++)
            {
                for (int i = 15; i >= 0; i--)
                    (palette == 0 ? new FrontendPalettes(colours).EndingLogoBackground : new FrontendPalettes(colours).EndingLogoObjects).Write(i,
                        (paletteArtwork ?? throw new InvalidOperationException(
                            "Ending logo requires installed palette artwork."))
                            [EndingPaletteId.LogoCrossfade].Color((PaletteStep * 2 + palette) * 16 + i));
            }
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
            new Offset16<ScreenPixels>(unchecked((short)(direction * speeds[index])), unchecked((short)(-direction * speeds[index])));
        actor.WholePosition = moved;
        var target = index == 0 ? EndingLogoDefinitions.TopLanding : EndingLogoDefinitions.BottomLanding;
        if (index == 0 ? (short)(moved.X - target.X - 1) < 0 : (short)(moved.X - target.X) >= 0)
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
