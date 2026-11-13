using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction;

/// <summary>The installed content identities of the room objects' visual catalogs, which only the import pipeline and its diagnostics compare (#1502).</summary>
internal static class RoomObjectContentIdentity
{
    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectBlueDoorVisuals"/>.</summary>
    extension(InstalledRoomObjectBlueDoorVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectBlueDoorVisuals), visuals.Images, RoomObjectBlueDoorVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectRevealAndWallVisuals"/>.</summary>
    extension(InstalledRoomObjectRevealAndWallVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectRevealAndWallVisuals), visuals.Images, RoomObjectRevealAndWallVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectSetupBlocks"/>.</summary>
    extension(InstalledRoomObjectSetupBlocks blocks)
    {
        /// <summary>Hashes what every setup block shows, in published-name order (#1580).</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectSetupBlocks), content =>
        {
            Span<RoomBlockAppearance> shown = stackalloc RoomBlockAppearance[1];
            foreach (RoomObjectSetupBlock block in Enum.GetValues<RoomObjectSetupBlock>().OrderBy(RoomObjectSetupBlockFiles.Id, StringComparer.Ordinal))
            {
                content.AppendName("block", RoomObjectSetupBlockFiles.Id(block));
                shown[0] = blocks.Shown(block);
                content.AppendAppearances("shows", shown);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectBotwoonWallVisuals"/>.</summary>
    extension(InstalledRoomObjectBotwoonWallVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectBotwoonWallVisuals), visuals.Images, RoomObjectBotwoonWallVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectCollectibleVisuals"/>.</summary>
    extension(InstalledRoomObjectCollectibleVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectCollectibleVisuals), visuals.Images, RoomObjectCollectibleVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectColoredDoorVisuals"/>.</summary>
    extension(InstalledRoomObjectColoredDoorVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectColoredDoorVisuals), visuals.Images, RoomObjectColoredDoorVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectDownwardGateVisuals"/>.</summary>
    extension(InstalledRoomObjectDownwardGateVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectDownwardGateVisuals), visuals.Images, RoomObjectDownwardGateVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectEyeDoorVisuals"/>.</summary>
    extension(InstalledRoomObjectEyeDoorVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectEyeDoorVisuals), visuals.Images, RoomObjectEyeDoorVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectGrappleBlockVisuals"/>.</summary>
    extension(InstalledRoomObjectGrappleBlockVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectGrappleBlockVisuals), visuals.Images, RoomObjectGrappleBlockVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectGreyDoorVisuals"/>.</summary>
    extension(InstalledRoomObjectGreyDoorVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectGreyDoorVisuals), visuals.Images, RoomObjectGreyDoorVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectNoobTubeVisuals"/>.</summary>
    extension(InstalledRoomObjectNoobTubeVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectNoobTubeVisuals), visuals.Images, RoomObjectNoobTubeVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectSamusEaterVisuals"/>.</summary>
    extension(InstalledRoomObjectSamusEaterVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectSamusEaterVisuals), visuals.Images, RoomObjectSamusEaterVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectShotBlockVisuals"/>.</summary>
    extension(InstalledRoomObjectShotBlockVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectShotBlockVisuals), visuals.Images, RoomObjectShotBlockVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectStationVisuals"/>.</summary>
    extension(InstalledRoomObjectStationVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectStationVisuals), visuals.Images, RoomObjectStationVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectKraidVisuals"/>.</summary>
    extension(InstalledRoomObjectKraidVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectKraidVisuals), visuals.Images, RoomObjectKraidVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectLinkedRestoreVisuals"/>.</summary>
    extension(InstalledRoomObjectLinkedRestoreVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectLinkedRestoreVisuals), visuals.Images, RoomObjectLinkedRestoreVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectMaridiaElevatubeVisuals"/>.</summary>
    extension(InstalledRoomObjectMaridiaElevatubeVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectMaridiaElevatubeVisuals), visuals.Images, RoomObjectMaridiaElevatubeVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectSpeedBoosterVisuals"/>.</summary>
    extension(InstalledRoomObjectSpeedBoosterVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectSpeedBoosterVisuals), visuals.Images, RoomObjectSpeedBoosterVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectTourianAccessVisuals"/>.</summary>
    extension(RoomObjectTourianAccessVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity("RoomObjectTourianAccessVisualCatalog", visuals.Images, RoomObjectTourianAccessVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectCrocomireVisuals"/>.</summary>
    extension(InstalledRoomObjectCrocomireVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectCrocomireVisuals), visuals.Images, RoomObjectCrocomireVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledDraygonCannonFrames"/>.</summary>
    extension(InstalledDraygonCannonFrames visuals)
    {
        /// <summary>Hashes every cannon frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledDraygonCannonFrames), visuals.Images, RoomObjectDraygonCannonVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledPhantoonDoorFrames"/>.</summary>
    extension(InstalledPhantoonDoorFrames visuals)
    {
        /// <summary>Hashes every door frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledPhantoonDoorFrames), visuals.Images, RoomObjectPhantoonDoorVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledEscapeGateFrames"/>.</summary>
    extension(InstalledEscapeGateFrames visuals)
    {
        /// <summary>Hashes every gate frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledEscapeGateFrames), visuals.Images, RoomObjectEscapeGateVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledMotherBrainFakeDeathFrames"/>.</summary>
    extension(InstalledMotherBrainFakeDeathFrames visuals)
    {
        /// <summary>Hashes every fake-death room frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledMotherBrainFakeDeathFrames), visuals.Images, RoomObjectMotherBrainFakeDeathVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledMotherBrainGlassFrames"/>.</summary>
    extension(InstalledMotherBrainGlassFrames visuals)
    {
        /// <summary>Hashes every glass frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledMotherBrainGlassFrames), visuals.Images, RoomObjectMotherBrainGlassVisualFiles.Family);
    }

    /// <summary>
    /// Hashes a room object's installed frame pictures (#1557): each frame in published-name order, then each of its cells
    /// top to bottom and left to right, with its place and what it shows.
    /// </summary>
    private static string FrameImagesIdentity<TFrame>(string domain, RoomObjectFrameImages<TFrame> images, RoomObjectFrameFamily<TFrame> family)
        where TFrame : notnull =>
        SelectedPresentationHash.Create(domain, content =>
        {
            Span<RoomBlockAppearance> shown = stackalloc RoomBlockAppearance[1];
            foreach (KeyValuePair<TFrame, string> frame in family.Ids.OrderBy(pair => pair.Value, StringComparer.Ordinal))
            {
                RoomObjectFrameImage image = images.Image(frame.Key);
                content.AppendName("frame", frame.Value);
                content.Append("cells", image.Cells.Count);
                foreach (KeyValuePair<Offset16<RoomBlocks>, RoomBlockAppearance> cell in image.Cells.OrderBy(pair => pair.Key.Y).ThenBy(pair => pair.Key.X))
                {
                    content.Append("x", cell.Key.X);
                    content.Append("y", cell.Key.Y);
                    shown[0] = cell.Value;
                    content.AppendAppearances("block", shown);
                }
            }
        });

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectSporeSpawnCeilingVisuals"/>.</summary>
    extension(InstalledRoomObjectSporeSpawnCeilingVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectSporeSpawnCeilingVisuals), visuals.Images, RoomObjectSporeSpawnCeilingVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectBombTorizoHandVisuals"/>.</summary>
    extension(InstalledRoomObjectBombTorizoHandVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectBombTorizoHandVisuals), visuals.Images, RoomObjectBombTorizoHandVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectChozoStatueVisuals"/>.</summary>
    extension(InstalledRoomObjectChozoStatueVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectChozoStatueVisuals), visuals.Images, RoomObjectChozoStatueVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectElevatorPlatformVisuals"/>.</summary>
    extension(InstalledRoomObjectElevatorPlatformVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectElevatorPlatformVisuals), visuals.Images, RoomObjectElevatorPlatformVisualFiles.Family);
    }
}
