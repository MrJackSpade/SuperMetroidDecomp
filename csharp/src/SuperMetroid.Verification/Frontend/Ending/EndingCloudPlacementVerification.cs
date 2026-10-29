using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Frontend.Ending;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Compares ending scene B's cloud placement and motion with independently stepped actors.</summary>
internal static class EndingCloudPlacementVerification
{
    /// <summary>Checks ending scene B's cloud origins and motion through fade-out against independently stepped actors.</summary>
    internal static void VerifyEndingCloudPlacement()
    {
        Suite(nameof(VerifyEndingTakeoffColorMath), () => VerifyEndingTakeoffColorMath(checkWrapping: true));
        Suite(nameof(VerifyEndingTakeoffColorMath), () => VerifyEndingTakeoffColorMath(checkWrapping: false));
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var audio = new AudioQueues();
        var ending = CreateRetailEndingFixture(audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Stage.Phase != EndingCreditsPhase.FadeInEscapeSceneB; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.FadeInEscapeSceneB, ending.Stage.Phase, "cloud scene setup reached");
        // $8B:D731 spawns EED3/EED9/EEDF/EEE5 with parameter zero.
        // F0B2/F0E1/F0E9/F0F1 therefore all use X=128 and these four Y origins.
        short[] nativeY = [-96, -32, 288, 352];
        EndingSpriteRole[] roles = [EndingSpriteRole.CloudTopA, EndingSpriteRole.CloudTopB,
            EndingSpriteRole.CloudBottomA, EndingSpriteRole.CloudBottomB];
        EndingSpriteDefinition[] definitions = [EndingActorDefinitions.EscapeBCloudTopA,
            EndingActorDefinitions.EscapeBCloudTopB, EndingActorDefinitions.EscapeBCloudBottomA,
            EndingActorDefinitions.EscapeBCloudBottomB];
        var actors = definitions.Select((definition, index) => new EndingSprite(
            new IntroDiscoverySprite(new(128, unchecked((ushort)nativeY[index])), definition.Palette,
                definition.Program), roles[index])).ToArray();
        var cloudSprites = CinematicSheetFixtures.Drawer(RepositoryInstallation.Installation.LoadEndingObjectArt().CloudSprites);
        int checkedFrames = 0;
        for (int frame = 0; frame < 200; frame++)
        {
            ending.Step();
            if (ending.Stage.Phase is not (EndingCreditsPhase.FadeInEscapeSceneB or
                EndingCreditsPhase.EscapeSceneB or EndingCreditsPhase.FadeOutEscapeSceneB))
                break;
            for (int index = 0; index < actors.Length; index++)
            {
                var actor = actors[index];
                // F455/F478 select their moving callbacks on the first call. The
                // next call starts +/-1 Y per frame, even after zoom exceeds 176.
                // Do not reuse EndingCloudMotion as the expected-motion oracle.
                if (frame != 0)
                    actor.Sprite.WholePosition = actor.Sprite.WholePosition.WithY(
                        unchecked((ushort)(actor.Sprite.WholePosition.Y + (index < 2 ? 1 : -1))));
                actor.Sprite.Step();
            }
            var oam = ScreenSpritesFixtures.Blank();
            oam.BeginFrame();
            foreach (var actor in actors) actor.Sprite.Draw(oam, installedArt: cloudSprites);
            oam.FinalizeFrame();
            var actual = EndingCreditsPresentation.CaptureRenderSnapshot(ending);
            byte[] actualVram = ending.ShownVram();
            byte[] oamBytes = [.. oam.LowTable.ToArray(), .. oam.HighTable.ToArray()];
            var expectedMemory = FramePaint.FromOam(actualVram, actual.Paint.Colors,
                oamBytes, oam.LastFinalizedSpriteCount, SuperMetroid.AssetExtraction.Frontend.Ending.EndingSheetObjectSelection.Of(EndingCreditsPresentation.CurrentEscapeObjectSheet(ending)));
            var expected = DisplayRendering.Render(new LayeredScene(expectedMemory, actual.Layers, actual.Brightness));
            var pixels = ending.Render();
            int differs = pixels.ToArray().Zip(expected, (live, native) => live != native).ToList().IndexOf(true);
            AssertTrue(differs < 0, $"scene B cloud positions/motion frame {frame} ({ending.Stage.Phase})" + (differs < 0 ? ""
                : $": pixel ({differs % 256},{differs / 256}) live {pixels[differs]}, native {expected[differs]}; live draws " +
                    string.Join(", ", actual.Paint.SpriteDraws.OfType<ImageSpriteDraw>().Select(draw =>
                        $"{draw.Image.Size.X}x{draw.Image.Size.Y} at {draw.Corner.X},{draw.Corner.Y}")) + "; native objects " +
                    string.Join(", ", Enumerable.Range(0, oam.LastFinalizedSpriteCount).Select(index => oam.GetEntry(index)).Select(entry =>
                        $"{entry.TileNumber:X3}{(entry.IsLarge ? "L" : "s")} at {entry.Position.X},{entry.Position.Y}"))));
            checkedFrames++;
        }
        AssertEqual(EndingCreditsPhase.FadeInZebesExplosion, ending.Stage.Phase, "complete scene B checked through fade-out");
        Console.WriteLine($"Ending scene B: {checkedFrames} full frames match independently stepped native cloud origins/motion; graphics source is shared, not a full cartridge pixel oracle.");
    }

    /// <summary>Checks that the takeoff scene's floor wraps beyond its edges, or that it adds the floor to the frame's main pixels.</summary>
    internal static void VerifyEndingTakeoffColorMath(bool checkWrapping)
    {
        var audio = new AudioQueues();
        var ending = CreateRetailEndingFixture(audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Stage.Phase != EndingCreditsPhase.EscapeSceneA; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.EscapeSceneA, ending.Stage.Phase, "takeoff color-math scene reached");
        for (int frame = 0; frame < 90; frame++) ending.Step();
        TurningFloorLayer shown = EndingCreditsPresentation.CaptureRenderSnapshot(ending).LayersOf<TurningFloorLayer>().First();
        if (checkWrapping)
        {
            // SetupPpu_5_Mode7 ($8B:8293) writes M7SEL=0, not the $80 transparent-overflow mode.
            AssertEqual(FloorOverflowMode.Wrap, shown.Transform.Outside,
                "takeoff M7SEL=0 wraps rotated background instead of exposing square map edges");
            return;
        }
        // D6xx: TM=BG1|OBJ, TS=BG1, CGWSEL=2, CGADSUB=BG1|OBJ: the floor is added once to the main pixels and no
        // object is subtracted.
        AssertTrue(shown.AddsForeground && !shown.SubtractsSprites, "takeoff applies native BG1 subscreen to eligible main pixels");
    }

    /// <summary>Dispatches the requested ending takeoff wrapping or color-math suite.</summary>
    [VerificationCommand("--ending-takeoff-wrap")]
    [VerificationCommand("--ending-takeoff-math")]
    internal static int EndingTakeoffWrapCommand(string[] args)
    {
        Suite(nameof(VerifyEndingTakeoffColorMath), () => VerifyEndingTakeoffColorMath(args[0] == "--ending-takeoff-wrap"));
        return 0;
    }
}
