using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.Draygon.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to the repository installation's <see cref="InstalledDraygonCannonFrames"/>.</summary>
internal static class InstalledDraygonCannonFramesAccess
{
    /// <summary>The installed pictures of Draygon's cannon frames.</summary>
    extension(InstalledDraygonCannonFrames)
    {
        /// <summary>The pictures the repository installation holds, as installed.</summary>
        internal static InstalledDraygonCannonFrames Stock() => RepositoryInstallation.Installation.LoadRoomObjectDraygonCannonVisuals();
    }
}
