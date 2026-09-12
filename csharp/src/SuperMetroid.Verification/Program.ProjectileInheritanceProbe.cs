using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // Opt-in failing reproduction, not part of the green regression suite until
    // the movement producers and frame-boundary handoff are implemented together.
    private static void ProbeProjectileVelocityInheritance()
    {
        var initialize = typeof(SamusProjectileSystem)
            .GetMethod("InitializePowerBeamVelocity", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusProjectileSlot>>();
        var bus = new ProjectileInheritanceProbeBus();
        int failures = 0;
        // Snapshots are CameraYSubSpeed followed by the four native integer/fraction
        // pairs at $0DAA..0DB9. Deliberately preserve the cartridge's word ordering.
        ushort[][] snapshots =
        [
            [0, 0, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 3, 0, 0, 0, 0, 0],
            [0, 0xfffd, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0xfffc, 0, 0, 0],
            [0xabcd, 0, 0x1234, 0, 0x5678, 0, 0x9abc, 0, 0],
        ];
        for (int sample = 0; sample < snapshots.Length; sample++)
        {
            for (int i = 0; i < snapshots[sample].Length; i++)
                bus.Word(0x0da8 + i * 2, snapshots[sample][i]);
            // Equal synthetic cardinal/diagonal base speeds isolate inheritance.
            bus.Word(0x90c2d1, 0x0400);
            bus.Word(0x90c2d3, 0x0400);
            for (ushort direction = 0; direction < 10; direction++)
            {
                var slot = new SamusProjectileSlot(0) { Direction = direction };
                initialize(bus, slot);
                // Direct transcription of loads/LSR/ORA/ADC at $90:B218..B2F5.
                // Do not replace these overlapping reads with idealized fixed-point math.
                ushort upWord = bus.Word(0x0db1);
                int up = (upWord & 0xff00) == 0 ? 0 : (upWord >> 2) | 0xc000;
                short expectedX = unchecked((short)(direction switch
                {
                    1 or 2 or 3 => 0x0400 + bus.Word(0x0dad),
                    6 or 7 or 8 => -0x0400 + bus.Word(0x0da9),
                    _ => 0,
                }));
                short expectedY = unchecked((short)(direction switch
                {
                    0 or 1 or 8 or 9 => -0x0400 + up,
                    3 or 4 or 5 or 6 => 0x0400 + bus.Word(0x0db5),
                    _ => 0,
                }));
                if (slot.XVelocity == expectedX && slot.YVelocity == expectedY) continue;
                failures++;
                Console.WriteLine($"Snapshot {sample}, direction {direction}: native ({expectedX:X4},{expectedY:X4}), port ({slot.XVelocity:X4},{slot.YVelocity:X4})");
            }
        }
        AssertEqual(0, failures, "Projectile initialization must inherit native movement/camera overlapping words");
    }

    private sealed class ProjectileInheritanceProbeBus : ISnesAddressSpace
    {
        private readonly Dictionary<int, byte> _bytes = new();
        public byte ReadByte(int address) => _bytes.GetValueOrDefault(address);
        public void WriteByte(int address, byte value) => _bytes[address] = value;
        public ushort Word(int address) => (ushort)(ReadByte(address) | ReadByte(address + 1) << 8);
        public void Word(int address, ushort value)
        {
            WriteByte(address, (byte)value);
            WriteByte(address + 1, (byte)(value >> 8));
        }
    }
}
