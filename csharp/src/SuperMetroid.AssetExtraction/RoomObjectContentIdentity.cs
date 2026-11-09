using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Rooms.Doors;
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

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectBotwoonWallVisuals"/>.</summary>
    extension(InstalledRoomObjectBotwoonWallVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledRoomObjectBotwoonWallVisuals), visuals.Images, RoomObjectBotwoonWallVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectCollectibleVisuals"/>.</summary>
    extension(InstalledRoomObjectCollectibleVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectCollectibleVisuals), content =>
        {
            Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[1];
            foreach (var frame in RoomObjectCollectibleVisualFiles.Editable.OrderBy(frame => frame.Id, StringComparer.Ordinal))
            {
                content.AppendName("frame", frame.Id);
                content.Append("runs", 1);
                buffer[0] = visuals.GetAppearance(RoomObjectDraw.Of(frame.Draw));
                content.AppendAppearances("blocks", buffer);
            }
        });
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
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectDownwardGateVisuals), content =>
        {
            Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[5];
            foreach (var frame in RoomObjectDownwardGateVisualFiles.All.OrderBy(item => RoomObjectDownwardGateVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                DownwardGateObjectDraws.TryDescribe(frame.Draw, out var draw);
                content.AppendName("frame", RoomObjectDownwardGateVisualFiles.VisualId(frame.Draw));
                content.Append("runs", draw.RunCount);
                for (int run = 0; run < draw.RunCount; run++)
                {
                    int count = draw.BlockCount(run);
                    for (int cell = 0; cell < count; cell++) buffer[cell] = visuals.GetAppearance(frame.Draw, run, cell);
                    content.AppendAppearances("blocks", buffer[..count]);
                }
            }
        });
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
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectGrappleBlockVisuals), content =>
        {
            Span<RoomBlockAppearance> block = stackalloc RoomBlockAppearance[1];
            foreach (var draw in RoomObjectGrappleBlockVisualFiles.All.OrderBy(draw => RoomObjectGrappleBlockVisualFiles.VisualId(draw.Frame), StringComparer.Ordinal))
            {
                content.AppendName("frame", RoomObjectGrappleBlockVisualFiles.VisualId(draw.Frame));
                content.Append("runs", 1);
                block[0] = visuals.GetAppearance(RoomObjectDraw.Of(draw.Frame));
                content.AppendAppearances("blocks", block);
            }
        });
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
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectShotBlockVisuals), content =>
        {
            Span<RoomBlockAppearance> row = stackalloc RoomBlockAppearance[2];
            foreach (var draw in RoomObjectShotBlockVisualFiles.Calculated.OrderBy(RoomObjectShotBlockVisualFiles.VisualId, StringComparer.Ordinal))
            {
                content.AppendName("frame", RoomObjectShotBlockVisualFiles.VisualId(draw));
                content.Append("runs", draw.RunCount);
                for (int run = 0; run < draw.RunCount; run++)
                {
                    for (int block = 0; block < draw.BlocksPerRun; block++)
                        row[block] = visuals.GetAppearance(RoomObjectDraw.Of(draw), run, block);
                    content.AppendAppearances("blocks", row[..draw.BlocksPerRun]);
                }
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectStationVisuals"/>.</summary>
    extension(InstalledRoomObjectStationVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectStationVisuals), content =>
        {
            Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[2];
            foreach (var frame in RoomObjectStationVisualFiles.All.OrderBy(item => RoomObjectStationVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                var shape = RoomObjectStationVisualFiles.Describe(frame.Draw);
                content.AppendName("frame", RoomObjectStationVisualFiles.VisualId(frame.Draw));
                content.Append("runs", shape.RunCount);
                for (int run = 0; run < shape.RunCount; run++)
                {
                    int count = shape.BlockCount(run);
                    for (int cell = 0; cell < count; cell++) buffer[cell] = visuals.GetAppearance(frame.Draw, run, cell);
                    content.AppendAppearances("blocks", buffer[..count]);
                }
            }
        });
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

    /// <summary>The installed content identity of a <see cref="RoomObjectCrocomireVisualCatalog"/>.</summary>
    extension(RoomObjectCrocomireVisualCatalog visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(RoomObjectCrocomireVisualCatalog), visuals.Images, RoomObjectCrocomireVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="InstalledDraygonCannonFrames"/>.</summary>
    extension(InstalledDraygonCannonFrames visuals)
    {
        /// <summary>Hashes every cannon frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(InstalledDraygonCannonFrames), visuals.Images, RoomObjectDraygonCannonVisualFiles.Family);
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

    /// <summary>The installed content identity of a <see cref="RoomObjectSporeSpawnCeilingVisualCatalog"/>.</summary>
    extension(RoomObjectSporeSpawnCeilingVisualCatalog visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(RoomObjectSporeSpawnCeilingVisualCatalog), visuals.Images, RoomObjectSporeSpawnCeilingVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectBombTorizoHandVisualCatalog"/>.</summary>
    extension(RoomObjectBombTorizoHandVisualCatalog visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(RoomObjectBombTorizoHandVisualCatalog), visuals.Images, RoomObjectBombTorizoHandVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectChozoStatueVisualCatalog"/>.</summary>
    extension(RoomObjectChozoStatueVisualCatalog visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity(nameof(RoomObjectChozoStatueVisualCatalog), visuals.Images, RoomObjectChozoStatueVisualFiles.Family);
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectElevatorPlatformVisualCatalog"/>.</summary>
    extension(RoomObjectElevatorPlatformVisualCatalog visuals)
    {
        /// <summary>Hashes each frame's selected appearances run by run, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomObjectElevatorPlatformVisualCatalog), content =>
            {
                var buffer = new RoomBlockAppearance[4];
                foreach (var frame in RoomObjectElevatorPlatformVisualFiles.DrawLists.OrderBy(item => RoomObjectElevatorPlatformVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    ElevatorPlatformDraws.TryDescribe(frame.Draw, out var shape);
                    content.AppendName("frame", RoomObjectElevatorPlatformVisualFiles.VisualId(frame.Draw));
                    content.Append("runs", shape.RunCount);
                    for (int run = 0; run < shape.RunCount; run++)
                    {
                        int count = shape.BlockCount(run);
                        for (int cell = 0; cell < count; cell++)
                            buffer[cell] = visuals.GetAppearance(frame.Draw, run, cell);
                        content.AppendAppearances("blocks", buffer.AsSpan(0, count));
                    }
                }
            });
    }
}
