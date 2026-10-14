using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Phantoon body, eye, tentacle, and mouth programs against native instruction data.</summary>
internal static class PhantoonInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge and runs Phantoon instruction checks.</summary>
    internal static void VerifyPhantoonInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPhantoonInstructionProgramDefinitions), () => VerifyPhantoonInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Exercises reachable Phantoon programs and callbacks with compiled mechanics reads forbidden.</summary>
    internal static void VerifyPhantoonInstructionProgramDefinitions(
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
            InstructionMechanicsWord definition =
                PhantoonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa70000 | definition.Address),
                $"Phantoon mechanics word $A7:{definition.Address:X4}");
        }

        var guard = NewPhantoonInstructionReadGuard(rom);
        var executedOperands = new HashSet<ushort>();
        RoomEnemySystem enemies = CreatePhantoonInstructionSystem(guard);
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot eye = enemies.SlotTable.Slots[1];
        RoomEnemySlot tentacles = enemies.SlotTable.Slots[2];
        RoomEnemySlot mouth = enemies.SlotTable.Slots[3];

        foreach (ushort entry in new ushort[]
                 {
                     PhantoonInstructionProgramDefinitions.InvulnerableBody,
                     PhantoonInstructionProgramDefinitions.FullHitboxBody,
                     PhantoonInstructionProgramDefinitions.EyeHitboxBody,
                 })
        {
            RunPhantoonInstructionProgram(rom, executedOperands, enemies, body, entry, calls: 2);
            AssertEqual(unchecked((ushort)(entry + 4)), body.CurrentInstruction,
                $"Phantoon body program $A7:{entry:X4} reaches terminal sleep");
        }

        RunPhantoonInstructionProgram(
            rom, executedOperands, enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeOpen,
            calls: 4);
        AssertEqual((ushort)0xcc67, eye.CurrentInstruction,
            "Phantoon open-eye program reaches terminal sleep");
        AssertEqual(PhantoonInstructionProgramDefinitions.EyeHitboxBody,
            body.CurrentInstruction,
            "Phantoon open-eye callback installs the eye-only body hitbox");
        AssertTrue((enemies.PhantoonBehavior.Phantoon ?? throw new InvalidOperationException("Verification requires enemies.PhantoonBehavior.Phantoon.")).LastMaterializationSound.HasValue,
            "Phantoon open-eye callback queues its materialization sound");

        RunPhantoonInstructionProgram(
            rom, executedOperands, enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeClosed,
            calls: 2);
        RunPhantoonInstructionProgram(
            rom, executedOperands, enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeCloseAndPickNewPattern,
            calls: 3);
        AssertEqual(unchecked((ushort)(PhantoonInstructionProgramDefinitions.EyeClosed + 4)),
            eye.CurrentInstruction,
            "Phantoon close-and-pick program branches into the closed-eye frame");
        RunPhantoonInstructionProgram(
            rom, executedOperands, enemies,
            eye,
            PhantoonInstructionProgramDefinitions.EyeClose,
            calls: 3);
        AssertEqual(unchecked((ushort)(PhantoonInstructionProgramDefinitions.EyeClosed + 4)),
            eye.CurrentInstruction,
            "Phantoon close program branches into the closed-eye frame");
        RunPhantoonInstructionProgram(
            rom, executedOperands, enemies,
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
            RunPhantoonInstructionProgram(rom, executedOperands, enemies, eye, entry, calls: 2);
            AssertEqual(unchecked((ushort)(entry + 4)), eye.CurrentInstruction,
                $"Phantoon eye-direction program $A7:{entry:X4} reaches terminal sleep");
        }

        RunPhantoonInstructionProgram(
            rom, executedOperands, enemies,
            tentacles,
            PhantoonInstructionProgramDefinitions.InitialTentacles,
            calls: 5);
        AssertEqual(unchecked((ushort)(
                PhantoonInstructionProgramDefinitions.InitialTentacles + 4)),
            tentacles.CurrentInstruction,
            "Phantoon tentacle program loops to its first frame");

        int flamesBefore = enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive);
        RunPhantoonInstructionProgram(
            rom, executedOperands, enemies,
            mouth,
            PhantoonInstructionProgramDefinitions.MouthFollowUp,
            calls: 3);
        AssertEqual(unchecked((ushort)(
                PhantoonInstructionProgramDefinitions.InitialMouth + 4)),
            mouth.CurrentInstruction,
            "Phantoon mouth flame program falls through into its initial frame");
        AssertEqual(flamesBefore + 1,
            enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive),
            "Phantoon mouth callback spawns one casual flame");
        RunPhantoonInstructionProgram(
            rom, executedOperands, enemies,
            mouth,
            PhantoonInstructionProgramDefinitions.InitialMouth,
            calls: 2);

        AssertEqual(0, guard.ObservedWords.Count,
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
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa70000 | address), selected,
                "Exact native Phantoon visual operand");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa70000 | address),
                PhantoonInstructionProgramDefinitions.PresentationFrame(index),
                "Named Phantoon role selection equals the native operand");
            AssertEqual(selected, PhantoonInstructionProgramDefinitions.FrameAt(address),
                "Phantoon address dispatch equals indexed role selection");
            AssertTrue(CompiledEnemyVisualSelectors.IsCalculatedSelector(0xa70000 | address),
                "Phantoon selector belongs to calculated dispatch");
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

    /// <summary>Creates the four linked Phantoon parts and a deterministic random source for instruction execution.</summary>
    internal static RoomEnemySystem CreatePhantoonInstructionSystem(ISnesAddressSpace bus)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);

        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot eye = enemies.SlotTable.Slots[1];
        RoomEnemySlot tentacles = enemies.SlotTable.Slots[2];
        RoomEnemySlot mouth = enemies.SlotTable.Slots[3];
        body.EnemyDefinitionPointer = EnemyDefinitionId.PhantoonBody;
        eye.EnemyDefinitionPointer = EnemyDefinitionId.PhantoonEye;
        tentacles.EnemyDefinitionPointer = EnemyDefinitionId.PhantoonTentacles;
        mouth.EnemyDefinitionPointer = EnemyDefinitionId.PhantoonMouth;
        foreach (RoomEnemySlot slot in new[] { body, eye, tentacles, mouth })
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };

        var state = new PhantoonEnemyState(body)
        {
            Eye = eye,
            Tentacles = tentacles,
            Mouth = mouth,
        };
        enemies.PhantoonBehavior.Phantoon = state;
        return enemies;
    }

    /// <summary>Advances a selected program and compares each resulting visual selector with the native operand.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Phantoon instruction program")]
    private static void RunPhantoonInstructionProgram(
        ISnesAddressSpace rom, HashSet<ushort> executedOperands,
        RoomEnemySystem enemies,
        RoomEnemySlot slot,
        ushort entry,
        int calls)
    {
        slot.CurrentInstruction = entry;
        for (int call = 0; call < calls; call++)
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            ushort previousSprite = slot.SpritemapPointer;
            EnemyInstructionRunner.Process(enemies, [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0]);
            ushort record = unchecked((ushort)(slot.CurrentInstruction - 4));
            ushort nativeWord = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa70000 | record);
            if ((nativeWord & 0x8000) == 0)
                VerifyExecutedEnemySelector(rom, slot, executedOperands);
            else
                AssertEqual(previousSprite, slot.SpritemapPointer,
                    "Phantoon callback-to-sleep retains the previously selected sprite");
        }
    }

    /// <summary>Alternates body and mouth mechanics lookups to detect warmed allocations.</summary>
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

    /// <summary>Rejects compiled mechanics reads while observing any presentation-operand reads.</summary>
    private static CartridgeReadGuard NewPhantoonInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(PhantoonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Phantoon mechanics byte ${address:X6}.")
            .ObserveWords(0xa70000, PhantoonInstructionProgramDefinitions.PresentationWordCount,
                PhantoonInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the standalone Phantoon instruction program verification command.</summary>
    [VerificationCommand("--phantoon-instruction-program-definitions")]
    internal static int PhantoonInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyPhantoonInstructionProgramDefinitions), () => VerifyPhantoonInstructionProgramDefinitions());
        return 0;
    }
}
