using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks ending flyaway OAM priority when the afterglow competes with both starfields.</summary>
internal static class EndingExplosionSlotOrderVerification
{
    /// <summary>Confirms native descending actor slots give all 37 afterglow pieces OAM space before the starfields and preserve ship occlusion.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyEndingExplosionSlotOrder()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var audio = new CartridgeAudioState();
        var ending = CreateRetailEndingFixture(audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Stage.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, ending.Stage.Phase, "native flyaway handoff reached");
        for (int frame = 0; frame < 100; frame++) ending.Step();
        var definition = EndingCreditsRomData.Sprites.ExplosionAfterglow;
        var afterglow = new IntroDiscoverySprite(new(definition.Position.X, definition.Position.Y),
            definition.Attributes.Raw, definition.Program);
        afterglow.Step();
        var expected = ScreenSpritesFixtures.Blank();
        expected.BeginFrame();
        afterglow.Draw(expected, installedArt: EndingSheetFixtures.Drawer(ending, EndingObjectSheet.Explosion, CinematicSpriteFamily.EndingExplosion));
        expected.FinalizeFrame();
        AssertEqual(37, expected.LastFinalizedSpriteCount, "retail afterglow component count");
        var snapshot = EndingCreditsPresentation.CaptureRenderSnapshot(ending);
        var actual = snapshot.Paint;
        PlacedSpritePart[] afterglowParts = PlacedSpritePart.Of(expected.Draws);
        PlacedSpritePart[] liveParts = [.. actual.SpriteParts().Take(afterglowParts.Length)];
        int[] mismatches = [.. Enumerable.Range(0, afterglowParts.Length).Where(index => index >= liveParts.Length || liveParts[index] != afterglowParts[index])];
        AssertTrue(mismatches.Length == 0,
            "native slot order preserves all 37 afterglow pieces before starfield OAM overflow" +
            string.Concat(mismatches.Select(index => $" [part {index}: live {(index < liveParts.Length ? liveParts[index].Part.ToString() : "missing")}, native {afterglowParts[index].Part}]")));
        var background = snapshot.LayersOf<Mode7RenderLayer>().Single();
        AssertTrue(actual.SpriteParts().All(part => part.Part.Priority == 0), "live flyaway actors use native priority zero");
        var fixedWhite = snapshot.LayersOf<FixedColorAddRenderLayer>().Single();
        RenderLayer[] order = [fixedWhite, new ObjRenderLayer(FixedColor: fixedWhite), background];
        var nativePixels = DisplayRendering.Render(new LayeredScene(actual, order, snapshot.Brightness));
        var actualPixels = ending.Render();

        AssertTrue(actualPixels.SequenceEqual(nativePixels),
            "live getaway ship occludes priority-zero afterglow pixels");
    }
}
