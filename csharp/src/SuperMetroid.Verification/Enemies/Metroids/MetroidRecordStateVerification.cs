using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metroids;
namespace SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Metroid values the cartridge keeps in the enemy's common record.</summary>
internal static class MetroidRecordStateVerification
{
    /// <summary>
    /// Installs a Metroid state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyMetroidRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new MetroidEnemyState(new RoomSpriteObjectSlot(0), new RoomSpriteObjectSlot(1));
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Velocity = FixedVector<RoomPixels>.FromWords(1, 0, 0, 0);
        state.EscapeTimer = NativeCountdown.Start(1);
        state.Function = Enum.GetValues<MetroidAiFunction>()[^1];

        slot.Clear();
        AssertEqual<FixedVector<RoomPixels>>(FixedVector<RoomPixels>.Zero, state.Velocity, "a kill empties Velocity");
        AssertEqual<NativeCountdown>(NativeCountdown.Start(0), state.EscapeTimer, "a kill empties EscapeTimer");
        AssertEqual<MetroidAiFunction>(default(MetroidAiFunction), state.Function, "a kill empties Function");
        Console.WriteLine("Metroid record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Metroid record-state check.</summary>
    [VerificationCommand("--metroid-record-state")]
    internal static int MetroidRecordStateCommand()
    {
        VerifyMetroidRecordState();
        return 0;
    }
}
