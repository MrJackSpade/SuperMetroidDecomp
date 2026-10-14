using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>Checks compiled gunship liftoff dust data and real projectile allocation against cartridge table values.</summary>
internal static class GunshipDustDefinitionsVerification
{
    /// <summary>Compares all six dust definitions and wrapped spawn positions, then checks full-pool and invalid-parameter behavior.</summary>
    internal static void VerifyCompiledGunshipDust(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(SnesLittleEndian.Word(CompiledGunshipDustRomByte(rom, address), CompiledGunshipDustRomByte(rom, address + 1)));
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        enemies.BindFixtureBus(NewGunshipDustGuard(rom));
        var spawn = new Action<ushort, SamusState>(enemies.Projectiles.SpawnGunshipLiftoffDustCloud);
        var samus = new SamusState();
        var slot = enemies.Projectiles.EnemyProjectiles[^1];
        for (ushort parameter = 0; parameter <= 10; parameter += 2)
        {
            short dx = unchecked((short)Word(EnemyRomTablePointersGunship.DustXOffsetWords + parameter));
            ushort instruction = Word(EnemyRomTablePointersGunship.DustInstructionPointers + parameter);
            var definition = GunshipDustDefinitions.ForParameter(parameter);
            AssertEqual(dx, definition.XOffset, "All native dust signed X offsets");
            AssertEqual(instruction, definition.Instruction, "All native dust instruction selections");
            for (int origin = 0; origin <= ushort.MaxValue; origin++)
            {
                slot.Clear();
                // Every world X and Y, including independent positive/negative wrap,
                // enters the actual allocator/header initializer with stale slot values.
                samus.Position = samus.Position.WithWhole(new((ushort)origin, (ushort)(ushort.MaxValue - origin)));
                slot.Position = slot.Position.WithFractionY(0xffff).WithFractionX(0xffff);
                slot.Velocity = new(new SnesSignedEightEight(0x1234), new SnesSignedEightEight(0x1234));
                spawn(parameter, samus);
                AssertEqual(RoomEnemyProjectileKind.GunshipLiftoffDustCloud, slot.Kind, "Native reverse allocation chooses last free slot");
                AssertEqual(unchecked((ushort)(samus.Position.X.Whole + dx)), slot.Position.X.Whole, "Actual dust signed X placement and wrap");
                AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + Word(0x86a2c5))), slot.Position.Y.Whole, "Actual dust Y placement and wrap");
                AssertEqual(instruction, slot.InstructionPointer, "Actual dust program selection");
                AssertEqual(parameter, slot.Variable0, "Dust retains native even parameter");
                AssertEqual((ushort)1, slot.InstructionTimer.RawValue, "Dust begins on first program tick");
                AssertEqual((ushort)0, slot.Position.X.Fraction, "Dust clears stale X fraction");
                AssertEqual((ushort)0, slot.Position.Y.Fraction, "Dust clears stale Y fraction");
                AssertEqual((ushort)0, slot.Velocity.X.RawValue, "Dust clears stale X velocity");
                AssertEqual((ushort)0, slot.Velocity.Y.RawValue, "Dust clears stale Y velocity");
            }
        }
        foreach (var occupied in enemies.Projectiles.EnemyProjectiles)
        {
            occupied.Kind = RoomEnemyProjectileKind.PhantoonStartingFlame;
            occupied.Position = occupied.Position.WithWholeX(0x1234);
            occupied.InstructionPointer = 0x5678;
        }
        for (ushort parameter = 0; parameter <= 10; parameter += 2) spawn(parameter, samus);
        foreach (var occupied in enemies.Projectiles.EnemyProjectiles)
        {
            AssertEqual(RoomEnemyProjectileKind.PhantoonStartingFlame, occupied.Kind, "Full pool retains owner");
            AssertEqual((ushort)0x1234, occupied.Position.X.Whole, "Full pool retains X");
            AssertEqual((ushort)0x5678, occupied.InstructionPointer, "Full pool retains program");
        }
        foreach (ushort invalid in new ushort[] { 1, 3, 9, 11, 12, 0xffff })
            AssertThrows<ArgumentOutOfRangeException>(() => spawn(invalid, samus), "Invalid dust parameter still rejected before full-pool return");
        Console.WriteLine("Gunship dust: 12 native words, 393216 real wrapped spawns, full-pool retention and parameter validation pass with migrated reads forbidden.");
    }

    /// <summary>Reads a native dust-table byte solely as the verification oracle at the CPU bus boundary.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled Gunship dust")]
    private static byte CompiledGunshipDustRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Allows cartridge oracle reads while rejecting migrated dust-table reads and every bus write from the spawn path.</summary>
    private static CartridgeReadGuard NewGunshipDustGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .ForbidRange(0x86a2d6, 0x86a2ee - 0x86a2d6, _ => "Unexpected migrated gunship dust table read.")
            .RejectWrites(_ => "Unexpected gunship dust bus write.");
}
