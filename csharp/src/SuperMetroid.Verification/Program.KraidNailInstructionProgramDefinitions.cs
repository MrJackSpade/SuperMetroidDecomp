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
using static KraidSmallProgramsVerification;
using static KraidVisualSelectorsVerification;
using static SuiteTimingVerification;

/// <summary>Represents kraid nail instruction program definitions verification.</summary>
internal static class KraidNailInstructionProgramDefinitionsVerification
{
    /// <summary>verify kraid nail instruction program definitions.</summary>
    internal static void VerifyKraidNailInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKraidNailInstructionProgramDefinitions), () => VerifyKraidNailInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>verify kraid nail instruction program definitions.</summary>
    internal static void VerifyKraidNailInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(10, KraidNailInstructionProgramDefinitions.MechanicsWordCount,
            "Kraid fingernail compiled mechanics word count");
        AssertEqual(8, KraidNailInstructionProgramDefinitions.PresentationWordCount,
            "Kraid fingernail compiled visual operand count");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa70000 | 0x8b0c),
            KraidVisualDefinitions.InitialNailFrame,
            "compiled Kraid fingernail initial frame matches its cartridge selector");
        foreach ((int slotIndex, EnemyDefinitionId expectedDefinition) in new[]
                 {
                     (6, EnemyDefinitionId.KraidNail),
                     (7, EnemyDefinitionId.KraidNailBad),
                 })
        {
            int record = EnemyRomTablePointers.Kraid.PopulationRecords + slotIndex * 16;
            EnemyDefinitionId definition = EnemyDefinitionIds.FromHeaderPointer((ushort)(SnesLittleEndian.Word(KraidNailInstructionProgramDefinitionsRomByte(rom, record), KraidNailInstructionProgramDefinitionsRomByte(rom, record + 1))));
            ushort extraProperties = (ushort)(SnesLittleEndian.Word(KraidNailInstructionProgramDefinitionsRomByte(rom, record + 10), KraidNailInstructionProgramDefinitionsRomByte(rom, record + 11)));
            AssertEqual(expectedDefinition, definition,
                $"retail Kraid nail slot {slotIndex} matches the visual family");
            AssertTrue((extraProperties &
                    (ushort)EnemyExtraProperties.UsesExtendedSpritemap) == 0,
                $"retail Kraid nail slot {slotIndex} uses ordinary OAM composition");
        }
        Suite(nameof(VerifyKraidNailMechanicsMapping), () => VerifyKraidNailMechanicsMapping(rom));
        Suite(nameof(VerifyKraidNailPresentationMapping), () => VerifyKraidNailPresentationMapping());

        int forbiddenPresentationReadAttempts = 0;
        var guard = NewKraidNailInstructionGuard(rom, () => forbiddenPresentationReadAttempts++);
        foreach (EnemyDefinitionId definitionPointer in new EnemyDefinitionId[]
                 {
                     EnemyDefinitionId.KraidNail,
                     EnemyDefinitionId.KraidNailBad,
                 })
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = definitionPointer;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
            slot.CurrentInstruction = KraidNailInstructionProgramDefinitions.Loop;
            slot.InstructionTimer = 1;

            for (int frame = 0; frame < 9; frame++)
            {
                slot.InstructionTimer = 1;
                enemies.Instructions.ProcessInstructions(slot, null, null, Position16<RoomPixels>.Zero, (ushort)0);
            }
            AssertEqual(unchecked((ushort)(KraidNailInstructionProgramDefinitions.Loop + 4)),
                slot.CurrentInstruction,
                $"Kraid fingernail ${(int)definitionPointer:X4} loops to its first frame");
        }

        Suite(nameof(VerifyKraidNailVisualSelectors), () => VerifyKraidNailVisualSelectors(rom));
        AssertEqual(0, forbiddenPresentationReadAttempts,
            "Kraid fingernail programs never read installed visual selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Kraid fingernail production execution avoids compiled mechanics bytes");
        for (int index = 0;
             index < KraidNailInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                KraidNailInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertThrows<InvalidDataException>(
                () => KraidNailInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Kraid fingernail spritemap $A7:{address:X4} is rejected as mechanics");
        }
        AssertThrows<InvalidDataException>(
            () => KraidNailInstructionProgramDefinitions.ReadMechanicsWord(
                KraidNailInstructionProgramDefinitions.AdjacentPresentationData),
            "adjacent Kraid arm presentation data is rejected as fingernail mechanics");
        AssertThrows<InvalidDataException>(
            () => KraidVisualDefinitions.FrameAt(
                EnemyDefinitionId.KraidNail,
                KraidNailInstructionProgramDefinitions.AdjacentPresentationData),
            "adjacent Kraid visual operand is not a fingernail frame");

        foreach ((EnemyDefinitionId definition, int slotIndex) in new[]
                 {
                     (EnemyDefinitionId.KraidNail, 6),
                     (EnemyDefinitionId.KraidNailBad, 7),
                 })
        {
            var enemies = new RoomEnemySystem();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            enemies.BindFixtureBus(guard);
            enemies.KraidBehavior.Kraid = new KraidEnemyState();
            enemies.VerificationServices().IsAreaBossDefeated = (Func<bool>)(() => false);
            enemies.SlotTable.Slots[0].EnemyDefinitionPointer = EnemyDefinitionId.Kraid;
            RoomEnemySlot slot = enemies.SlotTable.Slots[slotIndex];
            slot.EnemyDefinitionPointer = definition;
            enemies.KraidBehavior.InitializeKraidNail(slot, slotIndex);
            AssertEqual(KraidVisualDefinitions.InitialNailFrame,
                slot.SpritemapPointer,
                $"Kraid fingernail slot {slotIndex} installs the compiled initial frame");
        }
        AssertEqual(0, forbiddenPresentationReadAttempts,
            "Kraid fingernail initialization never reads the initial frame from ROM");

        _ = ProbeKraidNailInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKraidNailInstructionAllocation();
        AssertTrue(checksum != 0, "Kraid fingernail allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kraid fingernail mechanics lookups allocate no storage");

        Console.WriteLine(
            "Kraid fingernail instruction mechanics: ten compiled words, both nail " +
            "definitions, eight compiled visual selectors, and both initial frames pass " +
            "with selector and mechanics ROM reads forbidden.");
    }

    /// <summary>kraid nail instruction program definitions rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid nail instruction program definitions")]
    private static byte KraidNailInstructionProgramDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>probe kraid nail instruction allocation.</summary>
    private static int ProbeKraidNailInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KraidNailInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KraidNailInstructionProgramDefinitions.Loop
                    : unchecked((ushort)(KraidNailInstructionProgramDefinitions.Loop + 0x20)));
        }
        return checksum;
    }

    /// <summary>new kraid nail instruction guard.</summary>
    private static CartridgeReadGuard NewKraidNailInstructionGuard(ISnesAddressSpace source, Action presentationRead) =>
        new CartridgeReadGuard(source)
            .Forbid(KraidNailInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Kraid fingernail mechanics byte ${address:X6}.")
            .Observe(IsKraidNailSelectorByte, _ => presentationRead())
            .ForbidWords(0xa70000, KraidNailInstructionProgramDefinitions.PresentationWordCount,
                KraidNailInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Production read installed Kraid nail selector ${address:X6}.");

    /// <summary>is kraid nail selector byte.</summary>
    private static bool IsKraidNailSelectorByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa70000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KraidNailInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort presentation = KraidNailInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            if (bankAddress == presentation || bankAddress == unchecked((ushort)(presentation + 1)))
                return true;
        }
        return false;
    }

    /// <summary>kraid nail instruction program definitions command.</summary>
    [VerificationCommand("--kraid-nail-instruction-program-definitions")]
    internal static int KraidNailInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyKraidNailInstructionProgramDefinitions), () => VerifyKraidNailInstructionProgramDefinitions());
        return 0;
    }
}
