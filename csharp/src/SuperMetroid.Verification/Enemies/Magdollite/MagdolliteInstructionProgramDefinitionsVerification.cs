using SuperMetroid.Core.Input;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Enemies.Magdollite;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks compiled Magdollite animation programs through real head, hand, and pillar behavior.</summary>
internal static class MagdolliteInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge and runs the Magdollite instruction checks.</summary>
    internal static void VerifyMagdolliteInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMagdolliteInstructionProgramDefinitions), () => VerifyMagdolliteInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled words to the cartridge and exercises all Magdollite program families without runtime instruction reads.</summary>
    internal static void VerifyMagdolliteInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyIdle), () => VerifyIdle(MagdolliteProgram.LeftIdle, "left"));
        Suite(nameof(VerifyIdle), () => VerifyIdle(MagdolliteProgram.RightIdle, "right"));
        Suite(nameof(VerifyThrow), () => VerifyThrow(MagdolliteProgram.LeftThrow, "left"));
        Suite(nameof(VerifyThrow), () => VerifyThrow(MagdolliteProgram.RightThrow, "right"));
        Suite(nameof(VerifySubmerge), () => VerifySubmerge(MagdolliteProgram.LeftSubmerge, "left"));
        Suite(nameof(VerifySubmerge), () => VerifySubmerge(MagdolliteProgram.RightSubmerge, "right"));
        Suite(nameof(VerifyEmerge), () => VerifyEmerge(MagdolliteProgram.LeftEmerge, "left"));
        Suite(nameof(VerifyEmerge), () => VerifyEmerge(MagdolliteProgram.RightEmerge, "right"));

        MagdolliteProgram[] pillarPrograms =
        [
            MagdolliteProgram.PillarPhase0,
            MagdolliteProgram.PillarPhase1,
            MagdolliteProgram.PillarPhase2,
            MagdolliteProgram.PillarPhase3,
            MagdolliteProgram.PillarPhase4,
            MagdolliteProgram.PillarPhase5,
            MagdolliteProgram.PillarPhase6,
            MagdolliteProgram.PillarPhase7,
        ];
        for (int phase = 0; phase < pillarPrograms.Length; phase++)
            VerifyStaticProgram(pillarPrograms[phase], MagdollitePart.RisingBody, $"pillar {phase}");
        Suite(nameof(VerifyStaticProgram), () => VerifyStaticProgram(
            MagdolliteProgram.PillarCap,
            MagdollitePart.TrackingOverlay,
            "pillar cap"));

        Suite(nameof(MagdolliteAnimationsVerification.VerifyMagdolliteAnimations), () => MagdolliteAnimationsVerification.VerifyMagdolliteAnimations(rom));
        Console.WriteLine("Magdollite: idle, throw, submerge, emerge, pillar and cap programs with their cues pass.");

        void VerifyIdle(MagdolliteProgram program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.Head);
            Install(slot, state, program);
            RunFrames(enemies, slot, 55);
            AssertEqual(program, state.Program.Program,
                $"Magdollite {direction} idle remains inside its loop");
        }

        void VerifyThrow(MagdolliteProgram program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.TrackingOverlay);
            state.ThrowOrigin = slot.Position.Whole;
            Install(slot, state, program);
            RunFrames(enemies, slot, 80);
            AssertEqual(Library2Sound.FireSpit, (enemies.MagdolliteBehavior.LastMagdolliteSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.MagdolliteBehavior.LastMagdolliteSoundEffect.")),
                $"Magdollite {direction} throw sound");
            AssertEqual(3,
                enemies.Projectiles.EnemyProjectiles.Count(projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite),
                $"Magdollite {direction} throw projectile count");
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} throw releases animation wait");
        }

        void VerifySubmerge(MagdolliteProgram program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot head, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.Head);
            Install(head, state, program);
            RunFrames(enemies, head, 80);
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} submerge releases animation wait");
            AssertTrue(!enemies.SlotTable.Slots[1].Properties.HasAny(EnemyProperties.Invisible),
                $"Magdollite {direction} submerge reveals pillar");
            AssertTrue(!enemies.SlotTable.Slots[2].Properties.HasAny(EnemyProperties.Invisible),
                $"Magdollite {direction} submerge reveals hand");
        }

        void VerifyEmerge(MagdolliteProgram program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot head, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.Head);
            enemies.SlotTable.Slots[1].Properties =
                enemies.SlotTable.Slots[1].Properties.Without(EnemyProperties.Invisible);
            enemies.SlotTable.Slots[2].Properties =
                enemies.SlotTable.Slots[2].Properties.Without(EnemyProperties.Invisible);
            Install(head, state, program);
            RunFrames(enemies, head, 80);
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} emerge releases animation wait");
            AssertTrue(enemies.SlotTable.Slots[1].Properties.HasAny(EnemyProperties.Invisible),
                $"Magdollite {direction} emerge hides pillar");
            AssertTrue(enemies.SlotTable.Slots[2].Properties.HasAny(EnemyProperties.Invisible),
                $"Magdollite {direction} emerge hides hand");
        }

        void VerifyStaticProgram(MagdolliteProgram program, MagdollitePart part, string name)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MagdolliteEnemyState state) = CreateSystem(part);
            Install(slot, state, program);
            RunFrames(enemies, slot, 3);
            // The cursor rests on the program's sleep step.
            AssertEqual(program, state.Program.Program, $"Magdollite {name} stays in its program");
            AssertTrue(MagdolliteAnimations.Programs.Steps(program)[state.Program.Position] is AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue>.HoldStep, $"Magdollite {name} reaches terminal sleep");
        }

        CreateSystemResult CreateSystem(
            MagdollitePart selectedPart)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemySlot, SamusState?> initialize = enemies.MagdolliteBehavior.InitializeMagdollite;
            for (int index = 0; index < 3; index++)
            {
                RoomEnemySlot member = enemies.SlotTable.Slots[index];
                member.Bind(EnemyDefinitionId.Magdollite, member.Definition);
                member.Bind(member.DefinitionId, default(RoomEnemyDefinition));
                member.SetSpawnParameter1(EnemyDefinitionId.Magdollite, (ushort)index);
                member.SetSpawnParameter2(EnemyDefinitionId.Magdollite, 0);
                member.Position = member.Position.WithWhole(new(0x0080, 0x0080));
                initialize(member, null);
            }

            RoomEnemySlot selected = enemies.SlotTable.Slots[(int)selectedPart];
            return new CreateSystemResult(enemies, selected, (enemies.MagdolliteStates[(int)selectedPart] ?? throw new InvalidOperationException("Verification requires the initialized Magdollite state.")));
        }

        static void Install(RoomEnemySlot slot, MagdolliteEnemyState state, MagdolliteProgram program)
        {
            state.Program = AnimationProgramCursor<MagdolliteProgram>.Start(program);
            slot.InstructionTimer = WrappingCountdown.Start(1);
            slot.Timer = WrappingCountdown.Start(0);
        }

        static void RunFrames(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Runs the standalone Magdollite instruction mechanics verification command.</summary>
    [VerificationCommand("--magdollite-instruction-mechanics")]
    internal static int MagdolliteInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyMagdolliteInstructionProgramDefinitions), () => VerifyMagdolliteInstructionProgramDefinitions());
        return 0;
    }
}
