using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: Handle_Earthquake_SoundEffect reloads its timer with ADC .baseTimer ($88:B245)
    // after QueueSound_Lib2_Max6 restores the entry flags, so it adds the carry MainGameLoop
    // has preserved since boot's DetermineNumberOfDemoSets ($80:8261). Without it, every
    // rumble in the 100% movie's rising-lava room came a frame early and the door's
    // sound-queue wait ended six updates before native.
    /// <summary>Verifies that boot SRAM state determines the preserved main-loop carry and that the carry contributes to the earthquake sound timer reload.</summary>
    private static void VerifyMainGameLoopCarry()
    {
        var bus = new TestAddressSpace();
        var saveRam = new SuperMetroidSaveRam(bus, RetailPresentationFixture());
        AssertTrue(saveRam.DetermineBootMainLoopCarry(), "blank SRAM: the third LoadFromSRAM's carry survives");
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot());
        saveRam.SetGameCompleted(false);
        AssertTrue(!saveRam.DetermineBootMainLoopCarry(), "a valid slot without the marker: zero is below 'id'");
        saveRam.SetGameCompleted(true);
        AssertTrue(saveRam.DetermineBootMainLoopCarry(), "the full marker leaves the final comparison's carry set");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        FieldInfo timer = typeof(RoomLayer3FxState).GetField("earthquakeSoundTimer", flags)!;
        FieldInfo index = typeof(RoomLayer3FxState).GetField("earthquakeSoundSequenceIndex", flags)!;
        foreach (bool carry in new[] { true, false })
        {
            var state = new RoomLayer3FxState();
            timer.SetValue(state, (ushort)0);
            index.SetValue(state, 0);
            // Native update 330722: the first entry (base 1) with RNG & 3 = 0 reloads to 2.
            state.PublishStatueEarthquakeSound(0xac0c, carry);
            AssertEqual((ushort)(carry ? 2 : 1), (ushort)timer.GetValue(state)!,
                $"earthquake reload with main-loop carry {carry}");
        }
        Console.WriteLine("Main-loop carry: boot SRAM decides it, and the earthquake timer reload adds it.");
    }
}
