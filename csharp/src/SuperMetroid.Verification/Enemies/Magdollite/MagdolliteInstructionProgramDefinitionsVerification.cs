using SuperMetroid.Core.Input;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
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
        CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyIdle), () => VerifyIdle(MagdolliteSegment.LeftIdle, "left"));
        Suite(nameof(VerifyIdle), () => VerifyIdle(MagdolliteSegment.RightIdle, "right"));
        Suite(nameof(VerifyThrow), () => VerifyThrow(MagdolliteSegment.LeftThrow, "left"));
        Suite(nameof(VerifyThrow), () => VerifyThrow(MagdolliteSegment.RightThrow, "right"));
        Suite(nameof(VerifySubmerge), () => VerifySubmerge(MagdolliteSegment.LeftSubmerge, "left"));
        Suite(nameof(VerifySubmerge), () => VerifySubmerge(MagdolliteSegment.RightSubmerge, "right"));
        Suite(nameof(VerifyEmerge), () => VerifyEmerge(MagdolliteSegment.LeftEmerge, "left"));
        Suite(nameof(VerifyEmerge), () => VerifyEmerge(MagdolliteSegment.RightEmerge, "right"));

        MagdolliteSegment[] pillarPrograms =
        [
            MagdolliteSegment.PillarPhase0,
            MagdolliteSegment.PillarPhase1,
            MagdolliteSegment.PillarPhase2,
            MagdolliteSegment.PillarPhase3,
            MagdolliteSegment.PillarPhase4,
            MagdolliteSegment.PillarPhase5,
            MagdolliteSegment.PillarPhase6,
            MagdolliteSegment.PillarPhase7,
        ];
        for (int phase = 0; phase < pillarPrograms.Length; phase++)
            VerifyStaticProgram(pillarPrograms[phase], MagdollitePart.RisingBody, $"pillar {phase}");
        Suite(nameof(VerifyStaticProgram), () => VerifyStaticProgram(
            MagdolliteSegment.PillarCap,
            MagdollitePart.TrackingOverlay,
            "pillar cap"));

        Suite(nameof(MagdolliteAnimationsVerification.VerifyMagdolliteAnimations), () => MagdolliteAnimationsVerification.VerifyMagdolliteAnimations(rom));
        Console.WriteLine("Magdollite: idle, throw, submerge, emerge, pillar and cap programs with their cues pass.");

        void VerifyIdle(MagdolliteSegment program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.Head);
            Install(slot, state, program);
            RunFrames(enemies, slot, 55);
            AssertEqual(program, state.Playback.Animation.Phases[state.Playback.Place.Phase].Segment,
                $"Magdollite {direction} idle remains inside its loop");
        }

        void VerifyThrow(MagdolliteSegment program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.TrackingOverlay);
            state.ThrowOrigin = slot.Position.Whole;
            Install(slot, state, program);
            RunFrames(enemies, slot, 80);
            AssertEqual(EnemySound.FireSpit, (enemies.Audio.LatestSound ?? throw new InvalidOperationException("Verification requires a pending Magdollite sound.")),
                $"Magdollite {direction} throw sound");
            AssertEqual(3,
                enemies.Projectiles.EnemyProjectiles.Count(projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite),
                $"Magdollite {direction} throw projectile count");
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} throw releases animation wait");
        }

        void VerifySubmerge(MagdolliteSegment program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot head, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.Head);
            Install(head, state, program);
            RunFrames(enemies, head, 80);
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} submerge releases animation wait");
            AssertTrue(!enemies.SlotTable.Slots[1].Presence.HasAny(EnemyPresence.Hidden),
                $"Magdollite {direction} submerge reveals pillar");
            AssertTrue(!enemies.SlotTable.Slots[2].Presence.HasAny(EnemyPresence.Hidden),
                $"Magdollite {direction} submerge reveals hand");
        }

        void VerifyEmerge(MagdolliteSegment program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot head, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.Head);
            enemies.SlotTable.Slots[1].Presence = enemies.SlotTable.Slots[1].Presence.Without(EnemyPresence.Hidden);
            enemies.SlotTable.Slots[2].Presence = enemies.SlotTable.Slots[2].Presence.Without(EnemyPresence.Hidden);
            Install(head, state, program);
            RunFrames(enemies, head, 80);
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} emerge releases animation wait");
            AssertTrue(enemies.SlotTable.Slots[1].Presence.HasAny(EnemyPresence.Hidden),
                $"Magdollite {direction} emerge hides pillar");
            AssertTrue(enemies.SlotTable.Slots[2].Presence.HasAny(EnemyPresence.Hidden),
                $"Magdollite {direction} emerge hides hand");
        }

        void VerifyStaticProgram(MagdolliteSegment program, MagdollitePart part, string name)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MagdolliteEnemyState state) = CreateSystem(part);
            Install(slot, state, program);
            RunFrames(enemies, slot, 3);
            // The playback holds past the last beat of an animation that holds there.
            AnimationPlayback<MagdolliteSegment> playback = state.Playback;
            AssertEqual(program, playback.Animation.Phases[playback.Place.Phase].Segment, $"Magdollite {name} stays in its program");
            AssertTrue(playback.Animation.End is AnimationEnd.Hold &&
                playback.Place == AnimationPlace.HoldingIn(playback.Animation.Phases.Count - 1, playback.Animation.Phases[^1].BeatCount),
                $"Magdollite {name} reaches terminal sleep");
        }

        CreateSystemResult CreateSystem(
            MagdollitePart selectedPart)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
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

        static void Install(RoomEnemySlot slot, MagdolliteEnemyState state, MagdolliteSegment program)
        {
            state.Playback = new(MagdolliteAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            slot.AnimationClock.Restart();
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
