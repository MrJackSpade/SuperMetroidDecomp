using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// The enemy sessions a verification case builds without a runtime (#1552): each receives, as the runtime's does, the
/// installation's enemy and boss over-read services and a rainbow-beam camera checkpoint of its own.
/// </summary>
internal static class EnemySessionFixtures
{
    /// <summary>
    /// A fresh power-on enemy session bound to <paramref name="unauthoredBoostCadence"/>, the installation's enemy and boss
    /// over-read services and a fresh rainbow-beam camera checkpoint.
    /// </summary>
    /// <param name="unauthoredBoostCadence">The unauthored Speed Booster cadence the case shares with its Samus, if any.</param>
    internal static EnemySession WithInstalledServices(UnauthoredBoostCadence unauthoredBoostCadence) =>
        new(unauthoredBoostCadence, InstalledFixtures.EnemyOverreads, InstalledFixtures.BossAndIntroOverreads, new RainbowBeamCameraCheckpoint());

    /// <summary>
    /// A fresh power-on enemy session with a cadence of its own, the installation's enemy and boss over-read services and a
    /// fresh rainbow-beam camera checkpoint.
    /// </summary>
    internal static EnemySession WithInstalledServices() => WithInstalledServices(new UnauthoredBoostCadence());
}
