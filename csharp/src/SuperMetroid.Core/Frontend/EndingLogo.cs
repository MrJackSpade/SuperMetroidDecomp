using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Native logo actors followed by E58A's sixteen palette transfers.</summary>
internal sealed class EndingLogo
{
    private readonly ISnesAddressSpace bus;
    private readonly Action landed;
    private readonly IntroDiscoverySprite[] actors = new IntroDiscoverySprite[4];
    private readonly int[] speeds = [EndingLogoDefinitions.InitialSpeed, EndingLogoDefinitions.InitialSpeed];
    private readonly bool[] settled = new bool[2];
    public bool CrossfadeStarted { get; private set; }
    public int PaletteStep { get; private set; }
    public bool Completed => PaletteStep == EndingLogoDefinitions.PaletteSteps;

    public EndingLogo(ISnesAddressSpace bus, SnesCgram cgram, Action landed)
    {
        this.bus = bus;
        this.landed = landed;
        for (int i = 0; i < actors.Length; i++)
        {
            var origin = EndingLogoDefinitions.Origin(i);
            ushort list = RomDataReader.ReadWordFixedBank(bus,
                IntroCinematicRomData.Banks.CinematicCode | (EndingLogoDefinitions.Actors[i] + 4));
            actors[i] = new IntroDiscoverySprite(origin.X, origin.Y, SnesObjPalettes.Index7.Raw, list);
        }
        for (int i = 0; i < 16; i++) cgram.SetColor(16 + i, 0);
        cgram.LoadFromBus(bus, EndingLogoDefinitions.InitialPalette, 16, 240);
    }

    public void Step(SnesCgram cgram)
    {
        if (Completed) throw new InvalidOperationException("Completed logo must hand off to percentage text.");
        // The cinematic function runs before sprite instructions, so the first palette
        // pair is copied on the call after F25E, not during the call that requests it.
        if (CrossfadeStarted)
        {
            for (int palette = 0; palette < 2; palette++)
            {
                int pointer = RomDataReader.ReadWordFixedBank(bus,
                    EndingLogoDefinitions.PaletteTable + PaletteStep * 4 + palette * 2);
                for (int i = 15; i >= 0; i--)
                    cgram.SetColor((palette == 0 ? 16 : 240) + i,
                        RomDataReader.ReadWordFixedBank(bus, (IntroCinematicRomData.Banks.Spritemaps << 16) | (pointer - (15 - i) * 2)));
            }
            if (++PaletteStep == EndingLogoDefinitions.PaletteSteps) return;
        }
        for (int i = 0; i < actors.Length; i++)
        {
            if (i < 2 && !settled[i]) MoveHalf(i);
            actors[i].Step(bus, Instruction);
        }
    }

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

    private ushort? Instruction(ushort opcode, ushort cursor)
    {
        if (opcode != EndingLogoDefinitions.GreyOutInstruction) return null;
        CrossfadeStarted = true;
        return cursor;
    }

    public OamBuffer Draw()
    {
        var oam = new OamBuffer(); oam.BeginFrame();
        if (!Completed)
            foreach (var actor in actors)
                actor.Draw(bus, oam, EndingLogoDefinitions.Camera, EndingLogoDefinitions.Camera);
        oam.FinalizeFrame(); return oam;
    }
}
