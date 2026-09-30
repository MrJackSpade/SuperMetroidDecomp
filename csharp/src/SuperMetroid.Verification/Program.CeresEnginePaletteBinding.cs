using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>
    /// Reproduces #1150 at the Ceres-to-Zebes palette handoff, without replaying
    /// the escape or giving production simulation access to a cartridge.
    /// </summary>
    private static void VerifyCeresEnginePaletteBinding(string installationRoot)
    {
        var installation = new GameInstallation(Path.GetFullPath(installationRoot));
        AreaMapPresentationCatalog stock = AreaMapPresentationCatalog.Load(installation.MapDirectory, null);
        IntroCinematicArtworkCatalog artwork = installation.LoadIntroCinematicArt();
        int destination = CeresCinematicLightPaletteFxProgramMechanicsDefinitions.GunshipEngineColorIndex / sizeof(ushort);
        ushort[] nativeColors = [0x7fff, 0]; // Pinned $8D:C880/$C886, not runtime ROM reads.
        var game = new SuperMetroidGame(SuperMetroidAddressSpace.CreateWithoutCartridge());
        game.BindMapPresentation(stock);
        game.BindIntroCinematicArt(artwork);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!
            .SetValue(game, SuperMetroidGameState.CeresGoesBoom);

        // Enter via the real frontend constructor call, then select exactly the
        // phase reported by the player. No preceding controller playthrough is needed.
        game.Step(0);
        CeresDestructionCinematicState scene = Scene(game);
        typeof(CeresDestructionCinematicState).GetProperty(nameof(scene.Phase))!
            .SetValue(scene, CeresDestructionPhase.WaitForZebesMusicQueue);
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
        string edits = Path.Combine(Path.GetTempPath(), "SuperMetroid-ceres-engine-1150-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(edits);
        try
        {
            string paletteFile = Path.Combine(installation.MapDirectory, RoomPaletteFxPresentationFormat.FileName);
            JsonObject document = JsonNode.Parse(File.ReadAllText(paletteFile))!.AsObject();
            string key = document.Select(property => property.Key).Single(name =>
                name.Equals(nameof(RoomPaletteFxPresentationDocument.CeresGunshipEngineLights), StringComparison.OrdinalIgnoreCase));
            document[key] = new JsonArray(
                new JsonArray(new JsonObject { ["red"] = 31, ["green"] = 0, ["blue"] = 0 }),
                new JsonArray(new JsonObject { ["red"] = 0, ["green"] = 31, ["blue"] = 0 }));
            File.WriteAllText(Path.Combine(edits, RoomPaletteFxPresentationFormat.FileName), document.ToJsonString());
            AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(installation.MapDirectory, edits);
            restored.BindMapPresentation(edited);
            restored.BindIntroCinematicArt(artwork);
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
                AssertEqual(scene.BackgroundX, restoredScene.BackgroundX, "palette rebind preserves camera X");
                AssertEqual(scene.BackgroundY, restoredScene.BackgroundY, "palette rebind preserves camera Y");
                AssertEqual(game.FrameNumber, restored.FrameNumber, "palette rebind preserves frame clock");
            }

            // Earlier debugger graphs can lack the lazily created palette owner.
            // Its reconstruction must receive the rebound provider too.
            typeof(CeresDestructionCinematicState).GetField("paletteFx", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(restoredScene, null);
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
            (CeresDestructionCinematicState)typeof(SuperMetroidGame)
                .GetField("ceresDestruction", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(owner)!;

        static ushort[] Colors(CeresDestructionCinematicState owner) =>
            ((SnesCgram)typeof(CeresDestructionCinematicState)
                .GetField("cgram", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(owner)!).Colors.ToArray();
    }
}
