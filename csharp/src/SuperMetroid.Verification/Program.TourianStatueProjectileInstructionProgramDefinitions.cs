using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Registers the Tourian statue projectile-program checks against the pinned retail cartridge.</summary>
    private static void VerifyTourianStatueProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyTourianStatueProjectileInstructionProgramDefinitions), () => VerifyTourianStatueProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks native mechanics words and executes statue projectile families while guarding compiled ROM data.</summary>
    /// <param name="rom">Pinned retail address space used to verify words and compare installed sprite selectors.</param>
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
        var guard = new TourianStatueProjectileInstructionReadGuard(rom);
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
        ushort tailY = tail.YPosition;
        Run(tail, 1);
        AssertEqual(unchecked((ushort)(tailY + 8)), tail.YPosition,
            "particle tail applies its first compiled Y displacement");
        Run(tail, 1);
        AssertEqual(unchecked((ushort)(tailY + 12)), tail.YPosition,
            "particle tail applies its second compiled Y displacement");
        Run(tail, 1);
        AssertEqual(unchecked((ushort)(tailY + 14)), tail.YPosition,
            "particle tail applies its third compiled Y displacement");
        Run(tail, 1);
        AssertTrue(!tail.IsActive, "particle tail deletes after four authored poses");

        spawnChild.Invoke(enemies, [particle, TourianStatueRomData.Splash]);
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
            EnemyProjectileCodePointers.PreInst_EnemyProj_TourianStatueBaseDecoration_AllowProcess,
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

        AssertEqual(0, guard.ObservedPresentationWords.Count,
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
            spawnStatue.Invoke(enemies, [kind, (ushort)128, (ushort)128]);
            return enemies.EnemyProjectiles.First(projectile => projectile.Kind == kind);
        }

        void Run(RoomEnemyProjectileSlot projectile, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, null, (ushort)0, (ushort)0]);
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
            }
        }
    }

    /// <summary>Repeatedly reads the compiled eye-glow and Phantoon words so the caller can measure warmed lookup allocations.</summary>
    /// <returns>A checksum that keeps the repeated mechanics reads observable.</returns>
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

    /// <summary>Rejects compiled mechanics reads and records accesses to installed Tourian statue presentation operands.</summary>
    /// <param name="source">Address space supplying cartridge bytes not rejected by the guard.</param>
    private sealed class TourianStatueProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation-word addresses observed while projectile instructions resolve their visual frames.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Counts cartridge reads rejected because they target compiled Tourian statue mechanics.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge access through the compiled-mechanics read guard.</summary>
        /// <param name="address">Cartridge address requested by projectile execution.</param>
        /// <returns>The source byte for an allowed address.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled mechanics, tracks presentation operands, and forwards other reads.</summary>
        /// <param name="address">CPU address requested by the projectile system.</param>
        /// <returns>The wrapped source byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled Tourian statue mechanics.</exception>
        public byte ReadByte(int address)
        {
            if (TourianStatueProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(
                    address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Tourian statue mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < TourianStatueProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = TourianStatueProjectileInstructionProgramDefinitions
                        .PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes to the wrapped address space.</summary>
        /// <param name="address">Address to update.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
