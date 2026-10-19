using SuperMetroid.Core.Hardware;
using System.Reflection;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Saves;
namespace SuperMetroid.Verification.Runtime;
using SuperMetroid.Verification.Harness;

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
        var saveRam = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        AssertTrue(saveRam.DetermineBootMainLoopCarry(), "blank SRAM: the third LoadFromSRAM's carry survives");
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot());
        saveRam.SetGameCompleted(false);
        AssertTrue(!saveRam.DetermineBootMainLoopCarry(), "a valid slot without the marker: zero is below 'id'");
        saveRam.SetGameCompleted(true);
        AssertTrue(saveRam.DetermineBootMainLoopCarry(), "the full marker leaves the final comparison's carry set");

        foreach (bool carry in new[] { true, false })
        {
            var stateSession = new RoomFxSession();
            var state = new RoomLayer3FxState(stateSession);
            state.earthquakeSoundTimer = NativeCountdown.Start((ushort)0);
            state.earthquakeSoundSequenceIndex = 0;
            // Native update 330722: the first entry (base 1) with RNG & 3 = 0 reloads to 2.
            state.PublishStatueEarthquakeSound(0xac0c, carry);
            AssertEqual((ushort)(carry ? 2 : 1), state.earthquakeSoundTimer.RawValue,
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
