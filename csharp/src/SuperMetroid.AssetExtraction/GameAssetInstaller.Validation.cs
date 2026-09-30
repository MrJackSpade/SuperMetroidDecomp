using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction;

public static partial class GameAssetInstaller
{
    /// <summary>
    /// Strictly validates every required extracted resource using the exact startup
    /// contract. Unlike a repair-admission query, preserves the failing path/format
    /// exception for tools. Never opens a ROM or imports, repairs or replaces content.
    /// </summary>
    public static GameInstallation ValidateExtractedContent(string root)
    {
        var installation = new GameInstallation(Path.GetFullPath(root));
        if (!Directory.Exists(installation.ContentDirectory))
            throw new DirectoryNotFoundException($"Extracted content directory is missing: {installation.ContentDirectory}");
        using FileStream gate = Lock(installation.Root);
        ValidateRequiredExtractedContent(installation);
        return installation;
    }

    /// <summary>Shared strict implementation; startup may use its exception to decide whether repair is needed.</summary>
    private static void ValidateRequiredExtractedContent(GameInstallation installation)
    {
        string receiptPath = Path.Combine(installation.ContentDirectory, GameInstallationLayout.ReceiptFileName);
        var receipt = JsonSerializer.Deserialize<InstallationReceipt>(File.ReadAllText(receiptPath));
        if (receipt is null || receipt.FormatVersion != GameInstallationLayout.FormatVersion)
            throw new InvalidDataException($"Installation receipt {receiptPath} requires format " +
                $"{GameInstallationLayout.FormatVersion}; found {receipt?.FormatVersion.ToString() ?? "null"}. Reimport the supported cartridge.");
        if (receipt.RomSha256 != SupportedCartridge.Sha256)
            throw new InvalidDataException($"Installation receipt {receiptPath} has unsupported source provenance.");
        if (receipt.RoomArtIndexSha256 != Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(installation.RoomArtIndexPath))))
            throw new InvalidDataException($"Room artwork index failed its receipt hash: {installation.RoomArtIndexPath}");
        _ = RoomArtIndexFiles.Load(installation.ContentDirectory);
        // This is the startup inventory, not a sample of whichever room was run.
        // Keep the declared stock-resource checks in one ordered inventory.
        // Completeness of each domain's required references is audited separately.
        _ = ExtractedAudioAssetCatalog.Load(installation.AudioDirectory);
        AreaMapPresentationCatalog.ValidateStock(installation.MapDirectory);
        GameplayBasePaletteFiles.ValidateStock(installation.GameplayBasePaletteDirectory);
        StandardObjectArtworkFiles.ValidateStock(installation.StandardObjectDirectory);
        _ = ProjectilePresentationFiles.Load(installation.ProjectileDirectory, null);
        EnemyTileArtworkFiles.ValidateStock(installation.EnemyTileDirectory);
        RoomCharacterArtworkFiles.ValidateStock(installation.RoomCharacterDirectory);
        SamusBodyArtworkFiles.ValidateStock(installation.SamusBodyDirectory);
        IntroCinematicArtworkFiles.ValidateStock(installation.IntroCinematicDirectory);
        EndingMode7ArtworkFiles.ValidateStock(installation.EndingMode7Directory);
        EndingObjectArtworkFiles.ValidateStock(installation.EndingObjectDirectory);
        EndingPaletteArtworkFiles.ValidateStock(installation.EndingPaletteDirectory);
        RoomStaticPaletteArtworkFiles.ValidateStock(installation.RoomPaletteDirectory);
        RoomMetatileArtworkFiles.ValidateStock(installation.RoomMetatileDirectory);
        RoomBackgroundTilemapArtworkFiles.ValidateStock(installation.RoomBackgroundTilemapDirectory);
        RoomVisualLayoutFiles.ValidateStock(installation.RoomVisualLayoutDirectory);
        RoomPlmShotBlockVisualFiles.ValidateStock(installation.RoomPlmShotBlockVisualDirectory);
        RoomPlmGrappleBlockVisualFiles.ValidateStock(installation.RoomPlmGrappleBlockVisualDirectory);
        RoomPlmStationVisualFiles.ValidateStock(installation.RoomPlmStationVisualDirectory);
        RoomPlmBlueDoorVisualFiles.ValidateStock(installation.RoomPlmBlueDoorVisualDirectory);
        RoomPlmColoredDoorVisualFiles.ValidateStock(installation.RoomPlmColoredDoorVisualDirectory);
        RoomPlmGreyDoorVisualFiles.ValidateStock(installation.RoomPlmGreyDoorVisualDirectory);
        RoomPlmEyeDoorVisualFiles.ValidateStock(installation.RoomPlmEyeDoorVisualDirectory);
        RoomPlmMotherBrainGlassVisualFiles.ValidateStock(installation.RoomPlmMotherBrainGlassVisualDirectory);
        RoomPlmNoobTubeVisualFiles.ValidateStock(installation.RoomPlmNoobTubeVisualDirectory);
        RoomPlmDownwardGateVisualFiles.ValidateStock(installation.RoomPlmDownwardGateVisualDirectory);
        RoomPlmElevatorPlatformVisualFiles.ValidateStock(installation.RoomPlmElevatorPlatformVisualDirectory);
        RoomPlmEscapeGateVisualFiles.ValidateStock(installation.RoomPlmEscapeGateVisualDirectory);
        RoomPlmBombTorizoHandVisualFiles.ValidateStock(installation.RoomPlmBombTorizoHandVisualDirectory);
        RoomPlmDraygonCannonVisualFiles.ValidateStock(installation.RoomPlmDraygonCannonVisualDirectory);
        RoomPlmChozoStatueVisualFiles.ValidateStock(installation.RoomPlmChozoStatueVisualDirectory);
        RoomPlmLinkedRestoreVisualFiles.ValidateStock(installation.RoomPlmLinkedRestoreVisualDirectory);
        RoomPlmTourianAccessVisualFiles.ValidateStock(installation.RoomPlmTourianAccessVisualDirectory);
        RoomPlmSpeedBoosterVisualFiles.ValidateStock(installation.RoomPlmSpeedBoosterVisualDirectory);
        RoomPlmMaridiaElevatubeVisualFiles.ValidateStock(installation.RoomPlmMaridiaElevatubeVisualDirectory);
        RoomPlmSporeSpawnCeilingVisualFiles.ValidateStock(installation.RoomPlmSporeSpawnCeilingVisualDirectory);
        RoomPlmSamusEaterVisualFiles.ValidateStock(installation.RoomPlmSamusEaterVisualDirectory);
        RoomPlmBotwoonWallVisualFiles.ValidateStock(installation.RoomPlmBotwoonWallVisualDirectory);
        RoomPlmKraidVisualFiles.ValidateStock(installation.RoomPlmKraidVisualDirectory);
        RoomPlmCrocomireVisualFiles.ValidateStock(installation.RoomPlmCrocomireVisualDirectory);
        RoomPlmMotherBrainFakeDeathVisualFiles.ValidateStock(installation.RoomPlmMotherBrainFakeDeathVisualDirectory);
        RoomPlmCollectibleVisualFiles.ValidateStock(installation.RoomPlmCollectibleVisualDirectory);
        RoomPlmDynamicCollectibleArtFiles.ValidateStock(installation.RoomPlmDynamicCollectibleArtDirectory);
        XrayRevealVisualFiles.ValidateStock(installation.XrayRevealVisualDirectory);
        RoomSkyTilemapArtworkFiles.ValidateStock(installation.RoomBackgroundTilemapDirectory);
    }
}
