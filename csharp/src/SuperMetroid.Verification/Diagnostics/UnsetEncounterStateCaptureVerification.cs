using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Diagnostics;

/// <summary>
/// Confirms #2649 and #2651: a debugger state captures a Norfair Pipe Bug its formation has not launched, and Lower Norfair
/// Ridley before his roar begins his arena's reveal. Each reading of those fails until it is set, and a door's autosave,
/// which captures the whole game as Samus enters a room, read them and threw: a follower still waiting in Purple Farming or
/// the Bat Cave. The state holds what is unset as unset, and restores it so.
/// </summary>
internal static class UnsetEncounterStateCaptureVerification
{
    /// <summary>Captures and restores a waiting Pipe Bug's and an unrevealed Ridley's state.</summary>
    internal static void VerifyUnsetEncounterStatesCapture()
    {
        var waiting = new NorfairPipeBugState(NorfairPipeBugAction.WaitForFormation);
        using (var stream = new MemoryStream())
        {
            DebuggerObjectGraphSerializer.Serialize(stream, waiting);
            stream.Position = 0;
            NorfairPipeBugState restored = DebuggerObjectGraphSerializer.Deserialize<NorfairPipeBugState>(stream);
            AssertEqual(NorfairPipeBugAction.WaitForFormation, restored.Action, "a waiting Pipe Bug's action survives its debugger state");
            AssertThrows<InvalidOperationException>(() => _ = restored.Role, "a waiting Pipe Bug restores with no role");
        }
        var ridley = new NorfairRidleyState();
        using (var stream = new MemoryStream())
        {
            DebuggerObjectGraphSerializer.Serialize(stream, ridley);
            stream.Position = 0;
            NorfairRidleyState restored = DebuggerObjectGraphSerializer.Deserialize<NorfairRidleyState>(stream);
            AssertThrows<InvalidOperationException>(() => _ = restored.ArenaReveal, "Ridley before his roar restores with his arena's reveal unbegun");
        }
    }

    /// <summary>Runs the unset encounter state capture check.</summary>
    [VerificationCommand("--unset-encounter-state-capture")]
    internal static int UnsetEncounterStateCaptureCommand()
    {
        Suite(nameof(VerifyUnsetEncounterStatesCapture), VerifyUnsetEncounterStatesCapture);
        return 0;
    }
}
