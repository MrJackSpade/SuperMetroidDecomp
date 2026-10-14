using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;

/// <summary>Checks compiled Magdollite animation programs through real head, hand, and pillar behavior.</summary>
internal static class MagdolliteInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge and runs the Magdollite instruction checks.</summary>
    internal static void VerifyMagdolliteInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMagdolliteInstructionProgramDefinitions), () => VerifyMagdolliteInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled words to the cartridge and exercises all Magdollite program families without runtime instruction reads.</summary>
    internal static void VerifyMagdolliteInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < MagdolliteInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MagdolliteInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa80000 | definition.Address)),
                $"Magdollite instruction mechanics word $A8:{definition.Address:X4}");
        }

        var guard = NewMagdolliteInstructionProgramGuard(rom);
        Suite(nameof(VerifyIdle), () => VerifyIdle(MagdolliteInstructionProgramDefinitions.LeftIdle, "left"));
        Suite(nameof(VerifyIdle), () => VerifyIdle(MagdolliteInstructionProgramDefinitions.RightIdle, "right"));
        Suite(nameof(VerifyThrow), () => VerifyThrow(MagdolliteInstructionProgramDefinitions.LeftThrow, "left"));
        Suite(nameof(VerifyThrow), () => VerifyThrow(MagdolliteInstructionProgramDefinitions.RightThrow, "right"));
        Suite(nameof(VerifySubmerge), () => VerifySubmerge(MagdolliteInstructionProgramDefinitions.LeftSubmerge, "left"));
        Suite(nameof(VerifySubmerge), () => VerifySubmerge(MagdolliteInstructionProgramDefinitions.RightSubmerge, "right"));
        Suite(nameof(VerifyEmerge), () => VerifyEmerge(MagdolliteInstructionProgramDefinitions.LeftEmerge, "left"));
        Suite(nameof(VerifyEmerge), () => VerifyEmerge(MagdolliteInstructionProgramDefinitions.RightEmerge, "right"));

        ushort[] pillarPrograms =
        [
            MagdolliteInstructionProgramDefinitions.PillarPhase0,
            MagdolliteInstructionProgramDefinitions.PillarPhase1,
            MagdolliteInstructionProgramDefinitions.PillarPhase2,
            MagdolliteInstructionProgramDefinitions.PillarPhase3,
            MagdolliteInstructionProgramDefinitions.PillarPhase4,
            MagdolliteInstructionProgramDefinitions.PillarPhase5,
            MagdolliteInstructionProgramDefinitions.PillarPhase6,
            MagdolliteInstructionProgramDefinitions.PillarPhase7,
        ];
        for (int phase = 0; phase < pillarPrograms.Length; phase++)
            VerifyStaticProgram(pillarPrograms[phase], MagdollitePart.RisingBody, $"pillar {phase}");
        Suite(nameof(VerifyStaticProgram), () => VerifyStaticProgram(
            MagdolliteInstructionProgramDefinitions.PillarCap,
            MagdollitePart.TrackingOverlay,
            "pillar cap"));

        for (int index = 0;
             index < MagdolliteInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                MagdolliteInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa80000 | address)),
                EnemySpritemapDefinitions.MagdolliteFrameAt(address),
                $"compiled Magdollite presentation $A8:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Magdollite mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => MagdolliteInstructionProgramDefinitions.ReadMechanicsWord(0xac9e),
            "Magdollite spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MagdolliteInstructionProgramDefinitions.ReadMechanicsWord(0xae12),
            "adjacent Magdollite callback code is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.MagdolliteFrameAt(0xae12),
            "adjacent Magdollite callback code is rejected as presentation");

        _ = ProbeMagdolliteInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMagdolliteInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Magdollite allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Magdollite mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"Magdollite instruction mechanics: " +
            $"{MagdolliteInstructionProgramDefinitionsTooling.MechanicsWordCount} compiled words, " +
            "seventeen complete head/pillar/hand programs, six real lava spawns, and " +
            $"{MagdolliteInstructionProgramDefinitionsTooling.PresentationWordCount} compiled " +
            "visual selections pass with both source classes forbidden.");

        void VerifyIdle(ushort program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, _) =
                CreateSystem(MagdollitePart.Head);
            Install(slot, program);
            RunFrames(enemies, slot, 55);
            AssertTrue(slot.CurrentInstruction >= program &&
                slot.CurrentInstruction <= unchecked((ushort)(program + 0x10)),
                $"Magdollite {direction} idle remains inside its loop");
        }

        void VerifyThrow(ushort program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.TrackingOverlay);
            state.ThrowOrigin = slot.Position.Whole;
            Install(slot, program);
            RunFrames(enemies, slot, 80);
            AssertEqual((ushort)0x0061, (enemies.MagdolliteBehavior.LastMagdolliteSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.MagdolliteBehavior.LastMagdolliteSoundEffect.")),
                $"Magdollite {direction} throw sound");
            AssertEqual(3,
                enemies.Projectiles.EnemyProjectiles.Count(projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite),
                $"Magdollite {direction} throw projectile count");
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} throw releases animation wait");
        }

        void VerifySubmerge(ushort program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot head, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.Head);
            Install(head, program);
            RunFrames(enemies, head, 80);
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} submerge releases animation wait");
            AssertTrue(!enemies.SlotTable.Slots[1].Properties.HasAny(EnemyProperties.Invisible),
                $"Magdollite {direction} submerge reveals pillar");
            AssertTrue(!enemies.SlotTable.Slots[2].Properties.HasAny(EnemyProperties.Invisible),
                $"Magdollite {direction} submerge reveals hand");
        }

        void VerifyEmerge(ushort program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot head, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.Head);
            enemies.SlotTable.Slots[1].Properties =
                enemies.SlotTable.Slots[1].Properties.Without(EnemyProperties.Invisible);
            enemies.SlotTable.Slots[2].Properties =
                enemies.SlotTable.Slots[2].Properties.Without(EnemyProperties.Invisible);
            Install(head, program);
            RunFrames(enemies, head, 80);
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} emerge releases animation wait");
            AssertTrue(enemies.SlotTable.Slots[1].Properties.HasAny(EnemyProperties.Invisible),
                $"Magdollite {direction} emerge hides pillar");
            AssertTrue(enemies.SlotTable.Slots[2].Properties.HasAny(EnemyProperties.Invisible),
                $"Magdollite {direction} emerge hides hand");
        }

        void VerifyStaticProgram(ushort program, MagdollitePart part, string name)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, _) = CreateSystem(part);
            Install(slot, program);
            RunFrames(enemies, slot, 3);
            AssertEqual(unchecked((ushort)(program + 4)), slot.CurrentInstruction,
                $"Magdollite {name} reaches terminal sleep");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot, MagdolliteEnemyState State) CreateSystem(
            MagdollitePart selectedPart)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot, SamusState?> initialize = enemies.MagdolliteBehavior.InitializeMagdollite;
            for (int index = 0; index < 3; index++)
            {
                RoomEnemySlot member = enemies.SlotTable.Slots[index];
                member.EnemyDefinitionPointer = EnemyDefinitionId.Magdollite;
                member.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
                member.Parameter1 = (ushort)index;
                member.Parameter2 = 0;
                member.Position = member.Position.WithWhole(new(0x0080, 0x0080));
                initialize(member, null);
            }

            RoomEnemySlot selected = enemies.SlotTable.Slots[(int)selectedPart];
            return (enemies, selected, (enemies.MagdolliteStates[(int)selectedPart] ?? throw new InvalidOperationException("Verification requires the initialized Magdollite state.")));
        }

        static void Install(RoomEnemySlot slot, ushort program)
        {
            slot.CurrentInstruction = program;
            slot.InstructionTimer = NativeCountdown.Start(1);
            slot.Timer = NativeCountdown.Start(0);
        }

        static void RunFrames(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Repeatedly resolves a compiled idle instruction to measure warmed lookup allocations.</summary>
    private static int ProbeMagdolliteInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MagdolliteInstructionProgramDefinitions.ReadMechanicsWord(
                MagdolliteInstructionProgramDefinitions.LeftIdle);
        }
        return checksum;
    }

    /// <summary>Rejects production reads of the installed Magdollite mechanics and presentation bytes.</summary>
    private static CartridgeReadGuard NewMagdolliteInstructionProgramGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => MagdolliteInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                (SnesAddressMath.BankBase(address)) == 0xa80000 &&
                (MagdolliteInstructionProgramDefinitions.IsPresentationWord(unchecked((ushort)address)) ||
                 MagdolliteInstructionProgramDefinitions.IsPresentationWord(unchecked((ushort)(address - 1)))),
            address => $"Production read compiled Magdollite instruction byte ${address:X6}.");

    /// <summary>Runs the standalone Magdollite instruction mechanics verification command.</summary>
    [VerificationCommand("--magdollite-instruction-mechanics")]
    internal static int MagdolliteInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyMagdolliteInstructionProgramDefinitions), () => VerifyMagdolliteInstructionProgramDefinitions());
        return 0;
    }
}
