using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.CompiledEnemyVisualSelectorAssertionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Viola control words and surface-dependent projectile loops against retail instruction data.</summary>
internal static class ViolaInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail data and runs the Viola instruction program suite.</summary>
    internal static void VerifyViolaInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyViolaInstructionProgramDefinitions), () => VerifyViolaInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks Viola control words, four surface initializers, shared loop playback, and ROM isolation.</summary>
    internal static void VerifyViolaInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < ViolaInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ViolaInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | definition.Address),
                $"Viola mechanics word $A3:{definition.Address:X4}");
        }

        var guard = NewViolaInstructionReadGuard(rom);
        foreach (CrawlerSurfaceOrientation orientation in Enum.GetValues<CrawlerSurfaceOrientation>())
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Viola;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            // InitAI_Viola ($A3:B678) selects with the property bits; a different
            // population parameter proves the parameter is not the source.
            slot.CurrentInstruction = (ushort)((ushort)orientation ^ 1);
            slot.Properties = (ushort)orientation;
            slot.Parameter1 = 0;
            enemies.CrawlersBehavior.InitializeCrawler(slot, CrawlerAnimationFamily.Viola, (ushort?)6);

            ushort entry = CrawlerAnimationDefinitions.InitialInstruction(
                CrawlerAnimationFamily.Viola, orientation);
            AssertEqual(entry, slot.CurrentInstruction,
                $"Viola real initializer selects {orientation}");
            ExecuteViolaProgram(enemies, slot, callCount: 15);
            CrawlerEnemyFunction expected = orientation is
                CrawlerSurfaceOrientation.UpsideRight or
                CrawlerSurfaceOrientation.UpsideLeft
                    ? CrawlerEnemyFunction.CrawlingVertically
                    : CrawlerEnemyFunction.CrawlingHorizontally;
            AssertEqual(expected, (enemies.CrawlerStates[0] ?? throw new InvalidOperationException("Verification requires enemies.CrawlerStates[0].")).Function,
                $"Viola {orientation} publishes native movement function");
            AssertEqual(unchecked((ushort)(ViolaInstructionProgramDefinitions.NormalLoop + 4)),
                slot.CurrentInstruction,
                $"Viola {orientation} enters and loops all fourteen shared frames");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Viola execution uses compiled spritemap selectors");
        for (int index = 0;
             index < ViolaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                ViolaInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, EnemyDefinitionId.Viola,
                0xa3, address, $"Viola $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Viola mechanics byte");

        AssertThrows<InvalidDataException>(
            () => ViolaInstructionProgramDefinitions.ReadMechanicsWord(
                ViolaInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Viola spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ViolaInstructionProgramDefinitions.ReadMechanicsWord(
                ViolaInstructionProgramDefinitions.UnusedXFlipped),
            "retail-unused Viola program is outside production mechanics");

        _ = ProbeViolaInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeViolaInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Viola allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Viola mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Viola instruction mechanics: thirty compiled words, all four surface entries, " +
            "the shared fourteen-frame loop, and fourteen compiled spritemap selectors pass with " +
            "mechanics bytes forbidden.");
    }

    /// <summary>Advances the real enemy instruction runner through a bounded Viola program.</summary>
    private static void ExecuteViolaProgram(
        RoomEnemySystem enemies,
        RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int call = 0; call < callCount; call++)
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Alternates entry and loop word lookups to measure warmed allocation.</summary>
    private static int ProbeViolaInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ViolaInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ViolaInstructionProgramDefinitions.UpsideRight
                    : ViolaInstructionProgramDefinitions.NormalLoop);
        }
        return checksum;
    }

    /// <summary>Forbids compiled Viola mechanics bytes and observes presentation word reads.</summary>
    private static CartridgeReadGuard NewViolaInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(ViolaInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Viola mechanics byte ${address:X6}.")
            .ObserveWords(0xa30000, ViolaInstructionProgramDefinitions.PresentationWordCount, ViolaInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the Viola instruction program verification command.</summary>
    [VerificationCommand("--viola-instruction-program-definitions")]
    internal static int ViolaInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyViolaInstructionProgramDefinitions), () => VerifyViolaInstructionProgramDefinitions());
        return 0;
    }
}
