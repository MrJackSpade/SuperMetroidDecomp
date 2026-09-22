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
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0;
             index < RidleyInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            RidleyInstructionMechanicsWord definition =
                RidleyInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                ReadRidleyWord(rom, 0xa60000 | definition.Address),
                definition.Value,
                $"Ridley mechanics word $A6:{definition.Address:X4}");
        }

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
            RunProgram(guard, program, facingDirection: 0, ceresRidleyDefinition);
            RunProgram(guard, program, facingDirection: 2, ceresRidleyDefinition);
        }

        RunProgram(
            guard,
            RidleyInstructionProgramDefinitions.TransitionToFlying,
            facingDirection: 0,
            ceresRidleyDefinition);
        RunProgram(
            guard,
            RidleyInstructionProgramDefinitions.TransitionToFlying,
            facingDirection: 2,
            RoomEnemySystem.NorfairRidleyDefinition);

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Ridley execution avoids compiled mechanics bytes");
        AssertEqual(
            RidleyInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Ridley extended-spritemap operands remain live cartridge reads");
        for (int index = 0;
             index < RidleyInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                RidleyInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Ridley presentation $A6:{address:X4}");
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
            $"{RidleyInstructionProgramDefinitions.MechanicsWordCount} compiled words, " +
            "nine production entry programs, both facing paths, and " +
            $"{RidleyInstructionProgramDefinitions.PresentationWordCount} live " +
            "extended-spritemap reads pass with mechanics bytes forbidden.");
    }

    private static void RunProgram(
        RidleyInstructionReadGuard guard,
        ushort program,
        ushort facingDirection,
        ushort enemyDefinition)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
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

    private static ushort ReadRidleyWord(SuperMetroidAddressSpace source, int address) =>
        unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));

    private sealed class RidleyInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (RidleyInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Ridley mechanics ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa60000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < RidleyInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        RidleyInstructionProgramDefinitions.PresentationWordAddress(index);
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

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
