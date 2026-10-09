using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Runs the compiled-mechanics and production-lifecycle checks using the pinned retail ROM.</summary>
    private static void VerifyDownwardGateProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyDownwardGateProjectileInstructionProgramDefinitions), () => VerifyDownwardGateProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks native mechanics words, both gate lifecycles, executed visual operands, and allocation behavior.</summary>
    /// <param name="rom">Retail address space used to compare compiled values with cartridge data.</param>
    private static void VerifyDownwardGateProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance |
            BindingFlags.NonPublic;
        for (int index = 0;
             index < DownwardGateProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DownwardGateProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"downward-gate projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = new DownwardGateProjectileInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo move = typeof(RoomEnemySystem).GetMethod(
            "RunDownwardGateProjectileMovement", flags)!;

        RoomEnemySystem closingSystem = NewSystem();
        RoomEnemyProjectileSlot closing = Spawn(
            closingSystem,
            RoomEnemyProjectileKind.DownwardGateMoving);
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.Moving,
            closing.InstructionPointer,
            "real downward-moving gate producer selects the named program");
        AssertEqual((ushort)32, closing.YPosition,
            "downward-moving gate begins at its PLM row");
        Process(closingSystem, closing);
        AssertEqual((ushort)0x0100, closing.YVelocity,
            "downward-moving gate installs positive one-pixel velocity");
        AssertEqual(DownwardGateEnemyProjectileRomData.MovementPreInstruction,
            closing.PreInstruction,
            "downward-moving gate installs its translated movement callback");
        RunMovementFrames(closingSystem, closing, 64);
        AssertTrue(closing.IsActive,
            "downward-moving gate remains resident after reaching the closed position");
        AssertEqual((ushort)96, closing.YPosition,
            "downward-moving gate traverses exactly four sixteen-pixel stages");
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.ClosedSleep,
            closing.InstructionPointer,
            "downward-moving gate hands off to the closed sleep");
        AssertEqual(EnemyProjectileCodePointers.RTS_868170, closing.PreInstruction,
            "downward-moving gate clears movement when it parks");

        RoomEnemySystem openingSystem = NewSystem();
        RoomEnemyProjectileSlot opening = Spawn(
            openingSystem,
            RoomEnemyProjectileKind.DownwardGateClosed);
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.Closed,
            opening.InstructionPointer,
            "real closed-gate producer selects the named program");
        AssertEqual((ushort)96, opening.YPosition,
            "closed gate begins four blocks below its PLM row");
        Process(openingSystem, opening);
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.ClosedSleep,
            opening.InstructionPointer,
            "closed gate displays its parked frame and sleeps");
        openingSystem.ApplyDownwardGateProjectileRequest(
            new DownwardGateProjectileRequest(
                DownwardGateProjectileOperation.Wake,
                0,
                42),
            roomWidthInBlocks: 16);
        Process(openingSystem, opening);
        AssertEqual(unchecked((ushort)-0x0100), opening.YVelocity,
            "woken gate retains its authored upward velocity");
        AssertEqual(DownwardGateEnemyProjectileRomData.MovementPreInstruction,
            opening.PreInstruction,
            "woken gate installs its translated movement callback");
        RunMovementFrames(openingSystem, opening, 63);
        AssertTrue(opening.IsActive,
            "opening gate remains active one pixel before its fourth stage completes");
        AssertEqual((ushort)33, opening.YPosition,
            "opening gate reaches the final pixel before deletion");
        RunMovementFrames(openingSystem, opening, 1);
        AssertTrue(!opening.IsActive,
            "opening gate deletes after exactly four sixteen-pixel stages");

        RoomEnemySystem shotSystem = NewSystem();
        RoomEnemyProjectileSlot shot = Spawn(
            shotSystem,
            RoomEnemyProjectileKind.DownwardGateClosed);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        shot.InstructionTimer = 1;
        Process(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "downward-gate shot reaction reaches shared compiled deletion");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Projectile presentation performs zero live cartridge reads");
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        for (int index = 0;
             index < DownwardGateProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = DownwardGateProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production executes downward-gate presentation $86:{address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                $"compiled visual selector exists at $86:{address:X4}");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                $"compiled visual selector matches native operand $86:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared downward-gate mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => DownwardGateProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xe546),
            "downward-gate spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DownwardGateProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xe533),
            "downward-gate callback body is rejected as mechanics");

        _ = ProbeDownwardGateProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDownwardGateProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "downward-gate allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed downward-gate mechanics lookups allocate no storage");

        Console.WriteLine(
            "Downward-gate projectile instruction mechanics: twenty-eight compiled words, " +
            "both real producers, four-stage close/open lifecycles, shared deletion, and " +
            "nine executed native sprite compositions pass with zero live operand reads.");

        RoomEnemySystem NewSystem()
        {
            var system = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(system, guard);
            return system;
        }

        RoomEnemyProjectileSlot Spawn(
            RoomEnemySystem system,
            RoomEnemyProjectileKind kind)
        {
            system.ApplyDownwardGateProjectileRequest(
                new DownwardGateProjectileRequest(
                    DownwardGateProjectileOperation.Spawn,
                    (ushort)kind,
                    42),
                roomWidthInBlocks: 16);
            return system.EnemyProjectiles.Single(projectile => projectile.Kind == kind);
        }

        void Process(RoomEnemySystem system, RoomEnemyProjectileSlot projectile)
        {
            process.Invoke(system, [projectile, new SamusState(), (ushort)0, (ushort)0]);
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
        }

        void RunMovementFrames(
            RoomEnemySystem system,
            RoomEnemyProjectileSlot projectile,
            int count)
        {
            for (int frame = 0; frame < count; frame++)
            {
                move.Invoke(null, [projectile]);
                if (projectile.IsActive)
                    Process(system, projectile);
            }
        }
    }

    /// <summary>Repeatedly resolves compiled mechanics words and returns a checksum to keep the lookups observable.</summary>
    /// <returns>Checksum accumulated from the moving and closed program entry words.</returns>
    private static int ProbeDownwardGateProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DownwardGateProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DownwardGateProjectileInstructionProgramDefinitions.Moving
                    : DownwardGateProjectileInstructionProgramDefinitions.Closed);
        }
        return checksum;
    }

    /// <summary>Tracks presentation-operand reads and rejects live reads of compiled projectile mechanics.</summary>
    /// <param name="source">Underlying address space used for reads outside compiled mechanics.</param>
    private sealed class DownwardGateProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation-word addresses whose bytes production requested from the cartridge.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempted cartridge reads from compiled mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Forwards a cartridge read through the mechanics guard and presentation-word tracker.</summary>
        /// <param name="address">Cartridge address requested by production code.</param>
        /// <returns>The source byte when the address is not compiled mechanics data.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled mechanics.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, records presentation operands, and forwards other reads.</summary>
        /// <param name="address">Bus address requested by production code.</param>
        /// <returns>The source byte when the address is not compiled mechanics data.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled mechanics.</exception>
        public byte ReadByte(int address)
        {
            if (DownwardGateProjectileInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled downward-gate mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < DownwardGateProjectileInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = DownwardGateProjectileInstructionProgramDefinitions
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

        /// <summary>Forwards writes directly to the underlying address space.</summary>
        /// <param name="address">Bus address receiving the write.</param>
        /// <param name="value">Byte value to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
