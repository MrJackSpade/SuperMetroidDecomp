using System.Globalization;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;

/// <summary>Compares every published pause fade brightness with a retail CPU capture.</summary>
internal static class PauseFadeComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 120 || rows.Any(row => row.Length != 5))
            throw new InvalidDataException("Expected four complete 30-frame cartridge fade captures.");
        var game = PauseChargeCarryAudit.CreateFixture(rom, out _);
        long sequence = 0;
        void Step(SnesButton input) => game.StepCaptured((ushort)input, ++sequence, 1);
        void Reach(SuperMetroidGameState state, SnesButton input)
        {
            for (int i = 0; i < 100 && game.GameState != state; i++) Step(input);
            if (game.GameState != state) throw new InvalidDataException($"Never reached {state}.");
        }
        Step(SnesButton.Start);
        SuperMetroidGameState[] stages = [SuperMetroidGameState.PausingDarkening,
            SuperMetroidGameState.PausedA, SuperMetroidGameState.UnpausingA, SuperMetroidGameState.Unpausing];
        SuperMetroidGameState[] next = [SuperMetroidGameState.Pausing, SuperMetroidGameState.PausedB,
            SuperMetroidGameState.UnpausingB, SuperMetroidGameState.MainGameplay];
        int compared = 0;
        for (int stage = 0; stage < stages.Length; stage++)
        {
            // These transitions go through the production menu and input filter;
            // the test does not set game states, fade counters, or brightness.
            Reach(stages[stage], stage == 2 ? SnesButton.Start : 0);
            for (int frame = 0; frame < 30; frame++)
            {
                if (game.GameState != stages[stage])
                    throw new InvalidDataException($"Fade {stages[stage]} ended early at {frame}.");
                var rendered = game.StepCaptured(0, ++sequence, 1);
                var row = rows[compared++];
                if (int.Parse(row[0]) != stage || int.Parse(row[1]) != frame)
                    throw new InvalidDataException("Cartridge fade capture is out of order.");
                int expected = int.Parse(row[2], NumberStyles.HexNumber) & 15;
                var snapshot = rendered.Snapshot ?? throw new InvalidDataException("Missing captured fade.");
                var passes = snapshot.BrightnessPasses;
                int actual = passes.Length > 0 ? passes[^1] : throw new InvalidDataException("Missing brightness pass.");
                if (actual != expected)
                    throw new InvalidDataException($"{stages[stage]} frame {frame}: cartridge brightness {expected}, C# {actual}.");
            }
            if (game.GameState != next[stage])
                throw new InvalidDataException($"Fade {stages[stage]} did not end on cartridge frame 30.");
        }
        Console.WriteLine($"Pause fade cartridge comparison: {compared} brightness samples match; all four phase boundaries match.");
        return 0;
    }
}
