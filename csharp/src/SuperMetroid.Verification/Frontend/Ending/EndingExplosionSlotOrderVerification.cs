using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks the ending flyaway's drawing order when the afterglow competes with both starfields.</summary>
internal static class EndingExplosionSlotOrderVerification
{
    /// <summary>Confirms the running order draws all 37 afterglow pieces before the starfields and preserves ship occlusion.</summary>
    internal static void VerifyEndingExplosionSlotOrder()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var audio = AudioQueues.Standalone();
        var ending = CreateRetailEndingFixture(audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Stage.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, ending.Stage.Phase, "native flyaway handoff reached");
        for (int frame = 0; frame < 100; frame++) ending.Step();
        var definition = EndingActorDefinitions.ExplosionAfterglow;
        var afterglow = new IntroDiscoverySprite(new(definition.Position.X, definition.Position.Y),
            EndingShades.Drawn(definition.Colouring), definition.Animation);
        afterglow.Step();
        var expected = ScreenSpritesFixtures.Blank();
        expected.BeginFrame();
        afterglow.Draw(expected, installedArt: EndingSheetFixtures.Drawer(ending, EndingSpriteSheets.Explosion, CinematicSpriteFamily.EndingExplosion));
        expected.FinalizeFrame();
        AssertEqual(37, expected.LastFinalizedSpriteCount, "retail afterglow component count");
        var snapshot = EndingCreditsPresentation.CaptureRenderSnapshot(ending);
        PlacedSpritePart[] afterglowParts = PlacedSpritePart.Of(expected.Draws);
        PlacedSpritePart[] liveParts = [.. snapshot.SpriteParts().Take(afterglowParts.Length)];
        int[] mismatches = [.. Enumerable.Range(0, afterglowParts.Length).Where(index => index >= liveParts.Length || liveParts[index] != afterglowParts[index])];
        AssertTrue(mismatches.Length == 0,
            "the running order draws all 37 afterglow pieces before the starfields" +
            string.Concat(mismatches.Select(index => $" [part {index}: live {(index < liveParts.Length ? liveParts[index].Part.ToString() : "missing")}, native {afterglowParts[index].Part}]")));
        // The white sky, then the afterglow behind the departing gunship, tinted with the sky's white, then the gunship
        // (#1514).
        var fixedWhite = snapshot.LayersOf<ScreenTintLayer>().Single();
        RgbColor white = fixedWhite.Colour;
        AssertTrue(snapshot.Layers is [ScreenTintLayer, SpritesLayer { Tint: { } tint }, TurningFloorLayer] && tint == white,
            "the live getaway ship covers the afterglow, which shares the sky's white");
    }
}
