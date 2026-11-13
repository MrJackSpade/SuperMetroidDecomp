using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>Checks crawler orientation tables and their live initialization and surface-switch selection.</summary>
internal static class CrawlerAnimationDefinitionsVerification
{
    /// <summary>Compares crawler orientation tables with cartridge words and production initial/surface instruction selection.</summary>
    internal static void VerifyCrawlerAnimationDefinitions(CartridgeImportAddressSpace rom)
    {
        InitialTable[] initialTables =
        [
            new InitialTable(CrawlerAnimationFamily.Shared, 0xa3e2cc, null),
            new InitialTable(CrawlerAnimationFamily.Viola, 0xa3b667, CrawlerAnimationFamily.Viola),
            new InitialTable(CrawlerAnimationFamily.Sciser, 0xa396db, CrawlerAnimationFamily.Sciser),
            new InitialTable(CrawlerAnimationFamily.Zero, 0xa3992b, CrawlerAnimationFamily.Zero),
            new InitialTable(CrawlerAnimationFamily.HZoomer, 0xa3e03b, null),
        ];
        CrawlerSurfaceOrientation[] orientations =
        [
            CrawlerSurfaceOrientation.UpsideRight,
            CrawlerSurfaceOrientation.UpsideLeft,
            CrawlerSurfaceOrientation.UpsideDown,
            CrawlerSurfaceOrientation.UpsideUp,
        ];

        foreach ((CrawlerAnimationFamily family, int address, _) in initialTables)
        for (int orientation = 0; orientation < 4; orientation++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + orientation * 2)),
                NativeEnemyAnimationOracle.EntryOf(CrawlerNativeListAddresses.Entries, CrawlerSurfaceAnimations.ProgramFor(
                    family, orientations[orientation])),
                $"crawler initial selector {family}/{orientation}");
        }

        // Each species column of the four native surface tables selects its family's animation for that surface.
        int[] surfaceTables = [0xa3e648, 0xa3e654, 0xa3e630, 0xa3e63c];
        for (int column = 0; column < SuperMetroid.AssetExtraction.Enemies.CrawlerSurfaceTableOrder.ColumnCount; column++)
        for (int orientation = 0; orientation < 4; orientation++)
        {
            CrawlerAnimationFamily family = SuperMetroid.AssetExtraction.Enemies.CrawlerSurfaceTableOrder.FamilyAt(column * 2);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((surfaceTables[orientation] + column * 2)),
                NativeEnemyAnimationOracle.EntryOf(CrawlerNativeListAddresses.Entries, CrawlerSurfaceAnimations.ProgramFor(
                    family, orientations[orientation])),
                $"crawler surface selector column {column} ({family})/{orientation}");
        }

        foreach ((CrawlerAnimationFamily family, _, CrawlerAnimationFamily? speciesSurfaceFamily) in initialTables)
        for (ushort orientation = 0; orientation < 4; orientation++)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(family == CrawlerAnimationFamily.HZoomer ? EnemyDefinitionId.HZoomer : EnemyDefinitionId.Zoomer, slot.Definition);
            // The codec decodes the initialization word into the surface the crawler starts on, and the property word's
            // low bits into the side of its block.
            AssertEqual(new CrawlerSpawnSettings(orientations[orientation], orientations[orientation], CrawlSpeed: new SignedEightEight(CrawlerSpeeds.ForParameter(0)), SurfaceFamily: CrawlerAnimationFamily.Shared),
                SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Zoomer,
                    new SuperMetroid.AssetExtraction.Enemies.PopulationWords(orientation, orientation, 0, 0, 0)),
                $"crawler population words {orientation} decode to its surface and block side");
            slot.SetSpawnSettings(new CrawlerSpawnSettings(orientations[orientation], CrawlerSurfaceOrientation.UpsideRight, CrawlSpeed: new SignedEightEight(CrawlerSpeeds.ForParameter(0)), SurfaceFamily: CrawlerAnimationFamily.Shared));
            if (family == CrawlerAnimationFamily.Viola)
            {
                // InitAI_Viola ($A3:B678) selects with its block side, not its surface.
                slot.SetSpawnSettings(new ViolaSpawnSettings(orientations[orientation], CrawlSpeed: new SignedEightEight(CrawlerSpeeds.ForParameter(0)), SurfaceFamily: CrawlerAnimationFamily.Shared));
            }
            slot.SetSpawnParameter1(family == CrawlerAnimationFamily.Viola ? EnemyDefinitionId.Viola : EnemyDefinitionId.Zoomer, 0);
            if (family == CrawlerAnimationFamily.HZoomer)
            {
                new Action<RoomEnemySlot>(CrawlersBehavior.InitializeHZoomer)(slot);
            }
            else
            {
                new Action<RoomEnemySlot, CrawlerAnimationFamily, CrawlerAnimationFamily?>(CrawlersBehavior.InitializeCrawler)(
                        slot, family, speciesSurfaceFamily);
            }

            AssertEqual(new AnimationPlayback<CrawlerProgram>(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.ProgramFor(
                    family, (CrawlerSurfaceOrientation)orientation)), AnimationPlace.StartOfAnimation),
                (enemies.CrawlerStates[0] ?? throw new InvalidOperationException("The crawler has no state.")).Playback,
                $"crawler production initial selector {family}/{orientation}");
        }

        Action<RoomEnemySlot, CrawlerEnemyState> setVertical = CrawlersBehavior.SetCrawlerVerticalSurfaceInstruction;
        Action<RoomEnemySlot, CrawlerEnemyState> setHorizontal = CrawlersBehavior.SetCrawlerHorizontalSurfaceInstruction;
        foreach (CrawlerAnimationFamily surfaceFamily in new[] { CrawlerAnimationFamily.Shared, CrawlerAnimationFamily.Viola, CrawlerAnimationFamily.Sciser, CrawlerAnimationFamily.Zero })
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            var state = new CrawlerEnemyState { SurfaceFamily = surfaceFamily };

            state.Velocity = state.Velocity.WithX(new SignedEightEight(0xffff));
            setHorizontal(slot, state);
            AssertEqual(new AnimationPlayback<CrawlerProgram>(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.ProgramFor(
                    surfaceFamily, CrawlerSurfaceOrientation.UpsideRight)), AnimationPlace.StartOfAnimation),
                state.Playback, $"crawler production right surface {surfaceFamily}");
            state.Velocity = state.Velocity.WithX(new SignedEightEight(1));
            setHorizontal(slot, state);
            AssertEqual(new AnimationPlayback<CrawlerProgram>(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.ProgramFor(
                    surfaceFamily, CrawlerSurfaceOrientation.UpsideLeft)), AnimationPlace.StartOfAnimation),
                state.Playback, $"crawler production left surface {surfaceFamily}");
            state.Velocity = state.Velocity.WithY(new SignedEightEight(0xffff));
            setVertical(slot, state);
            AssertEqual(new AnimationPlayback<CrawlerProgram>(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.ProgramFor(
                    surfaceFamily, CrawlerSurfaceOrientation.UpsideDown)), AnimationPlace.StartOfAnimation),
                state.Playback, $"crawler production down surface {surfaceFamily}");
            state.Velocity = state.Velocity.WithY(new SignedEightEight(1));
            setVertical(slot, state);
            AssertEqual(new AnimationPlayback<CrawlerProgram>(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.ProgramFor(
                    surfaceFamily, CrawlerSurfaceOrientation.UpsideUp)), AnimationPlace.StartOfAnimation),
                state.Playback, $"crawler production up surface {surfaceFamily}");
        }

        AssertThrows<InvalidDataException>(
            () => CrawlerSurfaceAnimations.ProgramFor(
                (CrawlerAnimationFamily)5, CrawlerSurfaceOrientation.UpsideRight),
            "crawler initial family beyond authored tables");
        Console.WriteLine(
            "Crawler animation definitions: forty-four native selectors and all family/orientation initializers plus 16 real surface handoffs pass with source tables forbidden.");
    }

}
