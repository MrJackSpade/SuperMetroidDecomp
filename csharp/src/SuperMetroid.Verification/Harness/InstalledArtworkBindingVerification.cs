using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Harness;

/// <summary>Binds an installation's host catalogs to bare games, and guards cartridge reads, for the checks that need them.</summary>
internal static class InstalledArtworkBindingVerification
{
    /// <summary>
    /// Binds the installation's catalogs, the ones a host binds, to a new or restored game, as a host binds every game it
    /// runs. <paramref name="enemyArtwork"/> replaces the installed enemy, boss and area artwork; when the action's second
    /// argument is false, the intro cinematic artwork is left as the game has it.
    /// </summary>
    internal static Action<SuperMetroidGame, bool> BindInstalledArtwork(GameInstallation installation,
        ImportEnemyArtwork? enemyArtwork = null) =>
        BindInstalledArtwork(InstalledGameBindings.Artwork(installation), enemyArtwork);

    /// <summary>Binds <paramref name="installed"/>, with <paramref name="enemyArtwork"/>'s enemy and boss artwork when given.</summary>
    internal static Action<SuperMetroidGame, bool> BindInstalledArtwork(GameArtwork installed, ImportEnemyArtwork? enemyArtwork)
    {
        GameArtwork artwork = installed;
        if (enemyArtwork is not null)
            artwork = artwork with
            {
                EnemyFrameDisplays = enemyArtwork.Enemies.FrameDisplays,
                KraidEyes = enemyArtwork.Bosses.KraidColors?.Eyes(),
            };
        // An already-finished intro is deliberately not rebound for a restored gameplay room: its restored DMA side effect
        // would test a retired cinematic, not the room under examination.
        return (game, bindIntro) => game.BindArtwork(bindIntro
            ? artwork
            : artwork with { IntroCinematicArt = game.Artwork.IntroCinematicArt });
    }

    /// <summary>Wraps <paramref name="source"/> with a guard rejecting every cartridge read.</summary>
    internal static MutableCartridgeReadGuard NewCartridgeReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => SnesAddressMath.BankNumber(address) is not (0x7e or 0x7f) && (address & 0x8000) != 0,
                address => $"Installed content reread cartridge byte ${address:X6}.");
        return guard;
    }
}
