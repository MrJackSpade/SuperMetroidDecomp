using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Extract the new bundle through production import code, then confirm only
    /// the reported gunship frames, legacy-override inheritance and landing draw.
    /// Native bytes are an extraction/reference oracle, never the gameplay bus.
    /// </summary>
    private static void VerifyExtractedGunshipCompositions(string installationRoot, string romPath, string outputRoot)
    {
        var imported = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath(romPath));
        string directory = Path.Combine(Path.GetFullPath(outputRoot), "enemy-tiles-" + Guid.NewGuid().ToString("N"));
        EnemyTileArtworkFiles.Extract(imported, directory, SupportedCartridge.Sha256);
        EnemyTileArtworkFiles.ValidateStock(directory);
        EnemyTileArtworkCatalog stock = EnemyTileArtworkFiles.Load(directory, overrideDirectory: null);
        EnemySpritemapCatalog compositions = stock.Spritemaps!;
        EnemySpritemapDefinition[] shipFrames = GunshipVisualDefinitions.Frames();
        AssertEqual(GunshipVisualDefinitions.FrameCount, shipFrames.Length,
            "#1154 gunship exports its two hull and eleven pad compositions");
        foreach (EnemySpritemapDefinition frame in shipFrames)
        {
            EnemySpritemapPart[] native = EnemySpritemapCatalog.CompileParts(
                EnemySpritemapFiles.ExtractParts(imported, frame.Bank, frame.Pointer), frame.Name);
            AssertTrue(compositions.TryGetDisplay(frame.Bank, frame.Pointer, out var installed),
                $"#1154 installed gunship composition {frame.Name} is resolvable");
            AssertTrue(native.SequenceEqual(installed),
                $"#1154 {frame.Name} retains native offsets, sizes, tiles, priority and flips");
        }

        // Schema 60 is the player's pre-fix format. Its edited ordinary frame
        // must survive, while the newly added gunship frames inherit current stock.
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var stockDocument = JsonSerializer.Deserialize<EnemySpritemapDocument>(
            File.ReadAllText(Path.Combine(directory, EnemySpritemapDefinitions.FileName)), jsonOptions)!;
        var oldFrames = EnemySpritemapDefinitions.Frames[..EnemySpritemapDefinitions.PreGunshipFrameCount].ToArray();
        var legacy = stockDocument with
        {
            Version = EnemySpritemapDefinitions.PreGunshipVersion,
            Frames = oldFrames.ToDictionary(frame => frame.Name, frame => stockDocument.Frames[frame.Name].ToArray()),
            DisplayFrames = oldFrames.ToDictionary(frame => frame.Name, frame => frame.Name),
        };
        string editedName = oldFrames[0].Name;
        legacy.Frames[editedName][0] = legacy.Frames[editedName][0] with { OffsetX = 7 };
        using var legacyJson = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(legacy, jsonOptions));
        var merged = EnemySpritemapCatalog.Load(legacyJson, compositions);
        AssertTrue(merged.TryGetDisplay(oldFrames[0].Bank, oldFrames[0].Pointer, out var edited),
            "#1154 schema-60 edited frame remains resolvable");
        AssertTrue(edited.SequenceEqual(EnemySpritemapCatalog.CompileParts(legacy.Frames[editedName], editedName)),
            "#1154 schema-60 override edit survives the upgrade");
        foreach (EnemySpritemapDefinition frame in shipFrames)
        {
            AssertTrue(merged.TryGetDisplay(frame.Bank, frame.Pointer, out var inherited) &&
                compositions.TryGetDisplay(frame.Bank, frame.Pointer, out var expected) &&
                inherited.SequenceEqual(expected),
                $"#1154 legacy override inherits new {frame.Name} from stock");
        }
        VerifyGunshipLandingCompositions(installationRoot, stock);
        Console.WriteLine("#1154: all 13 gunship compositions match native OAM; schema-60 edits survive and inherit ship art. Bundle: " + directory);
    }

    /// <summary>
    /// #1154: reproduce the first Landing Site fade-in draw after the Ceres
    /// destruction handoff. Retain the actual initialized gameplay/HUD owner,
    /// as the reported escape-to-Landing-Site transition does. No replay is needed.
    /// </summary>
    private static void VerifyGunshipLandingCompositions(string installationRoot, EnemyTileArtworkCatalog? enemyArtwork = null)
    {
        var installation = new GameInstallation(Path.GetFullPath(installationRoot));
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var game = new SuperMetroidGame(bus);
        PrepareRomFreeBindings(installation, enemyArtwork)(game, true);
        game.BindRoomPlmBlueDoorVisuals(installation.LoadRoomPlmBlueDoorVisuals());
        game.BindRoomPlmElevatorPlatformVisuals(installation.LoadRoomPlmElevatorPlatformVisuals());
        long sequence = 0;
        SetState(game, SuperMetroidGameState.SetUpNewGame);
        game.StepCaptured(0, ++sequence, 1);
        game.RuntimeForVerification!.System.SetBossBits(AreaId.Ceres, BossBits.AreaBoss);
        game.RuntimeForVerification.System.LoadSavedLoadingGameState(SaveLoadingGameStates.CeresDestruction);
        SetState(game, SuperMetroidGameState.CeresGoesBoom);
        game.StepCaptured(0, ++sequence, 1);
        var scene = (CeresDestructionCinematicState)typeof(SuperMetroidGame)
            .GetField("ceresDestruction", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(game)!;
        typeof(CeresDestructionCinematicState).GetProperty(nameof(scene.Phase))!
            .SetValue(scene, CeresDestructionPhase.Finished);
        game.StepCaptured(0, ++sequence, 1);
        game.StepCaptured(0, ++sequence, 1);
        AssertEqual(AreaId.Crateria, game.RuntimeForVerification!.ActiveRoom!.AreaIndex,
            "#1154 fixture reaches the reported Landing Site through the existing loader");
        // The fifteen native loading waits publish black; only the subsequent
        // gameplay fade owns the gunship OAM draw that failed in the player report.
        for (int wait = 0; wait < 15; wait++) game.StepCaptured(0, ++sequence, 1);
        AssertEqual(SuperMetroidGameState.MainGameplayFadeIn, game.GameState,
            "#1154 fixture reaches the reported first fade-in frame");
        game.StepCaptured(0, ++sequence, 1);
        Console.WriteLine("#1154: reported first Landing Site fade-in draw completes with installed enemy compositions (RAM-only gameplay).");
    }
}
