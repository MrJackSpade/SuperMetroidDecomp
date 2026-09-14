using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyChargeFlareDefinitions(SuperMetroidAddressSpace bus)
    {
        var guard = new ChargeFlareDefinitionGuard(bus);
        for (int address = 0x908000; address <= 0x90ffff; address++)
        {
            AssertEqual(bus.ReadByte(address), ChargeFlareAnimationDefinitions.ReadByte(guard, address), "Flare cadence retains every native byte and adjacent read");
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, address), ChargeFlareAnimationDefinitions.ReadWord(guard, address), "Flare cadence retains unaligned and bank-wrapped words");
        }
        var system = new SamusProjectileSystem();
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var advance = typeof(SamusProjectileSystem).GetMethod("AdvanceFlareComponent", flags)!.CreateDelegate<Action<ISnesAddressSpace, int>>(system);
        var frames = (ushort[])typeof(SamusProjectileSystem).GetField("_flareFrames", flags)!.GetValue(system)!;
        var timers = (ushort[])typeof(SamusProjectileSystem).GetField("_flareTimers", flags)!.GetValue(system)!;
        for (int component = 0; component < 3; component++)
        {
            frames[component] = 0; timers[component] = 0;
            ushort expectedFrame = 0, expectedTimer = 0;
            for (int tick = 0; tick < 1024; tick++)
            {
                Reference(component, ref expectedFrame, ref expectedTimer);
                advance(guard, component);
                AssertEqual((expectedFrame, expectedTimer), (frames[component], timers[component]), "Flare production cadence matches native repeated rewind/restart trajectory");
            }
            foreach (ushort timer in new ushort[] { 0, 1, 2, 0x8000, 0xffff })
            for (int frame = 0; frame <= 256; frame++)
            {
                expectedFrame = frames[component] = frame == 256 ? ushort.MaxValue : (ushort)frame;
                expectedTimer = timers[component] = timer;
                Reference(component, ref expectedFrame, ref expectedTimer);
                advance(guard, component);
                AssertEqual((expectedFrame, expectedTimer), (frames[component], timers[component]), "Flare timer sign, frame wrap and adjacent stream reads match native");
            }
        }
        Console.WriteLine("Charge flare: 52 compiled bytes, every upper-bank byte/word, 3072 loop ticks and 3855 boundary-state advances preserve native cadence with timing ROM reads forbidden.");

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

    private sealed class ChargeFlareDefinitionGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x90c481 and < 0x90c4b5)
                throw new InvalidDataException("Charge flare still reads cadence ROM.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
