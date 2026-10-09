using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Checks the compiled Stoke instruction tables against the retail ROM and exercises
    /// production walking and attack behavior while rejecting reads of compiled table bytes.
    /// </summary>
    private static void VerifyStokeInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyStokeInstructionProgramDefinitions), () => VerifyStokeInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Validates Stoke instruction mechanics and visual selectors against <paramref name="rom"/>,
    /// then runs the compiled programs through the production enemy instruction processor.
    /// </summary>
    /// <param name="rom">Retail address space used to compare extracted instruction words.</param>
    private static void VerifyStokeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int index = 0;
             index < StokeInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                StokeInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadStokeInstructionWord(rom, 0xa20000 | definition.Address),
                $"Stoke instruction mechanics word $A2:{definition.Address:X4}");
        }

        for (int index = 0;
             index < StokeInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = StokeInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(ReadStokeInstructionWord(rom, 0xa20000 | address),
                OwtchStokeVisualDefinitions.FrameAt(RoomEnemySystem.StokeDefinition, address),
                $"compiled Stoke frame selector $A2:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => OwtchStokeVisualDefinitions.FrameAt(RoomEnemySystem.StokeDefinition, 0x8934),
            "Stoke timing word is not a visual selector");

        var guard = new StokeInstructionProgramReadGuard(rom);
        HashSet<ushort> observedFrames = [];
        Suite(nameof(VerifyWalking), () => VerifyWalking(
            StokeInstructionProgramDefinitions.MovingLeft,
            StokeDirection.Left,
            StokeAiFunction.MovingLeft,
            0x8938,
            "left"));
        Suite(nameof(VerifyWalking), () => VerifyWalking(
            StokeInstructionProgramDefinitions.MovingRight,
            StokeDirection.Right,
            StokeAiFunction.MovingRight,
            0x895e,
            "right"));
        Suite(nameof(VerifyAttack), () => VerifyAttack(
            StokeInstructionProgramDefinitions.AttackingLeft,
            StokeDirection.Left,
            StokeAiFunction.MovingLeft,
            0x8938,
            expectedProjectileDirection: 0,
            direction: "left"));
        Suite(nameof(VerifyAttack), () => VerifyAttack(
            StokeInstructionProgramDefinitions.AttackingRight,
            StokeDirection.Right,
            StokeAiFunction.MovingRight,
            0x895e,
            expectedProjectileDirection: 1,
            direction: "right"));

        AssertEqual(10, observedFrames.Count, "all Stoke walking/attack frames execute");
        foreach (ushort frame in new ushort[]
                 { 0x8aca, 0x8ad6, 0x8ae7, 0x8af3, 0x8aff,
                   0x8b15, 0x8b21, 0x8b32, 0x8b3e, 0x8b4a })
            AssertTrue(observedFrames.Contains(frame), $"Stoke live frame ${frame:X4}");

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Stoke mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => StokeInstructionProgramDefinitions.ReadMechanicsWord(0x8936),
            "interleaved Stoke spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => StokeInstructionProgramDefinitions.ReadMechanicsWord(0x897e),
            "adjacent Stoke callback code is rejected as mechanics");

        _ = ProbeStokeInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeStokeInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Stoke allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Stoke mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Stoke instruction mechanics: twenty-six compiled words, both walking " +
            "loops, both attacks and real directional projectile spawns, and twelve " +
            "compiled visual selectors pass with source bytes forbidden.");

        void VerifyWalking(
            ushort program,
            StokeDirection expectedDirection,
            StokeAiFunction expectedFunction,
            ushort expectedLoopCursor,
            string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, StokeEnemyState state) =
                CreateStoke(expectedDirection);
            Install(slot, program);
            RunStokeProgram(enemies, slot, 41);
            AssertEqual(expectedDirection, state.Direction,
                $"Stoke {direction} walking callback direction");
            AssertEqual(expectedFunction, state.Function,
                $"Stoke {direction} walking callback function");
            AssertEqual(expectedLoopCursor, slot.CurrentInstruction,
                $"Stoke {direction} walking program returns through native goto");
        }

        void VerifyAttack(
            ushort program,
            StokeDirection expectedDirection,
            StokeAiFunction expectedFunction,
            ushort expectedLoopCursor,
            ushort expectedProjectileDirection,
            string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, StokeEnemyState state) =
                CreateStoke(expectedDirection);
            state.Function = StokeAiFunction.IdleDuringAttack;
            Install(slot, program);
            RunStokeProgram(enemies, slot, 33);

            AssertEqual(expectedDirection, state.Direction,
                $"Stoke {direction} attack restores direction");
            AssertEqual(expectedFunction, state.Function,
                $"Stoke {direction} attack restores walking function");
            AssertEqual(expectedLoopCursor, slot.CurrentInstruction,
                $"Stoke {direction} attack enters walking loop");
            RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles.Single(
                candidate => candidate.Kind == RoomEnemyProjectileKind.StokeProjectile);
            AssertEqual(expectedProjectileDirection, projectile.DirectionParameter,
                $"Stoke {direction} attack projectile direction");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot, StokeEnemyState State) CreateStoke(
            StokeDirection direction)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeStoke", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.StokeDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = (ushort)direction;
            slot.XPosition = 0x0080;
            slot.YPosition = 0x0080;
            initialize(slot);
            return (enemies, slot, enemies.StokeStates[0]!);
        }

        static void Install(RoomEnemySlot slot, ushort program)
        {
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            slot.Timer = 0;
        }

        void RunStokeProgram(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
            {
                process.Invoke(enemies, arguments);
                if (slot.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame))
                    observedFrames.Add(slot.SpritemapPointer);
            }
        }
    }

    /// <summary>
    /// Warms and repeatedly reads a compiled Stoke mechanics word so the caller can measure
    /// whether steady-state table access allocates memory.
    /// </summary>
    private static int ProbeStokeInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += StokeInstructionProgramDefinitions.ReadMechanicsWord(
                StokeInstructionProgramDefinitions.MovingLeft);
        }
        return checksum;
    }

    /// <summary>
    /// Reads one little-endian 16-bit instruction word from the supplied bus.
    /// </summary>
    /// <param name="bus">Address space containing the instruction bytes.</param>
    /// <param name="address">Address of the low byte; the next address supplies the high byte.</param>
    /// <returns>The two bytes combined with the low byte in the least significant position.</returns>
    private static ushort ReadStokeInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>
    /// Wraps the imported address space and fails if production execution tries to fetch
    /// compiled Stoke mechanics or presentation bytes from source memory.
    /// </summary>
    /// <param name="source">Underlying address space for reads and writes outside the guarded ranges.</param>
    private sealed class StokeInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the number of attempted reads from compiled Stoke table bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge-source read through the guarded address-space read path.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        /// <returns>The byte from the wrapped source, unless the address is forbidden.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Reads from the wrapped bus, rejecting addresses occupied by compiled Stoke
        /// mechanics or presentation data and counting each rejected attempt.
        /// </summary>
        /// <param name="address">Address requested by production code.</param>
        /// <returns>The wrapped source byte when the address is outside guarded tables.</returns>
        public byte ReadByte(int address)
        {
            if (StokeInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                IsPresentationByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Stoke instruction byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>
        /// Determines whether an address is either byte of a compiled Stoke presentation word.
        /// </summary>
        /// <param name="address">Address to compare with presentation-word locations.</param>
        /// <returns><see langword="true"/> when the address falls within a listed word in bank $A2.</returns>
        private static bool IsPresentationByte(int address)
        {
            if ((address & 0xff0000) != 0xa20000)
                return false;
            ushort bankAddress = unchecked((ushort)address);
            for (int index = 0;
                 index < StokeInstructionProgramDefinitionsTooling.PresentationWordCount;
                 index++)
            {
                ushort presentation =
                    StokeInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                if (bankAddress == presentation ||
                    bankAddress == unchecked((ushort)(presentation + 1)))
                    return true;
            }
            return false;
        }

        /// <summary>Forwards writes unchanged to the wrapped address space.</summary>
        /// <param name="address">Destination address.</param>
        /// <param name="value">Byte to store at the destination.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
