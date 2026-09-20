using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Protects the enemy-data catalog from accidentally acquiring WRAM addresses,
    /// duplicate names, or pointers outside mapped cartridge space. With the private
    /// retail ROM present, this also reads both ends of representative multi-byte ranges.
    /// </summary>
    static void VerifyEnemyRomTablePointerCatalog()
    {
        Type[] families = typeof(EnemyRomTablePointers).GetNestedTypes(
            BindingFlags.Public | BindingFlags.NonPublic);
        AssertTrue(families.Length >= 14, "enemy ROM data is grouped by owning family");

        FieldInfo[] fields = families
            .SelectMany(family => family.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(field => field.IsLiteral && field.FieldType == typeof(int))
            .ToArray();
        AssertTrue(fields.Length >= 50, "enemy ROM data catalog covers fixed data tables");
        foreach (FieldInfo field in fields)
        {
            int address = (int)field.GetRawConstantValue()!;
            AssertTrue(
                address is >= 0x808000 and <= 0xffffff,
                $"{field.DeclaringType!.Name}.{field.Name} is a mapped SNES ROM address");
        }

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                $"  Enemy ROM data: {fields.Length} named ranges are structurally valid; " +
                "retail ROM reads skipped (private input absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach ((int start, int byteLength) in RepresentativeEnemyRomRanges())
        {
            _ = bus.ReadByte(start);
            _ = bus.ReadByte(start + byteLength - 1);
        }
        VerifyCompiledEnemyInstructionSelectors(bus);

        Console.WriteLine(
            $"  Enemy ROM data: {fields.Length} named ranges and " +
            $"{RepresentativeEnemyRomRanges().Length} representative retail ranges verified.");
    }

    /// <summary>
    /// Uses one nontrivial range from every represented storage shape: byte arrays,
    /// word arrays, palettes, pointer arrays, interleaved records, and transfer tables.
    /// </summary>
    private static (int Start, int ByteLength)[] RepresentativeEnemyRomRanges() =>
    [
        (EnemyRomTablePointers.Common.SignedSineCosineWords, 512),
        (EnemyRomTablePointers.Torizo.WakeXPositions, 4),
        (EnemyRomTablePointers.ChozoStatue.CarryVelocityWords, 64),
        (EnemyRomTablePointers.Gunship.DustInstructionPointers, 12),
        (EnemyRomTablePointers.Crocomire.DeathGraphicsSourceWords, 14),
        (EnemyRomTablePointers.DeadSidehopper.HorizontalVelocityWords, 8),
        (EnemyRomTablePointers.Gunship.LiftoffVramDestinationWords, 10),
        (EnemyRomTablePointers.Kraid.RoomBackgroundPaletteWords, 32),
        (EnemyRomTablePointers.Phantoon.FirstRoundHidingTimerWords, 16),
        (EnemyRomTablePointers.Ridley.HealthPaletteWords, 84),
        (EnemyRomTablePointers.TourianStatue.StatuePaletteWords, 32),
        (EnemyRomTablePointers.WorkRobot.InitialInstructionListWords, 4),
    ];

    private static void VerifyCompiledEnemyInstructionSelectors(
        SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => unchecked((ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));

        AssertEqual(Word(EnemyRomTablePointers.Torizo.SuperMissileInstructionPointers),
            GoldenTorizoProjectileDefinitions.GetReflectedSuperMissileInstruction(false),
            "Golden Torizo left reflected-Super list");
        AssertEqual(Word(EnemyRomTablePointers.Torizo.SuperMissileInstructionPointers + 2),
            GoldenTorizoProjectileDefinitions.GetReflectedSuperMissileInstruction(true),
            "Golden Torizo right reflected-Super list");
        for (int parameter = 0; parameter < 4; parameter++)
            AssertEqual(Word(EnemyRomTablePointers.WorkRobot.InitialInstructionListWords + parameter * 2),
                WorkRobotInitializationDefinitions.GetInitialInstruction(parameter),
                $"deactivated Work Robot initial selector {parameter}");
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
            AssertEqual(Word(EnemyRomTablePointers.TourianStatue.InstructionListWords + parameter),
                TourianEntranceStatueInstructionProgramDefinitions.GetInitialInstruction(parameter),
                $"Tourian entrance statue initial selector {parameter}");
        AssertThrows<ArgumentOutOfRangeException>(
            () => WorkRobotInitializationDefinitions.GetInitialInstruction(4),
            "Work Robot selector rejects values beyond native accepted overread");
        AssertThrows<ArgumentOutOfRangeException>(
            () => TourianEntranceStatueInstructionProgramDefinitions.GetInitialInstruction(1),
            "Tourian statue selector rejects odd byte offsets");

        var guarded = new EnemyInstructionSelectionReadGuard(rom);
        Type type = typeof(RoomEnemySystem);
        MethodInfo noPowerRobot = type.GetMethod(
            "InitializeWorkRobotNoPower", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (ushort parameter = 0; parameter < 4; parameter++)
        {
            var system = new RoomEnemySystem();
            var slot = new RoomEnemySlot(0) { Parameter1 = parameter };
            noPowerRobot.Invoke(system, [slot, new WorkRobotEnemyState(slot)]);
            AssertEqual(WorkRobotInitializationDefinitions.GetInitialInstruction(parameter),
                slot.CurrentInstruction,
                $"Work Robot production initializer {parameter} avoids selector ROM");
        }

        MethodInfo statueInitializer = type.GetMethod(
            "InitializeTourianEntranceStatue", BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            var system = new RoomEnemySystem();
            type.GetField("_bus", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(system, guarded);
            type.GetField("_cgram", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(system, new SnesCgram());
            var slot = new RoomEnemySlot(0) { Parameter1 = parameter };
            statueInitializer.Invoke(system, [slot]);
            AssertEqual(
                TourianEntranceStatueInstructionProgramDefinitions.GetInitialInstruction(
                    parameter),
                slot.CurrentInstruction,
                $"Tourian statue production initializer {parameter} avoids selector ROM");
        }

        MethodInfo spawnReflectedSuper = type.GetMethod(
            "SpawnGoldenTorizoSuperMissile", BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (bool facingRight in new[] { false, true })
        {
            var system = new RoomEnemySystem();
            type.GetField("_bus", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(system, guarded);
            var torizo = new RoomEnemySlot(0)
            {
                Parameter1 = facingRight ? (ushort)0x8000 : (ushort)0,
                XPosition = 128,
                YPosition = 128,
            };
            spawnReflectedSuper.Invoke(system, [torizo]);
            RoomEnemyProjectileSlot projectile = system.EnemyProjectiles.Single(
                candidate => candidate.IsActive);
            AssertEqual(
                GoldenTorizoProjectileDefinitions.GetReflectedSuperMissileInstruction(facingRight),
                projectile.InstructionPointer,
                $"Golden Torizo production reflected-Super selector {facingRight} avoids ROM");
        }

        Console.WriteLine(
            "  Enemy instruction selectors: nine native words and all three production initializers avoid their source tables.");
    }

    private sealed class EnemyInstructionSelectionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= EnemyRomTablePointers.Torizo.SuperMissileInstructionPointers and
                    < EnemyRomTablePointers.Torizo.SuperMissileInstructionPointers + 4 ||
                address is >= EnemyRomTablePointers.WorkRobot.InitialInstructionListWords and
                    < EnemyRomTablePointers.WorkRobot.InitialInstructionListWords + 8 ||
                address is >= EnemyRomTablePointers.TourianStatue.InstructionListWords and
                    < EnemyRomTablePointers.TourianStatue.InstructionListWords + 6)
                throw new InvalidOperationException(
                    $"Enemy instruction selector still reads compiled ROM byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
