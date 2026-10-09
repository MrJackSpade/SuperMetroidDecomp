using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Native logo actors followed by E58A's sixteen palette transfers.</summary>
internal sealed class EndingLogo
{
    /// <summary>Address space used by actor instructions and rendering.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Callback invoked when the left-moving logo half reaches its landing point.</summary>
    private readonly Action landed;
    /// <summary>Four discovery-sprite actors that assemble the ending logo.</summary>
    private readonly IntroDiscoverySprite[] actors = new IntroDiscoverySprite[4];
    /// <summary>Accelerating horizontal speeds for the two independently moving logo halves.</summary>
    private readonly int[] speeds = [EndingLogoDefinitions.InitialSpeed, EndingLogoDefinitions.InitialSpeed];
    /// <summary>Tracks whether each moving half has snapped to its final position.</summary>
    private readonly bool[] settled = new bool[2];
    /// <summary>Installed initial and crossfade palette source used for future CGRAM transfers.</summary>
    [NonSerialized] private EndingPaletteCatalog? paletteArtwork;
    /// <summary>Whether the logo instruction has started the later palette-copy sequence.</summary>
    public bool CrossfadeStarted { get; private set; }
    /// <summary>Number of paired palette transfers already applied after crossfade begins.</summary>
    public int PaletteStep { get; private set; }
    /// <summary>Whether all native palette-transfer steps have completed and scene handoff is ready.</summary>
    public bool Completed => PaletteStep == EndingLogoDefinitions.PaletteSteps;

    /// <summary>Creates the four actors, initializes the target CGRAM colors, and stores the landing callback.</summary>
    /// <param name="bus">Address space used to execute actor instructions and draw their sprites.</param>
    /// <param name="cgram">Palette memory initialized with the logo's starting colors.</param>
    /// <param name="landed">Callback run when the left logo half reaches its landing point.</param>
    /// <param name="paletteArtwork">Installed palette source required to initialize and advance the logo crossfade.</param>
    /// <exception cref="InvalidOperationException">Palette artwork is unavailable.</exception>
    public EndingLogo(ISnesAddressSpace bus, SnesCgram cgram, Action landed,
        EndingPaletteCatalog? paletteArtwork = null)
    {
        this.bus = bus;
        this.landed = landed;
        this.paletteArtwork = paletteArtwork;
        for (int i = 0; i < actors.Length; i++)
        {
            var origin = EndingLogoDefinitions.Origin(i);
            EndingLogoActorDefinition definition = EndingLogoDefinitions.Actor(i);
            actors[i] = new IntroDiscoverySprite(
                origin.X,
                origin.Y,
                SnesObjPalettes.Index7.Raw,
                definition.InstructionList);
            actors[i].PreInstructionPointerForDiscovery(definition.PreInstruction);
        }
        for (int i = 0; i < 16; i++) cgram.SetColor(16 + i, 0);
        (paletteArtwork ?? throw new InvalidOperationException(
            "Ending logo requires installed palette artwork."))
            [EndingPaletteId.LogoInitial].LoadTo(cgram, 0, 16, 240);
    }

    /// <summary>Restored scene state keeps its current CGRAM; only future transfers change.</summary>
    public void BindPaletteArtwork(EndingPaletteCatalog? value) => paletteArtwork = value;

    /// <summary>Advances palette transfers, the two moving halves, and each actor's instruction stream once.</summary>
    /// <param name="cgram">Live palette memory receiving the next crossfade entries.</param>
    /// <param name="instructionWord">Optional instruction-word reader used by the actors' native lists.</param>
    /// <exception cref="InvalidOperationException">The palette sequence is complete and the caller has not handed off the scene.</exception>
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
                    cgram.SetColor((palette == 0 ? 16 : 240) + i,
                        (paletteArtwork ?? throw new InvalidOperationException(
                            "Ending logo requires installed palette artwork."))
                            [EndingPaletteId.LogoCrossfade].Color((PaletteStep * 2 + palette) * 16 + i));
            }
            if (++PaletteStep == EndingLogoDefinitions.PaletteSteps) return;
        }
        for (int i = 0; i < actors.Length; i++)
        {
            if (i < 2 && !settled[i]) MoveHalf(i);
            actors[i].Step(bus, Instruction, instructionWord);
        }
    }

    /// <summary>Moves one assembling half toward its authored landing coordinate and accelerates until contact.</summary>
    /// <param name="index">Moving half index: zero for the left actor and one for the right actor.</param>
    private void MoveHalf(int index)
    {
        var actor = actors[index];
        int direction = index == 0 ? -1 : 1;
        actor.XPosition = unchecked((ushort)(actor.XPosition + direction * speeds[index]));
        actor.YPosition = unchecked((ushort)(actor.YPosition - direction * speeds[index]));
        var target = index == 0 ? EndingLogoDefinitions.TopLanding : EndingLogoDefinitions.BottomLanding;
        if (index == 0 ? (short)(actor.XPosition - target.X - 1) < 0 : (short)(actor.XPosition - target.X) >= 0)
        {
            actor.XPosition = target.X; actor.YPosition = target.Y;
            settled[index] = true;
            if (index == 0) landed();
        }
        else speeds[index] += EndingLogoDefinitions.Acceleration;
    }

    /// <summary>Recognizes the logo's grey-out instruction and signals that palette crossfade should begin.</summary>
    /// <param name="opcode">Actor instruction opcode currently being processed.</param>
    /// <param name="cursor">Instruction cursor following the opcode.</param>
    /// <returns>The unchanged cursor when the grey-out instruction is consumed; otherwise null for ordinary dispatch.</returns>
    private ushort? Instruction(ushort opcode, ushort cursor)
    {
        if (opcode != EndingLogoDefinitions.GreyOutInstruction) return null;
        CrossfadeStarted = true;
        return cursor;
    }

    /// <summary>Adds the active logo actors to OAM using installed artwork when supplied.</summary>
    /// <param name="installedArt">Optional editable sprite parts used in place of the actors' stock artwork.</param>
    /// <param name="destination">Existing OAM buffer to append to, or null to create and finalize a new frame.</param>
    /// <returns>The supplied buffer or a newly finalized buffer containing the visible actors.</returns>
    public OamBuffer Draw(EndingLogoSpritePresentation? installedArt = null, OamBuffer? destination = null)
    {
        var oam = destination ?? new OamBuffer();
        if (destination is null) oam.BeginFrame();
        if (!Completed)
            foreach (var actor in actors)
                actor.Draw(bus, oam, EndingLogoDefinitions.Camera,
                    EndingLogoDefinitions.Camera, installedArt);
        if (destination is null) oam.FinalizeFrame();
        return oam;
    }
}
