using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Programs;
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
        (RoomEnemySystem enemies, RoomEnemySlot slot, PuyoEnemyState state) = Create(artwork);
        AssertEqual(new AnimationPlayback<PuyoProgram>(PuyoAnimations.StartingWith(PuyoProgram.GroundedFast), AnimationPlace.StartOfAnimation), state.Playback, "Puyo starts on its fast grounded loop");
        foreach (PuyoProgram loop in new[] { PuyoProgram.GroundedFast, PuyoProgram.GroundedMedium, PuyoProgram.GroundedSlow })
        {
            Start(slot, state, loop);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(PuyoAnimations.Segments, loop, PuyoFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(PuyoAnimations.Segments, loop)), $"Puyo {loop} loop");
        }
        foreach (PuyoProgram pose in new[] { PuyoProgram.RightFrame0LeftFrame4, PuyoProgram.RightFrame1LeftFrame3, PuyoProgram.Frame2,
                     PuyoProgram.RightFrame3LeftFrame1, PuyoProgram.RightFrame4LeftFrame0 })
        {
            Start(slot, state, pose);
            ushort frame = PuyoAnimationsConstants.Spritemaps(pose).Single();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([frame, frame], [1, 1]), EnemyInstructionRunner.ShownFrames(slot, 3), $"Puyo {pose} pose");
        }
        if (artwork is not null)
            foreach (ushort pose in PuyoAnimationsConstants.Spritemaps(Enum.GetValues<PuyoProgram>()).Distinct())
                AssertTrue((artwork.Enemies.Spritemaps ?? throw new InvalidOperationException("Verification requires installed spritemaps.")).TryGet(PuyoFrameToolingDefinitions.Bank, pose, out _),
                    $"Puyo pose ${pose:X4} has installed artwork");
        Console.WriteLine("  Puyo: the fast-loop start, three grounded loops and five airborne poses pass.");
    }

    /// <summary>A Puyo after its real initializer.</summary>
    private static PuyoInstructionProgramDefinitionsCreateResult Create(ImportEnemyArtwork? artwork)
    {
        var enemies = new RoomEnemySystem((artwork is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(artwork.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artwork?.Bosses };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Puyo, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnSettings(new PuyoSpawnSettings(HopCooldown: 0, HopDistance: 0));
        PuyoBehavior.InitializePuyo(slot);
        return new PuyoInstructionProgramDefinitionsCreateResult(enemies, slot, enemies.PuyoStates[0] ?? throw new InvalidOperationException("Puyo has no state."));
    }

    /// <summary>Starts <paramref name="program"/> with its first step due on the next update.</summary>
    private static void Start(RoomEnemySlot slot, PuyoEnemyState state, PuyoProgram program)
    {
        state.Playback = new(PuyoAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.Restart();
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
