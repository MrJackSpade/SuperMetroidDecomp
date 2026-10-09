using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Proves that every production Ridley mechanics word matches the pinned cartridge and
    /// that both facing paths execute while those cartridge bytes are inaccessible.
    /// </summary>
    private static void VerifyRidleyInstructionProgramDefinitions()
    {
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        ushort ceresRidleyDefinition = (ushort)typeof(RoomEnemySystem)
            .GetField("CeresRidleyDefinition", staticFlags)!.GetRawConstantValue()!;
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0;
             index < RidleyInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                RidleyInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                ReadRidleyWord(rom, 0xa60000 | definition.Address),
                definition.Value,
                $"Ridley mechanics word $A6:{definition.Address:X4}");
        }

        var executedOperands = new HashSet<ushort>();
        var guard = new RidleyInstructionReadGuard(rom);
        ushort[] programs =
        [
            RidleyInstructionProgramDefinitions.Initial,
            RidleyInstructionProgramDefinitions.CeresLunge,
            RidleyInstructionProgramDefinitions.RetrieveBabyMetroid,
            RidleyInstructionProgramDefinitions.OpeningRoar,
            RidleyInstructionProgramDefinitions.DeathRoar,
            RidleyInstructionProgramDefinitions.TurnFromLeftToRight,
            RidleyInstructionProgramDefinitions.TurnFromRightToLeft,
            RidleyInstructionProgramDefinitions.Fireballing,
        ];
        foreach (ushort program in programs)
        {
            RunProgram(rom, executedOperands, guard, program, facingDirection: 0, ceresRidleyDefinition);
            RunProgram(rom, executedOperands, guard, program, facingDirection: 2, ceresRidleyDefinition);
        }

        RunProgram(rom, executedOperands, guard,
            RidleyInstructionProgramDefinitions.TransitionToFlying,
            facingDirection: 0,
            ceresRidleyDefinition);
        RunProgram(rom, executedOperands, guard,
            RidleyInstructionProgramDefinitions.TransitionToFlying,
            facingDirection: 2,
            RoomEnemySystem.NorfairRidleyDefinition);

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Ridley execution avoids compiled mechanics bytes");
        AssertEqual(0,
            guard.ObservedPresentationWords.Count,
            "Ridley extended-spritemap selection performs zero live cartridge reads");
        AssertEqual(RidleyInstructionProgramDefinitionsTooling.PresentationWordCount, executedOperands.Count,
            "Ridley programs execute every native visual operand");
        for (int index = 0;
             index < RidleyInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                RidleyInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution selects Ridley presentation $A6:{address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa6, address, out ushort selector),
                "Ridley presentation has a compiled selector");
            AssertEqual(ReadRidleyWord(rom, 0xa60000 | address), selector,
                "Ridley compiled selector matches the cartridge");
        }

        AssertThrows<InvalidDataException>(
            () => RidleyInstructionProgramDefinitions.ReadMechanicsWord(0xe53e),
            "Ridley extended-spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => RidleyInstructionProgramDefinitions.ReadMechanicsWord(0xe828),
            "unused Ridley projectile code is outside the compiled program family");

        _ = ProbeRidleyInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeRidleyInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Ridley allocation probe consumes live mechanics data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Ridley mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"Ridley instruction mechanics: " +
            $"{RidleyInstructionProgramDefinitionsTooling.MechanicsWordCount} compiled words, " +
            "nine production entry programs, both facing paths, and " +
            $"{RidleyInstructionProgramDefinitionsTooling.PresentationWordCount} native " +
            "extended-sprite selections pass with zero live operand reads.");
    }

    /// <summary>Runs one Ridley instruction program to its terminal sleep and checks every selected visual operand.</summary>
    /// <param name="rom">The retail address space used to verify executed sprite selectors.</param>
    /// <param name="executedOperands">Set collecting the native visual words reached by the program.</param>
    /// <param name="guard">Address space that rejects runtime mechanics reads and records presentation accesses.</param>
    /// <param name="program">Bank-$A6 entry address of the instruction sequence to execute.</param>
    /// <param name="facingDirection">Ridley's facing value for this execution path.</param>
    /// <param name="enemyDefinition">Enemy definition pointer installed before the program runs.</param>
    /// <exception cref="InvalidDataException">The program fails to reach a compiled sleep command within the frame limit.</exception>
    private static void RunProgram(
        SuperMetroidAddressSpace rom,
        HashSet<ushort> executedOperands,
        RidleyInstructionReadGuard guard,
        ushort program,
        ushort facingDirection,
        ushort enemyDefinition)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem
        {
            TileArtwork = RepositoryInstallation.EnemyTiles,
            CeresRidleyColors = RetailPresentationFixture().CeresRidleyColors,
        };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());

        RoomEnemySlot slot = enemies.Slots[0];
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        slot.EnemyDefinitionPointer = (ushort)typeof(RoomEnemySystem)
            .GetField("CeresRidleyDefinition", staticFlags)!.GetRawConstantValue()!;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        typeof(RoomEnemySystem).GetMethod("InitializeCeresRidley", flags)!
            .Invoke(enemies, [slot]);

        slot.EnemyDefinitionPointer = enemyDefinition;
        slot.CurrentInstruction = program;
        slot.InstructionTimer = 1;
        enemies.Ridley!.FacingDirection = facingDirection;
        var samus = new SamusState { Health = 99 };
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        object?[] arguments =
            [slot, samus, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

        for (int frame = 0; frame < 1000; frame++)
        {
            process.Invoke(enemies, arguments);
            if (slot.InstructionTimer != 0)
            {
                ushort operand = unchecked((ushort)(slot.CurrentInstruction - 2));
                executedOperands.Add(operand);
                AssertEqual(ReadRidleyWord(rom, 0xa60000 | operand), slot.SpritemapPointer,
                    $"executed Ridley frame $A6:{operand:X4} matches the native extended sprite selector");
            }
            if (slot.InstructionTimer == 0 &&
                RidleyInstructionProgramDefinitions.ReadMechanicsWord(
                    slot.CurrentInstruction) == CommonEnemyInstructionCodes.Sleep)
            {
                return;
            }
        }

        throw new InvalidDataException(
            $"Ridley program $A6:{program:X4}, facing {facingDirection}, did not sleep.");
    }

    /// <summary>Repeatedly resolves the fireball program's mechanics entry to measure warmed lookup allocations.</summary>
    /// <returns>A checksum that consumes the mechanics values returned by the probe.</returns>
    private static int ProbeRidleyInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += RidleyInstructionProgramDefinitions.ReadMechanicsWord(
                RidleyInstructionProgramDefinitions.Fireballing);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian word from two adjacent cartridge bytes.</summary>
    /// <param name="source">The address space containing the reference word.</param>
    /// <param name="address">The absolute address of the word's low byte.</param>
    /// <returns>The word formed from the addressed byte and the following byte.</returns>
    private static ushort ReadRidleyWord(SuperMetroidAddressSpace source, int address) =>
        unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));

    /// <summary>
    /// Wraps cartridge access to reject reads of compiled Ridley mechanics and record reads of
    /// presentation words while production programs execute.
    /// </summary>
    /// <param name="source">The underlying address space used for permitted reads and forwarded writes.</param>
    private sealed class RidleyInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Bank-$A6 presentation-word addresses whose bytes were requested through this guard.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempted reads from compiled Ridley mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes an import cartridge read through the mechanics guard and presentation tracking.</summary>
        /// <param name="address">The absolute cartridge address to read.</param>
        /// <returns>The underlying byte when the address is not compiled mechanics data.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled mechanics, tracks presentation-word reads, and forwards other bytes.</summary>
        /// <param name="address">The absolute address to read.</param>
        /// <returns>The byte supplied by the underlying address space.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled Ridley mechanics.</exception>
        public byte ReadByte(int address)
        {
            if (RidleyInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Ridley mechanics ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa60000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < RidleyInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        RidleyInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
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

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">The absolute address receiving the write.</param>
        /// <param name="value">The byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
