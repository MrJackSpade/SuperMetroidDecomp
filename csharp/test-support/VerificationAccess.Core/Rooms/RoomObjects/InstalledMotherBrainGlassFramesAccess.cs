using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to the repository installation's <see cref="InstalledMotherBrainGlassFrames"/>.</summary>
internal static class InstalledMotherBrainGlassFramesAccess
{
    /// <summary>The installed pictures of Mother Brain's glass frames.</summary>
    extension(InstalledMotherBrainGlassFrames)
    {
        /// <summary>The pictures the repository installation holds, as installed.</summary>
        internal static InstalledMotherBrainGlassFrames Stock() => RepositoryInstallation.Installation.LoadRoomObjectMotherBrainGlassVisuals();
    }
}
