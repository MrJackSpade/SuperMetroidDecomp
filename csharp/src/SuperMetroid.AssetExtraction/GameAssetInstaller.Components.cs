using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction;

public static partial class GameAssetInstaller
{
    /// <summary>One extraction stage: its progress text, extractor, and stock validator.</summary>
    /// <param name="Progress">User-facing progress text reported before extraction begins.</param>
    /// <param name="Extract">Action that writes this resource into the staging directory.</param>
    /// <param name="Validate">Stock-content validator run immediately after extraction.</param>
    private sealed record InstallerStep(
        string Progress,
        Action<CartridgeImportAddressSpace, string, CancellationToken> Extract,
        Action<string> Validate);

    /// <summary>
    /// One extracted-content directory. A repair keeps a component whose installed files still
    /// validate and re-extracts only the components that fail, then republishes atomically.
    /// </summary>
    /// <param name="DirectoryName">Content subdirectory owned by this component.</param>
    /// <param name="Steps">Extraction and validation stages performed for the component.</param>
    private sealed record InstallerComponent(string DirectoryName, params InstallerStep[] Steps)
    {
        /// <summary>Runs each stage's stock validator against the component's installed directory.</summary>
        /// <param name="contentDirectory">Root extracted-content directory.</param>
        public void Validate(string contentDirectory)
        {
            string directory = Path.Combine(contentDirectory, DirectoryName);
            foreach (InstallerStep step in Steps) step.Validate(directory);
        }
    }

    /// <summary>Adapts a non-cancellable extractor to the installer stage contract.</summary>
    /// <param name="progress">User-facing stage progress text.</param>
    /// <param name="extract">Extractor that writes one component directory.</param>
    /// <param name="validate">Validator for the resulting stock resources.</param>
    /// <returns>An installer stage that invokes the extractor and validator.</returns>
    private static InstallerStep Step(string progress,
        Action<CartridgeImportAddressSpace, string> extract, Action<string> validate) =>
        new(progress, (bus, directory, _) => extract(bus, directory), validate);

