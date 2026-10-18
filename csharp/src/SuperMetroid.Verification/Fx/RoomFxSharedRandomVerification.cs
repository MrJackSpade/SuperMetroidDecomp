using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Fx.RoomFxRomDataVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;

/// <summary>Checks the liquid HDMA callback's use of shared RNG across freeze and room reload boundaries.</summary>
internal static class RoomFxSharedRandomVerification
{
    /// <summary>Confirms lava and acid byte swaps, frozen callback installation, and reset after leaving or reloading a liquid room.</summary>
    internal static void VerifyRoomFxSharedRandomState(TestAddressSpace bus, SnesVram vram,
        SnesCgram cgram, RoomLayer3FxState fx, ushort record, RoomFxType type)
    {
        bool swaps = type is RoomFxType.Lava or RoomFxType.Acid;
        var system = new Bank80SystemState(0x1234);
        fx.AdvanceHdmaSharedState(system, false);
        AssertEqual(0x1234, system.RandomNumber, $"{type} first pass installs callback without executing it");
        fx.AdvanceHdmaSharedState(system, true);
        AssertEqual(0x1234, system.RandomNumber, $"{type} frozen pre-instruction preserves RNG");
        fx.AdvanceHdmaSharedState(system, false);
        AssertEqual(swaps ? 0x3412 : 0x1234, system.RandomNumber, $"{type} native byte swap before global RNG");
        fx.AdvanceHdmaSharedState(system, false);
        AssertEqual(0x1234, system.RandomNumber, $"{type} second swap restores original bytes");

        // Installing the callback is instruction-list work, even when its eventual
        // pre-instruction will return early for frozen gameplay.
        LoadSyntheticRoomFx(fx, bus, vram, cgram, record);
        fx.AdvanceHdmaSharedState(system, true);
        fx.AdvanceHdmaSharedState(system, false);
        AssertEqual(swaps ? 0x3412 : 0x1234, system.RandomNumber, $"{type} frozen startup still installs callback");
        fx.Load(vram, cgram, 0, null, 0);
        system.SetRandomNumber(0x1234);
        fx.AdvanceHdmaSharedState(system, false);
        fx.AdvanceHdmaSharedState(system, false);
        AssertEqual(0x1234, system.RandomNumber, "leaving liquid room removes RNG owner");

        // Leave the existing visual tests with a freshly loaded effect, proving
        // that reloading does not retain the previous room's callback phase.
        LoadSyntheticRoomFx(fx, bus, vram, cgram, record);
        fx.AdvanceHdmaSharedState(system, false);
        AssertEqual(0x1234, system.RandomNumber, $"{type} reload restores initialization delay");
        LoadSyntheticRoomFx(fx, bus, vram, cgram, record);
    }
}
