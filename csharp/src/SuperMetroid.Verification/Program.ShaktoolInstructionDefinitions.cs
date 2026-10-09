using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies Shaktool's orientation and collision selectors against native words and production handoffs.</summary>
    /// <param name="rom">Cartridge address space used to read the native selector tables.</param>
    private static void VerifyShaktoolInstructionDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        ushort Word(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        for (ushort bucket = 0; bucket <= 0x00e0; bucket += 0x0020)
        {
            AssertEqual(Word(0xaadd15 + (bucket >> 5) * 2),
                ShaktoolInstructionDefinitions.ForOrientationBucket(bucket),
                $"Shaktool orientation list {bucket:X2}");
        }
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(Word(0xaadf13 + index * 2),
                ShaktoolInstructionDefinitions.CollisionForSegment(index),
                $"Shaktool collision list {index}");
        }

        RoomEnemySystem enemies = CreateCompiledShaktoolGroup(rom, flags);
        RoomEnemySlot center = enemies.Slots[3];
        ShaktoolSegmentState centerState = enemies.ShaktoolSegments[3]!;
        ShaktoolSegmentState nextState = enemies.ShaktoolSegments[4]!;
        var orient = typeof(RoomEnemySystem).GetMethod(
                "AdvanceAndOrientShaktoolCenter", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ShaktoolSegmentState>>(enemies);
        for (ushort bucket = 0; bucket <= 0x00e0; bucket += 0x0020)
        {
            ushort midpoint = unchecked((ushort)(bucket << 8));
            center.Parameter1 = 0;
            centerState.OrbitAngle = unchecked((ushort)(midpoint ^ 0x8000));
            centerState.AngularVelocity = 0;
            centerState.OrientationAndAcceleration = 0;
            nextState.OrbitAngle = midpoint;
            orient(center, centerState);
            AssertEqual(bucket, unchecked((ushort)(
                    centerState.OrientationAndAcceleration & 0x00ff)),
                $"Shaktool production orientation bucket {bucket:X2}");
            AssertEqual(ShaktoolInstructionDefinitions.ForOrientationBucket(bucket),
                center.CurrentInstruction,
                $"Shaktool production orientation list {bucket:X2}");
        }

        RoomEnemySlot tail = enemies.Slots[6];
        ShaktoolSegmentState tailState = enemies.ShaktoolSegments[6]!;
        var reverse = typeof(RoomEnemySystem).GetMethod(
                "ReverseShaktoolAfterCollision", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ShaktoolSegmentState, ushort, ushort>>(
                enemies);
        reverse(tail, tailState, tail.XPosition, tail.YPosition);
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(ShaktoolInstructionDefinitions.CollisionForSegment(index),
                enemies.Slots[index].CurrentInstruction,
                $"Shaktool production collision list {index}");
        }

        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionDefinitions.ForOrientationBucket(1),
            "unaligned Shaktool orientation bucket");
        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionDefinitions.ForOrientationBucket(0x0100),
            "Shaktool orientation bucket past table");
        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionDefinitions.CollisionForSegment(-1),
            "negative Shaktool collision segment");
        Console.WriteLine(
            "Shaktool instruction definitions: 22 native selectors and all orientation and collision-reversal production handoffs pass with source tables forbidden.");
    }

    /// <summary>Loads the retail cartridge and runs the Shaktool instruction-program verification.</summary>
    private static void VerifyShaktoolInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyShaktoolInstructionProgramDefinitions), () => VerifyShaktoolInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled Shaktool mechanics words and executes the reachable instruction programs with reads guarded.</summary>
    /// <param name="rom">Retail cartridge address space used as the native instruction reference.</param>
    private static void VerifyShaktoolInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int index = 0;
             index < ShaktoolInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ShaktoolInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadShaktoolInstructionWord(rom, 0xaa0000 | definition.Address),
                $"Shaktool instruction mechanics word $AA:{definition.Address:X4}");
        }

        var guard = new ShaktoolInstructionReadGuard(rom);
        RoomEnemySystem enemies = CreateCompiledShaktoolGroup(guard, flags);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;

        ushort[] steadyPrograms =
        [
            ShaktoolInstructionProgramDefinitions.SawHandPrimaryPiece,
            ShaktoolInstructionProgramDefinitions.SawHandFinalPiece,
            ShaktoolInstructionProgramDefinitions.ArmPieceNormal,
            ShaktoolInstructionProgramDefinitions.HeadAimingLeft,
            ShaktoolInstructionProgramDefinitions.HeadAimingUpLeft,
            ShaktoolInstructionProgramDefinitions.HeadAimingUp,
            ShaktoolInstructionProgramDefinitions.HeadAimingUpRight,
            ShaktoolInstructionProgramDefinitions.HeadAimingRight,
            ShaktoolInstructionProgramDefinitions.HeadAimingDownRight,
            ShaktoolInstructionProgramDefinitions.HeadAimingDown,
            ShaktoolInstructionProgramDefinitions.HeadAimingDownLeft,
        ];
        for (int index = 0; index < steadyPrograms.Length; index++)
        {
            RoomEnemySlot segment = enemies.Slots[Math.Min(index, 6)];
            segment.CurrentInstruction = steadyPrograms[index];
            RunForcedShaktoolInstructions(process, enemies, segment, 5);
        }

        RoomEnemySlot tail = enemies.Slots[6];
        ShaktoolSegmentState tailState = enemies.ShaktoolSegments[6]!;
        var reverse = typeof(RoomEnemySystem).GetMethod(
                "ReverseShaktoolAfterCollision", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ShaktoolSegmentState, ushort, ushort>>(
                enemies);
        reverse(tail, tailState, tail.XPosition, tail.YPosition);
        for (int index = 0; index < 7; index++)
            RunForcedShaktoolInstructions(process, enemies, enemies.Slots[index], 8);

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Shaktool execution uses compiled spritemap selectors");
        for (int index = 0;
             index < ShaktoolInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                ShaktoolInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, RoomEnemySystem.ShaktoolDefinition,
                0xaa, address, $"Shaktool $AA:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => ShaktoolInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Shaktool spritemap $AA:{address:X4} is rejected as mechanics");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Shaktool mechanics byte");
        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionProgramDefinitions.ReadMechanicsWord(
                ShaktoolInstructionProgramDefinitions.FirstAdjacentCodeRoutine),
            "adjacent Shaktool code routine is rejected as instruction mechanics");

        _ = ProbeShaktoolInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeShaktoolInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Shaktool allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed Shaktool mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Shaktool instruction mechanics: 110 compiled words, all 21 reachable " +
            "programs and 15 compiled spritemap selectors pass with mechanics bytes forbidden.");
    }

    /// <summary>Runs a fixed number of reflected instruction-handler steps with the actor timer primed each time.</summary>
    /// <param name="process">Room instruction dispatcher to invoke.</param>
    /// <param name="enemies">Owning enemy system containing the Shaktool segment.</param>
    /// <param name="segment">Segment whose instruction list and position are advanced.</param>
    /// <param name="steps">Number of handler calls to perform.</param>
    /// <returns>True if the segment's position changed during any step.</returns>
    private static bool RunForcedShaktoolInstructions(
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot segment,
        int steps)
    {
        bool moved = false;
        object?[] arguments =
            [segment, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int step = 0; step < steps; step++)
        {
            (ushort X, ushort Y) before = (segment.XPosition, segment.YPosition);
            segment.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
            moved |= segment.XPosition != before.X || segment.YPosition != before.Y;
        }
        return moved;
    }

    /// <summary>Repeats a compiled mechanics lookup to provide input for the warmed allocation measurement.</summary>
    /// <returns>A checksum of the repeated instruction-word values.</returns>
    private static int ProbeShaktoolInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ShaktoolInstructionProgramDefinitions.ReadMechanicsWord(
                ShaktoolInstructionProgramDefinitions.HeadAimingDown);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian instruction word from a cartridge address space.</summary>
    /// <param name="bus">Cartridge address space containing the word bytes.</param>
    /// <param name="address">Address of the low byte.</param>
    /// <returns>The combined 16-bit instruction word.</returns>
    private static ushort ReadShaktoolInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Initializes the seven linked Shaktool enemy slots against the supplied source-reading guard.</summary>
    /// <param name="rom">Address space installed for the enemy system's instruction reads.</param>
    /// <param name="flags">Reflection binding flags used to access private initialization members.</param>
    /// <returns>The enemy system containing the initialized Shaktool group.</returns>
    private static RoomEnemySystem CreateCompiledShaktoolGroup(
        ISnesAddressSpace rom,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies, new ShaktoolInstructionReadGuard(rom));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeShaktool", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        for (int index = 0; index < 7; index++)
        {
            RoomEnemySlot slot = enemies.Slots[index];
            slot.EnemyDefinitionPointer = RoomEnemySystem.ShaktoolDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xaa };
            slot.Parameter2 = unchecked((ushort)(index * 2));
            slot.XPosition = unchecked((ushort)(0x0100 + index * 16));
            slot.YPosition = 0x0200;
            initialize(slot);
        }
        return enemies;
    }

    /// <summary>Rejects runtime reads of compiled Shaktool mechanics and records presentation-word accesses.</summary>
    /// <param name="source">Underlying address space used for permitted reads and writes.</param>
    private sealed class ShaktoolInstructionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation word addresses observed during runtime instruction execution.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of runtime reads rejected for targeting compiled mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge reads through the Shaktool source guard.</summary>
        /// <param name="address">Cartridge address requested by production code.</param>
        /// <returns>The underlying byte when the request is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects migrated mechanics/selector reads, tracks presentation words, and forwards other reads.</summary>
        /// <param name="address">CPU bus address requested by production code.</param>
        /// <returns>The underlying byte when the request is permitted.</returns>
        public byte ReadByte(int address)
        {
            if (ShaktoolInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Shaktool mechanics byte ${address:X6}.");
            }
            if (address is >= 0xaadd15 and < 0xaadd25 or >= 0xaadf13 and < 0xaadf2f)
            {
                throw new InvalidOperationException(
                    $"Shaktool attempted migrated instruction-selector read ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xaa0000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ShaktoolInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        ShaktoolInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards a write to the underlying address space.</summary>
        /// <param name="address">CPU address receiving the byte.</param>
        /// <param name="value">Byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
