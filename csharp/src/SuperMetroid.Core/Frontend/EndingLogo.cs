using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>Native logo actors followed by E58A's sixteen palette transfers.</summary>
internal sealed class EndingLogo
{
    private readonly ISnesAddressSpace bus;
    private readonly Action landed;
    private readonly IntroDiscoverySprite[] actors = new IntroDiscoverySprite[4];
    private readonly int[] speeds = [EndingLogoDefinitions.InitialSpeed, EndingLogoDefinitions.InitialSpeed];
    private readonly bool[] settled = new bool[2];
    [NonSerialized] private EndingPaletteCatalog? paletteArtwork;
    public bool CrossfadeStarted { get; internal set; }
    public int PaletteStep { get; internal set; }
    public bool Completed => PaletteStep == EndingLogoDefinitions.PaletteSteps;

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

    private ushort Instruction(EndingLogoInstruction opcode, ushort cursor)
    {
        if (opcode != EndingLogoInstruction.GreyOut)
            throw new InvalidOperationException($"Undefined EndingLogoInstruction {opcode}.");
        CrossfadeStarted = true;
        return cursor;
    }

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
