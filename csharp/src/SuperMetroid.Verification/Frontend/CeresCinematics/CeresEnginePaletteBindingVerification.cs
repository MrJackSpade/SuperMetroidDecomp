using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Frontend.CeresCinematics;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>Checks that Ceres gunship engine colors remain bound after a debugger state restore and artwork replacement.</summary>
internal static class CeresEnginePaletteBindingVerification
{
    /// <summary>
    /// Reproduces #1150 at the Ceres-to-Zebes palette handoff, without replaying
    /// the escape or giving production simulation access to a cartridge.
    /// </summary>
    internal static void VerifyCeresEnginePaletteBinding(string installationRoot)
    {
        var installation = new GameInstallation(Path.GetFullPath(installationRoot));
        AreaMapPresentation stock = MapPresentationFiles.Load(installation.MapDirectory, null);
        IntroCinematicArtwork artwork = installation.LoadIntroCinematicArt();
        InstalledSpriteSheets sheets = installation.LoadSpriteSheets();
        RgbColor[] nativeColors = [RgbColor.White, RgbColor.Black]; // Pinned $8D:C880/$C886, not runtime ROM reads.
        var game = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        game.BindArtwork(game.Artwork with
        {
            MapPresentation = stock,
            IntroCinematicArt = artwork,
            // The destruction scene's actors draw from the installed sprite sheets, as a full binding supplies them.
            SpriteSheets = sheets,
        });
        game.GameState = SuperMetroidGameState.CeresGoesBoom;

        // Enter via the real frontend constructor call, then select exactly the
        // phase reported by the player. No preceding controller playthrough is needed.
        // $8B:C11B's nine NMI waits and its setup step precede the selected phase.
        for (int step = 0; step <= CeresDestructionSceneDefinitions.InitialFrameWaits; step++)
            game.Step(0);
        CeresDestructionCinematicState scene = Scene(game);
        scene.Phase = CeresDestructionPhase.FlyToZebesInitial;
        for (int frame = 0; frame < 4; frame++)
        {
            game.Step(0);
            AssertEqual(nativeColors[frame % nativeColors.Length], EngineColour(scene, CinematicSheetFixtures.CeresFloor(stock.RoomPaletteFx)),
                $"#1150 stock engine color and native one-frame cadence, tick {frame}");
        }

        // Restore, then bind edited art through the same frontend entry point as the desktop
        // host; an art replacement must change the next color, not restart the program's timer.
        using var state = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(state, game);
        state.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(state);
        using var editsScratch = new TestScratchDirectory("ceres-engine-1150");
        string edits = editsScratch.Root;
        Directory.CreateDirectory(edits);
        try
        {
            string paletteFile = Path.Combine(installation.MapDirectory, RoomPaletteFxPresentationFormat.FileName);
            JsonObject document = RequiredJson.Parse(File.ReadAllText(paletteFile)).AsObject();
            string key = document.Select(property => property.Key).Single(name =>
                name.Equals(nameof(RoomPaletteFxPresentationDocument.CeresGunshipEngineLights), StringComparison.OrdinalIgnoreCase));
            document[key] = new JsonArray(
                new JsonArray(new JsonObject { ["red"] = 31, ["green"] = 0, ["blue"] = 0 }),
                new JsonArray(new JsonObject { ["red"] = 0, ["green"] = 31, ["blue"] = 0 }));
            File.WriteAllText(Path.Combine(edits, RoomPaletteFxPresentationFormat.FileName), document.ToJsonString());
            AreaMapPresentation edited = MapPresentationFiles.Load(installation.MapDirectory, edits);
            // Installed artwork is not saved state; the host rebinds all of it after a restore.
            restored.BindArtwork(restored.Artwork with
            {
                MapPresentation = edited,
                IntroCinematicArt = artwork,
                SpriteSheets = sheets,
            });
            CeresDestructionCinematicState restoredScene = Scene(restored);
            RgbColor[] replacementColors = [new(31, 0, 0), new(0, 31, 0)];
            for (int frame = 0; frame < 4; frame++)
            {
                game.Step(0);
                restored.Step(0);
                AssertEqual(scene.EngineShading, restoredScene.EngineShading, "palette rebind preserves the flicker's record");
                AssertEqual(replacementColors[frame % replacementColors.Length],
                    EngineColour(restoredScene, CinematicSheetFixtures.CeresFloor(edited.RoomPaletteFx)),
                    $"#1150 the edited installation's floor is pre-coloured in the edited colours, tick {frame}");
                AssertEqual(scene.Phase, restoredScene.Phase, "palette rebind preserves cinematic phase");
                AssertEqual(scene.Zoom, restoredScene.Zoom, "palette rebind preserves camera scale");
                AssertEqual(scene.Background.X.Whole, restoredScene.Background.X.Whole, "palette rebind preserves camera X");
                AssertEqual(scene.Background.Y.Whole, restoredScene.Background.Y.Whole, "palette rebind preserves camera Y");
                AssertEqual(game.FrameNumber, restored.FrameNumber, "palette rebind preserves frame clock");
            }

            // Earlier debugger graphs can lack the lazily created palette owner.
            // Its reconstruction must receive the rebound provider too.
            restoredScene.PaletteFx = null;
            restored.Step(0);
            AssertEqual(replacementColors[0], EngineColour(restoredScene, CinematicSheetFixtures.CeresFloor(edited.RoomPaletteFx)),
                "#1150 reconstructed palette owner shows the program's first record again");
        }
        finally
        {
            Directory.Delete(edits, recursive: true);
        }
        Console.WriteLine("#1150: stock engine flicker record cadence, edited installation's floor shades, unchanged scene clock/camera and legacy-owner reconstruction pass (RAM-only).");

        static CeresDestructionCinematicState Scene(SuperMetroidGame owner) =>
            (owner.Session.ScreenAs<CeresDestructionCinematicState>() ?? throw new InvalidOperationException("Verification requires owner.ceresDestruction."));

        // The scene names the record the flicker last showed (#1512); the colour that record shows is the one the import
        // pre-coloured the floor's engine ink in for it.
        static RgbColor EngineColour(CeresDestructionCinematicState owner, SpriteSheet floor)
        {
            PictureShade shade = owner.EngineShading.Shades.Single();
            AssertEqual(PaletteFxShades.Of(PaletteFxColours.CutsceneGunshipEngine, new PaletteFxFrame(0)).Effect, shade.Effect,
                "the floor's shading names the gunship engine flicker's record");
            return floor.Image.Shade(shade).Colours.Single();
        }
    }

    /// <summary>Runs the focused Ceres engine palette binding regression against the installed artwork.</summary>
    [VerificationCommand("--ceres-engine-palette-binding")]
    internal static int CeresEnginePaletteBindingCommand()
    {
        string ceresPaletteInstallation = RepositoryInstallation.Installation.Root;
        Suite(nameof(VerifyCeresEnginePaletteBinding), () => VerifyCeresEnginePaletteBinding(ceresPaletteInstallation));
        return 0;
    }
}
