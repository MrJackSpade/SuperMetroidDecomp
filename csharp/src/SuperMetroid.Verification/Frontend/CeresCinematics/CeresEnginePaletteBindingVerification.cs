using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Fx.Artwork;
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

/// <summary>Checks that Ceres gunship engine colors remain bound after a debugger state restore and artwork replacement.</summary>
internal static class CeresEnginePaletteBindingVerification
{
    /// <summary>
    /// Reproduces #1150 at the Ceres-to-Zebes palette handoff, without replaying
    /// the escape or giving production simulation access to a cartridge.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyCeresEnginePaletteBinding(string installationRoot)
    {
        var installation = new GameInstallation(Path.GetFullPath(installationRoot));
        AreaMapPresentationCatalog stock = AreaMapPresentationFiles.Load(installation.MapDirectory, null);
        IntroCinematicArtworkCatalog artwork = installation.LoadIntroCinematicArt();
        int destination = CeresCinematicLightPaletteFxColorDefinitionsConstants.GunshipEngineColorIndex / sizeof(ushort);
        ushort[] nativeColors = [0x7fff, 0]; // Pinned $8D:C880/$C886, not runtime ROM reads.
        var game = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        game.BindArtwork(game.Artwork with
        {
            MapPresentation = stock,
            IntroCinematicArt = artwork,
        });
        game.GameState = SuperMetroidGameState.CeresGoesBoom;

        // Enter via the real frontend constructor call, then select exactly the
        // phase reported by the player. No preceding controller playthrough is needed.
        // $8B:C11B's nine NMI waits and its setup step precede the selected phase.
        for (int step = 0; step <= CeresDestructionRomData.InitialFrameWaits; step++)
            game.Step(0);
        CeresDestructionCinematicState scene = Scene(game);
        scene.Phase = CeresDestructionPhase.FlyToZebesInitial;
        for (int frame = 0; frame < 4; frame++)
        {
            game.Step(0);
            AssertEqual(nativeColors[frame % nativeColors.Length], Colors(scene)[destination],
                $"#1150 stock engine color and native one-frame cadence, tick {frame}");
        }

        // A state restore deliberately loses nonserialized presentation bindings.
        // Rebind through the same frontend entry point as the desktop host; an art
        // replacement must change the next color, not restart the program's timer.
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
            AreaMapPresentationCatalog edited = AreaMapPresentationFiles.Load(installation.MapDirectory, edits);
            restored.BindArtwork(restored.Artwork with
            {
                MapPresentation = edited,
                IntroCinematicArt = artwork,
            });
            CeresDestructionCinematicState restoredScene = Scene(restored);
            ushort[] replacementColors = [31, 31 << 5];
            for (int frame = 0; frame < 4; frame++)
            {
                game.Step(0);
                restored.Step(0);
                AssertEqual(replacementColors[frame % replacementColors.Length], Colors(restoredScene)[destination],
                    $"#1150 restored scene uses rebound edited colors, tick {frame}");
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
            AssertEqual(replacementColors[0], Colors(restoredScene)[destination],
                "#1150 reconstructed palette owner receives installed colors");
        }
        finally
        {
            Directory.Delete(edits, recursive: true);
        }
        Console.WriteLine("#1150: stock engine flicker, edited-color state rebind, unchanged scene clock/camera and legacy-owner reconstruction pass (RAM-only).");

        static CeresDestructionCinematicState Scene(SuperMetroidGame owner) =>
            (owner.Session.ScreenAs<CeresDestructionCinematicState>() ?? throw new InvalidOperationException("Verification requires owner.ceresDestruction."));

        static Bgr555[] Colors(CeresDestructionCinematicState owner) =>
            owner.palette.Colors.ToArray();
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
