using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyChargeFlareDefinitions(SuperMetroidAddressSpace bus)
    {
        for (int address = 0x90c481; address < 0x90c4b5; address++)
        {
            AssertEqual(bus.ReadByte(address), ChargeFlareAnimationDefinitions.ReadByte(address), "Compiled flare cadence byte matches cartridge");
            if (address < 0x90c4b4)
                AssertEqual(RomDataReader.ReadWordFixedBank(bus, address), ChargeFlareAnimationDefinitions.ReadWord(address), "Compiled flare cadence word matches cartridge");
        }
        foreach (int address in new[] { 0x908000, 0x90c480, 0x90c4b5, 0x90ffff })
            AssertThrows<InvalidDataException>(
                () => ChargeFlareAnimationDefinitions.ReadByte(address),
                "Unknown flare cadence address fails instead of reading arbitrary movement-bank data");
        var system = new SamusProjectileSystem();
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var advance = typeof(SamusProjectileSystem).GetMethod("AdvanceFlareComponent", flags)!.CreateDelegate<Action<int>>(system);
        var frames = (ushort[])typeof(SamusProjectileSystem).GetField("_flareFrames", flags)!.GetValue(system)!;
        var timers = (ushort[])typeof(SamusProjectileSystem).GetField("_flareTimers", flags)!.GetValue(system)!;
        for (int component = 0; component < 3; component++)
        {
            frames[component] = 0; timers[component] = 0;
            ushort expectedFrame = 0, expectedTimer = 0;
            for (int tick = 0; tick < 1024; tick++)
            {
                Reference(component, ref expectedFrame, ref expectedTimer);
                advance(component);
                AssertEqual((expectedFrame, expectedTimer), (frames[component], timers[component]), "Flare production cadence matches native repeated rewind/restart trajectory");
            }
            int maximumLiveFrame = component == 0 ? 29 : 5;
            foreach (ushort timer in new ushort[] { 0, 1, 2, 0x8000, 0xffff })
            for (int frame = 0; frame <= maximumLiveFrame; frame++)
            {
                expectedFrame = frames[component] = frame == 256 ? ushort.MaxValue : (ushort)frame;
                expectedTimer = timers[component] = timer;
                Reference(component, ref expectedFrame, ref expectedTimer);
                advance(component);
                AssertEqual((expectedFrame, expectedTimer), (frames[component], timers[component]), "Flare timer sign and authored frame boundaries match native");
            }
        }
        Console.WriteLine("Charge flare: 52 compiled bytes, loud non-catalog rejection, 3072 loop ticks and 210 authored boundary-state advances preserve native cadence with timing ROM reads forbidden.");

        void Reference(int component, ref ushort frame, ref ushort timer)
        {
            timer = unchecked((ushort)(timer - 1));
            if ((short)timer >= 0) return;
            ushort pointer = RomDataReader.ReadWordFixedBank(bus, 0x90c481 + component * 2);
            frame = unchecked((ushort)(frame + 1));
            byte Delay(int selected) => bus.ReadByte(0x900000 | unchecked((ushort)(pointer + selected)));
            byte delay = Delay(frame);
            if (delay == 255) { frame = 0; delay = Delay(frame); }
            else if (delay == 254) { frame = unchecked((ushort)(frame - Delay(frame + 1))); delay = Delay(frame); }
            timer = delay;
        }
    }

}
