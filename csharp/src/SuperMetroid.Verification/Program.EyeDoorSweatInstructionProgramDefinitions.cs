using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Loads the retail ROM and runs the Eye Door sweat instruction-program verification against its cartridge data.
    /// </summary>
    private static void VerifyEyeDoorSweatInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyEyeDoorSweatInstructionProgramDefinitions), () => VerifyEyeDoorSweatInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Checks compiled mechanics against ROM words and exercises falling, floor-impact, shared-delete, and installed-visual behavior.
    /// </summary>
    /// <param name="rom">The retail cartridge address space used to verify the original instruction words and frame durations.</param>
    private static void VerifyEyeDoorSweatInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < EyeDoorSweatInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                EyeDoorSweatInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadEyeDoorSweatInstructionWord(rom, definition.Address),
                $"Eye Door sweat mechanics word $86:{definition.Address:X4}");
        }

        var guard = new EyeDoorSweatInstructionReadGuard(rom);
        var observedOperands = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            instanceFlags)!;
        var runPreInstruction = typeof(RoomEnemySystem).GetMethod(
                "RunEyeDoorSweatPreInstruction",
                BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemyProjectileSlot, RoomLevelData>>();

        const int roomWidth = 16;
        var foreground = new ushort[roomWidth * 16];
        foreground[5 * roomWidth + 6] =
            (ushort)((int)RoomCollisionType.SolidBlock << 12);
        RoomLevelData level = CreateRoom(
            roomWidth,
            16,
            foreground,
            new byte[foreground.Length],
            blockDefinitions: new byte[0x400 * 8]);
        var samus = new SamusState();
        var system = new Bank80SystemState();
        var request = new EyeDoorProjectileRequest(
            EyeDoorEnemyProjectileRomData.SweatDefinition,
            Parameter: 4,
            PlmBlockIndex: 4 * roomWidth + 7,
            DoorBit: 0);

        enemies.SpawnEyeDoorProjectile(request, roomWidth, system);
        RoomEnemyProjectileSlot sweat = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSweat);
        AssertEqual(
            EyeDoorSweatInstructionProgramDefinitions.Initial,
            sweat.InstructionPointer,
            "Eye Door sweat definition selects the named initial program");

        RunForcedTicks(sweat, 2);
        AssertEqual(
            EyeDoorSweatInstructionProgramDefinitions.Initial + 4,
            sweat.InstructionPointer,
            "Eye Door sweat falling loop returns and schedules its frame again");

        runPreInstruction(sweat, level);
        AssertEqual(
            EyeDoorSweatInstructionProgramDefinitions.Impact,
            sweat.InstructionPointer,
            "real floor collision selects the named Eye Door sweat impact program");
        AssertEqual(EyeDoorEnemyProjectileRomData.SmokeInertPreInstruction,
            sweat.PreInstruction,
            "Eye Door sweat impact disables movement with the native inert pre-instruction");
        AssertEqual((ushort)1, sweat.InstructionTimer,
            "Eye Door sweat impact requests a same-pass animation tick");
        AssertEqual((ushort)76, sweat.YPosition,
            "Eye Door sweat applies the native four-pixel floor-impact correction");

        RunForcedTicks(sweat, 4);
        AssertTrue(!sweat.IsActive,
            "Eye Door sweat impact program deletes after all three presentation frames");

        enemies.SpawnEyeDoorProjectile(request, roomWidth, system);
        RoomEnemyProjectileSlot shot = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSweat &&
                projectile.IsActive);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTicks(shot, 1);
        AssertTrue(!shot.IsActive,
            "Eye Door sweat shot reaction reaches the compiled shared delete program");

        AssertEqual(
            EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount,
            observedOperands.Count,
            "all live Eye Door sweat spritemap operands are selected from installed artwork");
        for (int index = 0;
             index < EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                EyeDoorSweatInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(observedOperands.Contains(address),
                $"production execution selects Eye Door sweat presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "projectile visuals do not read cartridge bytes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Eye Door sweat and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => EyeDoorSweatInstructionProgramDefinitions.ReadMechanicsWord(0xb617),
            "Eye Door sweat spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EyeDoorSweatInstructionProgramDefinitions.ReadMechanicsWord(0xb62d),
            "adjacent Eye Door origin table is rejected as sweat mechanics");

        _ = ProbeEyeDoorSweatInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEyeDoorSweatInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Eye Door sweat allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Eye Door sweat mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Eye Door sweat instruction mechanics: eight compiled words, the complete " +
            "falling loop, real floor-impact handoff, three impact frames, shared shot " +
            "deletion, and four installed visual operands pass with mechanics bytes forbidden.");

        void ObserveFrame(RoomEnemyProjectileSlot projectile)
        {
            if (!projectile.IsActive || projectile.InstructionTimer == 0) return;
            ushort operand = unchecked((ushort)(projectile.InstructionPointer - 2));
            AssertEqual(operand, projectile.PresentationOperandAddress,
                "timed projectile frame retains its installed visual operand");
            AssertEqual(ReadEyeDoorSweatInstructionWord(rom, unchecked((ushort)(operand - 2))),
                projectile.InstructionTimer, "projectile frame duration matches cartridge data");
            observedOperands.Add(projectile.PresentationOperandAddress);
        }
        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, samus, (ushort)0, (ushort)0]);
                ObserveFrame(projectile);
            }
        }
    }

    /// <summary>
    /// Warms repeated lookups of the compiled falling and impact mechanics words for the allocation measurement.
    /// </summary>
    /// <returns>A non-zero checksum that consumes the lookup results.</returns>
    private static int ProbeEyeDoorSweatInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EyeDoorSweatInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? EyeDoorSweatInstructionProgramDefinitions.Initial
                    : EyeDoorSweatInstructionProgramDefinitions.Impact);
        }
        return checksum;
    }

    /// <summary>
    /// Reads adjacent bytes from the enemy-projectile bank and combines them in cartridge little-endian order.
    /// </summary>
    /// <param name="source">The address space supplying the reference instruction bytes.</param>
    /// <param name="address">The bank-relative address of the word's low byte.</param>
    /// <returns>The unsigned 16-bit value stored at that address.</returns>
    private static ushort ReadEyeDoorSweatInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    /// <summary>
    /// Guards compiled Eye Door sweat and shared-delete mechanics while tracking any cartridge reads of installed presentation operands.
    /// </summary>
    /// <param name="source">The underlying address space used for reads and writes permitted by the audit.</param>
    private sealed class EyeDoorSweatInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>
        /// Gets the presentation-word addresses observed while the production projectile code is running.
        /// </summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>
        /// Gets the number of attempts to read compiled Eye Door sweat or shared-delete mechanics bytes.
        /// </summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>
        /// Routes an importer read through the Eye Door sweat mechanics guard.
        /// </summary>
        /// <param name="address">The cartridge address requested by the importer.</param>
        /// <returns>The byte at the address when it is outside both guarded mechanics ranges.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Rejects reads of compiled mechanics data, tracks presentation-operand reads, and forwards permitted addresses.
        /// </summary>
        /// <param name="address">The bus address to inspect and read.</param>
        /// <returns>The byte returned by the wrapped address space for a permitted read.</returns>
        public byte ReadByte(int address)
        {
            if (EyeDoorSweatInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Eye Door sweat mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = EyeDoorSweatInstructionProgramDefinitions
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

        /// <summary>
        /// Forwards a write to the wrapped address space without changing its address or value.
        /// </summary>
        /// <param name="address">The destination bus address.</param>
        /// <param name="value">The byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
