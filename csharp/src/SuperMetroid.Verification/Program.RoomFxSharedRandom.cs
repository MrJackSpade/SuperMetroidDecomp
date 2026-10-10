using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies liquid room-FX participation in the shared RNG lifecycle, including callback installation, frozen updates, native byte swapping, effect removal, and room reload initialization.</summary>
    /// <param name="bus">Address space used to load the synthetic room effect.</param>
    /// <param name="vram">Video memory receiving the effect's graphics during loading.</param>
    /// <param name="cgram">Color memory receiving the effect's palette during loading.</param>
    /// <param name="fx">Room layer-three effect whose shared-state callback lifecycle is checked.</param>
    /// <param name="record">Native room-FX record used to initialize the effect.</param>
    /// <param name="type">Effect type that determines whether the native update swaps the RNG word's bytes.</param>
    private static void VerifyRoomFxSharedRandomState(TestAddressSpace bus, SnesVram vram,
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
        fx.Load(bus, vram, cgram, 0, 0, 0);
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
