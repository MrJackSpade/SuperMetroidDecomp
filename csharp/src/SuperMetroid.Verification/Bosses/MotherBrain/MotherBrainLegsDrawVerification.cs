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

    /// <summary>Runs the Mother Brain legs draw check.</summary>
    [VerificationCommand("--mother-brain-legs-draw")]
    internal static int MotherBrainLegsDrawCommand()
    {
        Suite(nameof(VerifyMotherBrainLegsShowHerColouring), VerifyMotherBrainLegsShowHerColouring);
        return 0;
    }
}
