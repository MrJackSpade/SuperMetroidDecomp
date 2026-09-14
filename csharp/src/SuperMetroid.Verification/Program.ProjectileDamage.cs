using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyProjectileDamage(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8);
        int[] headers = Enumerable.Range(0, 24).Select(i => 0x938431 + i * 22).Concat(new[]
        {
            0x938641, 0x938657, 0x93866d, 0x938671, 0x938675, 0x938679, 0x93867d, 0x938681,
            0x938685, 0x938689, 0x93868d, 0x938691, 0x938695, 0x9386ab, 0x9386c1, 0x9386d7,
        }).ToArray();
        var bus = new ProjectileDamageReadGuard(rom, headers);
        foreach (int address in headers)
            AssertEqual(Word(address), SamusProjectileDamageDefinitions.Read(bus, address), "Compiled native damage header");
        for (int address = 0x938000; address <= 0x93ffff; address++)
            AssertEqual(Word(address), SamusProjectileDamageDefinitions.Read(rom, address), "Exact header selection preserves adjacent/unaligned/bank-wrapped reads");

        var room = CreateRoom(32, 16, new ushort[512], new byte[512]);
        MethodInfo Method(string name) => typeof(SamusProjectileSystem).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!;
        SamusState Samus() => new() { Pose = 1, XPosition = 128, YPosition = 128, Missiles = 10, SuperMissiles = 10 };
        void Check(SamusProjectileSlot slot, int table, int index, int direction)
        {
            int data = 0x930000 | Word(table + index * 2);
            AssertEqual(Word(data), slot.Damage, "Production initializer uses compiled damage");
            AssertEqual(Word(data + 2 + direction * 2), slot.InstructionPointer, "Animation list selection unchanged");
            AssertEqual((ushort)rom.ReadByte(0x930000 | (slot.InstructionPointer + 4)), slot.XRadius, "Physical X radius unchanged");
            AssertEqual((ushort)rom.ReadByte(0x930000 | (slot.InstructionPointer + 5)), slot.YRadius, "Physical Y radius unchanged");
            AssertEqual((ushort)1, slot.InstructionTimer, "Initializer timer unchanged");
        }
        for (ushort beam = 0; beam < 12; beam++)
        foreach (bool charged in new[] { false, true })
        {
            var system = new SamusProjectileSystem();
            var samus = Samus(); samus.EquippedBeams = beam;
            Method("TryFireBeam").Invoke(system, new object?[] { bus, room, samus, (ushort)SnesButton.X, new SamusBombProjectileSystem(), null, charged });
            var slot = system.Slots[0];
            int table = charged ? SamusProjectileRomData.Beams.ChargedDataPointers : SamusProjectileRomData.Beams.UnchargedDataPointers;
            Check(slot, table, beam, slot.Direction & 15);
            system.ReflectFromEnemy(bus, 0);
            Check(slot, table, beam, slot.Direction & 15);
        }
        foreach (ushort item in new ushort[] { 1, 2 })
        {
            var system = new SamusProjectileSystem(); var samus = Samus(); samus.SelectedHudItem = item;
            var result = system.TryFireMissile(bus, samus, (ushort)SnesButton.X, 0, new SamusBombProjectileSystem());
            AssertEqual((int?)0, result.Slot, "Missile allocation succeeds");
            Check(system.Slots[0], SamusProjectileRomData.NonBeam.DataPointers, item, system.Slots[0].Direction & 15);
            system.ReflectFromEnemy(bus, 0);
            Check(system.Slots[0], SamusProjectileRomData.NonBeam.DataPointers, item, system.Slots[0].Direction & 15);
            if (item == 2)
            {
                Method("SpawnSuperMissileLink").Invoke(system, new object[] { bus, samus, system.Slots[0] });
                AssertEqual((ushort)300, system.Slots[1].Damage, "Invisible link remains occupied/nonzero damage");
                AssertEqual((ushort)300, system.Slots[0].Damage, "Link allocation does not replace its owner");
            }
        }
        var hyper = new SamusProjectileSystem();
        Method("TryFireHyperBeam").Invoke(hyper, new object?[] { bus, room, Samus(), new SamusBombProjectileSystem(), null });
        AssertEqual((ushort)1000, hyper.Slots[0].Damage, "Hyper retains its post-initialization damage override");
        foreach (ushort beam in new ushort[] { 1, 2, 4, 8 })
        {
            var system = new SamusProjectileSystem(); var samus = Samus();
            samus.EquippedBeams = beam; samus.SelectedHudItem = 3; samus.PowerBombs = 10;
            AssertTrue(system.TryActivateCombo(bus, samus, new SamusBombProjectileSystem(), out _), "Combo initializes with damage ROM reads forbidden");
            foreach (var slot in system.Slots.Take(4))
                AssertEqual((ushort)(beam == 2 ? 90 : 300), slot.Damage, "SBA/ordinary Ice/Spazer trail damage");
        }
        var initializeBomb = typeof(SamusBombProjectileSystem).GetMethod("InitializeBombFromRom", BindingFlags.NonPublic | BindingFlags.Static)!;
        foreach (ushort type in new ushort[] { 0x0300, 0x0500, 0x8500 })
        {
            var slot = new SamusBombProjectileSlot(0) { Type = type };
            initializeBomb.Invoke(null, new object[] { bus, slot });
            AssertEqual((ushort)(type == 0x0300 ? 200 : 30), slot.Damage, "Power/ordinary/spread bomb damage");
        }
        var combo = typeof(SamusProjectileSystem).GetMethod("InitializeComboData", BindingFlags.NonPublic | BindingFlags.Static)!;
        bool rejectedMarker = false;
        try
        {
            combo.Invoke(null, new object[] { bus, new SamusProjectileSlot(0) { Type = 0x8025 }, false, true });
        }
        catch (TargetInvocationException error) when (error.InnerException is InvalidDataException invalid && invalid.Message == "Negative native combo projectile damage.")
        {
            rejectedMarker = true;
        }
        AssertTrue(rejectedMarker, "Compiled unused negative marker still reaches the existing loud rejection");
        Console.WriteLine("Projectile damage: 40 headers, complete high-bank address scan and all seven initializer paths pass with damage reads forbidden.");
    }

    private sealed class ProjectileDamageReadGuard(ISnesAddressSpace source, int[] headers) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (headers.Contains(address) || headers.Contains(address - 1))
                throw new InvalidDataException($"Projectile damage still reads compiled ROM header ${address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
