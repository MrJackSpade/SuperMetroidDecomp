using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Artwork;

/// <summary>
/// Checks that a pose's art origin is any number of pixels above or below Samus's center (#1627): the body artwork keeps
/// origins past a signed byte's range exactly as installed.
/// </summary>
internal static class SamusArtOriginRangeVerification
{
    /// <summary>Builds body artwork whose art origins lie past -128 and 127 and reads each back.</summary>
    internal static void VerifySamusArtOriginRange()
    {
        InstalledSamusBodyArtwork installed = InstalledFixtures.SamusBody.Gameplay;
        const SamusPoseId high = SamusPoseId.MorphBallGroundRightPose, low = SamusPoseId.MorphBallGroundLeftPose;
        var artwork = new InstalledSamusBodyArtwork(new SamusBodyArtworkContent
        {
            OriginRises = new Dictionary<SamusPoseId, IReadOnlyList<int?>> { [high] = [0], [low] = [0] },
            GraphicsYOffsets = new Dictionary<SamusPoseId, int> { [high] = 200, [low] = -200 },
            DeathSteps = installed.DeathSteps,
            ArmCannon = installed.ArmCannon,
        });
        AssertEqual(200, artwork.GraphicsYOffset(high), "an art origin above 127 pixels is kept");
        AssertEqual(-200, artwork.GraphicsYOffset(low), "an art origin below -128 pixels is kept");
        Console.WriteLine("Samus art origins: origins past a signed byte's range are kept exactly.");
    }
}
