using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>
/// The repository ROM's extracted installation in the ignored <see cref="CacheRoot"/>, shared by the
/// verifier and the developer tools. Each process loads its catalogs once; every fixture built
/// here keeps its own bus and mutable runtime state.
/// </summary>
internal static class RepositoryInstallation
{
    /// <summary>
    /// Ignored, repository-relative home of persistent local caches that runs reuse: this extracted
    /// installation, suite-duration hints and built diagnostic probes.
    /// </summary>
    internal const string CacheRoot = "csharp/.cache";

    /// <summary>
    /// How long a verifier waits for another process that holds the setup lock. The parallel
    /// suite starts every verifier at once, so on a fresh cache one process extracts the
    /// installation while the others wait and then reuse it.
    /// </summary>
    private static readonly TimeSpan SetupLockWait = TimeSpan.FromMinutes(30);

    /// <summary>Installs the pinned repository ROM into ignored verifier content on first access.</summary>
    private static readonly Lazy<GameInstallation> installation =
        new(() => SharedFixtureTiming.Load("Fixture: repository installation", InstallShared));

    /// <summary>Opens, repairs or extracts the one installation every check in the process shares.</summary>
    [PrivateInstallation("The shared repository installation itself: every other check reads it or copies it instead of installing.")]
    private static GameInstallation InstallShared()
    {
        string root = Path.GetFullPath(Path.Combine(CacheRoot, "verification-installed-content"));
        DateTime deadline = DateTime.UtcNow + SetupLockWait;
        while (true)
        {
            var steps = new InstallStepTiming();
            try
            {
                GameInstallation installed = GameAssetInstaller.EnsureInstalled(root, progress: steps) ??
                    GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root, progress: steps);
                steps.Finish();
                return installed;
            }
            catch (IOException error) when (error.InnerException is IOException && DateTime.UtcNow < deadline)
            {
                // Another verifier holds the setup lock while it extracts; retry until it publishes.
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }

    /// <summary>What the game binds of the installation's Samus body, loaded once per process: the artwork binds it.</summary>
    internal static Lazy<SuperMetroid.AssetExtraction.Samus.InstalledSamusBody> SamusBodyLoader { get; } =
        new(() => SharedFixtureTiming.Load("Fixture: repository Samus body", () => Installation.LoadSamusBody()));

    /// <summary>What the game binds of the installation's Samus body, whose game catalog <see cref="Artwork"/> binds.</summary>
    internal static SuperMetroid.AssetExtraction.Samus.InstalledSamusBody SamusBody => SamusBodyLoader.Value;

    /// <summary>The installation's presentation catalogs, loaded once per process and bound to every fixture.</summary>
    internal static Lazy<GameArtwork> ArtworkLoader { get; } =
        new(() => SharedFixtureTiming.Load("Fixture: repository artwork",
            () => Installation.LoadGameArtwork(Installation.LoadMaps(), Installation.LoadProjectiles(), Installation.LoadEnemyTiles(), SamusBody)));

    /// <summary>Shares immutable extracted catalogs while each fixture owns its bus and mutable state.</summary>
    internal static GameInstallation Installation => installation.Value;

    /// <summary>The installation's presentation catalogs, shared by every fixture in the process.</summary>
    internal static GameArtwork Artwork => ArtworkLoader.Value;

    /// <summary>A gameplay runtime bound to the installation's presentation catalogs, playing under the requested assists.</summary>
    internal static SuperMetroidRuntime CreateRuntime(
        bool playerInvincibilityEnabled = false, bool infiniteAmmoEnabled = false,
        MapRevealMode mapRevealMode = MapRevealMode.None, bool preventEscapeTimeout = false,
        SuperMetroid.Core.Compatibility.SharedRandom? random = null, SavedEncounterReset? encounterResetOnLoad = null)
    {
        var assists = new GameplayAssists { SamusCannotDie = playerInvincibilityEnabled, AmmoNeverRunsOut = infiniteAmmoEnabled, MapReveal = mapRevealMode, EscapeTimerHoldsAtOneSecond = preventEscapeTimeout, EncounterResetOnLoad = encounterResetOnLoad };
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(assists, Artwork, random);
        BindRuntime(runtime);
        return runtime;
    }

    /// <summary>Rebinds the host catalogs to a runtime restored from a debugger state, which omits them.</summary>
    internal static void BindRuntime(SuperMetroidRuntime runtime) => runtime.BindArtwork(Artwork, previous: null);
}

