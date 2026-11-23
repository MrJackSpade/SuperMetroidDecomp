using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Powamp;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Input;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

/// <summary>Checks Powamp's initial animations and the inflation its balloon records as its animation shows each pose.</summary>
internal static class PowampBehaviorVerification
{
    /// <summary>
    /// The real initializers start the balloon deflated and the body on its slow pulse. The body's own AI starts each of
    /// the balloon's other animations: its rest's end the inflation from deflated, its release the sinking, and its fatal
    /// hit while sunk halfway the inflation from halfway. At every update of each, played through the production
    /// animation pass, the inflation the balloon records (#1845) stands at the pose the native pointer the game's AI reads
    /// names: that sequence's first list plus four bytes per shown pose, the pose last shown, or the sequence's first
    /// before it shows one.
    /// </summary>
    internal static void VerifyPowampBehavior(CartridgeImportAddressSpace rom)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // The balloon and its body are placed as their room places them, whose settings name the part each is (#2163).
        (RoomEnemyPlace balloon, RoomEnemyPlace body, PowampBodyState bodyState) = PowampPairFixtures.PlacePair(enemies);
        AssertStarted(balloon.RequireSpeciesState<PowampBalloonState>(), PowampBalloonSegment.Deflated, "the balloon starts deflated");
        AssertEqual(new AnimationPlayback<PowampBodySegment>(PowampAnimations.BodyStartingAt[PowampBodySegment.BodySlow], AnimationPlace.StartOfAnimation), bodyState.Playback,
            "the body starts on its slow pulse");

        // The ROM oracle holds every balloon beat to a four-byte native frame record, so the native pointer at a place
        // is its segment's entry plus four bytes per beat passed, the list's closing control at its end included; the
        // native record there is the pose's own.
        IImportCartridgeSource cartridge = rom;
        PowampBalloonState balloonState = balloon.RequireSpeciesState<PowampBalloonState>();
        object?[] arguments = [balloon, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
        // The body stands low enough in the room that its balloon, placed above it, stays in the room.
        body.Position = body.Position.WithWholeY(100);
        RoomLevelData level = VerificationHelpersVerification.CreateEmptyRoom(new(16, 16));
        SamusState samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        // Plays the balloon's animation from where it stands for its next updates, checking its inflation at each.
        void Play(PowampBalloonSegment start, int updates)
        {
            for (int update = 0; update < updates; update++)
            {
                AssertBalloonInflation(cartridge, balloonState, $"Powamp balloon {start} after {update} updates");
                balloon.AnimationClock.StepOnNextUpdate();
                EnemyInstructionRunner.Process(arguments);
            }
            AssertBalloonInflation(cartridge, balloonState, $"Powamp balloon {start} after {updates} updates");
        }
        // Runs one update of the body's AI in an action whose countdown ends on it.
        void RunBody(PowampAction action)
        {
            bodyState.Action = action;
            bodyState.ActionTimer = UpdateCountdown.For(1);
            enemies.PowampBehavior.RunPowampMain(body.HeldOccupant(), body, bodyState, enemies.PowampBehavior.PlacedBalloon(body), level, samus);
        }
        int Length(PowampBalloonSegment start) => PowampAnimations.BalloonStartingAt[start].Phases.Sum(phase => phase.BeatCount);

        Play(PowampBalloonSegment.Deflated, Length(PowampBalloonSegment.Deflated));
        RunBody(PowampAction.DeflatedResting);
        AssertStarted(balloonState, PowampBalloonSegment.InflatingFromDeflated, "the rest's end inflates the balloon from deflated");
        Play(PowampBalloonSegment.InflatingFromDeflated, Length(PowampBalloonSegment.InflatingFromDeflated));
        RunBody(PowampAction.GrappledResting);
        AssertStarted(balloonState, PowampBalloonSegment.StartSinking, "the ungrappled body's release sinks the balloon");
        Play(PowampBalloonSegment.StartSinking, Length(PowampBalloonSegment.StartSinking));
        // Sinking again, the balloon shows full and then halfway; a fatal hit there restarts its inflation from halfway.
        RunBody(PowampAction.GrappledResting);
        Play(PowampBalloonSegment.StartSinking, 2);
        AssertEqual(new PowampBalloonInflation(PowampBalloonSequence.Sinking, PowampBalloonPose.Halfway), balloonState.Inflation,
            "the sinking balloon stands halfway after its full pose");
        RunBody(PowampAction.FatalDamage);
        AssertStarted(balloonState, PowampBalloonSegment.InflatingFromHalfway, "a fatal hit sunk halfway inflates the balloon from halfway");
        Play(PowampBalloonSegment.InflatingFromHalfway, Length(PowampBalloonSegment.InflatingFromHalfway));
        Console.WriteLine("  Powamp behavior: both initial animations and the balloon's inflation at every update of its animations pass.");
    }

