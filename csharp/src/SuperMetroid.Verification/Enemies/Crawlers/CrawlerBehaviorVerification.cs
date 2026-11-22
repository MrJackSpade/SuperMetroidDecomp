using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Animation;
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
    /// For every crawler family and surface, the real initializer starts that surface's animation and settles the
    /// crawler into the crawl that surface sets (vertical crawling on the side walls, horizontal on floors and ceilings,
    /// with the orange Zoomer's own crawls), and its poses loop on time. Every pose has installed artwork when supplied.
    /// </summary>
    internal static void VerifyCrawlerBehavior(ImportEnemyArtwork? artwork = null)
    {
        CrawlerBehaviorFamily[] families =
        [
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Shared, EnemyDefinitionId.Zoomer),
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Viola, EnemyDefinitionId.Viola),
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Sciser, EnemyDefinitionId.Sciser),
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Zero, EnemyDefinitionId.Zero),
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Shared, EnemyDefinitionId.OrangeZoomer),
        ];
        foreach ((CrawlerAnimationFamily family, EnemyDefinitionId definition) in families)
        foreach (CrawlerSurfaceOrientation orientation in Enum.GetValues<CrawlerSurfaceOrientation>())
        {
            var enemies = EnemySystemFixtures.New((artwork is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(artwork.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork?.Bosses);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(definition, place.KeptDefinition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            // The population entry names the surface; InitAI_Viola reads its block side instead.
            if (family == CrawlerAnimationFamily.Viola)
                place.SetSpawnSettings(new ViolaSpawnSettings(orientation, CrawlSpeed: new SignedEightEight(CrawlerSpeeds.ForParameter(0))));
            else if (definition == EnemyDefinitionId.OrangeZoomer)
                place.SetSpawnSettings(new OrangeZoomerSpawnSettings(orientation, CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: new SignedEightEight(CrawlerSpeeds.ForParameter(0))));
            else
                place.SetSpawnSettings(new CrawlerSpawnSettings(orientation, CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: new SignedEightEight(CrawlerSpeeds.ForParameter(0))));
            // The crawler is set up as the population load sets it up, its species building its entity.
            _ = enemies.LoadAuthoredPopulation();
            CrawlerEnemyState state = enemies.CrawlerStates[0] ?? throw new InvalidOperationException("The crawler has no state.");
            CrawlerSegment program = CrawlerSurfaceAnimations.AnimationFor(family, orientation);
            string context = $"{family} {orientation}";
            AssertEqual(new AnimationPlayback<CrawlerSegment>(CrawlerAnimations.StartingWith(program), AnimationPlace.StartOfAnimation), state.Playback, $"{context} starts its surface animation");

            // The surface animation runs on into the loop it then repeats.
            CrawlerSegment loop = CrawlerAnimations.StartingWith(program).Phases[^1].Segment;
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(CrawlerAnimations.Segments, loop, CrawlerFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(CrawlerAnimations.Segments, loop)), $"{context} loop");
            bool sideWall = orientation is CrawlerSurfaceOrientation.OnLeftWall or CrawlerSurfaceOrientation.OnRightWall;
            CrawlerAction expected = sideWall ? CrawlerAction.CrawlingVertically : CrawlerAction.CrawlingHorizontally;
            AssertEqual<CrawlerAction?>(CrawlerAction.Settling, state.Action, $"{context} settles first");
            AssertEqual(expected, CrawlersBehavior.CrawlActionOn(state), $"{context} settles into its surface's crawl");
            if (artwork is not null)
                foreach (ushort pose in CrawlerAnimationsConstants.Spritemaps(loop))
                    AssertTrue((artwork.Enemies.Spritemaps ?? throw new InvalidOperationException("Verification requires installed spritemaps.")).TryGet(CrawlerFrameToolingDefinitions.Bank, pose, out _),
                        $"{context} pose ${pose:X4} has installed artwork");
        }
        Console.WriteLine("  Crawler behavior: all four families' four surfaces, the orange Zoomer's with its own setup, start, settle into their crawls and loop on time.");
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
