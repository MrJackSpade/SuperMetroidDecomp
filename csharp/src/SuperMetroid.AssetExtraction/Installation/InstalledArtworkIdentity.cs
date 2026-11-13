using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// The content identities of every installed artwork domain in a bound <see cref="GameArtwork"/> set: room, Samus,
/// room PLM, gameplay, ending, intro and enemy art. Hosts append these to the map, audio and projectile identities.
/// </summary>
public static class InstalledArtworkIdentity
{
    /// <summary>Fingerprints an already loaded set; nothing is read here. Every catalog must be bound.</summary>
    /// <param name="artwork">The bound artwork set.</param>
    /// <param name="enemies">The installed enemy directory the set's enemy frames and Kraid's colours were bound from.</param>
    /// <param name="sources">The installed artwork only the import reads, which the game binds none of (#1616).</param>
    /// <remarks>
    /// The tilesets' colours and the starting and common sprite colours are palette documents only the import reads, to
    /// pre-colour the sheets the game shows (#1544), and the character pages are import data the installation never holds
    /// (#1636); the sheets' identities stand for them, and no host loads them.
    /// </remarks>
    public static IEnumerable<KeyValuePair<string, string>> Create(GameArtwork artwork, InstalledEnemyArtwork enemies,
        InstalledArtworkSources sources)
    {
        Ensure.NotNull(artwork);
        Ensure.NotNull(enemies);
        Ensure.NotNull(sources);
        if (!ReferenceEquals(artwork.SamusBodyArt, sources.SamusBody.Gameplay))
            throw new InvalidOperationException("The installed artwork set's Samus body was not bound from the body whose identity is asked for.");
        return RoomPresentationIdentity.Create(Require(artwork.RoomVisualLayouts))
            .Append(KeyValuePair.Create(GameInstallationLayout.SamusBodyDirectoryName, sources.SamusBody.ContentIdentity))
            .Concat(RoomObjectPresentationIdentity.Create(new RoomObjectVisualCatalogSet
            {
                ShotBlock = Require(artwork.RoomObjectShotBlockVisuals),
                GrappleBlock = Require(artwork.RoomObjectGrappleBlockVisuals),
                Station = Require(artwork.RoomObjectStationVisuals),
                BlueDoor = Require(artwork.RoomObjectBlueDoorVisuals),
                ColoredDoor = Require(artwork.RoomObjectColoredDoorVisuals),
                GreyDoor = Require(artwork.RoomObjectGreyDoorVisuals),
                EyeDoor = Require(artwork.RoomObjectEyeDoorVisuals),
                MotherBrainGlass = Require(artwork.RoomObjectMotherBrainGlassVisuals),
                NoobTube = Require(artwork.RoomObjectNoobTubeVisuals),
                DownwardGate = Require(artwork.RoomObjectDownwardGateVisuals),
                ElevatorPlatform = Require(artwork.RoomObjectElevatorPlatformVisuals),
                EscapeGate = Require(artwork.RoomObjectEscapeGateVisuals),
                BombTorizoHand = Require(artwork.RoomObjectBombTorizoHandVisuals),
                DraygonCannon = Require(artwork.RoomObjectDraygonCannonVisuals),
                ChozoStatue = Require(artwork.RoomObjectChozoStatueVisuals),
                LinkedRestore = Require(artwork.RoomObjectLinkedRestoreVisuals),
                TourianAccess = Require(artwork.RoomObjectTourianAccessVisuals),
                SpeedBooster = Require(artwork.RoomObjectSpeedBoosterVisuals),
                MaridiaElevatube = Require(artwork.RoomObjectMaridiaElevatubeVisuals),
                SporeSpawnCeiling = Require(artwork.RoomObjectSporeSpawnCeilingVisuals),
                SamusEater = Require(artwork.RoomObjectSamusEaterVisuals),
                BotwoonWall = Require(artwork.RoomObjectBotwoonWallVisuals),
                Kraid = Require(artwork.RoomObjectKraidVisuals),
                Crocomire = Require(artwork.RoomObjectCrocomireVisuals),
                MotherBrainFakeDeath = Require(artwork.RoomObjectMotherBrainFakeDeathVisuals),
                Collectible = Require(artwork.RoomObjectCollectibleVisuals),
                RevealAndWall = Require(artwork.RoomObjectRevealAndWallVisuals),
                SetupBlocks = Require(artwork.RoomObjectSetupBlocks),
            }))
            .Concat(GameplayPresentationIdentity.Create(Require(artwork.XrayRevealVisuals)))
            .Concat(EndingPresentationIdentity.Create(sources.EndingObjects, Require(artwork.EndingBackdrops)))
            .Append(KeyValuePair.Create(GameInstallationLayout.IntroCinematicDirectoryName, sources.Intro.ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.SpriteSheetDirectoryName, Require(artwork.SpriteSheets).ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.EnemySheetDirectoryName, Require(artwork.EnemySheets).ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.RoomBlockSheetDirectoryName, Require(artwork.RoomBlockSheets).ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.BossBodySheetDirectoryName, Require(artwork.BossBodySheets).ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.EnemyTileDirectoryName, enemies.DirectoryIdentity));
    }

    /// <summary>Fails identity collection when a bound installation omits any required artwork catalog.</summary>
    private static T Require<T>(T? catalog) where T : class =>
        catalog ?? throw new InvalidOperationException($"The installed artwork set has no {typeof(T).Name}.");
}
