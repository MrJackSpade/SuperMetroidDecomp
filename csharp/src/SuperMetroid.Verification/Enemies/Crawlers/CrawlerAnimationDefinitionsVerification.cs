using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>Checks crawler orientation tables and their live initialization and surface-switch selection.</summary>
internal static class CrawlerAnimationDefinitionsVerification
{
    /// <summary>Compares crawler orientation tables with cartridge words and production initial/surface instruction selection.</summary>
    internal static void VerifyCrawlerAnimationDefinitions(CartridgeImportAddressSpace rom)
    {
        InitialTable[] initialTables =
        [
            new InitialTable(CrawlerAnimationFamily.Shared, 0xa3e2cc),
            new InitialTable(CrawlerAnimationFamily.Viola, 0xa3b667),
            new InitialTable(CrawlerAnimationFamily.Sciser, 0xa396db),
            new InitialTable(CrawlerAnimationFamily.Zero, 0xa3992b),
            new InitialTable(CrawlerAnimationFamily.Shared, 0xa3e03b, OrangeZoomer: true),
        ];
        CrawlerSurfaceOrientation[] orientations =
        [
            CrawlerSurfaceOrientation.OnLeftWall,
            CrawlerSurfaceOrientation.OnRightWall,
            CrawlerSurfaceOrientation.OnCeiling,
            CrawlerSurfaceOrientation.OnFloor,
        ];

        foreach ((CrawlerAnimationFamily family, int address, _) in initialTables)
        for (int orientation = 0; orientation < 4; orientation++)
        {
            // The selector's entry leads into the crawl's list (#2061).
            AssertEqual(CrawlerSurfaceAnimations.AnimationFor(family, orientations[orientation]),
                NativeEnemyAnimationOracle.LeadsInto(rom, CrawlerNativeListAddresses.Bank, CrawlerNativeListAddresses.Entries,
                    CrawlerAnimationsVerification.Rules, rom.ReadCartridgeWord((address + orientation * 2))),
                $"crawler initial selector {family}/{orientation}");
        }

        // Each species column of the four native surface tables selects its family's animation for that surface.
        int[] surfaceTables = [0xa3e648, 0xa3e654, 0xa3e630, 0xa3e63c];
        for (int column = 0; column < SuperMetroid.AssetExtraction.Enemies.CrawlerSurfaceTableOrder.ColumnCount; column++)
        for (int orientation = 0; orientation < 4; orientation++)
        {
            CrawlerAnimationFamily family = SuperMetroid.AssetExtraction.Enemies.CrawlerSurfaceTableOrder.FamilyAt(column * 2);
            AssertEqual(CrawlerSurfaceAnimations.AnimationFor(family, orientations[orientation]),
                NativeEnemyAnimationOracle.LeadsInto(rom, CrawlerNativeListAddresses.Bank, CrawlerNativeListAddresses.Entries,
                    CrawlerAnimationsVerification.Rules, rom.ReadCartridgeWord((surfaceTables[orientation] + column * 2))),
                $"crawler surface selector column {column} ({family})/{orientation}");
        }

        foreach ((CrawlerAnimationFamily family, _, bool orangeZoomer) in initialTables)
        for (ushort orientation = 0; orientation < 4; orientation++)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            // The place holds a crawler of the family the table serves, whose species its setup reads the family from.
            EnemyDefinitionId species = orangeZoomer ? EnemyDefinitionId.OrangeZoomer : family switch
            {
                CrawlerAnimationFamily.Viola => EnemyDefinitionId.Viola,
                CrawlerAnimationFamily.Sciser => EnemyDefinitionId.Sciser,
                CrawlerAnimationFamily.Zero => EnemyDefinitionId.Zero,
                CrawlerAnimationFamily.Shared => EnemyDefinitionId.Zoomer,
                _ => throw new ArgumentOutOfRangeException(nameof(family), family, "No crawler family has that value."),
            };
            place.Bind(species, place.KeptDefinition);
            // The codec decodes the initialization word into the surface the crawler starts on, and the property word's
            // low bits into the side of its block.
            AssertEqual(new CrawlerSpawnSettings(orientations[orientation], orientations[orientation], CrawlSpeed: new SignedEightEight(CrawlerSpeeds.ForParameter(0))),
                SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Zoomer,
                    new SuperMetroid.AssetExtraction.Enemies.PopulationWords(orientation, orientation, 0, 0, 0)),
                $"crawler population words {orientation} decode to its surface and block side");
            // The orange Zoomer's entry carries its own settings: the same surface, block side and speed.
            if (orangeZoomer)
                place.SetSpawnSettings(new OrangeZoomerSpawnSettings(orientations[orientation], CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: new SignedEightEight(CrawlerSpeeds.ForParameter(0))));
            else
                place.SetSpawnSettings(new CrawlerSpawnSettings(orientations[orientation], CrawlerSurfaceOrientation.OnLeftWall, CrawlSpeed: new SignedEightEight(CrawlerSpeeds.ForParameter(0))));
            if (family == CrawlerAnimationFamily.Viola)
            {
                // InitAI_Viola ($A3:B678) selects with its block side, not its surface.
                place.SetSpawnSettings(new ViolaSpawnSettings(orientations[orientation], CrawlSpeed: new SignedEightEight(CrawlerSpeeds.ForParameter(0))));
            }
            place.SetSpawnParameter1(species, 0);
            // The crawler is set up as the population load sets it up, its species building its entity.
            _ = enemies.LoadAuthoredPopulation();

