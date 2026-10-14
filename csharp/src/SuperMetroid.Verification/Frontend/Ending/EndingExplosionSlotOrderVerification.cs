using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.TestSupport.RepositoryInstallation;
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
        var ending = CreateRetailEndingFixture(bus, audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(bus, default);
        }
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, ending.Phase, "native flyaway handoff reached");
        for (int frame = 0; frame < 100; frame++) ending.Step();
        var definition = EndingCreditsRomData.Sprites.ExplosionAfterglow;
        var afterglow = new IntroDiscoverySprite(new(definition.Position.X, definition.Position.Y),
            definition.Attributes.Raw, definition.Program);
        afterglow.Step();
        var expected = new OamBuffer();
        expected.BeginFrame();
        afterglow.Draw(expected, installedArt: RepositoryInstallation.Installation.LoadEndingObjectArt().ExplosionSprites);
        expected.FinalizeFrame();
        AssertEqual(37, expected.LastFinalizedSpriteCount, "retail afterglow component count");
        var actual = ending.Presentation.CaptureRenderSnapshot().Memory;
        AssertTrue(actual.Oam[..(37 * 4)].SequenceEqual(expected.LowTable[..(37 * 4)]),
            "native slot order preserves all 37 afterglow pieces before starfield OAM overflow");
        var snapshot = ending.Presentation.CaptureRenderSnapshot();
        var background = snapshot.LayersOf<Mode7RenderLayer>().Single();
        AssertTrue(Enumerable.Range(0, actual.ModeledSpriteCount).All(index =>
            (actual.Oam[index * 4 + 3] & 0x30) == 0), "live flyaway actors use native priority zero");
        var fixedWhite = snapshot.LayersOf<FixedColorAddRenderLayer>().Single();
        RenderLayer[] order = [fixedWhite, new ObjRenderLayer(FixedColor: fixedWhite), background];
        var nativePixels = SoftwareLayeredSnapshotRenderer.Render(new(actual, order,
            snapshot.ObjectSelection, snapshot.Brightness));
        var actualPixels = ending.Presentation.Render();

        AssertTrue(actualPixels.SequenceEqual(nativePixels),
            "live getaway ship occludes priority-zero afterglow pixels");
    }
}
