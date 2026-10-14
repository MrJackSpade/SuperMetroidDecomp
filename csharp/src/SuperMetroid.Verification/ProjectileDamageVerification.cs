using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;

/// <summary>Checks compiled projectile damage and special beam mechanics against native data and guarded production initialization.</summary>
internal static class ProjectileDamageVerification
{
    /// <summary>Exercises beam, missile, bomb, hyper, and combo initializers with cartridge damage reads forbidden.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for projectile damage")]
    internal static void VerifyProjectileDamage(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(address);
        Suite(nameof(VerifyComboMechanicsDefinitions), () => VerifyComboMechanicsDefinitions(rom, Word));
        int[] headers = Enumerable.Range(0, 24).Select(i => 0x938431 + i * 22).Concat(new[]
        {
            0x938641, 0x938657, 0x93866d, 0x938671, 0x938675, 0x938679, 0x93867d, 0x938681,
            0x938685, 0x938689, 0x93868d, 0x938691, 0x938695, 0x9386ab, 0x9386c1, 0x9386d7,
        }).ToArray();
        var bus = NewProjectileDamageReadGuard(rom);
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

        var room = CreateRoom(new(32, 16), new ushort[512], new byte[512]);
        SamusState Samus() => new() { Pose = SamusPoseId.FacingRightNormalPose, Position = new(new(128, 0), new(128, 0)), Missiles = 10, SuperMissiles = 10 };
        void Check(SamusProjectileSlot slot, int table, int index, int direction)
        {
            int data = 0x930000 | Word(table + index * 2);
            AssertEqual(Word(data), slot.Damage, "Production initializer uses compiled damage");
            AssertEqual(Word(data + 2 + direction * 2), slot.InstructionPointer, "Animation list selection unchanged");
            AssertEqual((ushort)rom.ReadCartridgeByte(0x930000 | (slot.InstructionPointer + 4)), slot.Radius.X, "Physical X radius unchanged");
            AssertEqual((ushort)rom.ReadCartridgeByte(0x930000 | (slot.InstructionPointer + 5)), slot.Radius.Y, "Physical Y radius unchanged");
            AssertEqual((ushort)1, slot.InstructionTimer.RawValue, "Initializer timer unchanged");
        }
        for (ushort beam = 0; beam < 12; beam++)
        foreach (bool charged in new[] { false, true })
        {
            var system = new SamusProjectileSystem();
            var samus = Samus(); samus.EquippedBeams = beam;
            system.Firing.TryFireBeam(bus, room, samus, (ushort)SnesButton.X, new SamusBombProjectileSystem(), null, charged);
            var slot = system.Slots[0];
            int table = charged ? SamusProjectileRomData.Beams.ChargedDataPointers : SamusProjectileRomData.Beams.UnchargedDataPointers;
            Check(slot, table, beam, slot.Direction & 15);
            system.EnemyImpacts.ReflectFromEnemy(bus, 0);
            Check(slot, table, beam, slot.Direction & 15);
        }
        foreach (ushort item in new ushort[] { 1, 2 })
        {
            var system = new SamusProjectileSystem(); var samus = Samus(); samus.SelectedHudItem = item;
            var result = system.Firing.TryFireMissile(bus, samus, (ushort)SnesButton.X, 0, new SamusBombProjectileSystem());
            AssertEqual((int?)0, result.Slot, "Missile allocation succeeds");
            Check(system.Slots[0], SamusProjectileRomData.NonBeam.DataPointers, item, system.Slots[0].Direction & 15);
            system.EnemyImpacts.ReflectFromEnemy(bus, 0);
            Check(system.Slots[0], SamusProjectileRomData.NonBeam.DataPointers, item, system.Slots[0].Direction & 15);
            if (item == 2)
            {
                system.Motion.SpawnSuperMissileLink(bus, samus, system.Slots[0]);
                AssertEqual((ushort)300, system.Slots[1].Damage, "Invisible link remains occupied/nonzero damage");
                AssertEqual((ushort)300, system.Slots[0].Damage, "Link allocation does not replace its owner");
            }
        }
        var hyper = new SamusProjectileSystem();
        hyper.Firing.TryFireHyperBeam(bus, room, Samus(), new SamusBombProjectileSystem(), null);
        AssertEqual((ushort)1000, hyper.Slots[0].Damage, "Hyper retains its post-initialization damage override");
        foreach (ushort beam in new ushort[] { 1, 2, 4, 8 })
        {
            var system = new SamusProjectileSystem(); var samus = Samus();
            samus.EquippedBeams = beam; samus.SelectedHudItem = 3; samus.PowerBombs = 10;
            AssertTrue(system.Combos.TryActivateCombo(bus, samus, new SamusBombProjectileSystem(), out _), "Combo initializes with damage ROM reads forbidden");
            foreach (var slot in system.Slots.Take(4))
                AssertEqual((ushort)(beam == 2 ? 90 : 300), slot.Damage, "SBA/ordinary Ice/Spazer trail damage");
        }
        foreach (ushort type in new ushort[] { 0x0300, 0x0500, 0x8500 })
        {
            var slot = new SamusBombProjectileSlot(0) { Type = type };
            SamusBombProjectileSystem.InitializeBombFromRom(slot);
            AssertEqual((ushort)(type == 0x0300 ? 200 : 30), slot.Damage, "Power/ordinary/spread bomb damage");
        }
        bool rejectedMarker = false;
        try
        {
            BeamCombos.InitializeComboData(new SamusProjectileSlot(0) { Type = 0x8025 }, false, true);
        }
        catch (InvalidDataException invalid) when (invalid.Message == "Negative native combo projectile damage.")
        {
            rejectedMarker = true;
        }
        AssertTrue(rejectedMarker, "Compiled unused negative marker still reaches the existing loud rejection");
        Console.WriteLine("Projectile initialization: 357 selection words, 40 damage headers, loud non-catalog rejection and all seven initializer paths pass with selection/damage reads forbidden.");
    }

