using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.FaceBlocks;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>
/// #2477: the Blue Brinstar face blocks' room-wide glow, played by the shared animation player over its typed cycle of glow
/// levels, against the cartridge's glow colour records.
/// </summary>
internal static class BlueBrinstarFaceBlockGlowVerification
{
    /// <summary>The bank-$A8 address of the face blocks' glow colour records: one record of four colours per step.</summary>
    private const int GlowColourRecords = 0xa8e7cc;

    /// <summary>The colours each glow colour record holds.</summary>
    private const int RecordColours = 4;

    /// <summary>
    /// The cycle's steps are the cartridge's records, in order: two steps show one level exactly where their records hold the
    /// same colours. A block's start shows the last step's level, the glow steps to the next level every sixteen updates, from
    /// the first step, through the whole cycle and round again, and a block's waking restarts the step's sixteen updates and
    /// keeps its level.
    /// </summary>
    internal static void VerifyBlueBrinstarFaceBlockGlow(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        IReadOnlyList<FaceBlockGlowLevel> cycle = FaceBlockGlowAnimations.Cycle;
        ushort[][] records = [.. Enumerable.Range(0, cycle.Count).Select(step => Enumerable.Range(0, RecordColours)
            .Select(colour => cartridge.ReadCartridgeWord(GlowColourRecords + 2 * (RecordColours * step + colour))).ToArray())];
        for (int step = 0; step < cycle.Count; step++)
        {
            for (int other = 0; other < cycle.Count; other++)
            {
                AssertEqual(records[step].SequenceEqual(records[other]), cycle[step] == cycle[other],
                    $"face-block glow steps {step} and {other} share a level exactly where their colour records match");
            }
        }

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.FaceBlock, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        place.SetSpawnParameter1(EnemyDefinitionId.FaceBlock, 64);
        place.SetSpawnParameter2(EnemyDefinitionId.FaceBlock, 0);
        place.Position = place.Position.WithWhole(new(256, 256));
        enemies.BlueBrinstarFaceBlockBehavior.InitializeBlueBrinstarFaceBlock(place, RoomEnemyFrameFixtures.StandaloneSamus());
        BlueBrinstarFaceBlockEnemyState state = enemies.BlueBrinstarFaceBlockStates[0]
            ?? throw new InvalidOperationException("The face block has no state.");
        // Samus has the Morph Ball but stands far from the block, which starts the glow and does not wake.
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), InstalledFixtures.SamusArtwork())
        {
            CollectedItems = SamusEquipmentFlags.MorphBall,
            Position = FixedPosition<RoomPixels>.FromWhole(new(512, 256)),
        };
        enemies.BlueBrinstarFaceBlockBehavior.RunBlueBrinstarFaceBlockMain(place, state, samus);
        AssertEqual(Shaded(cycle[^1]), place.Shading, "a started glow shows its last step's level");

        int updates = 0;
        for (int step = 0; step < 2 * cycle.Count; step++)
        {
            StepUntilLevelChanges(enemies, place, ref updates, FaceBlockGlowDefinitions.FrameDuration - 1);
            enemies.BlueBrinstarFaceBlockBehavior.StepGlow();
            updates++;
            AssertEqual(Shaded(cycle[step % cycle.Count]), place.Shading, $"face-block glow step {step} shows its level after {updates} updates");
        }

        // Waking partway through a step restarts its sixteen updates and keeps its level.
        enemies.BlueBrinstarFaceBlockBehavior.StepGlow();
        enemies.BlueBrinstarFaceBlockBehavior.StepGlow();
        samus.Position = FixedPosition<RoomPixels>.FromWhole(new(224, 256));
        enemies.BlueBrinstarFaceBlockBehavior.RunBlueBrinstarFaceBlockMain(place, state, samus);
        AssertTrue(state.Activated, "the face block wakes for Samus beside it");
        AssertEqual(Shaded(cycle[^1]), place.Shading, "a waking block keeps the glow's level");
        StepUntilLevelChanges(enemies, place, ref updates, FaceBlockGlowDefinitions.FrameDuration - 1);
        enemies.BlueBrinstarFaceBlockBehavior.StepGlow();
        AssertEqual(Shaded(cycle[0]), place.Shading, "the step after a waking shows sixteen updates after it");
        Console.WriteLine($"  Face block glow: {cycle.Count} steps of {FaceBlockGlowAnimations.Levels.Count} levels match bank $A8's records; two cycles and a waking pass.");
    }

    /// <summary>Steps the glow <paramref name="steps"/> times, requiring the level to hold throughout.</summary>
    private static void StepUntilLevelChanges(RoomEnemySystem enemies, RoomEnemyPlace place, ref int updates, int steps)
    {
        PictureShading shown = place.Shading;
        for (int step = 0; step < steps; step++)
        {
            enemies.BlueBrinstarFaceBlockBehavior.StepGlow();
            updates++;
            AssertEqual(shown, place.Shading, $"the face-block glow holds its level after {updates} updates");
        }
    }

    /// <summary>The shading of glow level <paramref name="level"/>.</summary>
    private static PictureShading Shaded(FaceBlockGlowLevel level) => PictureShading.Shaded(FaceBlockShades.Glow(level));

    /// <summary>Runs the face-block glow check.</summary>
    [VerificationCommand("--face-block-glow")]
    internal static int FaceBlockGlowCommand()
    {
        Suite(nameof(VerifyBlueBrinstarFaceBlockGlow), () => VerifyBlueBrinstarFaceBlockGlow(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
