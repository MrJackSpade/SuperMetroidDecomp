using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Represents choot instruction program definitions verification.</summary>
internal static class ChootInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies choot instruction program definitions.</summary>
    internal static void VerifyChootInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyChootInstructionProgramDefinitions), () => VerifyChootInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies choot instruction program definitions.</summary>
    internal static void VerifyChootInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog? artwork = null)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        for (int index = 0;
             index < ChootInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ChootInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadChootInstructionWord(rom, 0xa20000 | definition.Address),
                $"Choot instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = NewChootInstructionProgramReadGuard(rom, forbidPresentation: true);
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        Type enemySystemType = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        var initialize = new Action<RoomEnemySlot>(enemies.ChootBehavior.InitializeChoot);
        var prepareJump = new Action<RoomEnemySlot, ChootEnemyState>(ChootBehavior.PrepareChootJump);
        var runJump = new Action<RoomEnemySlot, ChootEnemyState>(ChootBehavior.RunChootJump);

        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Choot;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.Parameter1 = 0x0001;
        slot.Parameter2 = 0;
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
        initialize(slot);
        ChootEnemyState state = enemies.ChootStates[0]!;
        object?[] processArguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];

        RunChootInstructionProgram(
            enemies,
            processArguments,
            slot,
            ChootInstructionProgramDefinitions.Idle,
            timedFrameCount: 1);
        AssertTrue((slot.Properties & (ushort)EnemyProperties.ProcessOffScreen) == 0,
            "Choot idle program disables off-screen processing");

        state.JumpDelayTimer = 0;
        prepareJump(slot, state);
        AssertEqual(ChootInstructionProgramDefinitions.Jumping, slot.CurrentInstruction,
            "Choot real preparation handoff installs jumping program");
        RunChootInstructionProgram(
            enemies,
            processArguments,
            slot,
            ChootInstructionProgramDefinitions.Jumping,
            timedFrameCount: 2);
        AssertTrue((slot.Properties & (ushort)EnemyProperties.ProcessOffScreen) != 0,
            "Choot jumping program enables off-screen processing");

        state.YSpeedTableIndex = 0x0100;
        runJump(slot, state);
        AssertEqual(ChootInstructionProgramDefinitions.Falling, slot.CurrentInstruction,
            "Choot real apex handoff installs falling program");
        RunChootInstructionProgram(
            enemies,
            processArguments,
            slot,
            ChootInstructionProgramDefinitions.Falling,
            timedFrameCount: 2);

        AssertEqual(0, guard.ObservedWords.Count,
            "Choot production programs avoid cartridge visual selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Choot mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => ChootInstructionProgramDefinitions.ReadMechanicsWord(0xd830),
            "Choot spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ChootInstructionProgramDefinitions.ReadMechanicsWord(0xd84c),
            "adjacent Choot path table is rejected as mechanics");

        _ = ProbeChootInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeChootInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Choot allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Choot mechanics lookups allocate no per-frame storage");

        Console.WriteLine(artwork is null
            ? "Choot instruction mechanics: eleven compiled words, three real program " +
              "handoffs, and five compiled visual selectors preserve native timing."
            : "Installed Choot: idle, jump, and fall programs execute without " +
              "cartridge visual-selector reads.");
    }

    /// <summary>Runs choot instruction program.</summary>
    private static void RunChootInstructionProgram(
        RoomEnemySystem enemies,
        object?[] processArguments,
        RoomEnemySlot slot,
        ushort entry,
        int timedFrameCount)
    {
        AssertEqual(entry, slot.CurrentInstruction, "Choot compiled program entry");
        for (int frame = 0; frame < timedFrameCount; frame++)
        {
            slot.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, processArguments);
        }
        slot.InstructionTimer = 1;
        EnemyInstructionRunner.Process(enemies, processArguments);
        AssertEqual(unchecked((ushort)(entry + timedFrameCount * 4 + 2)),
            slot.CurrentInstruction,
            "Choot program reaches terminal sleep");
    }

    /// <summary>Probes choot instruction mechanics allocation.</summary>
    private static int ProbeChootInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ChootInstructionProgramDefinitions.ReadMechanicsWord(
                (index % 3) switch
                {
                    0 => ChootInstructionProgramDefinitions.Idle,
                    1 => ChootInstructionProgramDefinitions.Jumping,
                    _ => ChootInstructionProgramDefinitions.Falling,
                });
        }
        return checksum;
    }

    /// <summary>Reads choot instruction word.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Choot instruction words")]
    internal static ushort ReadChootInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    /// <summary>New choot instruction program read guard.</summary>
    private static CartridgeReadGuard NewChootInstructionProgramReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false)
    {
        CartridgeReadGuard guard = new CartridgeReadGuard(source)
            .Forbid(ChootInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Choot mechanics byte ${address:X6}.");
        return forbidPresentation
            ? guard.ForbidWords(0xa20000, ChootInstructionProgramDefinitionsTooling.PresentationWordCount,
                ChootInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Choot read visual-selector byte ${address:X6}.")
            : guard.ObserveWords(0xa20000, ChootInstructionProgramDefinitionsTooling.PresentationWordCount,
                ChootInstructionProgramDefinitionsTooling.PresentationWordAddress);
    }

    /// <summary>Choot instruction mechanics command.</summary>
    [VerificationCommand("--choot-instruction-mechanics")]
    internal static int ChootInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyChootInstructionProgramDefinitions), () => VerifyChootInstructionProgramDefinitions());
        return 0;
    }
}