    /// <summary>Runs cost, angle, sine-offset, and activation checks for compiled special beam data.</summary>
    private static void VerifyComboMechanicsDefinitions(
        SuperMetroidAddressSpace rom,
        Func<int, ushort> readWord)
    {
        Suite(nameof(VerifyComboPowerBombCostAlgorithm), () => VerifyComboPowerBombCostAlgorithm(readWord));
        Suite(nameof(VerifyComboOriginAngleAlgorithm), () => VerifyComboOriginAngleAlgorithm(readWord));
        Suite(nameof(VerifyComboMotionAndActivation), () => VerifyComboMotionAndActivation(rom));
    }

    /// <summary>Compares every combo Power Bomb cost with the native table and checks invalid beam indices.</summary>
    internal static void VerifyComboPowerBombCostAlgorithm(Func<int, ushort> readWord)
    {
        for (int beam = 0; beam < 12; beam++)
            AssertEqual(readWord(SamusComboRomData.Costs + beam * 2),
                SamusComboMechanicsDefinitions.GetPowerBombCost(SamusBeamCombinations.FromTableIndex(beam)),
                $"compiled combo Power Bomb cost {beam}");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusComboMechanicsDefinitions.GetPowerBombCost(SamusBeamCombination.SpazerPlasma),
            "combo cost rejects a beam index beyond the native table");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusComboMechanicsDefinitions.GetPowerBombCost(SamusBeamCombinations.FromTableIndex(-1)),
            "combo cost rejects a negative beam index");
        Console.WriteLine("Combo cost algorithm: all twelve original cost words and bounds match.");
    }

    /// <summary>Compares the four combo origin angles with native words and checks slot bounds.</summary>
    internal static void VerifyComboOriginAngleAlgorithm(Func<int, ushort> readWord)
    {
        for (int slot = 0; slot < 4; slot++)
            AssertEqual(readWord(SamusComboRomData.OriginAngles + slot * 2),
                SamusComboMechanicsDefinitions.GetOriginAngle(slot),
                $"compiled combo origin angle {slot}");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusComboMechanicsDefinitions.GetOriginAngle(4),
            "combo angle rejects a fifth projectile slot");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusComboMechanicsDefinitions.GetOriginAngle(-1),
            "combo angle rejects a negative projectile slot");
        Console.WriteLine("Combo origin algorithm: all four original angle words and bounds match.");
    }

    /// <summary>Compares all byte-valued angle and amplitude pairs with the native sine-offset calculation.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for combo sine offset alias")]
    internal static void VerifyComboSineOffsetAlias(SuperMetroidAddressSpace rom)
    {
        (ushort X, ushort Y) NativeOffset(int angle, int amplitude)
        {
            ushort Component(int phase)
            {
                bool negative = phase >= 128;
                int index = negative ? unchecked((byte)(phase + 128)) : phase;
                int address = SamusComboRomData.PositiveSine + index * 2;
                int magnitude = (SnesEightEight.Multiply(rom.ReadCartridgeByte(address), (byte)amplitude)) +
                    rom.ReadCartridgeByte(address + 1) * (byte)amplitude;
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

    }

    /// <summary>Checks combo activation cost, origin angles, and first motion step while compiled table reads are forbidden.</summary>
    private static void VerifyComboMotionAndActivation(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyComboSineOffsetAlias), () => VerifyComboSineOffsetAlias(rom));
        var guarded = NewComboMechanicsReadGuard(rom);
        foreach (ushort beam in new ushort[] { 1, 2, 4, 8 })
        {
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            var samus = new SamusState
            {
                Pose = SamusPoseId.FacingRightNormalPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
                EquippedBeams = beam,
                SelectedHudItem = 3,
                PowerBombs = 10,
            };
            AssertTrue(projectiles.Combos.TryActivateCombo(
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
            ushort amplitude = beam == 2 ? (ushort)32 : unchecked((ushort)first.Velocity.X.SignedRawValue);
            (ushort X, ushort Y) offset = SamusComboMechanicsDefinitions.GetSineOffset(
                angle, amplitude);
            if (beam == 2)
                projectiles.Combos.StepIceCombo(guarded, samus, first, shared, Position16<RoomPixels>.Zero);
            else if (beam == 4)
                projectiles.Combos.StepSpazerCombo(guarded, samus, first, shared, 0);
            else
                projectiles.Combos.StepPlasmaCombo(samus, first, shared, Position16<RoomPixels>.Zero);
            AssertEqual(unchecked((ushort)(samus.Position.X.Whole + offset.X)), first.Position.X.Whole,
                $"beam {beam:X} production X consumes compiled sine offset");
            AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + offset.Y)), first.Position.Y.Whole,
                $"beam {beam:X} production Y consumes compiled sine offset");
        }

        Console.WriteLine(
            "  Special beam mechanics: twelve costs, four origin angles and 65,536 sine offsets match cartridge data; all four producers avoid those ROM tables.");
    }

    /// <summary>Rejects native damage-header and selection-table reads during projectile initialization.</summary>
    private static MutableCartridgeReadGuard NewProjectileDamageReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= 0x9383c1 and < 0x9386db,
            address => new InvalidDataException($"Projectile damage still reads compiled ROM header ${address:X6}."));
        return guard;
    }

    /// <summary>Rejects combo cost, origin-angle, and sine-table reads from the production path.</summary>
    private static MutableCartridgeReadGuard NewComboMechanicsReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= SamusComboRomData.Costs and < SamusComboRomData.Costs + 24 or
                >= SamusComboRomDataConstants.OriginAngles and < SamusComboRomDataConstants.OriginAngles + 8 or
                >= SamusComboRomDataConstants.PositiveSine and < SamusComboRomDataConstants.PositiveSine + 512,
            address => $"Special beam mechanics still read compiled ROM byte ${address:X6}.");
        return guard;
    }
}
