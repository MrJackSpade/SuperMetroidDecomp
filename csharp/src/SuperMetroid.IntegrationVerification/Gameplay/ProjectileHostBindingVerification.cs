using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.IntegrationVerification.Gameplay;
using SuperMetroid.Diagnostics.Sessions;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks a host's binding of installed projectile art at startup.</summary>
internal static class ProjectileHostBindingVerification
{
    /// <summary>Verifies host binding on a private copy of the repository installation, into which the session writes its host options.</summary>
    public static int Run()
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopy();
        GameInstallation installation = copy.Installation;
        InstalledProjectilePresentation projectiles = installation.LoadProjectiles();
        using (var session = new InstalledGameSession(installation.Root))
        {
            CheckFrameBindings(session.Game.Artwork.ProjectileFrameBindings, projectiles.FrameBindings);
            CheckGrapple(session.Game.Artwork, projectiles.Grapple);
            CheckFlare(session.Game.Artwork.ChargeFlarePlacement ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.ChargeFlarePlacement."), projectiles.FlarePlacement);
        }
        Console.WriteLine("PASS installed projectile binding: the host binds the installed frame choices, flare placement and Grapple presentation at startup.");
        return 0;
    }

    /// <summary>Checks that the host selected the installed projectile frame binding table.</summary>
    private static void CheckFrameBindings(object? actual, InstalledProjectileFrameBindings expected)
    {
        if (actual is not InstalledProjectileFrameBindings catalog ||
            catalog.Resolve(0x86db) != expected.Resolve(0x86db))
            throw new InvalidDataException("Host did not bind current projectile frame choices.");
    }

    /// <summary>Checks flare placement for both running states and every direction.</summary>
    private static void CheckFlare(ChargeFlarePlacements actual, ChargeFlarePlacements expected)
    {
        if (actual is null) throw new InvalidDataException("Host did not bind flare placement.");
        foreach (ChargeFlarePlacement placement in ChargeFlarePlacements.Placements(ChargeFlareSource.Beam))
            if (actual.Resolve(placement.Running, placement.Aim) != expected.Resolve(placement.Running, placement.Aim))
                throw new InvalidDataException("Host restored stale flare placement.");
    }

    /// <summary>Checks the Grapple flare placement and swing choices the host bound.</summary>
    private static void CheckGrapple(GameArtwork actual, InstalledGrapplePresentation expected)
    {
        CheckFlare((actual.GrappleFlarePlacement ?? throw new InvalidOperationException("Verification requires actual.GrappleFlarePlacement.")), expected.FlarePlacement);
        if (actual.GrappleSwingFrames is null) throw new InvalidDataException("Host did not bind Grapple swing frames.");
        foreach (SuperMetroid.Core.Samus.Grapple.GrappleSwingDirection direction in SuperMetroid.Core.Samus.Grapple.GrappleSwingDirection.All)
            if (actual.GrappleSwingFrames.Resolve(direction) != expected.SwingFrames.Resolve(direction))
                throw new InvalidDataException("Host restored stale Grapple swing frames.");
    }

}
