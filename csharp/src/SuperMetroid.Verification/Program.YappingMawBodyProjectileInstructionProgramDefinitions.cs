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
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;

/// <summary>Checks both Yapping Maw body-projectile poses and their terminal sleep instructions.</summary>
internal static class YappingMawBodyProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge as the oracle for body-projectile instruction words.</summary>
    internal static void VerifyYappingMawBodyProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYappingMawBodyProjectileInstructionProgramDefinitions), () => VerifyYappingMawBodyProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Exercises real up- and down-facing spawns, sprite selection, sleep, and shared shot deletion.</summary>
    internal static void VerifyYappingMawBodyProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"Yapping Maw body-projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewYappingMawBodyProjectileInstructionReadGuard(rom);
        RoomEnemyProjectileSlot? deletionCandidate = null;

        foreach ((ushort parameter, ushort expected, string facing) in new[]
        {
            ((ushort)0, YappingMawBodyProjectileInstructionProgramDefinitions.FacingDown,
                "down"),
            ((ushort)1, YappingMawBodyProjectileInstructionProgramDefinitions.FacingUp,
                "up"),
        })
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            RoomEnemySlot owner = enemies.SlotTable.Slots[0];
            owner.EnemyDefinitionPointer = EnemyDefinitionId.YappingMaw;
            owner.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            owner.Position = owner.Position.WithWhole(new(128, 112));
            owner.Parameter1 = 128;
            owner.Parameter2 = parameter;
            enemies.YappingMawBehavior.InitializeYappingMaw(owner);

            RoomEnemyProjectileSlot[] bodies = enemies.YappingMawStates[0]!
                .BodyProjectiles
                .OfType<RoomEnemyProjectileSlot>()
                .ToArray();
            AssertEqual(4, bodies.Length,
                $"real {facing}-facing Yapping Maw producer spawns four body links");
            foreach (RoomEnemyProjectileSlot body in bodies)
            {
                AssertEqual(expected, body.InstructionPointer,
                    $"real {facing}-facing Yapping Maw body selects its named pose");
                RunForcedTick(enemies, body);
                AssertEqual(unchecked((ushort)(expected + 2)), body.PresentationOperandAddress,
                    $"actual {facing}-facing body selects its installed presentation operand");
                AssertEqual(EnemyProjectileSpritemapDefinitions.BlankSpritemap, body.SpritemapPointer,
                    $"actual {facing}-facing body defers artwork resolution to the draw pass");
                AssertEqual(unchecked((ushort)(expected + 4)), body.InstructionPointer,
                    $"{facing}-facing Yapping Maw body schedules its terminal sleep");
                RunForcedTick(enemies, body);
                AssertEqual(unchecked((ushort)(expected + 2)), body.PresentationOperandAddress,
                    $"actual {facing}-facing body selects its installed presentation operand");
                AssertEqual(EnemyProjectileSpritemapDefinitions.BlankSpritemap, body.SpritemapPointer,
                    $"actual {facing}-facing body defers artwork resolution to the draw pass");
                AssertEqual(unchecked((ushort)(expected + 4)), body.InstructionPointer,
                    $"{facing}-facing Yapping Maw body sleeps at the authored opcode");
                AssertEqual((ushort)0, body.InstructionTimer.RawValue,
                    $"{facing}-facing Yapping Maw sleep leaves the timer stopped");
            }

            deletionCandidate ??= bodies[0];

            void RunForcedTick(
                RoomEnemySystem system,
                RoomEnemyProjectileSlot body)
            {
                body.InstructionTimer = NativeCountdown.Start(1);
                system.Projectiles.ProcessEnemyProjectileInstructions(body, new SamusState(), Position16<RoomPixels>.Zero);
                VerifyExecutedProjectileFrame(rom, body, spriteArtwork, executedOperands);
            }
        }

        var deletionSystem = new RoomEnemySystem();
        deletionSystem.BindFixtureBus(guard);
        RoomEnemyProjectileSlot deletion = deletionCandidate!;
        deletion.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        deletion.InstructionTimer = NativeCountdown.Start(1);
        deletionSystem.Projectiles.ProcessEnemyProjectileInstructions(deletion, new SamusState(), Position16<RoomPixels>.Zero);
        AssertTrue(!deletion.IsActive,
            "Yapping Maw body shot reaction reaches the compiled shared delete program");

        AssertEqual(0, guard.ObservedWords.Count,
            "YappingMawBodyProjectile execution performs no live spritemap operand reads");
        AssertEqual(YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "YappingMawBodyProjectile executes every native visual operand");
        for (int index = 0; index < YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"YappingMawBodyProjectile executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "YappingMawBodyProjectile has a compiled visual selector");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address), selector,
                "YappingMawBodyProjectile compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids Yapping Maw body and shared-delete mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => YappingMawBodyProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                0xec58),
            "Yapping Maw body spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => YappingMawBodyProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                0xec62),
            "adjacent Yapping Maw body initializer is rejected as mechanics");

        _ = ProbeYappingMawBodyProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeYappingMawBodyProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Yapping Maw body allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Yapping Maw body mechanics lookups allocate no storage");

        Console.WriteLine(
            "Yapping Maw body-projectile instruction mechanics: four compiled words, " +
            "both real facing producers, eight terminal sleeps, shared shot deletion, " +
            "and both installed sprite frames pass with mechanics and visual operand ROM reads forbidden.");
    }

    /// <summary>Alternates both compiled pose words repeatedly to expose allocation in mechanics lookup.</summary>
    private static int ProbeYappingMawBodyProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += YappingMawBodyProjectileInstructionProgramDefinitions
                .ReadMechanicsWord((index & 1) == 0
                    ? YappingMawBodyProjectileInstructionProgramDefinitions.FacingDown
                    : YappingMawBodyProjectileInstructionProgramDefinitions.FacingUp);
        }
        return checksum;
    }

    /// <summary>Rejects private and shared mechanics reads while recording live body spritemap operand reads.</summary>
    private static CartridgeReadGuard NewYappingMawBodyProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => YappingMawBodyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Yapping Maw body mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordCount, YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the Yapping Maw body-projectile instruction check from the command dispatcher.</summary>
    [VerificationCommand("--yapping-maw-body-projectile-instruction-mechanics")]
    internal static int YappingMawBodyProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyYappingMawBodyProjectileInstructionProgramDefinitions), () => VerifyYappingMawBodyProjectileInstructionProgramDefinitions());
        return 0;
    }
}
