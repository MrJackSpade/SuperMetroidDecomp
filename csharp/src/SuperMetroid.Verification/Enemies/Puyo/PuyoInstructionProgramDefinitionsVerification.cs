using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Puyo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Puyo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Puyo's initial animation, its grounded loops and its one-pose airborne programs.</summary>
internal static class PuyoInstructionProgramDefinitionsVerification
{
    /// <summary>
    /// The real initializer starts the fast grounded loop; each grounded loop runs on time and each airborne program
    /// shows its pose once and holds. With installed artwork supplied, every pose has an installed composition.
    /// </summary>
    internal static void VerifyPuyoInstructionProgramDefinitions(ImportEnemyArtwork? artwork = null)
    {
        (RoomEnemySystem enemies, RoomEnemyPlace place, PuyoEnemyState state) = Create(artwork);
        AssertEqual(new AnimationPlayback<PuyoSegment>(PuyoAnimations.StartingWith(PuyoSegment.GroundedFast), AnimationPlace.StartOfAnimation), state.Playback, "Puyo starts on its fast grounded loop");
        foreach (PuyoSegment loop in new[] { PuyoSegment.GroundedFast, PuyoSegment.GroundedMedium, PuyoSegment.GroundedSlow })
        {
            Start(place, state, loop);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(PuyoAnimations.Segments, loop, PuyoFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(PuyoAnimations.Segments, loop)), $"Puyo {loop} loop");
        }
        foreach (PuyoSegment pose in new[] { PuyoSegment.TiltedFarRight, PuyoSegment.TiltedRight, PuyoSegment.Upright,
                     PuyoSegment.TiltedLeft, PuyoSegment.TiltedFarLeft })
        {
            Start(place, state, pose);
            ushort frame = PuyoAnimationsConstants.Spritemaps(pose).Single();
            // A still pose is held (#2276): it is new on the update it shows and stays without showing anew.
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([frame], [1]), EnemyInstructionRunner.ShownFrames(place, 3), $"Puyo {pose} pose");
        }
        if (artwork is not null)
            foreach (ushort pose in PuyoAnimationsConstants.Spritemaps(Enum.GetValues<PuyoSegment>()).Distinct())
                AssertTrue((artwork.Enemies.Spritemaps ?? throw new InvalidOperationException("Verification requires installed spritemaps.")).TryGet(PuyoFrameToolingDefinitions.Bank, pose, out _),
                    $"Puyo pose ${pose:X4} has installed artwork");
        Console.WriteLine("  Puyo: the fast-loop start, three grounded loops and five airborne poses pass.");
    }

    /// <summary>A Puyo after its real initializer.</summary>
    private static PuyoInstructionProgramDefinitionsCreateResult Create(ImportEnemyArtwork? artwork)
    {
        var enemies = EnemySystemFixtures.New((artwork is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(artwork.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork?.Bosses);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Puyo, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        place.SetSpawnSettings(new PuyoSpawnSettings(HopCooldown: 0, HopDistance: 0));
        PuyoBehavior.InitializePuyo(place.HeldOccupant(), place);
        return new PuyoInstructionProgramDefinitionsCreateResult(enemies, place, enemies.PuyoStates[0] ?? throw new InvalidOperationException("Puyo has no state."));
    }

    /// <summary>Starts <paramref name="program"/> with its first step due on the next update.</summary>
    private static void Start(RoomEnemyPlace place, PuyoEnemyState state, PuyoSegment program)
    {
        state.Playback = new(PuyoAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        place.AnimationClock.Restart();
    }

    /// <summary>Runs the Puyo checks and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--puyo-instruction-mechanics")]
    internal static int PuyoInstructionMechanicsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPuyoInstructionProgramDefinitions), () => VerifyPuyoInstructionProgramDefinitions());
        Suite(nameof(PuyoAnimationsVerification.VerifyPuyoAnimations), () => PuyoAnimationsVerification.VerifyPuyoAnimations(rom));
        return 0;
    }
}
