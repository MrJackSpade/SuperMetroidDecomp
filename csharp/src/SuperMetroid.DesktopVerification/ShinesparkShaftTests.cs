using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    /// <summary>
    /// Replays the preserved $03/$00 report without changing live slots or synthesizing
    /// charge/pose state. Only the comparison run replenishes health; both trajectories
    /// otherwise use identical controller inputs through the real frontend/runtime.
    /// </summary>
    private static void VerifyShinesparkShaft()
    {
        ReplayShinesparkShaft(false);
        ReplayShinesparkShaft(true);
        foreach (SnesButton downInput in new[]
        {
            SnesButton.Down,
            SnesButton.Down | SnesButton.B,
            SnesButton.Down | SnesButton.Right,
            SnesButton.Down | SnesButton.Right | SnesButton.B,
        })
            ReplayShinesparkShaft(false, downInput);
    }

    private static void ReplayShinesparkShaft(bool replenishHealth, SnesButton? storageOnlyInput = null)
    {
        var loaded = DebuggerFixtureLoader.Load("issue-371-shinespark-shaft", 0);
        var game = loaded.Game;
        var runtime = game.RuntimeForVerification!;
        var samus = runtime.Samus!;
        Check(samus.Health == 1, "The preserved report must start at one energy.");
        if (replenishHealth) samus.Health = samus.MaxHealth;
        // Walk away from the shaft, boost back, tap Down for one accepted frame, let the
        // crouch settle, then hold Jump. Coordinates keep the run on this room's runway
        // and the launch inside the narrow shaft rather than touching its side wall.
        int stage = 0;
        int stageTicks = 0;
        for (int frame = 0; frame < 600; frame++)
        {
            if (stage == 0 && samus.XPosition < 710) { stage = 1; stageTicks = 0; }
            if (stage == 1 && samus.XPosition > 1478) { stage = 2; stageTicks = 0; }
            if (stage == 2 && stageTicks == 1) { stage = 3; stageTicks = 0; }
            if (stage == 3 && stageTicks == 10) { stage = 4; stageTicks = 0; }
            ushort input = stage switch
            {
                0 => (ushort)SnesButton.Left,
                1 => (ushort)(SnesButton.Right | SnesButton.B),
                2 => (ushort)(storageOnlyInput ?? (SnesButton.Right | SnesButton.B | SnesButton.Down)),
                4 => (ushort)SnesButton.A,
                _ => 0,
            };
            game.Step(input);
            stageTicks++;
            if (stage == 2)
            {
                // Native $91:F7B0 stores 180; the same frontend frame ticks the palette.
                Check(samus.Shinespark.Phase == ShinesparkPhase.Stored && samus.Shinespark.ShineTimer == 179,
                    $"One-frame Down did not store the native charge: input={input:X4}, phase={samus.Shinespark.Phase}, timer={samus.Shinespark.ShineTimer}.");
                if (storageOnlyInput is { } chord)
                {
                    Console.WriteLine($"PASS charge storage: one-frame {chord}; timer=179 after same-frame palette tick.");
                    return;
                }
            }
            if (samus.Shinespark.Phase == ShinesparkPhase.Crash)
            {
                var movement = runtime.LastShinesparkMovement!.Value;
                if (replenishHealth)
                {
                    // The normal room ceiling is at pixel 48; the vertical-spark pose's
                    // center is 19 pixels below it. This checks the entire shaft height,
                    // not simply that Samus moved upward or that the frame did not crash.
                    Check(!movement.EndedByLowEnergy && movement.EndedByCollision && samus.YPosition == 67,
                        $"Recharged spark failed to clear the shaft: Y={samus.YPosition}, {movement}.");
                    Console.WriteLine($"PASS recharged replay: reaches shaft ceiling at Y={samus.YPosition}; energy={samus.Health}.");
                }
                else
                {
                    Check(movement.EndedByLowEnergy && !movement.EndedByCollision && samus.YPosition == 619,
                        $"Saved-energy spark did not reproduce the native low-energy stop: {movement}.");
                    Console.WriteLine($"PASS exact saved-energy replay: stops at Y={samus.YPosition}, energy=1, without collision.");
                }
                return;
            }
        }
        throw new InvalidDataException("Shinespark replay did not reach its expected endpoint.");
    }
}
