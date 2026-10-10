using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// Verifies the native Power Bomb response resets Ridley's neutral tail fields without
    /// overwriting animated foot displacement, and that inactive reaction gates preserve state.
    /// </summary>
    /// <remarks>Exercises every tail-angle delta and the fight-mode, grab-state, and bomb-armed gates.</remarks>
    private static void VerifyRidleyPowerBombNeutralTail()
    {
        var prepare = typeof(RoomEnemySystem).GetMethod("PrepareNorfairRidleyCombatFrame", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusBombProjectileSystem>>();
        var slot = new RoomEnemySystem().Slots[0];
        // $A6:BD2C reads $0CEE, which laying a power bomb arms at once.
        var armed = new SamusBombProjectileSystem();
        armed.PowerBombExplosion.Arm();
        var unarmed = new SamusBombProjectileSystem();
        // Native $A6:BD2C calls $B84D: write one to tail function and angle delta,
        // not to the independently animated grabbed-Samus foot displacement.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            var state = new RidleyEnemyState
            {
                FightMode = 1, GrabState = 0,
                TailFunctionIndex = 4, TailAngleDelta = (ushort)raw,
                FeetDistanceIndex = (ushort)(ushort.MaxValue - raw),
            };
            prepare(slot, state, armed);
            AssertEqual((ushort)1, state.TailFunctionIndex, "Power Bomb sets neutral tail function");
            AssertEqual((ushort)1, state.TailAngleDelta, "Power Bomb resets native tail angle delta");
            AssertEqual((ushort)(ushort.MaxValue - raw), state.FeetDistanceIndex, "Power Bomb preserves animated foot displacement");
            AssertEqual(RidleyAiFunction.NorfairGrabApproach, state.Function, "Power Bomb lunge handoff");
        }
        // $FFFF is the death mode: BMI rejects it before the flag is read.
        foreach (ushort fight in new ushort[] { 0, 1, 2, 0xffff })
        foreach (ushort grab in new ushort[] { 0, 1 })
        foreach (bool flag in new[] { false, true })
        {
            if (fight == 1 && grab == 0 && flag) continue;
            var state = new RidleyEnemyState
            {
                FightMode = fight, GrabState = grab,
                TailFunctionIndex = 4, TailAngleDelta = 8, FeetDistanceIndex = 4,
                Function = RidleyAiFunction.NorfairHover,
            };
            prepare(slot, state, flag ? armed : unarmed);
            AssertEqual((ushort)4, state.TailFunctionIndex, "Inactive reaction preserves tail function");
            AssertEqual((ushort)8, state.TailAngleDelta, "Inactive reaction preserves angle delta");
            AssertEqual((ushort)4, state.FeetDistanceIndex, "Inactive reaction preserves feet");
            AssertEqual(RidleyAiFunction.NorfairHover, state.Function, "Inactive reaction preserves phase");
        }
        Console.WriteLine("Ridley Power Bomb neutral tail: 65536 field-preservation cases and inactive gates match native write ownership.");
    }
}
