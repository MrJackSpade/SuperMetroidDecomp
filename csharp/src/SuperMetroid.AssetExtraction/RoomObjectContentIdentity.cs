using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction;

/// <summary>The installed content identities of the room objects' visual catalogs, which only the import pipeline and its diagnostics compare (#1502).</summary>
internal static partial class RoomObjectContentIdentity
{
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

    /// <summary>The installed content identity of a <see cref="RoomObjectTourianAccessVisuals"/>.</summary>
    extension(RoomObjectTourianAccessVisuals visuals)
    {
        /// <summary>Hashes every frame's picture, in published-name order.</summary>
        public string ContentIdentity => FrameImagesIdentity("RoomObjectTourianAccessVisualCatalog", visuals.Images, RoomObjectTourianAccessVisualFiles.Family);
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

    /// <summary>The installed content identity of an <see cref="InstalledRoomObjectPictures"/> (#2557).</summary>
    extension(InstalledRoomObjectPictures pictures)
    {
        /// <summary>
        /// Hashes every frame's picture of the family <paramref name="family"/> lists, in published-name order, under the
        /// family's installed directory, <paramref name="directory"/>.
        /// </summary>
        public string ContentIdentity(string directory, RoomObjectFrameFamily<RoomObjectDraw> family) =>
            FrameImagesIdentity(directory, pictures.Images, family);
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

}
