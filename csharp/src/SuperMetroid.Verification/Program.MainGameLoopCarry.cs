using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.Core.Game;
using static AssertionsVerification;
using static RetailPresentationFixtureVerification;

/// <summary>Checks how boot SRAM comparison carry affects earthquake sound timer reloads.</summary>
internal static class MainGameLoopCarryVerification
{
    /// <summary>
    /// Confirms the boot comparison's carry survives into the earthquake timer ADC,
    /// preserving the native rumble cadence and following sound-queue wait.
    /// </summary>
    internal static void VerifyMainGameLoopCarry()
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

    /// <summary>Runs the focused boot-carry and quake-timer check.</summary>
    [VerificationCommand("--main-game-loop-carry")]
    internal static int MainGameLoopCarryCommand()
    {
        VerifyMainGameLoopCarry();
        return 0;
    }
}
