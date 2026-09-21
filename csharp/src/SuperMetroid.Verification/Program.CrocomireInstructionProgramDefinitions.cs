using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private const ushort CrocomireInstructionAuditRoom = 0xa98d;

    private static void VerifyCrocomireInstructionProgramDefinitions()
    {
        VerifyCrocomireInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyCrocomireInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < CrocomireInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            CrocomireInstructionMechanicsWord definition =
                CrocomireInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadCrocomireInstructionWord(rom, definition.Address),
                $"Crocomire mechanics word $A4:{definition.Address:X4}");
        }

        var guard = new CrocomireInstructionReadGuard(rom);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            rom,
            CrocomireInstructionAuditRoom);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(rom, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x0440,
            YPosition = 0x0078,
        };
        samus.RefreshCollisionRadii(rom);
        samus.InitializeAnimation(rom);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            guard,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            cameraX: 0x0400,
            setRoomScrollState: assets.Scrolls.SetStorage,
            isAreaMiniBossDefeated: () => false,
            setAreaMiniBossDefeated: () => { });

        RoomEnemySlot body = enemies.Slots[0];
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        object?[] arguments =
            [body, samus, assets.LevelData, (ushort)0x0400, (ushort)0, (ushort)0, (byte)0];

        // Each presentation operand follows a compiled duration. Entering at that duration
        // forces the real interpreter to obtain control from the catalog and artwork from
        // the cartridge without executing unrelated callbacks between frames.
        for (int index = 0;
             index < CrocomireInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort presentation =
                CrocomireInstructionProgramDefinitions.PresentationWordAddress(index);
            body.CurrentInstruction = unchecked((ushort)(presentation - 2));
            body.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
            AssertEqual(presentation, unchecked((ushort)(body.CurrentInstruction - 2)),
                $"Crocomire presentation handoff $A4:{presentation:X4}");
        }

        AssertEqual(236, guard.ObservedPresentationWords.Count,
            "all live Crocomire body/skeleton spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Crocomire mechanics byte");
        AssertThrows<InvalidDataException>(
            () => CrocomireInstructionProgramDefinitions.ReadMechanicsWord(
                CrocomireInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Crocomire spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CrocomireInstructionProgramDefinitions.ReadMechanicsWord(
                CrocomireInstructionProgramDefinitions.FirstExcludedUnreferencedProgram),
            "unreferenced Crocomire body program is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CrocomireInstructionProgramDefinitions.ReadMechanicsWord(
                CrocomireInstructionProgramDefinitions.FirstTongueProgram),
            "independently owned Crocomire tongue program is rejected as body mechanics");

        _ = ProbeCrocomireInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCrocomireInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Crocomire allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Crocomire mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Crocomire instruction mechanics: 434 compiled words across fight, reaction, " +
            "melting, bridge and skeleton programs; 236 live presentation reads pass " +
            "with mechanics bytes forbidden.");
    }

    private static int ProbeCrocomireInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CrocomireInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? CrocomireInstructionProgramDefinitions.Initial
                    : CrocomireInstructionProgramDefinitions.SkeletonFlowingDownRiver);
        }
        return checksum;
    }

    private static ushort ReadCrocomireInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa40000 | address) |
            source.ReadByte(0xa40000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class CrocomireInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (CrocomireInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Crocomire mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa40000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < CrocomireInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = CrocomireInstructionProgramDefinitions
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
