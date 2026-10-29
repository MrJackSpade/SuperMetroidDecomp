using SuperMetroid.Core.Rendering;
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
    [NonSerialized] private EndingPaletteSet? paletteArtwork;
    /// <summary>Becomes true when the grey-out instruction begins paired background and OBJ transfers.</summary>
    public bool CrossfadeStarted { get; internal set; }
    /// <summary>Number of paired palette transfers already applied.</summary>
    public int PaletteStep { get; internal set; }
    /// <summary>The logo hands off to percentage text after its sixteenth palette step.</summary>
    public bool Completed => PaletteStep == EndingLogoDefinitions.PaletteSteps;

    /// <summary>Creates four logo actors and loads the initial artwork palette into CGRAM.</summary>
    public EndingLogo(ScreenPalette colours, Action landed,
        EndingPaletteSet? paletteArtwork = null)
    {
        this.landed = landed;
        this.paletteArtwork = paletteArtwork;
        for (int i = 0; i < actors.Length; i++)
        {
            CinematicActorDefinition definition = EndingLogoActors.Actor(i);
            actors[i] = new IntroDiscoverySprite(
                EndingLogoActors.Origin(i),
                ScreenColourGroups.SpriteRow(ScreenColourGroup.EndingLogoSprites),
                definition.Program);
            actors[i].InstallPreInstruction(definition.PreInstruction);
        }
        Ensure.NotNull(colours).Colours(ScreenColourGroup.EndingLogoBackground).Fill(RgbColor.Black);
        RequirePaletteArtwork()[EndingPaletteId.LogoInitial].LoadTo(colours.Colours(ScreenColourGroup.EndingLogoSprites), 0);
    }

    /// <summary>Restored scene state keeps its current CGRAM; only future transfers change.</summary>
    public void BindPaletteArtwork(EndingPaletteSet? value) => paletteArtwork = value;

    /// <summary>Applies the pending crossfade pair before moving actors and interpreting their next instructions.</summary>
    public void Step(ScreenPalette colours)
    {
        if (Completed) throw new InvalidOperationException("Completed logo must hand off to percentage text.");
        // The cinematic function runs before sprite instructions, so the first palette
        // pair is copied on the call after F25E, not during the call that requests it.
        if (CrossfadeStarted)
        {
            EndingPalette crossfade = RequirePaletteArtwork()[EndingPaletteId.LogoCrossfade];
            LoadCrossfadeStep(Ensure.NotNull(colours).Colours(ScreenColourGroup.EndingLogoBackground), crossfade, EndingLogoLayer.Background);
            LoadCrossfadeStep(colours.Colours(ScreenColourGroup.EndingLogoSprites), crossfade, EndingLogoLayer.Sprites);
            if (++PaletteStep == EndingLogoDefinitions.PaletteSteps) return;
        }
        for (int i = 0; i < actors.Length; i++)
        {
            if (i < 2 && !settled[i]) MoveHalf(i);
            actors[i].Step(HandleCue);
        }
    }

    /// <summary>Loads the current crossfade step's colours of <paramref name="layer"/> into that layer's screen colours.</summary>
    private void LoadCrossfadeStep(ColourRun layerColours, EndingPalette crossfade, EndingLogoLayer layer)
    {
        for (int colour = 0; colour < layerColours.Count; colour++)
            layerColours.Write(colour, crossfade.LogoCrossfadeColour(PaletteStep, layer, colour));
    }

    /// <summary>The installed ending palettes the logo loads its colours from.</summary>
    private EndingPaletteSet RequirePaletteArtwork() => paletteArtwork ?? throw new InvalidOperationException(
        "Ending logo requires installed palette artwork.");

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
