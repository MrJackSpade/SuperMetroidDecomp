using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Independent transcription of the first foreground color and hold durations in
    /// bank $8D's four Norfair glow lists. Check every frame, including wraparound;
    /// checking only whether CGRAM changes misses incorrect colors and cadence.
    /// </summary>
    private static void VerifyNorfairGlowCycles(SuperMetroidAddressSpace bus)
    {
        int[] holds = [16, 4, 4, 5, 6, 7, 8, 8, 8, 8, 7, 6, 5, 4, 4, 16];
        ushort[] orange = [0x09fd, 0x0e3d, 0x165e, 0x1a9e, 0x1ebe, 0x22fe, 0x2b1f, 0x2f5f];
        ushort[] red = [0x09da, 0x0dda, 0x0dfa, 0x11fa, 0x161a, 0x1a1a, 0x1a3a, 0x225a];
        // Definition / CGRAM byte destination: $F08E, $F1D1, $F2D9, $F3E1.
        (ushort Definition, int Destination)[] streams =
            [(0xf785, 0x6a), (0xf789, 0x82), (0xf78d, 0xa2), (0xf791, 0xc2)];
        foreach (var stream in streams)
        {
            var fx = new RoomPaletteFxSystem();
            var colors = new SnesCgram();
            fx.SpawnDefinition(bus, stream.Definition, 0);
            ushort[] ramp = stream.Definition == 0xf791 ? red : orange;
            for (int cycle = 0; cycle < 3; cycle++)
            for (int phase = 0; phase < holds.Length; phase++)
            for (int tick = 0; tick < holds[phase]; tick++)
            {
                fx.Step(bus, colors, 0, 0, false, false);
                int rampIndex = phase < 8 ? phase : 15 - phase;
                AssertEqual(ramp[rampIndex], colors.Colors[stream.Destination / 2],
                    $"Norfair glow {stream.Definition:X4} cycle {cycle} phase {phase} tick {tick}");
            }
        }
        Console.WriteLine("  Norfair glow: four retail streams match exact foreground colors and frame holds across three cycles.");
    }
}