    /// <summary>Every extracted-content component, in extraction order.</summary>
    private static readonly InstallerComponent[] Components =
    [
        new(GameInstallationLayout.AudioDirectoryName,
            Step("Extracting audio…", (bus, d) => SpcAudioAssetExtractor.Extract(bus, d), d => _ = ExtractedAudioAssetCatalog.Load(d))),
        new(GameInstallationLayout.MapDirectoryName,
            new InstallerStep("Extracting map presentation...",
                (bus, d, cancellation) => MapPresentationExtractor.Extract(bus, d, SupportedCartridge.Sha256, cancellation),
                AreaMapPresentationCatalog.ValidateStock)),
        new(GameInstallationLayout.GameplayBasePaletteDirectoryName,
            Step("Extracting gameplay base palettes...", (bus, d) => GameplayBasePaletteFiles.Extract(bus, d, SupportedCartridge.Sha256), GameplayBasePaletteFiles.ValidateStock)),
        new(GameInstallationLayout.StandardObjectDirectoryName,
            Step("Extracting standard sprite artwork...", (bus, d) => StandardObjectArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), StandardObjectArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.ProjectileDirectoryName,
            Step("Extracting projectile compositions...", (bus, d) => ProjectilePresentationFiles.Extract(bus, d), d => _ = ProjectilePresentationFiles.Load(d, null))),
        new(GameInstallationLayout.EnemyTileDirectoryName,
            Step("Extracting ordinary enemy tile sheets...", (bus, d) => EnemyTileArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), EnemyTileArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.RoomCharacterDirectoryName,
            Step("Extracting room character artwork...", (bus, d) => RoomCharacterArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomCharacterArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.SamusBodyDirectoryName,
            Step("Extracting Samus body artwork...", (bus, d) => SamusBodyArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), SamusBodyArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.IntroCinematicDirectoryName,
            Step("Extracting opening-cinematic character artwork...", (bus, d) => IntroCinematicArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), IntroCinematicArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.EndingMode7DirectoryName,
            Step("Extracting ending Mode-7 artwork...", (bus, d) => EndingMode7ArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), EndingMode7ArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.EndingObjectDirectoryName,
            Step("Extracting ending object artwork...", (bus, d) => EndingObjectArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), EndingObjectArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.EndingPaletteDirectoryName,
            Step("Extracting ending palettes...", (bus, d) => EndingPaletteArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), EndingPaletteArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPaletteDirectoryName,
            Step("Extracting room base palettes...", (bus, d) => RoomStaticPaletteArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomStaticPaletteArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.RoomMetatileDirectoryName,
            Step("Extracting room block compositions...", (bus, d) => RoomMetatileArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomMetatileArtworkFiles.ValidateStock)),
        // Scrolling-sky tilemaps share the room-background directory and its manifest family.
        new(GameInstallationLayout.RoomBackgroundTilemapDirectoryName,
            Step("Extracting room background tilemaps...", (bus, d) => RoomBackgroundTilemapArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomBackgroundTilemapArtworkFiles.ValidateStock),
            Step("Extracting scrolling-sky tilemaps...", (bus, d) => RoomSkyTilemapArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomSkyTilemapArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.RoomVisualLayoutDirectoryName,
            Step("Extracting room visual layouts...", (bus, d) => RoomVisualLayoutFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomVisualLayoutFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName,
            Step("Extracting shot-block PLM visuals...", (bus, d) => RoomPlmShotBlockVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmShotBlockVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName,
            Step("Extracting Grapple-block PLM visuals...", (bus, d) => RoomPlmGrappleBlockVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmGrappleBlockVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmStationVisualDirectoryName,
            Step("Extracting station PLM visuals...", (bus, d) => RoomPlmStationVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmStationVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName,
            Step("Extracting blue-door PLM visuals...", (bus, d) => RoomPlmBlueDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmBlueDoorVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName,
            Step("Extracting colored-door PLM visuals...", (bus, d) => RoomPlmColoredDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmColoredDoorVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName,
            Step("Extracting grey-door PLM visuals...", (bus, d) => RoomPlmGreyDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmGreyDoorVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName,
            Step("Extracting eye-door PLM visuals...", (bus, d) => RoomPlmEyeDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmEyeDoorVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName,
            Step("Extracting Mother Brain glass PLM visuals...", (bus, d) => RoomPlmMotherBrainGlassVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmMotherBrainGlassVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName,
            Step("Extracting n00b-tube PLM visuals...", (bus, d) => RoomPlmNoobTubeVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmNoobTubeVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName,
            Step("Extracting downward-gate PLM visuals...", (bus, d) => RoomPlmDownwardGateVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmDownwardGateVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmElevatorPlatformVisualDirectoryName,
            Step("Extracting elevator-platform PLM visuals...", (bus, d) => RoomPlmElevatorPlatformVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmElevatorPlatformVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName,
            Step("Extracting Mother Brain escape-gate visuals...", (bus, d) => RoomPlmEscapeGateVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmEscapeGateVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName,
            Step("Extracting Bomb Torizo hand visuals...", (bus, d) => RoomPlmBombTorizoHandVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmBombTorizoHandVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName,
            Step("Extracting Draygon cannon visuals...", (bus, d) => RoomPlmDraygonCannonVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmDraygonCannonVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName,
            Step("Extracting Chozo statue terrain visuals...", (bus, d) => RoomPlmChozoStatueVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmChozoStatueVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName,
            Step("Extracting linked-block restoration visuals...", (bus, d) => RoomPlmLinkedRestoreVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmLinkedRestoreVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName,
            Step("Extracting Tourian access-floor visuals...", (bus, d) => RoomPlmTourianAccessVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmTourianAccessVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName,
            Step("Extracting Speed Booster bomb-reveal visuals...", (bus, d) => RoomPlmSpeedBoosterVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmSpeedBoosterVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName,
            Step("Extracting Maridia elevatube visuals...", (bus, d) => RoomPlmMaridiaElevatubeVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmMaridiaElevatubeVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName,
            Step("Extracting Spore Spawn ceiling visuals...", (bus, d) => RoomPlmSporeSpawnCeilingVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmSporeSpawnCeilingVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmSamusEaterVisualDirectoryName,
            Step("Extracting Samus Eater plant visuals...", (bus, d) => RoomPlmSamusEaterVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmSamusEaterVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName,
            Step("Extracting Botwoon wall visuals...", (bus, d) => RoomPlmBotwoonWallVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmBotwoonWallVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmKraidVisualDirectoryName,
            Step("Extracting Kraid room-object visuals...", (bus, d) => RoomPlmKraidVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmKraidVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName,
            Step("Extracting Crocomire room-object visuals...", (bus, d) => RoomPlmCrocomireVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmCrocomireVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName,
            Step("Extracting Mother Brain fake-death room visuals...", (bus, d) => RoomPlmMotherBrainFakeDeathVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmMotherBrainFakeDeathVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName,
            Step("Extracting collectible PLM visuals...", (bus, d) => RoomPlmCollectibleVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmCollectibleVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName,
            Step("Extracting permanent-item tile PNGs...", (bus, d) => RoomPlmDynamicCollectibleArtFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmDynamicCollectibleArtFiles.ValidateStock)),
        new(GameInstallationLayout.XrayRevealVisualDirectoryName,
            Step("Extracting X-ray reveal visuals...", (bus, d) => XrayRevealVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), XrayRevealVisualFiles.ValidateStock)),
    ];

    /// <summary>Content failures a repair rebuilds from the ROM, rather than errors to surface.</summary>
    private static bool IsRepairableContentFailure(Exception error) =>
        error is IOException or InvalidDataException or JsonException or InvalidOperationException;

    /// <summary>
    /// Validates installed content once. Returns the components whose files can be kept by a
    /// repair, and whether the whole installation (receipt, room index and every component)
    /// is complete. A receipt from another format or ROM keeps nothing.
    /// </summary>
    private static HashSet<InstallerComponent> InspectInstalledComponents(GameInstallation installation, out bool complete)
    {
        var valid = new HashSet<InstallerComponent>();
        complete = false;
        bool receiptAndIndexValid;
        try
        {
            ValidateReceipt(installation);
            receiptAndIndexValid = true;
        }
        catch (Exception error) when (IsRepairableContentFailure(error))
        {
            receiptAndIndexValid = false;
            if (!ReceiptMatchesFormatAndSource(installation)) return valid;
        }
        foreach (InstallerComponent component in Components)
        {
            try
            {
                component.Validate(installation.ContentDirectory);
                valid.Add(component);
            }
            catch (Exception error) when (IsRepairableContentFailure(error)) { }
        }
        complete = receiptAndIndexValid && valid.Count == Components.Length;
        return valid;
    }

    /// <summary>True when the receipt names this format and source, so its components may be reused.</summary>
    private static bool ReceiptMatchesFormatAndSource(GameInstallation installation)
    {
        try
        {
            string receiptPath = Path.Combine(installation.ContentDirectory, GameInstallationLayout.ReceiptFileName);
            var receipt = JsonSerializer.Deserialize<InstallationReceipt>(File.ReadAllText(receiptPath));
            return receipt is not null && receipt.FormatVersion == GameInstallationLayout.FormatVersion &&
                receipt.RomSha256 == SupportedCartridge.Sha256;
        }
        catch (Exception error) when (IsRepairableContentFailure(error)) { return false; }
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (string directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(destination, Path.GetRelativePath(source, file)));
    }
}
