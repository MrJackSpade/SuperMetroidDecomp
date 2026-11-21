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
        Suite(nameof(VerifyIdle), () => VerifyIdle(MagdolliteHeadSegment.LeftIdle, "left"));
        Suite(nameof(VerifyIdle), () => VerifyIdle(MagdolliteHeadSegment.RightIdle, "right"));
        Suite(nameof(VerifyThrow), () => VerifyThrow(MagdolliteOverlaySegment.LeftThrow, "left"));
        Suite(nameof(VerifyThrow), () => VerifyThrow(MagdolliteOverlaySegment.RightThrow, "right"));
        Suite(nameof(VerifySubmerge), () => VerifySubmerge(MagdolliteHeadSegment.LeftSubmerge, "left"));
        Suite(nameof(VerifySubmerge), () => VerifySubmerge(MagdolliteHeadSegment.RightSubmerge, "right"));
        Suite(nameof(VerifyEmerge), () => VerifyEmerge(MagdolliteHeadSegment.LeftEmerge, "left"));
        Suite(nameof(VerifyEmerge), () => VerifyEmerge(MagdolliteHeadSegment.RightEmerge, "right"));

        MagdolliteBodySegment[] pillarPrograms =
        [
            MagdolliteBodySegment.OneSegmentPillar,
            MagdolliteBodySegment.TwoSegmentPillar,
            MagdolliteBodySegment.ThreeSegmentPillar,
            MagdolliteBodySegment.FourSegmentPillar,
            MagdolliteBodySegment.FiveSegmentPillar,
            MagdolliteBodySegment.SixSegmentPillar,
            MagdolliteBodySegment.SevenSegmentPillar,
            MagdolliteBodySegment.EightSegmentPillar,
        ];
        for (int phase = 0; phase < pillarPrograms.Length; phase++)
            VerifyPillar(pillarPrograms[phase], $"pillar {phase}");
        Suite(nameof(VerifyCap), VerifyCap);

        Suite(nameof(MagdolliteAnimationsVerification.VerifyMagdolliteAnimations), () => MagdolliteAnimationsVerification.VerifyMagdolliteAnimations(rom));
        Console.WriteLine("Magdollite: idle, throw, submerge, emerge, pillar and cap programs with their cues pass.");

        void VerifyIdle(MagdolliteHeadSegment program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, MagdollitePartState part) =
                CreateSystem(typeof(MagdolliteHeadState));
            var state = (MagdolliteHeadState)part;
            state.Playback = new(MagdolliteHeadAnimations.StartingAt[program], AnimationPlace.StartOfAnimation);
            place.AnimationClock.Restart();
            RunFrames(enemies, place, 55);
            AssertEqual(program, state.Playback.Animation.Phases[state.Playback.Place.Phase].Segment,
                $"Magdollite {direction} idle remains inside its loop");
        }

        void VerifyThrow(MagdolliteOverlaySegment program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, MagdollitePartState state) =
                CreateSystem(typeof(MagdolliteOverlayState));
            var overlay = (MagdolliteOverlayState)state;
            overlay.ThrowOrigin = place.Position.Whole;
            // The hand waits for the throw it starts (#2439).
            overlay.Action = MagdolliteOverlayAction.WaitingForThrowAnimation;
            overlay.Playback = new(MagdolliteOverlayAnimations.StartingAt[program], AnimationPlace.StartOfAnimation);
            place.AnimationClock.Restart();
            RunFrames(enemies, place, 80);
            AssertEqual(EnemySound.FireSpit, (enemies.Audio.LatestSound ?? throw new InvalidOperationException("Verification requires a pending Magdollite sound.")),
                $"Magdollite {direction} throw sound");
            AssertEqual(3,
                enemies.Projectiles.EnemyProjectiles.Count(projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite),
                $"Magdollite {direction} throw projectile count");
            AssertEqual(MagdolliteOverlayAction.Thrown, overlay.Action,
                $"Magdollite {direction} throw finishes the hand's wait");
        }

        void VerifySubmerge(MagdolliteHeadSegment program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace head, MagdollitePartState state) =
                CreateSystem(typeof(MagdolliteHeadState));
            // The head waits for the submerge it starts (#2439).
            ((MagdolliteHeadState)state).Action = MagdolliteHeadAction.WaitingForAttackAnimation;
            ((MagdolliteHeadState)state).Playback = new(MagdolliteHeadAnimations.StartingAt[program], AnimationPlace.StartOfAnimation);
            head.AnimationClock.Restart();
            RunFrames(enemies, head, 80);
            AssertEqual(MagdolliteHeadAction.Submerged, ((MagdolliteHeadState)state).Action,
                $"Magdollite {direction} submerge finishes the head's wait");
            AssertTrue(!enemies.Places.InUpdateOrder[1].Presence.HasAny(EnemyPresence.Hidden),
                $"Magdollite {direction} submerge reveals pillar");
            AssertTrue(!enemies.Places.InUpdateOrder[2].Presence.HasAny(EnemyPresence.Hidden),
                $"Magdollite {direction} submerge reveals hand");
        }

        void VerifyEmerge(MagdolliteHeadSegment program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace head, MagdollitePartState state) =
                CreateSystem(typeof(MagdolliteHeadState));
            enemies.Places.InUpdateOrder[1].Presence = enemies.Places.InUpdateOrder[1].Presence.Without(EnemyPresence.Hidden);
            enemies.Places.InUpdateOrder[2].Presence = enemies.Places.InUpdateOrder[2].Presence.Without(EnemyPresence.Hidden);
            // The head waits for the emergence it starts (#2439).
            ((MagdolliteHeadState)state).Action = MagdolliteHeadAction.WaitingForReturnAnimation;
            ((MagdolliteHeadState)state).Playback = new(MagdolliteHeadAnimations.StartingAt[program], AnimationPlace.StartOfAnimation);
            head.AnimationClock.Restart();
            RunFrames(enemies, head, 80);
            AssertEqual(MagdolliteHeadAction.Surfaced, ((MagdolliteHeadState)state).Action,
                $"Magdollite {direction} emerge finishes the head's wait");
            AssertTrue(enemies.Places.InUpdateOrder[1].Presence.HasAny(EnemyPresence.Hidden),
                $"Magdollite {direction} emerge hides pillar");
            AssertTrue(enemies.Places.InUpdateOrder[2].Presence.HasAny(EnemyPresence.Hidden),
                $"Magdollite {direction} emerge hides hand");
        }

        void VerifyPillar(MagdolliteBodySegment program, string name)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, MagdollitePartState part) = CreateSystem(typeof(MagdolliteBodyState));
            var state = (MagdolliteBodyState)part;
            state.Playback = new(MagdolliteBodyAnimations.StartingAt[program], AnimationPlace.StartOfAnimation);
            place.AnimationClock.Restart();
            RunFrames(enemies, place, 3);
            AssertHeldAtEnd(state.Playback, program, name);
        }

        void VerifyCap()
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, MagdollitePartState part) = CreateSystem(typeof(MagdolliteOverlayState));
            var state = (MagdolliteOverlayState)part;
            state.Playback = new(MagdolliteOverlayAnimations.StartingAt[MagdolliteOverlaySegment.PillarCap], AnimationPlace.StartOfAnimation);
            place.AnimationClock.Restart();
            RunFrames(enemies, place, 3);
            AssertHeldAtEnd(state.Playback, MagdolliteOverlaySegment.PillarCap, "pillar cap");
        }

        // The playback holds past the last beat of an animation that holds there.
        static void AssertHeldAtEnd<TSegment>(AnimationPlayback<TSegment> playback, TSegment program, string name) where TSegment : struct
        {
            AssertEqual(program, playback.Animation.Phases[playback.Place.Phase].Segment, $"Magdollite {name} stays in its program");
            AssertTrue(playback.Animation.End is AnimationEnd.Hold &&
                playback.Place == AnimationPlace.HoldingIn(playback.Animation.Phases.Count - 1, playback.Animation.Phases[^1].BeatCount),
                $"Magdollite {name} reaches terminal sleep");
        }

        CreateSystemResult CreateSystem(
            Type selectedPart)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemyPlace, SamusState> initialize = static (place, samus) => place.InitializeAsPlaced(samus);
            for (int index = 0; index < 3; index++)
            {
                RoomEnemyPlace member = enemies.Places.InUpdateOrder[index];
                member.SetSpawnParameter1(EnemyDefinitionId.Magdollite, (ushort)index);
                member.SetSpawnParameter2(EnemyDefinitionId.Magdollite, 0);
                member.Bind(EnemyDefinitionId.Magdollite, member.KeptDefinition);
                member.Bind(member.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
                member.Position = member.Position.WithWhole(new(0x0080, 0x0080));
                initialize(member, RoomEnemyFrameFixtures.StandaloneSamus());
            }

            // Each placement spawned as the part its parameter names; the selected part is the one whose state's type names
            // it (#2474).
            int position = Enumerable.Range(0, 3).Single(index => enemies.MagdolliteStates[index]?.GetType() == selectedPart);
            RoomEnemyPlace selected = enemies.Places.InUpdateOrder[position];
            return new CreateSystemResult(enemies, selected, (enemies.MagdolliteStates[position] ?? throw new InvalidOperationException("Verification requires the initialized Magdollite state.")));
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
