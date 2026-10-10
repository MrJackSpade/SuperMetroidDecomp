using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    /// <summary>
    /// Replays the preserved $03/$00 report without changing live slots or synthesizing
    /// charge/pose state. Control runs change only host invincibility or health in memory;
    /// trajectories use identical controller inputs through the real frontend/runtime.
    /// </summary>
    private static void VerifyShinesparkShaft()
    {
        ReplayShinesparkShaft(false, invincibility: false);
        ReplayShinesparkShaft(false);
        ReplayShinesparkShaft(true);
        ReplayShinesparkShaft(false, launchHealth: 30);
        foreach (SnesButton downInput in new[]
        {
            SnesButton.Down,
            SnesButton.Down | SnesButton.B,
            SnesButton.Down | SnesButton.Right,
            SnesButton.Down | SnesButton.Right | SnesButton.B,
        })
            ReplayShinesparkShaft(false, downInput);
    }

    /// <summary>
    /// Replays the preserved shaft fixture through real game steps and checks either the
    /// expected full-height spark or the saved-energy stop. Optional controls isolate charge
    /// storage, invincibility, and launch-health cases without changing the saved fixture.
    /// </summary>
    /// <param name="replenishHealth">Restores Samus to maximum health before the replay and expects the spark to clear the shaft.</param>
    /// <param name="storageOnlyInput">If supplied, uses this Down-containing chord for the one-frame charge-storage check and returns before launch.</param>
    /// <param name="invincibility">Whether to retain the fixture's invincibility option; disabling it exercises saved-energy behavior.</param>
    /// <param name="launchHealth">Optional health value assigned while the spark is in windup, for checking launch-energy behavior.</param>
    private static void ReplayShinesparkShaft(bool replenishHealth, SnesButton? storageOnlyInput = null,
        bool invincibility = true, ushort? launchHealth = null)
    {
        var loaded = DebuggerFixtureLoader.Load("issue-371-shinespark-shaft", 0);
        var game = loaded.Game;
        var runtime = game.RuntimeForVerification!;
        var samus = runtime.Samus!;
        Check(runtime.PlayerInvincibilityEnabled, "The reported fixture must have invincibility enabled.");
        // Only the control run overrides the captured host option. Never alter the
        // player's saved graph on disk or add a mutable runtime cheat setter for tests.
        if (!invincibility)
            typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime)
                .GetField("<PlayerInvincibilityEnabled>k__BackingField",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(runtime, false);
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
            if (launchHealth is { } health && samus.Shinespark.Phase == ShinesparkPhase.Windup)
                samus.Health = health;
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
                // The crash handler keeps no cause word. `$90:D2BD`'s energy exit is taken
                // exactly when host invincibility is off and energy is below the native
                // sustaining threshold (the exit frame does not drain); any other crash
                // was begun by the shared terrain/enemy collision flag.
                bool endedByLowEnergy = !runtime.PlayerInvincibilityEnabled &&
                    unchecked((short)(samus.Health -
                        SamusSpecialSequenceRomData.Shinespark.MinimumSustainingEnergy)) < 0;
                if (replenishHealth || invincibility)
                {
                    // The normal room ceiling is at pixel 48; the vertical-spark pose's
                    // center is 19 pixels below it. This checks the entire shaft height,
                    // not simply that Samus moved upward or that the frame did not crash.
                    Check(!endedByLowEnergy && samus.YPosition == 67,
                        $"Invincible/recharged spark failed to clear the shaft: Y={samus.YPosition}, {movement}.");
                    if (!replenishHealth)
                        Check(samus.Health == 1, "Invincible spark must drain to one without restoring health or underflowing.");
                    Console.WriteLine($"PASS shaft cleared: invincibility={invincibility}, recharged={replenishHealth}, launchHealth={launchHealth}; Y={samus.YPosition}; energy={samus.Health}.");
                }
                else
                {
                    Check(endedByLowEnergy && samus.YPosition == 619,
                        $"Saved-energy spark did not reproduce the native low-energy stop: {movement}.");
                    Console.WriteLine($"PASS exact saved-energy replay: stops at Y={samus.YPosition}, energy=1, without collision.");
                }
                return;
            }
        }
        throw new InvalidDataException("Shinespark replay did not reach its expected endpoint.");
    }
}
