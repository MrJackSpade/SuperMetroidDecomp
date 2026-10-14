using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Checks Squeept hidden, jump, and follower instruction programs against bank-$A2 ROM and live enemy handoffs.</summary>
internal static class NorfairLavaJumperInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail cartridge data for the Norfair lava-jumper instruction suite.</summary>
    internal static void VerifyNorfairLavaJumperInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyNorfairLavaJumperInstructionProgramDefinitions), () => VerifyNorfairLavaJumperInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled words with ROM and drives real rise, animation-finished, and follower loops without migrated source reads.</summary>
    internal static void VerifyNorfairLavaJumperInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < NorfairLavaJumperInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                NorfairLavaJumperInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | definition.Address)),
                $"Norfair lava-jumper mechanics word $A2:{definition.Address:X4}");
        }

        var guard = NewNorfairLavaJumperInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);
        var initialize = new Action<RoomEnemySlot>(enemies.NorfairLavaJumpingEnemyBehavior.InitializeNorfairLavaJumpingEnemy);
        var runMain = new Action<RoomEnemySlot, NorfairLavaJumpingEnemyState>(enemies.NorfairLavaJumpingEnemyBehavior.RunNorfairLavaJumpingEnemyMain);

        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.EnemyDefinitionPointer = EnemyDefinitionId.Squeept;
        parent.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        parent.Position = parent.Position.WithWhole(new(0x0100, 0x01e0));
        initialize(parent);
        NorfairLavaJumpingEnemyState parentState =
            (enemies.NorfairLavaJumpingEnemyStates[0] ?? throw new InvalidOperationException("Verification requires enemies.NorfairLavaJumpingEnemyStates[0]."));
        object?[] processArguments =
            [parent, null, null, Position16<RoomPixels>.Zero, (ushort)0];

        RunNorfairLavaJumperProgram(
            enemies, processArguments, parent,
            NorfairLavaJumperInstructionProgramDefinitions.Hidden,
            NorfairLavaJumperInstructionProgramDefinitions.HiddenSleep,
            timedFrames: 1);

        runMain(parent, (parentState ?? throw new InvalidOperationException("Verification requires parentState.")));
        parentState.YVelocity = 0xfbc8;
        runMain(parent, parentState);
        AssertEqual(NorfairLavaJumperInstructionProgramDefinitions.Jump,
            parent.CurrentInstruction,
            "real lava-jumper rise handoff installs jump program");
        RunNorfairLavaJumperProgram(
            enemies, processArguments, parent,
            NorfairLavaJumperInstructionProgramDefinitions.Jump,
            NorfairLavaJumperInstructionProgramDefinitions.JumpSleep,
            timedFrames: 7);
        AssertTrue(parentState.AnimationFinished,
            "jump program publishes its real animation handshake");

        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.EnemyDefinitionPointer = EnemyDefinitionId.Squeept;
        follower.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        follower.Parameter1 = 0x8000;
        initialize(follower);
        AssertEqual(NorfairLavaJumperInstructionProgramDefinitions.Follower,
            follower.CurrentInstruction,
            "real follower initializer installs compiled loop");
        processArguments[0] = follower;
        for (int frame = 0; frame < 7; frame++)
        {
            follower.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, processArguments);
        }
        AssertEqual(unchecked((ushort)(
                NorfairLavaJumperInstructionProgramDefinitions.Follower + 4)),
            follower.CurrentInstruction,
            "follower program loops to its first timed frame");

        // EnemySpritemapDefinitions now supplies all fourteen selector operands from
        // installed artwork. Program.EnemySpritemapArtwork compares each selector
        // against the cartridge; this guard verifies that live execution never
        // falls back to either the mechanics or presentation ROM bytes.
        AssertEqual(0, guard.ObservedWords.Count,
            "lava-jumper execution uses compiled spritemap selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled lava-jumper mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => NorfairLavaJumperInstructionProgramDefinitions.ReadMechanicsWord(0xbe44),
            "lava-jumper presentation pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NorfairLavaJumperInstructionProgramDefinitions.ReadMechanicsWord(0xbe86),
            "adjacent lava-jumper velocity table is rejected as mechanics");

        _ = ProbeNorfairLavaJumperInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNorfairLavaJumperInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "lava-jumper allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed lava-jumper mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Norfair lava-jumper instruction mechanics: 23 compiled words, all three " +
            "production programs, the real handshake, and compiled spritemap selectors pass.");
    }

    /// <summary>Forces each timed animation frame and checks that the selected program reaches its terminal sleep opcode.</summary>
    private static void RunNorfairLavaJumperProgram(
        RoomEnemySystem enemies,
        object?[] arguments,
        RoomEnemySlot slot,
        ushort entry,
        ushort terminalSleep,
        int timedFrames)
    {
        AssertEqual(entry, slot.CurrentInstruction, "lava-jumper compiled program entry");
        for (int frame = 0; frame < timedFrames; frame++)
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
        }
        slot.InstructionTimer = NativeCountdown.Start(1);
        EnemyInstructionRunner.Process(enemies, arguments);
        AssertEqual(terminalSleep, slot.CurrentInstruction,
            "lava-jumper program reaches terminal sleep");
    }

    /// <summary>Repeats lookups from all three compiled programs to check warmed mechanics access allocates no memory.</summary>
    private static int ProbeNorfairLavaJumperInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NorfairLavaJumperInstructionProgramDefinitions.ReadMechanicsWord(
                (index % 3) switch
                {
                    0 => NorfairLavaJumperInstructionProgramDefinitions.Hidden,
                    1 => NorfairLavaJumperInstructionProgramDefinitions.Jump,
                    _ => NorfairLavaJumperInstructionProgramDefinitions.Follower,
                });
        }
        return checksum;
    }

    /// <summary>Forbids compiled mechanics reads and records attempted visual-selector reads from bank $A2.</summary>
    private static CartridgeReadGuard NewNorfairLavaJumperInstructionReadGuard(ISnesAddressSpace source)
    {
        var guard = new CartridgeReadGuard(source)
            .Forbid(NorfairLavaJumperInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled lava-jumper mechanics byte ${address:X6}.");
        return guard.Observe(address => (SnesAddressMath.BankBase(address)) == 0xa20000, address =>
        {
            ushort bankAddress = unchecked((ushort)address);
            for (int index = 0;
                 index < NorfairLavaJumperInstructionProgramDefinitions.PresentationWordCount;
                 index++)
            {
                ushort presentation =
                    NorfairLavaJumperInstructionProgramDefinitions.PresentationWordAddress(index);
                if (bankAddress == presentation ||
                    bankAddress == unchecked((ushort)(presentation + 1)))
                    guard.ObservedWords.Add(presentation);
            }
        });
    }

    /// <summary>Runs the Norfair lava-jumper mechanics verification command.</summary>
    [VerificationCommand("--norfair-lava-jumper-instruction-mechanics")]
    internal static int NorfairLavaJumperInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyNorfairLavaJumperInstructionProgramDefinitions), () => VerifyNorfairLavaJumperInstructionProgramDefinitions());
        return 0;
    }
}
