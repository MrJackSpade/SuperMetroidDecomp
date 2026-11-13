using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Frontend.CeresCinematics;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks that Ceres gunship engine colors remain bound after a debugger state restore and artwork rebind.</summary>
internal static class CeresEnginePaletteBindingVerification
{
    /// <summary>
    /// Reproduces #1150 at the Ceres-to-Zebes palette handoff, without replaying
    /// the escape or giving production simulation access to a cartridge.
    /// </summary>
    internal static void VerifyCeresEnginePaletteBinding()
    {
        // The repository installation's shared loads: the scene reads them and never writes them.
        AreaMapPresentation stock = InstalledFixtures.Maps;
        ImportIntroCinematicArtwork artwork = InstalledFixtures.IntroArtwork;
        InstalledSpriteSheets sheets = InstalledFixtures.SpriteSheets;
        RgbColor[] nativeColors = [RgbColor.White, RgbColor.Black]; // Pinned $8D:C880/$C886, not runtime ROM reads.
        var game = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        game.BindArtwork(game.Artwork with
        {
            MapPresentation = stock,
            IntroCinematicArt = artwork.BoundCatalog,
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
            AssertEqual(nativeColors[frame % nativeColors.Length], EngineColour(scene, CinematicSheetFixtures.CeresFloor(InstalledFixtures.ImportOf(stock).RoomPaletteFx)),
                $"#1150 stock engine color and native one-frame cadence, tick {frame}");
        }

        // Restore, then bind the installed art through the same frontend entry point as the desktop
        // host; the rebind must keep the flicker's program and timer, not restart them.
        using var state = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(state, game);
        state.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(state);
        // Installed artwork is not saved state; the host rebinds all of it after a restore.
        restored.BindArtwork(restored.Artwork with
        {
            MapPresentation = stock,
            IntroCinematicArt = artwork.BoundCatalog,
            SpriteSheets = sheets,
        });
        CeresDestructionCinematicState restoredScene = Scene(restored);
        for (int frame = 0; frame < 4; frame++)
        {
            game.Step(0);
            restored.Step(0);
            AssertEqual(scene.EngineShading, restoredScene.EngineShading, "palette rebind preserves the flicker's record");
            AssertEqual(EngineColour(scene, CinematicSheetFixtures.CeresFloor(InstalledFixtures.ImportOf(stock).RoomPaletteFx)),
                EngineColour(restoredScene, CinematicSheetFixtures.CeresFloor(InstalledFixtures.ImportOf(stock).RoomPaletteFx)),
                $"#1150 the restored scene shows the running scene's engine colour, tick {frame}");
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
        AssertEqual(nativeColors[0], EngineColour(restoredScene, CinematicSheetFixtures.CeresFloor(InstalledFixtures.ImportOf(stock).RoomPaletteFx)),
            "#1150 reconstructed palette owner shows the program's first record again");
        Console.WriteLine("#1150: stock engine flicker record cadence, restored-state rebind, unchanged scene clock/camera and legacy-owner reconstruction pass.");

        static CeresDestructionCinematicState Scene(SuperMetroidGame owner) =>
            (owner.Session.ScreenAs<CeresDestructionCinematicState>() ?? throw new InvalidOperationException("Verification requires owner.ceresDestruction."));

        // The scene names the record the flicker last showed (#1512); the colour that record shows is the one the import
        // pre-coloured the floor's engine ink in for it.
        static RgbColor EngineColour(CeresDestructionCinematicState owner, SpriteSheet floor)
        {
            PictureShade shade = owner.EngineShading.Shades.Single();
            AssertEqual(PaletteFxShades.Of(PaletteFxColours.CutsceneGunshipEngine, new PaletteFxFrame(PaletteFxProgram.CutsceneGunshipEngineFlicker1, 0)).Effect, shade.Effect,
                "the floor's shading names the gunship engine flicker's record");
            return floor.Image.Shade(shade).Substitutions.Single().Shown;
        }
    }

    /// <summary>Runs the focused Ceres engine palette binding regression against the installed artwork.</summary>
    [VerificationCommand("--ceres-engine-palette-binding")]
    internal static int CeresEnginePaletteBindingCommand()
    {
        Suite(nameof(VerifyCeresEnginePaletteBinding), VerifyCeresEnginePaletteBinding);
        return 0;
    }
}
