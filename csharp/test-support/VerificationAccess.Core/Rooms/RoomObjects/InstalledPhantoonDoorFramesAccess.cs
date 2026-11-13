using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to the repository installation's <see cref="InstalledPhantoonDoorFrames"/>.</summary>
internal static class InstalledPhantoonDoorFramesAccess
{
    /// <summary>The installed pictures of Phantoon's door frames.</summary>
    extension(InstalledPhantoonDoorFrames)
    {
        /// <summary>The pictures the repository installation holds, as installed.</summary>
        internal static InstalledPhantoonDoorFrames Stock() => RepositoryInstallation.Installation.LoadRoomObjectPhantoonDoorVisuals();
    }
}
