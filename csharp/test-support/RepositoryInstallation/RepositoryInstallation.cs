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
    private static readonly Lazy<GameInstallation> installation = new(() =>
    {
        string root = Path.GetFullPath(Path.Combine(CacheRoot, "verification-installed-content"));
        DateTime deadline = DateTime.UtcNow + SetupLockWait;
        while (true)
        {
            try
            {
                return GameAssetInstaller.EnsureInstalled(root) ??
                    GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
            }
            catch (IOException error) when (error.InnerException is IOException && DateTime.UtcNow < deadline)
            {
                // Another verifier holds the setup lock while it extracts; retry until it publishes.
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    });

    /// <summary>The installation's presentation catalogs, loaded once per process and bound to every fixture.</summary>
    private static readonly Lazy<GameArtwork> artwork =
        new(() => Installation.LoadGameArtwork(Installation.LoadMaps(), Installation.LoadProjectiles()));

    /// <summary>Shares immutable extracted catalogs while each fixture owns its bus and mutable state.</summary>
    internal static GameInstallation Installation => installation.Value;

    /// <summary>The installation's presentation catalogs, shared by every fixture in the process.</summary>
    internal static GameArtwork Artwork => artwork.Value;

    /// <summary>A gameplay runtime bound to the installation's presentation catalogs.</summary>
    internal static SuperMetroidRuntime CreateRuntime(
        bool playerInvincibilityEnabled = false, bool infiniteAmmoEnabled = false,
        MapRevealMode mapRevealMode = MapRevealMode.None, bool preventEscapeTimeout = false,
        SuperMetroid.Core.Progression.CartridgeRandom? random = null)
    {
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(new HostGameplayOverrides { PlayerInvincibilityEnabled = playerInvincibilityEnabled, InfiniteAmmoEnabled = infiniteAmmoEnabled, MapRevealMode = mapRevealMode, PreventEscapeTimeout = preventEscapeTimeout }, Installation.LoadGameplayBasePalettes(), random);
        BindRuntime(runtime);
        return runtime;
    }

    /// <summary>Rebinds the host catalogs to a runtime restored from a debugger state, which omits them.</summary>
    internal static void BindRuntime(SuperMetroidRuntime runtime) => runtime.BindArtwork(Artwork, previous: null);
}

