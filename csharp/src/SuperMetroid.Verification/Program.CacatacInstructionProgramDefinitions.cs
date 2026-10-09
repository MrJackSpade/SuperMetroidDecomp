using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    /// <summary>Registers the retail-ROM verification suite for compiled Cacatac instruction mechanics.</summary>
    private static void VerifyCacatacInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyCacatacInstructionProgramDefinitions), () => VerifyCacatacInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled instruction words and visual selectors, then executes the upright and inverted idle and attack paths through production systems with guarded reads.</summary>
    /// <param name="rom">Retail address space supplying native instruction words and visual operands for comparison.</param>
    private static void VerifyCacatacInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < CacatacInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                CacatacInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadCacatacInstructionWord(rom, 0xa20000 | definition.Address),
                $"Cacatac instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new CacatacInstructionProgramReadGuard(rom);
        Suite(nameof(VerifyIdle), () => VerifyIdle(upsideUp: true, CacatacInstructionProgramDefinitions.UpsideUpIdle));
        Suite(nameof(VerifyIdle), () => VerifyIdle(upsideUp: false, CacatacInstructionProgramDefinitions.UpsideDownIdle));
        Suite(nameof(VerifyAttack), () => VerifyAttack(
            upsideUp: true,
            CacatacInstructionProgramDefinitions.UpsideUpAttack,
            [
                CacatacSpikeDirection.LeftFacingUp,
                CacatacSpikeDirection.UpLeft,
                CacatacSpikeDirection.Up,
                CacatacSpikeDirection.UpRight,
                CacatacSpikeDirection.RightFacingUp,
            ]));
        Suite(nameof(VerifyAttack), () => VerifyAttack(
            upsideUp: false,
            CacatacInstructionProgramDefinitions.UpsideDownAttack,
            [
                CacatacSpikeDirection.LeftFacingDown,
                CacatacSpikeDirection.DownLeft,
                CacatacSpikeDirection.Down,
                CacatacSpikeDirection.DownRight,
                CacatacSpikeDirection.RightFacingDown,
            ]));

        for (int index = 0;
             index < CacatacInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = CacatacInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(ReadCacatacInstructionWord(rom, 0xa20000 | address),
                EnemySpritemapDefinitions.CacatacFrameAt(address),
                $"compiled Cacatac visual selector $A2:{address:X4} matches cartridge");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Cacatac mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => CacatacInstructionProgramDefinitions.ReadMechanicsWord(0x9e8e),
            "Cacatac spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CacatacInstructionProgramDefinitions.ReadMechanicsWord(0x9f2a),
            "adjacent Cacatac callback code is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.CacatacFrameAt(0x9f30),
            "uncompiled Cacatac visual selector is rejected loudly");

        _ = ProbeCacatacInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCacatacInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Cacatac allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Cacatac mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Cacatac instruction mechanics: fifty-six compiled words, four complete " +
            "idle/attack programs, ten real spike spawns, and twenty-four compiled visual " +
            "selectors pass with source bytes forbidden.");

        void VerifyIdle(bool upsideUp, ushort program)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, CacatacEnemyState state) =
                CreateSystem(upsideUp);
            RunFrames(enemies, slot, 65);
            AssertEqual(unchecked((ushort)(program + 6)), slot.CurrentInstruction,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} idle loops");
            AssertEqual(CacatacEnemyFunction.MovingLeft, state.Function,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} idle restores patrol");
        }

        void VerifyAttack(
            bool upsideUp,
            ushort program,
            CacatacSpikeDirection[] expectedDirections)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, CacatacEnemyState state) =
                CreateSystem(upsideUp);
            state.Function = CacatacEnemyFunction.Stopped;
            typeof(RoomEnemySystem).GetMethod("SetCacatacInstructionList", flags)!
                .Invoke(null, [slot, program]);
            RunFrames(enemies, slot, 53);

            ushort idle = upsideUp
                ? CacatacInstructionProgramDefinitions.UpsideUpIdle
                : CacatacInstructionProgramDefinitions.UpsideDownIdle;
            AssertEqual(unchecked((ushort)(idle + 6)), slot.CurrentInstruction,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} attack returns to idle");
            AssertEqual(CacatacEnemyFunction.MovingLeft, state.Function,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} attack resumes patrol");
            AssertEqual((ushort)0x0034, enemies.LastCacatacSoundEffect!.Value,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} attack queues sound");

            ushort[] actualDirections = enemies.EnemyProjectiles
                .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike)
                .Select(projectile => projectile.Variable0)
                .Order()
                .ToArray();
            ushort[] expected = expectedDirections.Select(value => (ushort)value).Order().ToArray();
            AssertEqual(expected.Length, actualDirections.Length,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} spike count");
            for (int index = 0; index < expected.Length; index++)
            {
                AssertEqual(expected[index], actualDirections[index],
                    $"Cacatac {(upsideUp ? "upright" : "inverted")} spike {index}");
            }
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot, CacatacEnemyState State) CreateSystem(
            bool upsideUp)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeCacatac", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.CacatacDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = upsideUp ? (ushort)0x0100 : (ushort)0;
            initialize(slot);
            return (enemies, slot, enemies.CacatacStates[0]!);
        }

        static void RunFrames(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    /// <summary>Warms and repeatedly reads a compiled idle-program word so the caller can measure steady-state lookup allocations.</summary>
    /// <returns>A checksum that keeps the repeated mechanics reads observable.</returns>
    private static int ProbeCacatacInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CacatacInstructionProgramDefinitions.ReadMechanicsWord(
                CacatacInstructionProgramDefinitions.UpsideUpIdle);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian instruction word from the supplied cartridge address space.</summary>
    /// <param name="bus">Retail address space containing the native instruction program.</param>
    /// <param name="address">Address of the low byte; the high byte is read from the following address.</param>
    /// <returns>The two bytes combined as an unsigned 16-bit word.</returns>
    private static ushort ReadCacatacInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Wraps cartridge access to reject reads of compiled Cacatac mechanics and presentation bytes during production execution.</summary>
    /// <param name="source">Underlying address space for reads outside the compiled instruction and artwork ranges, and for writes.</param>
    private sealed class CacatacInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads rejected for targeting compiled mechanics or presentation data.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge-import reads through the same compiled-byte guard as ordinary reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the address is outside compiled instruction and artwork ranges.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled mechanics or presentation bytes and forwards all other addresses.</summary>
        /// <param name="address">Address-space location requested by production code.</param>
        /// <returns>The underlying byte when the address is not compiled Cacatac data.</returns>
        /// <exception cref="InvalidOperationException">The address is owned by compiled Cacatac mechanics or presentation data.</exception>
        public byte ReadByte(int address)
        {
            if (CacatacInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                IsCompiledPresentationByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Cacatac instruction ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        /// <summary>Checks whether a byte address falls within either byte of a compiled spritemap-selector word in bank $A2.</summary>
        /// <param name="address">Full address-space byte address requested by the consumer.</param>
        /// <returns>True when the address matches a compiled presentation operand.</returns>
        private static bool IsCompiledPresentationByte(int address)
        {
            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < CacatacInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        CacatacInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>Forwards writes unchanged; the guard restricts reads from compiled data only.</summary>
        /// <param name="address">Address-space location to write.</param>
        /// <param name="value">Byte passed to the underlying address space.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
