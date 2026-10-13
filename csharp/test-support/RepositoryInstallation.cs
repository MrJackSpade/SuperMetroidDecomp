using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

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

    private static readonly Lazy<GameInstallation> installation = new(() =>
    {
        string root = Path.GetFullPath(Path.Combine(CacheRoot, "verification-installed-content"));
        return GameAssetInstaller.EnsureInstalled(root) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
    });

    private static readonly Lazy<Action<SuperMetroidRuntime>> runtimeBindings =
        new(() => InstalledRuntimeBindings.Create(Installation));

    internal static GameInstallation Installation => installation.Value;

    /// <summary>A gameplay runtime bound to the installation's presentation catalogs.</summary>
    internal static SuperMetroidRuntime CreateRuntime(ISnesAddressSpace addressSpace,
        bool playerInvincibilityEnabled = false, bool infiniteAmmoEnabled = false,
        MapRevealMode mapRevealMode = MapRevealMode.None, bool preventEscapeTimeout = false)
    {
        var runtime = new SuperMetroidRuntime(addressSpace, new HostGameplayOverrides { PlayerInvincibilityEnabled = playerInvincibilityEnabled, InfiniteAmmoEnabled = infiniteAmmoEnabled, MapRevealMode = mapRevealMode, PreventEscapeTimeout = preventEscapeTimeout }, Installation.LoadGameplayBasePalettes());
        BindRuntime(runtime);
        return runtime;
    }

    /// <summary>Rebinds the host catalogs to a runtime restored from a debugger state, which omits them.</summary>
    internal static void BindRuntime(SuperMetroidRuntime runtime) => runtimeBindings.Value(runtime);
}

