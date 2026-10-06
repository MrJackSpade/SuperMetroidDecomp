using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPhantoonInstructionProgramDefinitions()
    {
        VerifyPhantoonInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyPhantoonInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(58, PhantoonInstructionProgramDefinitions.MechanicsWordCount,
            "Phantoon compiled mechanics word count");
        AssertEqual(27, PhantoonInstructionProgramDefinitions.PresentationWordCount,
            "Phantoon presentation word count");
        for (int index = 0;
             index < PhantoonInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            PhantoonInstructionMechanicsWord definition =
                PhantoonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadPhantoonInstructionWord(rom, definition.Address),
                $"Phantoon mechanics word $A7:{definition.Address:X4}");
        }

        var guard = new PhantoonInstructionReadGuard(rom);
        var executedOperands = new HashSet<ushort>();
        RoomEnemySystem enemies = CreatePhantoonInstructionSystem(guard);
        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot eye = enemies.Slots[1];
        RoomEnemySlot tentacles = enemies.Slots[2];
        RoomEnemySlot mouth = enemies.Slots[3];
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        foreach (ushort entry in new ushort[]
                 {
                     PhantoonInstructionProgramDefinitions.InvulnerableBody,
                     PhantoonInstructionProgramDefinitions.FullHitboxBody,
                     PhantoonInstructionProgramDefinitions.EyeHitboxBody,
                 })
        {
            RunPhantoonInstructionProgram(rom, executedOperands, process, enemies, body, entry, calls: 2);
            AssertEqual(unchecked((ushort)(entry + 4)), body.CurrentInstruction,
                $"Phantoon body program $A7:{entry:X4} reaches terminal sleep");
        }

        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeOpen,
            calls: 4);
        AssertEqual((ushort)0xcc67, eye.CurrentInstruction,
            "Phantoon open-eye program reaches terminal sleep");
        AssertEqual(PhantoonInstructionProgramDefinitions.EyeHitboxBody,
            body.CurrentInstruction,
            "Phantoon open-eye callback installs the eye-only body hitbox");
        AssertTrue(enemies.Phantoon!.LastMaterializationSound.HasValue,
            "Phantoon open-eye callback queues its materialization sound");

        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeClosed,
            calls: 2);
        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeCloseAndPickNewPattern,
            calls: 3);
        AssertEqual(unchecked((ushort)(PhantoonInstructionProgramDefinitions.EyeClosed + 4)),
            eye.CurrentInstruction,
            "Phantoon close-and-pick program branches into the closed-eye frame");
        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeClose,
            calls: 3);
        AssertEqual(unchecked((ushort)(PhantoonInstructionProgramDefinitions.EyeClosed + 4)),
            eye.CurrentInstruction,
            "Phantoon close program branches into the closed-eye frame");
        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeballCentered,
            calls: 2);

        foreach (ushort entry in new ushort[]
                 {
                     PhantoonInstructionProgramDefinitions.EyeLookingUp,
                     PhantoonInstructionProgramDefinitions.EyeLookingUpRight,
                     PhantoonInstructionProgramDefinitions.EyeLookingRight,
                     PhantoonInstructionProgramDefinitions.EyeLookingDownRight,
                     PhantoonInstructionProgramDefinitions.EyeLookingDown,
                     PhantoonInstructionProgramDefinitions.EyeLookingDownLeft,
                     PhantoonInstructionProgramDefinitions.EyeLookingLeft,
                     PhantoonInstructionProgramDefinitions.EyeLookingUpLeft,
                 })
        {
            RunPhantoonInstructionProgram(rom, executedOperands, process, enemies, eye, entry, calls: 2);
            AssertEqual(unchecked((ushort)(entry + 4)), eye.CurrentInstruction,
                $"Phantoon eye-direction program $A7:{entry:X4} reaches terminal sleep");
        }

        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            tentacles,
            PhantoonInstructionProgramDefinitions.InitialTentacles,
            calls: 5);
        AssertEqual(unchecked((ushort)(
                PhantoonInstructionProgramDefinitions.InitialTentacles + 4)),
            tentacles.CurrentInstruction,
            "Phantoon tentacle program loops to its first frame");

        int flamesBefore = enemies.EnemyProjectiles.Count(projectile => projectile.IsActive);
        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            mouth,
            PhantoonInstructionProgramDefinitions.MouthFollowUp,
            calls: 3);
        AssertEqual(unchecked((ushort)(
                PhantoonInstructionProgramDefinitions.InitialMouth + 4)),
            mouth.CurrentInstruction,
            "Phantoon mouth flame program falls through into its initial frame");
        AssertEqual(flamesBefore + 1,
            enemies.EnemyProjectiles.Count(projectile => projectile.IsActive),
            "Phantoon mouth callback spawns one casual flame");
        RunPhantoonInstructionProgram(
            rom, executedOperands, process,
            enemies,
            mouth,
            PhantoonInstructionProgramDefinitions.InitialMouth,
            calls: 2);

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Enemy presentation performs zero live cartridge reads");
        AssertEqual(PhantoonInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Phantoon production execution avoids compiled mechanics bytes");
        for (int index = 0;
             index < PhantoonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                PhantoonInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution selects Phantoon presentation $A7:{address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa7, address, out ushort selected),
                "Every Phantoon visual operand has an installed selector");
            AssertEqual(ReadPhantoonInstructionWord(rom, address), selected,
                "Exact native Phantoon visual operand");
            AssertThrows<InvalidDataException>(
                () => PhantoonInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Phantoon presentation $A7:{address:X4} is rejected as mechanics");
        }
        AssertThrows<InvalidDataException>(
            () => PhantoonInstructionProgramDefinitions.ReadMechanicsWord(
                PhantoonInstructionProgramDefinitions.AdjacentCasualFlameTimers),
            "adjacent Phantoon casual-flame timer data is rejected as instruction mechanics");

        _ = ProbePhantoonInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePhantoonInstructionAllocation();
        AssertTrue(checksum != 0, "Phantoon instruction allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Phantoon instruction mechanics lookups allocate no storage");

        Console.WriteLine(
            "Phantoon instruction mechanics: 58 compiled words, all 19 reachable " +
            "programs, four callbacks, and 27 native sprite selections pass with zero live reads.");
    }

    private static RoomEnemySystem CreatePhantoonInstructionSystem(ISnesAddressSpace bus)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0));

        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot eye = enemies.Slots[1];
        RoomEnemySlot tentacles = enemies.Slots[2];
        RoomEnemySlot mouth = enemies.Slots[3];
        body.EnemyDefinitionPointer = RoomEnemySystem.PhantoonBodyDefinition;
        eye.EnemyDefinitionPointer = RoomEnemySystem.PhantoonEyeDefinition;
        tentacles.EnemyDefinitionPointer = RoomEnemySystem.PhantoonTentaclesDefinition;
        mouth.EnemyDefinitionPointer = RoomEnemySystem.PhantoonMouthDefinition;
        foreach (RoomEnemySlot slot in new[] { body, eye, tentacles, mouth })
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };

        var state = new PhantoonEnemyState(body)
        {
            Eye = eye,
            Tentacles = tentacles,
            Mouth = mouth,
        };
        typeof(RoomEnemySystem).GetField("_phantoonState", flags)!.SetValue(enemies, state);
        return enemies;
    }

    private static void RunPhantoonInstructionProgram(
        ISnesAddressSpace rom, HashSet<ushort> executedOperands,
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot slot,
        ushort entry,
        int calls)
    {
        slot.CurrentInstruction = entry;
        for (int call = 0; call < calls; call++)
        {
            slot.InstructionTimer = 1;
            ushort previousSprite = slot.SpritemapPointer;
            process.Invoke(
                enemies,
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            ushort record = unchecked((ushort)(slot.CurrentInstruction - 4));
            ushort nativeWord = (ushort)(rom.ReadByte(0xa70000 | record) |
                rom.ReadByte(0xa70000 | unchecked((ushort)(record + 1))) << 8);
            if ((nativeWord & 0x8000) == 0)
                VerifyExecutedEnemySelector(rom, slot, executedOperands);
            else
                AssertEqual(previousSprite, slot.SpritemapPointer,
                    "Phantoon callback-to-sleep retains the previously selected sprite");
        }
    }

    private static int ProbePhantoonInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PhantoonInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PhantoonInstructionProgramDefinitions.InvulnerableBody
                    : unchecked((ushort)(
                        PhantoonInstructionProgramDefinitions.InitialMouth + 4)));
        }
        return checksum;
    }

    private static ushort ReadPhantoonInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa70000 | address) |
            source.ReadByte(0xa70000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class PhantoonInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (PhantoonInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Phantoon mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa70000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < PhantoonInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = PhantoonInstructionProgramDefinitions
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

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
