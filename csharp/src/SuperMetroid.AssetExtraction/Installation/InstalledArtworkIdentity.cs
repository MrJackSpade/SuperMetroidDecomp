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
    public static IEnumerable<KeyValuePair<string, string>> Create(GameArtwork artwork)
    {
        Ensure.NotNull(artwork);
        return RoomPresentationIdentity.Create(Require(artwork.RoomCharacterArt), Require(artwork.RoomPaletteArt),
                Require(artwork.RoomMetatileArt), Require(artwork.RoomBackgroundTilemapArt), Require(artwork.RoomSkyTilemapArt),
                Require(artwork.RoomVisualLayouts))
            .Append(KeyValuePair.Create(GameInstallationLayout.SamusBodyDirectoryName, Require(artwork.SamusBodyArt).ContentIdentity))
            .Concat(RoomPlmPresentationIdentity.Create(new RoomPlmVisualCatalogSet
            {
                ShotBlock = Require(artwork.RoomPlmShotBlockVisuals),
                GrappleBlock = Require(artwork.RoomPlmGrappleBlockVisuals),
                Station = Require(artwork.RoomPlmStationVisuals),
                BlueDoor = Require(artwork.RoomPlmBlueDoorVisuals),
                ColoredDoor = Require(artwork.RoomPlmColoredDoorVisuals),
                GreyDoor = Require(artwork.RoomPlmGreyDoorVisuals),
                EyeDoor = Require(artwork.RoomPlmEyeDoorVisuals),
                MotherBrainGlass = Require(artwork.RoomPlmMotherBrainGlassVisuals),
                NoobTube = Require(artwork.RoomPlmNoobTubeVisuals),
                DownwardGate = Require(artwork.RoomPlmDownwardGateVisuals),
                ElevatorPlatform = Require(artwork.RoomPlmElevatorPlatformVisuals),
                EscapeGate = Require(artwork.RoomPlmEscapeGateVisuals),
                BombTorizoHand = Require(artwork.RoomPlmBombTorizoHandVisuals),
                DraygonCannon = Require(artwork.RoomPlmDraygonCannonVisuals),
                ChozoStatue = Require(artwork.RoomPlmChozoStatueVisuals),
                LinkedRestore = Require(artwork.RoomPlmLinkedRestoreVisuals),
                TourianAccess = Require(artwork.RoomPlmTourianAccessVisuals),
                SpeedBooster = Require(artwork.RoomPlmSpeedBoosterVisuals),
                MaridiaElevatube = Require(artwork.RoomPlmMaridiaElevatubeVisuals),
                SporeSpawnCeiling = Require(artwork.RoomPlmSporeSpawnCeilingVisuals),
                SamusEater = Require(artwork.RoomPlmSamusEaterVisuals),
                BotwoonWall = Require(artwork.RoomPlmBotwoonWallVisuals),
                Kraid = Require(artwork.RoomPlmKraidVisuals),
                Crocomire = Require(artwork.RoomPlmCrocomireVisuals),
                MotherBrainFakeDeath = Require(artwork.RoomPlmMotherBrainFakeDeathVisuals),
                Collectible = Require(artwork.RoomPlmCollectibleVisuals),
                DynamicCollectibleArt = Require(artwork.RoomPlmDynamicCollectibleArt),
            }))
            .Concat(GameplayPresentationIdentity.Create(Require(artwork.GameplayBasePalettes), Require(artwork.StandardObjectArt),
                Require(artwork.XrayRevealVisuals)))
            .Concat(EndingPresentationIdentity.Create(Require(artwork.EndingFloorArt), Require(artwork.EndingObjectArt),
                Require(artwork.EndingPaletteArt)))
            .Append(KeyValuePair.Create(GameInstallationLayout.IntroCinematicDirectoryName, Require(artwork.IntroCinematicArt).ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.SpriteSheetDirectoryName, Require(artwork.SpriteSheets).ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.EnemySheetDirectoryName, Require(artwork.EnemySheets).ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.RoomBlockSheetDirectoryName, Require(artwork.RoomBlockSheets).ContentIdentity))
            .Append(KeyValuePair.Create(GameInstallationLayout.EnemyTileDirectoryName, new InstalledEnemyArtwork(
                Require(artwork.EnemyTileArtwork), Require(artwork.BossArtwork), Require(artwork.AreaArtwork)).ContentIdentity));
    }

    /// <summary>Fails identity collection when a bound installation omits any required artwork catalog.</summary>
    private static T Require<T>(T? catalog) where T : class =>
        catalog ?? throw new InvalidOperationException($"The installed artwork set has no {typeof(T).Name}.");
}
