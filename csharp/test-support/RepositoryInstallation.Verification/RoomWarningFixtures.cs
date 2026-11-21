using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Fresh room warnings for checks that build Ceres Ridley's or Mother Brain's encounter outside a room (#2554): each is a
/// room's warning at its depth, showing from an installed warning picture sheet, with nothing written yet.
/// </summary>
internal static class RoomWarningFixtures
{
    /// <summary>An installed warning picture sheet.</summary>
    private static WrittenPictureSheet Sheet => InstalledFixtures.RoomAssets.BlockSheets.Written.Values.First();

    /// <summary>A fresh foreground warning, as Ceres Ridley's room gives him.</summary>
    internal static RoomWarning Ceres() => new RoomWarnings(Sheet).WithForeground;

    /// <summary>A fresh background warning, as Mother Brain's room gives her.</summary>
    internal static RoomWarning Zebes() => new RoomWarnings(Sheet).WithBackground;
}