            AssertEqual(new AnimationPlayback<CrawlerSegment>(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.AnimationFor(
                    family, orientations[orientation])), AnimationPlace.StartOfAnimation),
                (enemies.CrawlerStates[0] ?? throw new InvalidOperationException("The crawler has no state.")).Playback,
                $"crawler production initial selector {family}/{orientation}");
        }

        Action<RoomEnemyPlace, CrawlerEnemyState> setVertical = CrawlersBehavior.SetCrawlerVerticalSurfaceAnimation;
        Action<RoomEnemyPlace, CrawlerEnemyState> setHorizontal = CrawlersBehavior.SetCrawlerHorizontalSurfaceAnimation;
        foreach (CrawlerAnimationFamily surfaceFamily in new[] { CrawlerAnimationFamily.Shared, CrawlerAnimationFamily.Viola, CrawlerAnimationFamily.Sciser, CrawlerAnimationFamily.Zero })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            var state = new CrawlerEnemyState(CrawlerAction.Settling) { SurfaceFamily = surfaceFamily };

            state.Velocity = state.Velocity.WithX(new SignedEightEight(0xffff));
            setHorizontal(place, state);
            AssertEqual(new AnimationPlayback<CrawlerSegment>(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.AnimationFor(
                    surfaceFamily, CrawlerSurfaceOrientation.OnLeftWall)), AnimationPlace.StartOfAnimation),
                state.Playback, $"crawler production right surface {surfaceFamily}");
            state.Velocity = state.Velocity.WithX(new SignedEightEight(1));
            setHorizontal(place, state);
            AssertEqual(new AnimationPlayback<CrawlerSegment>(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.AnimationFor(
                    surfaceFamily, CrawlerSurfaceOrientation.OnRightWall)), AnimationPlace.StartOfAnimation),
                state.Playback, $"crawler production left surface {surfaceFamily}");
            state.Velocity = state.Velocity.WithY(new SignedEightEight(0xffff));
            setVertical(place, state);
            AssertEqual(new AnimationPlayback<CrawlerSegment>(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.AnimationFor(
                    surfaceFamily, CrawlerSurfaceOrientation.OnCeiling)), AnimationPlace.StartOfAnimation),
                state.Playback, $"crawler production down surface {surfaceFamily}");
            state.Velocity = state.Velocity.WithY(new SignedEightEight(1));
            setVertical(place, state);
            AssertEqual(new AnimationPlayback<CrawlerSegment>(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.AnimationFor(
                    surfaceFamily, CrawlerSurfaceOrientation.OnFloor)), AnimationPlace.StartOfAnimation),
                state.Playback, $"crawler production up surface {surfaceFamily}");
        }

        Console.WriteLine(
            "Crawler animation definitions: forty-four native selectors and all family/orientation initializers plus 16 real surface handoffs pass with source tables forbidden.");
    }

}
