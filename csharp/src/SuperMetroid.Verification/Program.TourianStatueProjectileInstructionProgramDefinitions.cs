using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyTourianStatueProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyTourianStatueProjectileInstructionProgramDefinitions), () => VerifyTourianStatueProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    private static void VerifyTourianStatueProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < TourianStatueProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                TourianStatueProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Tourian statue projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewTourianStatueProjectileInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 4));
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawnChild = typeof(RoomEnemySystem).GetMethod(
            "SpawnTourianParticleChild", flags)!;
        MethodInfo spawnStatue = typeof(RoomEnemySystem).GetMethod(
            "SpawnTourianEntranceStatueProjectile", flags)!;

        enemies.SpawnTourianUnlockEffect(0, soul: false);
        RoomEnemyProjectileSlot eye = Single(RoomEnemyProjectileKind.TourianStatueEyeGlow);
        Run(eye, 10);
        AssertEqual((ushort)0xb7db, eye.InstructionPointer,
            "eye glow reaches its sound and release tail after ten display frames");
        Run(eye, 1);
        AssertTrue(!eye.IsActive, "eye glow deletes after releasing four particles");
        AssertEqual(4, enemies.EnemyProjectiles.Count(
            projectile => projectile.Kind == RoomEnemyProjectileKind.TourianStatueParticle),
            "eye glow releases four real unlocking particles");
        AssertEqual((ushort)1, enemies.EarthquakeType, "eye glow starts the native earthquake");
        AssertEqual((ushort)0x20, enemies.EarthquakeTimer,
            "eye glow publishes the native earthquake duration bit");

        RoomEnemyProjectileSlot particle = Single(RoomEnemyProjectileKind.TourianStatueParticle);
        Run(particle, 3);
        RoomEnemyProjectileSlot tail = Single(RoomEnemyProjectileKind.TourianStatueTail);
        Run(particle, 2);
        AssertEqual(ushort.MaxValue, particle.GeneralTimer,
            "unset particle loop timer wraps from zero to FFFF like the cartridge");
        AssertEqual((ushort)0xb806, particle.InstructionPointer,
            "particle counted branch immediately begins its next display cycle");

        Run(tail, 1);
        ushort tailY = tail.Position.Y.Whole;
        Run(tail, 1);
        AssertEqual(unchecked((ushort)(tailY + 8)), tail.Position.Y.Whole,
            "particle tail applies its first compiled Y displacement");
        Run(tail, 1);
        AssertEqual(unchecked((ushort)(tailY + 12)), tail.Position.Y.Whole,
            "particle tail applies its second compiled Y displacement");
        Run(tail, 1);
        AssertEqual(unchecked((ushort)(tailY + 14)), tail.Position.Y.Whole,
            "particle tail applies its third compiled Y displacement");
        Run(tail, 1);
        AssertTrue(!tail.IsActive, "particle tail deletes after four authored poses");

        spawnChild.Invoke(enemies, [particle, RoomEnemyProjectileKind.TourianStatueSplash]);
        RoomEnemyProjectileSlot splash = Single(RoomEnemyProjectileKind.TourianStatueSplash);
        Run(splash, 4);
        AssertEqual((ushort)0xb7b1, splash.InstructionPointer,
            "water splash reaches deletion after four authored poses");
        Run(splash, 1);
        AssertTrue(!splash.IsActive, "water splash deletes on its fifth instruction tick");

        enemies.SpawnTourianUnlockEffect(2, soul: true);
        RoomEnemyProjectileSlot soul = Single(RoomEnemyProjectileKind.TourianStatueSoul);
        Run(soul, 3);
        AssertEqual((ushort)0xb852, soul.InstructionPointer,
            "statue soul loops through both authored poses");
        soul.InstructionPointer = TourianStatueProjectileInstructionProgramDefinitions.Delete;
        Run(soul, 1);
        AssertTrue(!soul.IsActive, "private Tourian delete list clears a soul actor");

        RoomEnemyProjectileSlot baseDecoration = SpawnStatue(
            RoomEnemyProjectileKind.TourianStatueBaseDecoration);
        Run(baseDecoration, 2);
        AssertEqual(
            (ushort)EnemyProjectilePreInstruction.TourianStatueBaseDecoration_AllowProcess,
            baseDecoration.PreInstruction,
            "base decoration installs its delayed position callback");
        AssertEqual((ushort)0xb866, baseDecoration.InstructionPointer,
            "base decoration reaches its stable loop");
        Run(baseDecoration, 1);
        AssertEqual((ushort)0xb866, baseDecoration.InstructionPointer,
            "base decoration loop remains stable");

        RoomEnemyProjectileSlot ridley = SpawnStatue(RoomEnemyProjectileKind.TourianStatueRidley);
        Run(ridley, 2);
        AssertEqual((ushort)0xb86e, ridley.InstructionPointer,
            "Ridley statue loop remains stable");
        RoomEnemyProjectileSlot phantoon = SpawnStatue(
            RoomEnemyProjectileKind.TourianStatuePhantoon);
        Run(phantoon, 2);
        AssertEqual((ushort)0xb876, phantoon.InstructionPointer,
            "Phantoon statue loop remains stable");

        AssertEqual(0, guard.ObservedWords.Count,
            "TourianStatueProjectile execution performs no live spritemap operand reads");
        AssertEqual(TourianStatueProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "TourianStatueProjectile executes every native visual operand");
        for (int index = 0; index < TourianStatueProjectileInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = TourianStatueProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"TourianStatueProjectile executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "TourianStatueProjectile has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "TourianStatueProjectile compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Tourian statue mechanics byte");
        AssertThrows<InvalidDataException>(
            () => TourianStatueProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xb7a3),
            "Tourian splash spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TourianStatueProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xb87a),
            "Tourian splash initializer is rejected as mechanics");

        _ = ProbeTourianStatueProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTourianStatueProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Tourian statue allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Tourian statue mechanics lookups allocate no storage");

        Console.WriteLine(
            "Tourian statue projectile instruction mechanics: fifty-eight compiled words, " +
            "all eight real actor families, and twenty-eight installed sprite frames match native OAM " +
            "with mechanics bytes forbidden.");

        RoomEnemyProjectileSlot Single(RoomEnemyProjectileKind kind) =>
            enemies.EnemyProjectiles.First(projectile => projectile.Kind == kind);

        RoomEnemyProjectileSlot SpawnStatue(RoomEnemyProjectileKind kind)
        {
            spawnStatue.Invoke(enemies, [kind, new Position16<RoomPixels>(128, 128)]);
            return enemies.EnemyProjectiles.First(projectile => projectile.Kind == kind);
        }

        void Run(RoomEnemyProjectileSlot projectile, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, null, Position16<RoomPixels>.Zero]);
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
            }
        }
    }

    private static int ProbeTourianStatueProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TourianStatueProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TourianStatueProjectileInstructionProgramDefinitions.EyeGlow
                    : TourianStatueProjectileInstructionProgramDefinitions.Phantoon);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewTourianStatueProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(TourianStatueProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Tourian statue mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, TourianStatueProjectileInstructionProgramDefinitions.PresentationWordCount, TourianStatueProjectileInstructionProgramDefinitions.PresentationWordAddress);
}
