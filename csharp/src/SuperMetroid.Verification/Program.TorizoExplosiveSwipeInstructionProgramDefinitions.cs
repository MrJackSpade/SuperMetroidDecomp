using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyTorizoExplosiveSwipeInstructionProgramDefinitions() =>
        Suite(nameof(VerifyTorizoExplosiveSwipeInstructionProgramDefinitions), () => VerifyTorizoExplosiveSwipeInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo explosive swipe instruction program definitions")]
    private static void VerifyTorizoExplosiveSwipeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Torizo explosive-swipe mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewTorizoExplosiveSwipeInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        var spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoExplosiveSwipe", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);

        RoomEnemySlot torizo = enemies.Slots[0];
        torizo.Position = torizo.Position.WithWhole(new(1000, 500));
        torizo.Parameter1 = 0;
        spawn(torizo, 0);
        RoomEnemyProjectileSlot swipe = enemies.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.BombTorizoExplosiveSwipe);
        AssertEqual(TorizoExplosiveSwipeInstructionProgramDefinitions.Initial,
            swipe.InstructionPointer, "real Torizo swipe selects compiled program");
        AssertEqual((ushort)970, swipe.Position.X.Whole, "real Torizo swipe initial X");
        AssertEqual((ushort)448, swipe.Position.Y.Whole, "real Torizo swipe initial Y");

        for (int frame = 0; frame < 25; frame++)
        {
            process.Invoke(enemies, [swipe, null, Position16<RoomPixels>.Zero]);
            AssertTrue(swipe.IsActive,
                $"Torizo explosive swipe remains active through frame {frame + 1}");
        }
        process.Invoke(enemies, [swipe, null, Position16<RoomPixels>.Zero]);
        AssertTrue(!swipe.IsActive,
            "Torizo explosive swipe deletes on the tick after five five-frame poses");

        AssertEqual(0, guard.ObservedWords.Count,
            "compiled Torizo swipe visuals require no cartridge reads");
        for (int index = 0; index < TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "Torizo swipe compiled selector exists");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "Torizo swipe compiled selector matches original operand");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Torizo explosive-swipe mechanics byte");
        AssertThrows<InvalidDataException>(
            () => TorizoExplosiveSwipeInstructionProgramDefinitions.ReadMechanicsWord(0xa4af),
            "Torizo explosive-swipe spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoExplosiveSwipeInstructionProgramDefinitions.ReadMechanicsWord(0xa4a8),
            "adjacent Torizo program data is rejected as explosive-swipe mechanics");

        _ = ProbeTorizoExplosiveSwipeInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTorizoExplosiveSwipeInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Torizo explosive-swipe allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Torizo explosive-swipe mechanics lookups allocate no storage");

        Console.WriteLine(
            "Torizo explosive-swipe instruction mechanics: seven compiled words, the " +
            "real producer's exact 25-frame lifetime, and five native compiled selectors pass " +
            "with mechanics bytes forbidden.");
    }

    private static int ProbeTorizoExplosiveSwipeInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TorizoExplosiveSwipeInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TorizoExplosiveSwipeInstructionProgramDefinitions.Initial
                    : (ushort)0xa4c1);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewTorizoExplosiveSwipeInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(TorizoExplosiveSwipeInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Torizo explosive-swipe byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordCount, TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordAddress);
}