    /// <summary>The balloon's animation stands at the start of <paramref name="segment"/>'s, as its owner just started it.</summary>
    private static void AssertStarted(PowampBalloonState balloon, PowampBalloonSegment segment, string context) =>
        AssertEqual(new AnimationPlayback<PowampBalloonSegment>(PowampAnimations.BalloonStartingAt[segment], AnimationPlace.StartOfAnimation),
            balloon.Playback, context);

    /// <summary>The poses the cartridge's inflating sequence shows, in the order its lists show them, four bytes per pose.</summary>
    internal static readonly PowampBalloonPose[] NativeInflatingPoses = [PowampBalloonPose.Deflated, PowampBalloonPose.Halfway, PowampBalloonPose.Full];

    /// <summary>The poses the cartridge's sinking sequence shows, in the order its lists show them, four bytes per pose.</summary>
    internal static readonly PowampBalloonPose[] NativeSinkingPoses = [PowampBalloonPose.Full, PowampBalloonPose.Halfway, PowampBalloonPose.Deflated];

    /// <summary>
    /// The inflation the balloon records stands for the native pointer its animation's place holds: its segment's list
    /// plus four bytes per beat passed, where the native record is the pose's own. The poses shown, the pointer's distance
    /// from its sequence's first list in four-byte records, name the pose the balloon sits at.
    /// </summary>
    private static void AssertBalloonInflation(IImportCartridgeSource cartridge, PowampBalloonState balloon, string context)
    {
        AnimationPlayback<PowampBalloonSegment> playback = balloon.Playback;
        AnimationPhase<PowampBalloonSegment> phase = playback.Animation.Phases[playback.Place.Phase];
        // A phase the play has not entered yet has passed none of its beats, and a held one all of them.
        int passed = phase.FirstBeat + Math.Clamp(playback.Place.Beat, 0, phase.BeatCount);
        PowampBalloonSegment segment = phase.Segment;
        PowampBalloonInflation inflation = balloon.Inflation;
        ushort sequence = NativeEnemyAnimationOracle.EntryOf(PowampNativeListAddresses.BalloonEntries,
            inflation.Sequence == PowampBalloonSequence.Sinking ? PowampBalloonSegment.StartSinking : PowampBalloonSegment.InflatingFromDeflated);
        ushort pointer = (ushort)(NativeEnemyAnimationOracle.EntryOf(PowampNativeListAddresses.BalloonEntries, segment) + 4 * passed);
        int shown = (pointer - sequence) / 4;
        AssertTrue(pointer == sequence + 4 * shown && shown is >= 0 and <= 3, $"{context}: the native pointer lies on one of its sequence's pose records");
        PowampBalloonPose[] poses = inflation.Sequence == PowampBalloonSequence.Sinking ? NativeSinkingPoses : NativeInflatingPoses;
        AssertEqual(poses[shown == 0 ? 0 : shown - 1], inflation.Pose, context);
        IReadOnlyList<AnimationBeat<PowampBalloonPose, NoAnimationCue>> beats = PowampAnimations.BalloonSegments.Beats(segment);
        // A pose the balloon rests on (#2439) shows its record until its list's sleep, whatever duration the record names.
        if (passed < beats.Count && beats[passed] is AnimationBeat<PowampBalloonPose, NoAnimationCue>.Frame frame)
        {
            AssertEqual(frame.Duration,
                (int)cartridge.ReadCartridgeWord((PowampNativeListAddresses.Bank << 16) | pointer), $"{context}: native pose record");
        }
        else if (passed < beats.Count)
        {
            AssertIs<AnimationBeat<PowampBalloonPose, NoAnimationCue>.HeldFrame>(beats[passed], context);
        }
    }

    /// <summary>Runs the Powamp behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--powamp-instruction-mechanics")]
    internal static int PowampBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPowampBehavior), () => VerifyPowampBehavior(rom));
        Suite(nameof(PowampAnimationsVerification.VerifyPowampAnimations), () => PowampAnimationsVerification.VerifyPowampAnimations(rom));
        return 0;
    }
}
