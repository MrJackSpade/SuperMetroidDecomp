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

/// <summary>Represents wrecked ship ghost instruction program definitions verification.</summary>
internal static class WreckedShipGhostInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies wrecked ship ghost instruction program definitions.</summary>
    internal static void VerifyWreckedShipGhostInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyWreckedShipGhostInstructionProgramDefinitions), () => VerifyWreckedShipGhostInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies wrecked ship ghost instruction program definitions.</summary>
    internal static void VerifyWreckedShipGhostInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < WreckedShipGhostInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                WreckedShipGhostInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | definition.Address),
                $"Wrecked Ship ghost mechanics word $A8:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewWreckedShipGhostInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        enemies.Presentation._cgram = new SnesCgram();
        var initialize = new Action<RoomEnemySlot>(enemies.WreckedShipGhostBehavior.InitializeWreckedShipGhost);

        RoomEnemySlot ghost = enemies.SlotTable.Slots[0];
        ghost.EnemyDefinitionPointer = EnemyDefinitionId.Coven;
        ghost.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        initialize(ghost);
        AssertEqual(WreckedShipGhostInstructionProgramDefinitions.Floating,
            ghost.CurrentInstruction,
            "real Wrecked Ship ghost initializer installs compiled floating program");

        object?[] processArguments =
            [ghost, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int call = 0; call < 4; call++)
        {
            ghost.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, processArguments);
        }

        AssertEqual(unchecked((ushort)(
                WreckedShipGhostInstructionProgramDefinitions.Floating + 4)),
            ghost.CurrentInstruction,
            "Wrecked Ship ghost loop completes its native goto and first repeated frame");
        AssertEqual(0, guard.ObservedWords.Count,
            "ghost visual selectors require no runtime cartridge reads");
        for (int index = 0; index < WreckedShipGhostInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = WreckedShipGhostInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa8, address, out ushort selector),
                $"ghost visual selector $A8:{address:X4} is compiled");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | address), selector,
                $"ghost visual selector $A8:{address:X4} preserves its native operand");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Wrecked Ship ghost mechanics byte");
        AssertThrows<InvalidDataException>(
            () => WreckedShipGhostInstructionProgramDefinitions.ReadMechanicsWord(
                WreckedShipGhostInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Wrecked Ship ghost spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => WreckedShipGhostInstructionProgramDefinitions.ReadMechanicsWord(
                WreckedShipGhostInstructionProgramDefinitions.FirstAdjacentConstant),
            "adjacent ghost constants are rejected as instruction mechanics");

        _ = ProbeWreckedShipGhostInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeWreckedShipGhostInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Wrecked Ship ghost allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Wrecked Ship ghost mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Wrecked Ship ghost instruction mechanics: five compiled words, the real " +
            "initializer, the complete floating loop, and three native compiled selectors " +
            "pass with zero runtime ROM reads.");
    }

    /// <summary>Probes wrecked ship ghost instruction mechanics allocation.</summary>
    private static int ProbeWreckedShipGhostInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += WreckedShipGhostInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? WreckedShipGhostInstructionProgramDefinitions.Floating
                    : WreckedShipGhostInstructionProgramDefinitions.LoopOpcode);
        }
        return checksum;
    }

    /// <summary>Creates wrecked ship ghost instruction read guard.</summary>
    private static CartridgeReadGuard NewWreckedShipGhostInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(WreckedShipGhostInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled ghost mechanics byte ${address:X6}.")
            .ObserveWords(0xa80000, WreckedShipGhostInstructionProgramDefinitions.PresentationWordCount, WreckedShipGhostInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Wrecked ship ghost instruction mechanics command.</summary>
    [VerificationCommand("--wrecked-ship-ghost-instruction-mechanics")]
    internal static int WreckedShipGhostInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyWreckedShipGhostInstructionProgramDefinitions), () => VerifyWreckedShipGhostInstructionProgramDefinitions());
        return 0;
    }
}
