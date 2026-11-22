using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// The content identities of every installed artwork domain in a bound <see cref="GameArtwork"/> set: room, Samus,
/// room PLM, gameplay, ending, intro and enemy art. Hosts append these to the map, audio and projectile identities.
/// </summary>
public static partial class InstalledArtworkIdentity
{
    /// <summary>
    /// Fingerprints the bound content (#1616, #1693): the artwork set the host bound, the enemy directory and the Samus body
    /// it was bound from. Nothing is read here: every identity is taken from content the host already loaded to bind the
    /// game, and every catalog must be bound.
    /// </summary>
    /// <param name="artwork">The bound artwork set.</param>
    /// <param name="enemies">The installed enemy directory the set's enemy frames and Kraid's colours were bound from.</param>
    /// <param name="samusBody">The Samus body the host loaded once and bound the set's body catalog from.</param>
    /// <remarks>
    /// The tilesets' colours and the starting and common sprite colours are palette documents only the import reads, to
    /// pre-colour the sheets the game shows (#1544), and the character pages are import data the installation never holds
    /// (#1636); the sheets' identities stand for them, and no host loads them.
    /// </remarks>
    public static IEnumerable<KeyValuePair<string, string>> Create(GameArtwork artwork, InstalledEnemyArtwork enemies,
        InstalledSamusBody samusBody)
    {
        Ensure.NotNull(artwork);
        Ensure.NotNull(enemies);
        Ensure.NotNull(samusBody);
        if (!ReferenceEquals(artwork.SamusBodyArt, samusBody.Gameplay))
            throw new InvalidOperationException("The installed artwork set's Samus body was not bound from the body whose identity is asked for.");
        return RoomPresentationIdentity.Create(artwork.RoomVisualLayouts)
            .Append(KeyValuePair.Create(GameInstallationLayout.SamusBodyDirectoryName, samusBody.ContentIdentity))
            .Concat(RoomObjectPresentationIdentity.Create(new RoomObjectVisualCatalogSet
            {
                ShotBlock = artwork.RoomObjectShotBlockVisuals,
                GrappleBlock = artwork.RoomObjectGrappleBlockVisuals,
                Station = artwork.RoomObjectStationVisuals,
                BlueDoor = artwork.RoomObjectBlueDoorVisuals,
                ColoredDoor = artwork.RoomObjectColoredDoorVisuals,
                GreyDoor = artwork.RoomObjectGreyDoorVisuals,
                EyeDoor = artwork.RoomObjectEyeDoorVisuals,
                MotherBrainGlass = artwork.RoomObjectMotherBrainGlassVisuals,
                NoobTube = artwork.RoomObjectNoobTubeVisuals,
                DownwardGate = artwork.RoomObjectDownwardGateVisuals,
                ElevatorPlatform = artwork.RoomObjectElevatorPlatformVisuals,
                EscapeGate = artwork.RoomObjectEscapeGateVisuals,
                BombTorizoHand = artwork.RoomObjectBombTorizoHandVisuals,
                DraygonCannon = artwork.RoomObjectDraygonCannonVisuals,
                PhantoonDoor = artwork.RoomObjectPhantoonDoorVisuals,
                ChozoStatue = artwork.RoomObjectChozoStatueVisuals,
                LinkedRestore = artwork.RoomObjectLinkedRestoreVisuals,
                TourianAccess = artwork.RoomObjectTourianAccessVisuals,
                SpeedBooster = artwork.RoomObjectSpeedBoosterVisuals,
                MaridiaElevatube = artwork.RoomObjectMaridiaElevatubeVisuals,
                SporeSpawnCeiling = artwork.RoomObjectSporeSpawnCeilingVisuals,
                SamusEater = artwork.RoomObjectSamusEaterVisuals,
                BotwoonWall = artwork.RoomObjectBotwoonWallVisuals,
                Kraid = artwork.RoomObjectKraidVisuals,
                Crocomire = artwork.RoomObjectCrocomireVisuals,
                MotherBrainFakeDeath = artwork.RoomObjectMotherBrainFakeDeathVisuals,
                Collectible = artwork.RoomObjectCollectibleVisuals,
                RevealAndWall = artwork.RoomObjectRevealAndWallVisuals,
                SetupBlocks = artwork.RoomObjectSetupBlocks,
            }))
            .Concat(GameplayPresentationIdentity.Create(artwork.XrayRevealVisuals))
            .Concat(EndingPresentationIdentity.Create(artwork.EndingBackdrops))
            .Append(KeyValuePair.Create(GameInstallationLayout.IntroCinematicDirectoryName, artwork.IntroCinematicArt.ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.SpriteSheetDirectoryName, artwork.SpriteSheets.ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.EnemySheetDirectoryName, artwork.EnemySheets.ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.RoomBlockSheetDirectoryName, artwork.RoomBlockSheets.ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.BossBodySheetDirectoryName, artwork.BossBodySheets.ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.EnemyTileDirectoryName, enemies.DirectoryIdentity));
    }
}
