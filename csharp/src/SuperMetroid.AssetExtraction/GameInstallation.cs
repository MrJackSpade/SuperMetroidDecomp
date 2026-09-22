using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction;

/// <summary>App-owned immutable cartridge/audio content, separate from persistent player data.</summary>
public sealed record GameInstallation(string Root)
{
    public string ContentDirectory => Path.Combine(Root, GameInstallationLayout.ContentDirectoryName);
    public string RomPath => Path.Combine(ContentDirectory, GameInstallationLayout.RomFileName);
    public string AudioDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.AudioDirectoryName);
    /// <summary>Persistent editable audio content, outside the replaceable stock installation.</summary>
    public string AudioOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.AudioDirectoryName);
    /// <summary>Validates stock content, then selects a complete compatible user override when present.</summary>
    public ExtractedAudioAssetCatalog LoadAudio() =>
        ExtractedAudioAssetCatalog.Load(AudioDirectory, AudioOverrideDirectory);
    public string MapDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.MapDirectoryName);
    public string ProjectileDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.ProjectileDirectoryName);
    public string RoomCharacterDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomCharacterDirectoryName);
    /// <summary>Editable room character art stays outside the replaceable stock game directory.</summary>
    public string RoomCharacterOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomCharacterDirectoryName);
    public RoomCharacterAtlasCatalog LoadRoomCharacters() =>
        RoomCharacterArtworkFiles.Load(RoomCharacterDirectory, RoomCharacterOverrideDirectory);
    public string ProjectileOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.ProjectileDirectoryName);
    public InstalledProjectilePresentation LoadProjectiles() => ProjectilePresentationFiles.Load(ProjectileDirectory, ProjectileOverrideDirectory);
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
    public const string ProjectileDirectoryName = "projectiles";
    public const string RoomCharacterDirectoryName = "room-characters";
    public const string ReceiptFileName = "installation.json";
    public const int FormatVersion = 2;
    internal const string PreviousDirectoryName = ".game.previous";
    internal const string StagingPrefix = ".game.install-";
    internal const string LockFileName = ".game-install.lock";
}
