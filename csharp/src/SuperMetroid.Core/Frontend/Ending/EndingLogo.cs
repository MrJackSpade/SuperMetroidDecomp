using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Native logo actors followed by E58A's sixteen palette transfers.</summary>
internal sealed class EndingLogo
{
    /// <summary>Instruction source for the four native logo actors.</summary>
    private readonly ISnesAddressSpace bus;
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
    public EndingLogo(ISnesAddressSpace bus, SnesCgram cgram, Action landed,
        EndingPaletteCatalog? paletteArtwork = null)
    {
        this.bus = bus;
        this.landed = landed;
        this.paletteArtwork = paletteArtwork;
        for (int i = 0; i < actors.Length; i++)
        {
            EndingLogoActorDefinition definition = EndingLogoDefinitions.Actor(i);
            actors[i] = new IntroDiscoverySprite(
                EndingLogoDefinitions.Origin(i),
                SnesObjPalettes.Index7.Raw,
                definition.InstructionList);
            actors[i].PreInstructionPointerForDiscovery(definition.PreInstruction);
        }
        new FrontendPalettes(cgram).EndingLogoBackground.Fill(Bgr555.Black);
        (paletteArtwork ?? throw new InvalidOperationException(
            "Ending logo requires installed palette artwork."))
            [EndingPaletteId.LogoInitial].LoadTo(cgram, 0, 16, 240);
    }

    /// <summary>Restored scene state keeps its current CGRAM; only future transfers change.</summary>
    public void BindPaletteArtwork(EndingPaletteCatalog? value) => paletteArtwork = value;

    /// <summary>Applies the pending crossfade pair before moving actors and interpreting their next instructions.</summary>
    public void Step(SnesCgram cgram, Func<ushort, ushort>? instructionWord = null)
    {
        if (Completed) throw new InvalidOperationException("Completed logo must hand off to percentage text.");
        // The cinematic function runs before sprite instructions, so the first palette
        // pair is copied on the call after F25E, not during the call that requests it.
        if (CrossfadeStarted)
        {
            for (int palette = 0; palette < 2; palette++)
            {
                for (int i = 15; i >= 0; i--)
                    (palette == 0 ? new FrontendPalettes(cgram).EndingLogoBackground : new FrontendPalettes(cgram).EndingLogoObjects).Write(i,
                        (paletteArtwork ?? throw new InvalidOperationException(
                            "Ending logo requires installed palette artwork."))
                            [EndingPaletteId.LogoCrossfade].Color((PaletteStep * 2 + palette) * 16 + i));
            }
            if (++PaletteStep == EndingLogoDefinitions.PaletteSteps) return;
        }
        for (int i = 0; i < actors.Length; i++)
        {
            if (i < 2 && !settled[i]) MoveHalf(i);
            actors[i].Step<EndingLogoInstruction>(Instruction, instructionWord);
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

    /// <summary>Handles the grey-out opcode by starting crossfade on the following step.</summary>
    private ushort Instruction(EndingLogoInstruction opcode, ushort cursor)
    {
        if (opcode != EndingLogoInstruction.GreyOut)
            throw new InvalidOperationException($"Undefined EndingLogoInstruction {opcode}.");
        CrossfadeStarted = true;
        return cursor;
    }

    /// <summary>Draws active logo actors into supplied OAM or a newly finalized frame buffer.</summary>
    public OamBuffer Draw(EndingLogoSpritePresentation? installedArt = null, OamBuffer? destination = null)
    {
        var oam = destination ?? new OamBuffer();
        if (destination is null) oam.BeginFrame();
        if (!Completed)
            foreach (var actor in actors)
                actor.Draw(oam, EndingLogoDefinitions.Camera, installedArt);
        if (destination is null) oam.FinalizeFrame();
        return oam;
    }
}
