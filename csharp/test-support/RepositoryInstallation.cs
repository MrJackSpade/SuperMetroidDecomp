using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// The repository ROM's extracted installation in ignored <c>csharp/test-temp</c>, shared by the
/// verifier and the developer tools. Each process loads its catalogs once; every fixture built
/// here keeps its own bus and mutable runtime state.
/// </summary>
internal static class RepositoryInstallation
{
    /// <summary>Installs the pinned repository ROM into ignored verifier content on first access.</summary>
    private static readonly Lazy<GameInstallation> installation = new(() =>
    {
        string root = Path.GetFullPath("csharp/test-temp/verification-installed-content");
        return GameAssetInstaller.EnsureInstalled(root) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
    });

    /// <summary>Builds host catalog bindings once, then applies them to each fresh or restored runtime.</summary>
    private static readonly Lazy<Action<SuperMetroidRuntime>> runtimeBindings =
        new(() => InstalledRuntimeBindings.Create(Installation));

    /// <summary>Shares immutable extracted catalogs while each fixture owns its bus and mutable state.</summary>
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

