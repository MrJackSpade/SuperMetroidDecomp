using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Zebetite values the cartridge keeps in the enemy's common record.</summary>
internal static class ZebetiteRecordStateVerification
{
    /// <summary>
    /// Installs a Zebetite state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyZebetiteRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.SetSpawnWords(EnemyDefinitionId.Zebetite, words => words);
        var state = new ZebetiteEnemyState(slot.Spawn.Population);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<ZebetiteAiFunction>()[^1];
        state.Generation = 1;
        state.GenerationFlags = Enum.GetValues<ZebetiteGenerationFlags>()[^1];

        slot.Clear();
        AssertEqual<ZebetiteAiFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<int>(0, state.Generation, "a kill empties Generation");
        AssertEqual<ZebetiteGenerationFlags>(default(ZebetiteGenerationFlags), state.GenerationFlags, "a kill empties GenerationFlags");
        Console.WriteLine("Zebetite record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Zebetite record-state check.</summary>
    [VerificationCommand("--zebetite-record-state")]
    internal static int ZebetiteRecordStateCommand()
    {
        VerifyZebetiteRecordState();
        return 0;
    }
}
