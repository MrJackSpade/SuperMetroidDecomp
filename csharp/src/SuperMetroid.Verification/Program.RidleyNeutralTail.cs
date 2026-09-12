using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyRidleyPowerBombNeutralTail()
    {
        var prepare = typeof(RoomEnemySystem).GetMethod("PrepareNorfairRidleyCombatFrame", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState>>();
        var slot = new RoomEnemySystem().Slots[0];
        // Native $A6:BD2C calls $B84D: write one to tail function and angle delta,
        // not to the independently animated grabbed-Samus foot displacement.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            var state = new RidleyEnemyState
            {
                FightMode = 1, PowerBombReactionLatched = 1, GrabState = 0,
                TailFunctionIndex = 4, TailAngleDelta = (ushort)raw,
                FeetDistanceIndex = (ushort)(ushort.MaxValue - raw),
            };
            prepare(slot, state);
            AssertEqual((ushort)1, state.TailFunctionIndex, "Power Bomb sets neutral tail function");
            AssertEqual((ushort)1, state.TailAngleDelta, "Power Bomb resets native tail angle delta");
            AssertEqual((ushort)(ushort.MaxValue - raw), state.FeetDistanceIndex, "Power Bomb preserves animated foot displacement");
            AssertEqual(RidleyAiFunction.NorfairGrabApproach, state.Function, "Power Bomb lunge handoff");
            AssertEqual((ushort)0, state.PowerBombReactionLatched, "Reaction latch consumed");
        }
        foreach (ushort fight in new ushort[] { 0, 1, 2 })
        foreach (ushort grab in new ushort[] { 0, 1 })
        foreach (ushort latch in new ushort[] { 0, 1 })
        {
            if (fight == 1 && grab == 0 && latch == 1) continue;
            var state = new RidleyEnemyState
            {
                FightMode = fight, GrabState = grab, PowerBombReactionLatched = latch,
                TailFunctionIndex = 4, TailAngleDelta = 8, FeetDistanceIndex = 4,
                Function = RidleyAiFunction.NorfairHover,
            };
            prepare(slot, state);
            AssertEqual((ushort)4, state.TailFunctionIndex, "Inactive reaction preserves tail function");
            AssertEqual((ushort)8, state.TailAngleDelta, "Inactive reaction preserves angle delta");
            AssertEqual((ushort)4, state.FeetDistanceIndex, "Inactive reaction preserves feet");
            AssertEqual(RidleyAiFunction.NorfairHover, state.Function, "Inactive reaction preserves phase");
        }
        Console.WriteLine("Ridley Power Bomb neutral tail: 65536 field-preservation cases and inactive gates match native write ownership.");
    }
}
