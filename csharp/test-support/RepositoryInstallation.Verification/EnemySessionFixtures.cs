using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// The enemy sessions a verification case builds without a runtime (#1552, #1616): each receives, as the runtime's does,
/// the installation's enemy and boss over-read services, a rainbow-beam camera checkpoint of its own and the enemy sheets
/// and frame displays its enemies draw from.
/// </summary>
internal static class EnemySessionFixtures
{
    /// <summary>
    /// A fresh power-on enemy session bound to the installation's services, a fresh rainbow-beam camera checkpoint, and
    /// the sheets and frame displays of the installation's enemy artwork. The sheets are composed when an enemy first
    /// draws, so a case that draws none loads none.
    /// </summary>
    internal static EnemySession WithInstalledServices() =>
        new(InstalledFixtures.EnemyOverreads, InstalledFixtures.BossAndIntroOverreads, new RainbowBeamCameraCheckpoint(),
            new SheetsOnFirstDraw(() => EnemySheetFixtures.For(InstalledFixtures.EnemyTiles)), InstalledFixtures.EnemyFrameDisplays);

    /// <summary>A fresh power-on enemy session drawing from <paramref name="tiles"/>' sheets and frame displays.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="tiles"/> holds only one of the two enemy frame documents.</exception>
    internal static EnemySession WithInstalledServices(EnemyTileSourceArtwork tiles)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        return new(InstalledFixtures.EnemyOverreads, InstalledFixtures.BossAndIntroOverreads, new RainbowBeamCameraCheckpoint(),
            EnemySheetFixtures.For(tiles), tiles.FrameDisplays ?? throw new InvalidOperationException(
                "The fixture's enemy artwork holds only one of the two enemy frame documents, so it has no frame displays."));
    }

    /// <summary>
    /// A fresh power-on enemy session drawing from <paramref name="tiles"/>' sheets, for a fixture artwork that authors its
    /// pictures but not its frames, with the installation's frame displays.
    /// </summary>
    internal static EnemySession WithInstalledFrames(EnemyTileSourceArtwork tiles)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        return new(InstalledFixtures.EnemyOverreads, InstalledFixtures.BossAndIntroOverreads, new RainbowBeamCameraCheckpoint(),
            EnemySheetFixtures.For(tiles), InstalledFixtures.EnemyFrameDisplays);
    }

    /// <summary>The sheets <paramref name="compose"/> gives, composed when an enemy first draws from them.</summary>
    private sealed class SheetsOnFirstDraw(Func<IEnemySheetSource> compose) : IEnemySheetSource
    {
        /// <summary>The composed sheets.</summary>
        private readonly Lazy<IEnemySheetSource> sheets = new(compose);

        /// <inheritdoc/>
        public PopulationArtwork Standard => sheets.Value.Standard;

        /// <inheritdoc/>
        public PopulationArtwork For(RoomEnemyPopulationDefinition population) => sheets.Value.For(population);
    }
}
