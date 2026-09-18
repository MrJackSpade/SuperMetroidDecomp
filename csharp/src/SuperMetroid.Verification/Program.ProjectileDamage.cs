using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyProjectileDamage(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8);
        VerifyComboMechanicsDefinitions(rom, Word);
        int[] headers = Enumerable.Range(0, 24).Select(i => 0x938431 + i * 22).Concat(new[]
        {
            0x938641, 0x938657, 0x93866d, 0x938671, 0x938675, 0x938679, 0x93867d, 0x938681,
            0x938685, 0x938689, 0x93868d, 0x938691, 0x938695, 0x9386ab, 0x9386c1, 0x9386d7,
        }).ToArray();
        var bus = new ProjectileDamageReadGuard(rom);
        foreach (int address in headers)
            AssertEqual(Word(address), SamusProjectileDamageDefinitions.Read(address), "Compiled native damage header");
        for (int address = 0x9383c1; address < 0x9386db; address += 2)
            AssertEqual(Word(address), SamusProjectileSelectionDefinitions.ReadWord(address), "All 357 selectors and 40 adjacent damage headers avoid ROM reads");
        foreach (int address in new[] { 0x938000, 0x938432, 0x9386db, 0x93ffff })
        {
            AssertThrows<InvalidDataException>(
                () => SamusProjectileDamageDefinitions.Read(address),
                "Unknown damage address fails instead of reading arbitrary cartridge data");
            AssertThrows<InvalidDataException>(
                () => SamusProjectileSelectionDefinitions.ReadWord(address),
                "Unknown selector address fails instead of reading arbitrary cartridge data");
        }

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
        Console.WriteLine("Projectile initialization: 357 selection words, 40 damage headers, loud non-catalog rejection and all seven initializer paths pass with selection/damage reads forbidden.");
    }

    private static void VerifyComboMechanicsDefinitions(
        SuperMetroidAddressSpace rom,
        Func<int, ushort> readWord)
    {
        for (int beam = 0; beam < 12; beam++)
            AssertEqual(readWord(SamusComboRomData.Costs + beam * 2),
                SamusComboMechanicsDefinitions.GetPowerBombCost(beam),
                $"compiled combo Power Bomb cost {beam}");
        for (int slot = 0; slot < 4; slot++)
            AssertEqual(readWord(SamusComboRomData.OriginAngles + slot * 2),
                SamusComboMechanicsDefinitions.GetOriginAngle(slot),
                $"compiled combo origin angle {slot}");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusComboMechanicsDefinitions.GetPowerBombCost(12),
            "combo cost rejects a beam index beyond the native table");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusComboMechanicsDefinitions.GetOriginAngle(4),
            "combo angle rejects a fifth projectile slot");

        (ushort X, ushort Y) NativeOffset(int angle, int amplitude)
        {
            ushort Component(int phase)
            {
                bool negative = phase >= 128;
                int index = negative ? unchecked((byte)(phase + 128)) : phase;
                int address = SamusComboRomData.PositiveSine + index * 2;
                int magnitude = (rom.ReadByte(address) * (byte)amplitude >> 8) +
                    rom.ReadByte(address + 1) * (byte)amplitude;
                return unchecked((ushort)(negative ? -magnitude : magnitude));
            }

            return (Component(angle), Component(unchecked((byte)(angle - 64))));
        }
        for (int angle = 0; angle < 256; angle++)
        for (int amplitude = 0; amplitude < 256; amplitude++)
            AssertEqual(NativeOffset(angle, amplitude),
                SamusComboMechanicsDefinitions.GetSineOffset(
                    (ushort)angle, (ushort)amplitude),
                $"compiled combo sine offset {angle}/{amplitude}");

        var guarded = new ComboMechanicsReadGuard(rom);
        foreach (ushort beam in new ushort[] { 1, 2, 4, 8 })
        {
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            var samus = new SamusState
            {
                Pose = SamusPoseIds.FacingRightNormalPose,
                XPosition = 128,
                YPosition = 128,
                EquippedBeams = beam,
                SelectedHudItem = 3,
                PowerBombs = 10,
            };
            AssertTrue(projectiles.TryActivateCombo(
                    guarded, samus, shared, out _),
                $"beam {beam:X} activates with cost/angle ROM reads forbidden");
            AssertEqual((ushort)9, samus.PowerBombs,
                $"beam {beam:X} consumes its compiled one-Power-Bomb cost");
            for (int slot = 0; slot < 4; slot++)
                AssertEqual(beam is 2 or 8
                        ? SamusComboMechanicsDefinitions.GetOriginAngle(slot)
                        : (ushort)0,
                    projectiles.Slots[slot].Variable,
                    $"beam {beam:X} slot {slot} receives its compiled origin angle");

            if (beam == 1)
                continue;
            SamusProjectileSlot first = projectiles.Slots[0];
            ushort angle = first.Variable;
            ushort amplitude = beam == 2 ? (ushort)32 : unchecked((ushort)first.XVelocity);
            (ushort X, ushort Y) offset = SamusComboMechanicsDefinitions.GetSineOffset(
                angle, amplitude);
            if (beam == 2)
                projectiles.StepIceCombo(guarded, samus, first, shared, 0, 0);
            else if (beam == 4)
                projectiles.StepSpazerCombo(guarded, samus, first, shared, 0);
            else
                projectiles.StepPlasmaCombo(guarded, samus, first, shared, 0, 0);
            AssertEqual(unchecked((ushort)(samus.XPosition + offset.X)), first.XPosition,
                $"beam {beam:X} production X consumes compiled sine offset");
            AssertEqual(unchecked((ushort)(samus.YPosition + offset.Y)), first.YPosition,
                $"beam {beam:X} production Y consumes compiled sine offset");
        }

        Console.WriteLine(
            "  Special beam mechanics: twelve costs, four origin angles and 65,536 sine offsets match cartridge data; all four producers avoid those ROM tables.");
    }

    private sealed class ProjectileDamageReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x9383c1 and < 0x9386db)
                throw new InvalidDataException($"Projectile damage still reads compiled ROM header ${address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class ComboMechanicsReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= SamusComboRomData.Costs and < SamusComboRomData.Costs + 24 ||
                address is >= SamusComboRomData.OriginAngles and < SamusComboRomData.OriginAngles + 8 ||
                address is >= SamusComboRomData.PositiveSine and < SamusComboRomData.PositiveSine + 512)
                throw new InvalidOperationException(
                    $"Special beam mechanics still read compiled ROM byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
