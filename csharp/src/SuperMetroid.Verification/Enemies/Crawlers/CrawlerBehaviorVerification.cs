using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks each crawler family's surface animations through the real initializers.</summary>
internal static class CrawlerBehaviorVerification
{
    /// <summary>
    /// For every crawler family and surface, the real initializer starts that surface's animation; its first step
    /// installs the movement function (vertical crawling on the side walls, horizontal on floors and ceilings, with the
    /// orange Zoomer's own functions) and its poses then loop on time. Every pose has installed artwork when supplied.
    /// </summary>
    internal static void VerifyCrawlerBehavior(ImportEnemyArtwork? artwork = null)
    {
        CrawlerBehaviorFamily[] families =
        [
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Shared, EnemyDefinitionId.Zoomer, null),
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Viola, EnemyDefinitionId.Viola, CrawlerAnimationFamily.Viola),
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Sciser, EnemyDefinitionId.Sciser, CrawlerAnimationFamily.Sciser),
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Zero, EnemyDefinitionId.Zero, CrawlerAnimationFamily.Zero),
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.HZoomer, EnemyDefinitionId.HZoomer, null),
        ];
        foreach ((CrawlerAnimationFamily family, EnemyDefinitionId definition, CrawlerAnimationFamily? speciesSurfaceFamily) in families)
        foreach (CrawlerSurfaceOrientation orientation in Enum.GetValues<CrawlerSurfaceOrientation>())
        {
            var enemies = new RoomEnemySystem((artwork is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(artwork.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artwork?.Bosses };
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(definition, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            // The population entry names the surface; InitAI_Viola reads its block side instead.
            if (family == CrawlerAnimationFamily.Viola)
                slot.SetSpawnSettings(new ViolaSpawnSettings(orientation, CrawlSpeed: new SignedEightEight(CrawlerSpeeds.ForParameter(0)), SurfaceFamily: CrawlerAnimationFamily.Shared));
            else
                slot.SetSpawnSettings(new CrawlerSpawnSettings(orientation, CrawlerSurfaceOrientation.UpsideRight, CrawlSpeed: new SignedEightEight(CrawlerSpeeds.ForParameter(0)), SurfaceFamily: CrawlerAnimationFamily.Shared));
            if (family == CrawlerAnimationFamily.HZoomer)
                CrawlersBehavior.InitializeHZoomer(slot);
            else
                CrawlersBehavior.InitializeCrawler(slot, family, speciesSurfaceFamily);
            CrawlerEnemyState state = enemies.CrawlerStates[0] ?? throw new InvalidOperationException("The crawler has no state.");
            CrawlerSegment program = CrawlerSurfaceAnimations.AnimationFor(family, orientation);
            string context = $"{family} {orientation}";
            AssertEqual(new AnimationPlayback<CrawlerSegment>(CrawlerAnimations.StartingWith(program), AnimationPlace.StartOfAnimation), state.Playback, $"{context} starts its surface animation");

            // The surface animation runs on into the loop it then repeats.
            CrawlerSegment loop = CrawlerAnimations.StartingWith(program).Phases[^1].Segment;
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(CrawlerAnimations.Segments, loop, CrawlerFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(CrawlerAnimations.Segments, loop)), $"{context} loop");
            bool sideWall = orientation is CrawlerSurfaceOrientation.UpsideRight or CrawlerSurfaceOrientation.UpsideLeft;
            CrawlerEnemyFunction expected = family == CrawlerAnimationFamily.HZoomer
                ? sideWall ? CrawlerEnemyFunction.HZoomerCrawlingVertically : CrawlerEnemyFunction.HZoomerCrawlingHorizontally
                : sideWall ? CrawlerEnemyFunction.CrawlingVertically : CrawlerEnemyFunction.CrawlingHorizontally;
            AssertEqual(expected, state.Function, $"{context} installs its movement function");
            if (artwork is not null)
                foreach (ushort pose in CrawlerAnimationsConstants.Spritemaps(loop))
                    AssertTrue((artwork.Enemies.Spritemaps ?? throw new InvalidOperationException("Verification requires installed spritemaps.")).TryGet(CrawlerFrameToolingDefinitions.Bank, pose, out _),
                        $"{context} pose ${pose:X4} has installed artwork");
        }
        Console.WriteLine("  Crawler behavior: all five families' four surfaces start, install their movement functions and loop on time.");
    }

    /// <summary>Runs the crawler behavior check and the program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--crawler-instruction-programs")]
    internal static int CrawlerBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCrawlerBehavior), () => VerifyCrawlerBehavior());
        Suite(nameof(CrawlerAnimationsVerification.VerifyCrawlerAnimations), () => CrawlerAnimationsVerification.VerifyCrawlerAnimations(rom));
        return 0;
    }
}
