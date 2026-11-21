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
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Input;

/// <summary>Checks Powamp's initial animations and the inflation its balloon records as its animation shows each pose.</summary>
internal static class PowampBehaviorVerification
{
    /// <summary>
    /// The real initializers start the balloon deflated and the body on its slow pulse. At every update of each of the
    /// balloon's animations, played through the production animation pass, the inflation the balloon records (#1845)
    /// matches the native pointer the game's AI reads: that sequence's first list plus four bytes per shown pose.
    /// </summary>
    internal static void VerifyPowampBehavior(CartridgeImportAddressSpace rom)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // The balloon and its body are placed as their room places them, whose settings name the part each is (#2163).
        RoomEnemyPlace balloon = enemies.Places.InUpdateOrder[0];
        balloon.SetSpawnParameter1(EnemyDefinitionId.Powamp, 1);
        balloon.Bind(EnemyDefinitionId.Powamp, balloon.KeptDefinition);
        balloon.Bind(balloon.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        PowampBehavior.InitializePowampBalloon(balloon, balloon.HeldOccupant().SpawnSettings<PowampBalloonSpawnSettings>());
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[1];
        body.BindPlaced(EnemyDefinitionId.Powamp, new PowampBodySpawnSettings(PowampContact.Accepts));
        body.Bind(EnemyDefinitionId.Powamp, body.KeptDefinition);
        body.Bind(body.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        PowampBehavior.InitializePowampBody(body);
        AssertEqual(new AnimationPlayback<PowampSegment>(PowampAnimations.StartingWith(PowampSegment.BalloonDeflated), AnimationPlace.StartOfAnimation), balloon.RequireSpeciesState<PowampBalloonState>().Playback,
            "the balloon starts deflated");
        AssertEqual(new AnimationPlayback<PowampSegment>(PowampAnimations.StartingWith(PowampSegment.BodySlow), AnimationPlace.StartOfAnimation), body.RequireSpeciesState<PowampBodyState>().Playback,
            "the body starts on its slow pulse");

        // The ROM oracle holds every balloon beat to a four-byte native frame record, so the native pointer at a place
        // is its segment's entry plus four bytes per beat passed, the list's closing control at its end included; the
        // native record there is the pose's own.
        IImportCartridgeSource cartridge = rom;
        PowampBalloonState balloonState = balloon.RequireSpeciesState<PowampBalloonState>();
        object?[] arguments = [balloon, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
        foreach (PowampSegment start in new[] { PowampSegment.BalloonInflatingFromDeflated, PowampSegment.BalloonInflatingFromHalfway, PowampSegment.BalloonStartSinking, PowampSegment.BalloonDeflated })
        {
            PowampBehavior.StartPowampBalloonAnimation(balloon, balloonState, start);
            int frames = PowampAnimations.StartingWith(start).Phases.Sum(phase => phase.BeatCount);
            for (int update = 0; update <= frames; update++)
            {
                AssertBalloonInflation(cartridge, balloonState, $"Powamp balloon {start} after {update} updates");
                balloon.AnimationClock.StepOnNextUpdate();
                EnemyInstructionRunner.Process(arguments);
            }
        }
        Console.WriteLine("  Powamp behavior: both initial animations and the balloon's inflation at every update of its animations pass.");
    }

    /// <summary>
    /// The inflation the balloon records stands for the native pointer its animation's place holds: its segment's list
    /// plus four bytes per beat passed, where the native record is the pose's own.
    /// </summary>
    private static void AssertBalloonInflation(IImportCartridgeSource cartridge, PowampBalloonState balloon, string context)
    {
        AnimationPlayback<PowampSegment> playback = balloon.Playback;
        AnimationPhase<PowampSegment> phase = playback.Animation.Phases[playback.Place.Phase];
        // A phase the play has not entered yet has passed none of its beats, and a held one all of them.
        int passed = phase.FirstBeat + Math.Clamp(playback.Place.Beat, 0, phase.BeatCount);
        PowampSegment segment = phase.Segment;
        PowampBalloonInflation inflation = balloon.Inflation;
        ushort sequence = NativeEnemyAnimationOracle.EntryOf(PowampNativeListAddresses.Entries,
            inflation.Sequence == PowampBalloonSequence.Sinking ? PowampSegment.BalloonStartSinking : PowampSegment.BalloonInflatingFromDeflated);
        ushort pointer = (ushort)(NativeEnemyAnimationOracle.EntryOf(PowampNativeListAddresses.Entries, segment) + 4 * passed);
        AssertEqual(pointer, (ushort)(sequence + 4 * inflation.PosesShown), context);
        IReadOnlyList<AnimationBeat<EnemyFrameId, NoAnimationCue>> beats = PowampAnimations.Segments.Beats(segment);
        // A pose the balloon rests on (#2439) shows its record until its list's sleep, whatever duration the record names.
        if (passed < beats.Count && beats[passed] is AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame frame)
        {
            AssertEqual(frame.Duration,
                (int)cartridge.ReadCartridgeWord((PowampNativeListAddresses.Bank << 16) | pointer), $"{context}: native pose record");
        }
        else if (passed < beats.Count)
        {
            AssertIs<AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame>(beats[passed], context);
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
