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

/// <summary>Checks Kago slow and fast programs, post-hit spawning, and allocation-free compiled lookup.</summary>
internal static class KagoInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail data and runs the Kago instruction program verification suite.</summary>
    internal static void VerifyKagoInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKagoInstructionProgramDefinitions), () => VerifyKagoInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled words, slow and fast playback, post-hit bug spawning, ROM isolation, and allocation-free lookup.</summary>
    internal static void VerifyKagoInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KagoInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                KagoInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadKagoInstructionWord(rom, 0xa80000 | definition.Address),
                $"Kago instruction mechanics word $A8:{definition.Address:X4}");
        }

        var guard = NewKagoInstructionProgramGuard(rom);
        var enemies = new RoomEnemySystem();
        Type enemySystemType = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 0);
        var initialize = new Action<RoomEnemySlot>(enemies.KagoBehavior.InitializeKago);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Kago;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.Parameter1 = 10;
        initialize(slot);
        AssertEqual(KagoInstructionProgramDefinitions.Slow, slot.CurrentInstruction,
            "Kago initializer program");

        object?[] processArguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int frame = 0; frame < 41; frame++)
            EnemyInstructionRunner.Process(enemies, processArguments);
        AssertEqual(unchecked((ushort)(KagoInstructionProgramDefinitions.Slow + 4)),
            slot.CurrentInstruction,
            "Kago slow program completes its native animation loop");
        AssertEqual((ushort)10, slot.InstructionTimer.RawValue,
            "Kago slow loop restores its ten-frame duration");

        KagoEnemyState state = enemies.KagoStates[0] ?? throw new InvalidDataException(
            "Kago initializer did not publish typed state.");
        enemies.KagoBehavior.ResolveKagoShotAfterCommon(slot, state);
        AssertTrue(state.UsesFastAnimation, "Kago real shot handoff selects fast animation");
        AssertEqual(KagoInstructionProgramDefinitions.Fast, slot.CurrentInstruction,
            "Kago shot program");
        AssertEqual(1, state.SpawnedBugCount,
            "Kago real shot handoff retains its projectile side effect");
        for (int frame = 0; frame < 13; frame++)
            EnemyInstructionRunner.Process(enemies, processArguments);
        AssertEqual(unchecked((ushort)(KagoInstructionProgramDefinitions.Fast + 4)),
            slot.CurrentInstruction,
            "Kago fast program completes its native animation loop");
        AssertEqual((ushort)3, slot.InstructionTimer.RawValue,
            "Kago fast loop restores its three-frame duration");

        AssertEqual(0, guard.ObservedWords.Count,
            "Kago's slow and fast loops use compiled spritemap selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Kago mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => KagoInstructionProgramDefinitions.ReadMechanicsWord(0xab20),
            "Kago spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => KagoInstructionProgramDefinitions.ReadMechanicsWord(0xab46),
            "adjacent Kago initializer code is rejected as instruction mechanics");

        _ = ProbeKagoInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKagoInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Kago allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kago mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Kago instruction mechanics: twelve compiled words, the complete slow and " +
            "post-hit loops, the real bug-spawning handoff, and eight compiled spritemap " +
            "selectors pass without instruction or presentation ROM reads.");
    }

    /// <summary>Repeatedly resolves a live slow-program word to measure warmed lookup allocation.</summary>
    private static int ProbeKagoInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
            checksum += KagoInstructionProgramDefinitions.ReadMechanicsWord(
                KagoInstructionProgramDefinitions.Slow);
        return checksum;
    }

    /// <summary>Reads a cartridge word as the oracle for compiled Kago instruction mechanics.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kago instruction words")]
    internal static ushort ReadKagoInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    /// <summary>Forbids compiled mechanics reads and observes remaining presentation word reads.</summary>
    private static CartridgeReadGuard NewKagoInstructionProgramGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(KagoInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Kago mechanics byte ${address:X6}.")
            .ObserveWords(0xa80000, KagoInstructionProgramDefinitionsTooling.PresentationWordCount, KagoInstructionProgramDefinitionsTooling.PresentationWordAddress);

    /// <summary>Runs the Kago instruction mechanics verification command.</summary>
    [VerificationCommand("--kago-instruction-mechanics")]
    internal static int KagoInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyKagoInstructionProgramDefinitions), () => VerifyKagoInstructionProgramDefinitions());
        return 0;
    }
}
