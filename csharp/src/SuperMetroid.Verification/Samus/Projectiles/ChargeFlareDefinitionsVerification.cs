using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks compiled charge-flare cadence words and interpreter advancement against native bank-$90 data.</summary>
internal static class ChargeFlareDefinitionsVerification
{
    /// <summary>Compares every compiled byte, rejects adjacent addresses, and checks repeated loop and timer-boundary transitions for all three flare components.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled charge-flare cadence words and the native cadence reference")]
    internal static void VerifyChargeFlareDefinitions(SuperMetroidAddressSpace bus)
    {
        for (int address = SamusCartridgeTables.ChargeFlarePrograms; address < 0x90c4b5; address++)
        {
            AssertEqual(CpuBusRegionRead.Read(bus, address), ChargeFlareAnimationDefinitions.ReadByte(address - SamusCartridgeTables.ChargeFlarePrograms),
                "Compiled flare cadence byte matches cartridge");
            if (address < 0x90c4b4)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address),
                    ChargeFlareAnimationDefinitions.ReadWord(address - SamusCartridgeTables.ChargeFlarePrograms), "Compiled flare cadence word matches cartridge");
        }
        foreach (int address in new[] { 0x908000, 0x90c486, 0x90c4b5, 0x90ffff })
            AssertThrows<InvalidDataException>(
                () => ChargeFlareAnimationDefinitions.ReadByte(address - SamusCartridgeTables.ChargeFlarePrograms),
                "Unknown flare cadence address fails instead of reading arbitrary movement-bank data");
        var system = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None);
        var advance = new Action<int>(system.Animation.AdvanceFlareComponent);
        var frames = system.Charge.FlareFrames;
        var timers = system.Charge.FlareTimers;
        for (int component = 0; component < 3; component++)
        {
            frames[component] = 0; timers[component] = WrappingCountdown.Start(0);
            ushort expectedFrame = 0, expectedTimer = 0;
            for (int tick = 0; tick < 1024; tick++)
            {
                Reference(component, ref expectedFrame, ref expectedTimer);
                advance(component);
                AssertEqual(new VerifyChargeFlareDefinitionsCase(expectedFrame, expectedTimer), new VerifyChargeFlareDefinitionsCase(frames[component], timers[component].RawValue), "Flare production cadence matches native repeated rewind/restart trajectory");
            }
            int maximumLiveFrame = component == 0 ? 29 : 5;
            foreach (ushort timer in new ushort[] { 0, 1, 2, 0x8000, 0xffff })
            for (int frame = 0; frame <= maximumLiveFrame; frame++)
            {
                expectedFrame = frames[component] = frame == 256 ? ushort.MaxValue : (ushort)frame;
                timers[component] = WrappingCountdown.FromRaw(timer);
                expectedTimer = timer;
                Reference(component, ref expectedFrame, ref expectedTimer);
                advance(component);
                AssertEqual(new VerifyChargeFlareDefinitionsCase(expectedFrame, expectedTimer), new VerifyChargeFlareDefinitionsCase(frames[component], timers[component].RawValue), "Flare timer sign and authored frame boundaries match native");
            }
        }
        Console.WriteLine("Charge flare: 52 compiled bytes, loud non-catalog rejection, 3072 loop ticks and 210 authored boundary-state advances preserve native cadence with timing ROM reads forbidden.");

        void Reference(int component, ref ushort frame, ref ushort timer)
        {
            timer = unchecked((ushort)(timer - 1));
            if ((short)timer >= 0) return;
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x90c481 + component * 2);
            frame = unchecked((ushort)(frame + 1));
            byte Delay(int selected) => CpuBusRegionRead.Read(bus, 0x900000 | unchecked((ushort)(pointer + selected)));
            byte delay = Delay(frame);
            if (delay == 255) { frame = 0; delay = Delay(frame); }
            else if (delay == 254) { frame = unchecked((ushort)(frame - Delay(frame + 1))); delay = Delay(frame); }
            timer = delay;
        }
    }

    /// <summary>Runs the charge flare cadence definitions against the retail cartridge.</summary>
    [VerificationCommand("--charge-flare-definitions")]
    internal static int ChargeFlareDefinitionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyChargeFlareDefinitions), () => VerifyChargeFlareDefinitions(rom));
        return 0;
    }
}
