using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainBodyPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's legs, her body's own picture, show her colouring (#2636): her damage, her beam, her greying and her phase
/// two recolour them with the rest of her.
/// </summary>
internal static class MotherBrainLegsDrawVerification
{
    /// <summary>Checks the body's prepared draw in every health band: her own shading, with the band's shade.</summary>
    internal static void VerifyMotherBrainLegsShowHerColouring()
    {
        var fixture = new MotherBrainPresentationFixture(InstalledFixtures.EnemyArtwork);
        fixture.Body.Frame = EnemyFrameId.MotherBrainBodyStanding;
        foreach (MotherBrainHealthBand band in MotherBrainHealthBandDefinitions.HighestFirst)
        {
            fixture.State.Colours.ShowHealth(band);
            EnemyDrawList.PreparedEnemyDraw legs = fixture.Inner.Enemies.Drawing.Advance(Position16<RoomPixels>.Zero)
                .Single(draw => ReferenceEquals(draw.Place, fixture.Body));
            AssertEqual(fixture.State.Colours.OwnShading, legs.Shading, $"her legs show her own colouring in health band {band}");
            AssertTrue(legs.Shading.Shades.Contains(MotherBrainShades.Health(band)), $"her legs show health band {band}'s colours");
        }
    }

    /// <summary>
    /// Checks every body frame's draws (#2633, #2634): her near leg's picture in front of the room's front tiles, then her far
    /// leg's own picture behind them and her body, in her shading, at her picture's place.
    /// </summary>
    internal static void VerifyMotherBrainFarLegStandsBehindTheFrontTiles()
    {
        var fixture = new MotherBrainPresentationFixture(InstalledFixtures.EnemyArtwork);
        foreach (EnemyFrameId frame in MotherBrainBodyVisualDefinitions.AnimationFrames)
        {
            fixture.Body.Frame = frame;
            var sprites = new SpriteFrameBuffer();
            sprites.BeginFrame();
            fixture.DrawInto(sprites);
            sprites.FinalizeFrame();
            ImageSpriteDraw[] body = [.. sprites.Draws.OfType<ImageSpriteDraw>()
                .Where(draw => draw.Image.Sheet.Name.StartsWith(nameof(EnemyDefinitionId.MotherBrainBody), StringComparison.Ordinal))];
            AssertEqual(2, body.Length, $"{frame} draws her near leg's picture and her far leg's");
            AssertEqual(ActorDepth.InFront, body[0].Image.Depth, $"{frame}'s near leg stands in front of the room's front tiles");
            AssertEqual(ActorDepth.BehindFrontParts, body[1].Image.Depth, $"{frame}'s far leg stands behind the room's front tiles");
            AssertEqual(body[0].Origin, body[1].Origin, $"{frame}'s far leg lies where her picture lies");
            AssertEqual(body[0].Shading, body[1].Shading, $"{frame}'s far leg shows her shading");
        }
    }

    /// <summary>Runs the Mother Brain legs draw checks.</summary>
    [VerificationCommand("--mother-brain-legs-draw")]
    internal static int MotherBrainLegsDrawCommand()
    {
        Suite(nameof(VerifyMotherBrainLegsShowHerColouring), VerifyMotherBrainLegsShowHerColouring);
        Suite(nameof(VerifyMotherBrainFarLegStandsBehindTheFrontTiles), VerifyMotherBrainFarLegStandsBehindTheFrontTiles);
        return 0;
    }
}
