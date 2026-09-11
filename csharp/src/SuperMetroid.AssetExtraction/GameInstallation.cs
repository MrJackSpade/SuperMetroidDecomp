using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>App-owned immutable cartridge/audio content, separate from persistent player data.</summary>
public sealed record GameInstallation(string Root)
{
    public string ContentDirectory => Path.Combine(Root, GameInstallationLayout.ContentDirectoryName);
    public string RomPath => Path.Combine(ContentDirectory, GameInstallationLayout.RomFileName);
    public string AudioDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.AudioDirectoryName);
    public string MapDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.MapDirectoryName);
    /// <summary>Outside the replaceable game directory: reinstall and stock repair preserve these edits.</summary>
    public string MapOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.MapDirectoryName);

    /// <summary>Loads installed maps and stock exploration masks without a cartridge address space.</summary>
    public AreaMapPresentationCatalog LoadMaps() => AreaMapPresentationCatalog.Load(MapDirectory, MapOverrideDirectory);
}

/// <summary>Shared on-disk layout used by the desktop host, Android host, and installation CLI.</summary>
public static class GameInstallationLayout
{
    public const string ContentDirectoryName = "game";
    public const string RomFileName = "SuperMetroid.smc";
    public const string AudioDirectoryName = "audio";
    public const string MapDirectoryName = "maps";
    public const string ReceiptFileName = "installation.json";
    public const int FormatVersion = 1;
    internal const string PreviousDirectoryName = ".game.previous";
    internal const string StagingPrefix = ".game.install-";
    internal const string LockFileName = ".game-install.lock";
}
