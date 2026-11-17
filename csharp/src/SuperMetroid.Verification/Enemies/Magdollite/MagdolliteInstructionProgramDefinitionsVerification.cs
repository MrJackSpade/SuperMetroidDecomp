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
            MagdolliteSegment.OneSegmentPillar,
            MagdolliteSegment.TwoSegmentPillar,
            MagdolliteSegment.ThreeSegmentPillar,
            MagdolliteSegment.FourSegmentPillar,
            MagdolliteSegment.FiveSegmentPillar,
            MagdolliteSegment.SixSegmentPillar,
            MagdolliteSegment.SevenSegmentPillar,
            MagdolliteSegment.EightSegmentPillar,
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
            (RoomEnemySystem enemies, RoomEnemyPlace place, MagdollitePartState state) =
                CreateSystem(MagdollitePart.Head);
            Install(place, state, program);
            RunFrames(enemies, place, 55);
            AssertEqual(program, state.Playback.Animation.Phases[state.Playback.Place.Phase].Segment,
                $"Magdollite {direction} idle remains inside its loop");
        }

        void VerifyThrow(MagdolliteSegment program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, MagdollitePartState state) =
                CreateSystem(MagdollitePart.TrackingOverlay);
            ((MagdolliteOverlayState)state).ThrowOrigin = place.Position.Whole;
            Install(place, state, program);
            RunFrames(enemies, place, 80);
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
            (RoomEnemySystem enemies, RoomEnemyPlace head, MagdollitePartState state) =
                CreateSystem(MagdollitePart.Head);
            Install(head, state, program);
            RunFrames(enemies, head, 80);
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} submerge releases animation wait");
            AssertTrue(!enemies.Places.InUpdateOrder[1].Presence.HasAny(EnemyPresence.Hidden),
                $"Magdollite {direction} submerge reveals pillar");
            AssertTrue(!enemies.Places.InUpdateOrder[2].Presence.HasAny(EnemyPresence.Hidden),
                $"Magdollite {direction} submerge reveals hand");
        }

        void VerifyEmerge(MagdolliteSegment program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace head, MagdollitePartState state) =
                CreateSystem(MagdollitePart.Head);
            enemies.Places.InUpdateOrder[1].Presence = enemies.Places.InUpdateOrder[1].Presence.Without(EnemyPresence.Hidden);
            enemies.Places.InUpdateOrder[2].Presence = enemies.Places.InUpdateOrder[2].Presence.Without(EnemyPresence.Hidden);
            Install(head, state, program);
            RunFrames(enemies, head, 80);
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} emerge releases animation wait");
            AssertTrue(enemies.Places.InUpdateOrder[1].Presence.HasAny(EnemyPresence.Hidden),
                $"Magdollite {direction} emerge hides pillar");
            AssertTrue(enemies.Places.InUpdateOrder[2].Presence.HasAny(EnemyPresence.Hidden),
                $"Magdollite {direction} emerge hides hand");
        }

        void VerifyStaticProgram(MagdolliteSegment program, MagdollitePart part, string name)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, MagdollitePartState state) = CreateSystem(part);
            Install(place, state, program);
            RunFrames(enemies, place, 3);
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
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemyPlace, SamusState> initialize = static (place, samus) => place.InitializeAsPlaced(samus);
            for (int index = 0; index < 3; index++)
            {
                RoomEnemyPlace member = enemies.Places.InUpdateOrder[index];
                member.SetSpawnParameter1(EnemyDefinitionId.Magdollite, (ushort)index);
                member.SetSpawnParameter2(EnemyDefinitionId.Magdollite, 0);
                member.Bind(EnemyDefinitionId.Magdollite, member.Definition);
                member.Bind(member.DefinitionId, default(RoomEnemyDefinition));
                member.Position = member.Position.WithWhole(new(0x0080, 0x0080));
                initialize(member, RoomEnemyFrameFixtures.StandaloneSamus());
            }

            // Each placement spawned as the part its parameter names; the selected part is the one whose state says so.
            int position = Enumerable.Range(0, 3).Single(index => enemies.MagdolliteStates[index]?.Part == selectedPart);
            RoomEnemyPlace selected = enemies.Places.InUpdateOrder[position];
            return new CreateSystemResult(enemies, selected, (enemies.MagdolliteStates[position] ?? throw new InvalidOperationException("Verification requires the initialized Magdollite state.")));
        }

        static void Install(RoomEnemyPlace place, MagdollitePartState state, MagdolliteSegment program)
        {
            state.Playback = new(MagdolliteAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            place.AnimationClock.Restart();
        }

        static void RunFrames(RoomEnemySystem enemies, RoomEnemyPlace place, int frames)
        {
            object?[] arguments = [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
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
