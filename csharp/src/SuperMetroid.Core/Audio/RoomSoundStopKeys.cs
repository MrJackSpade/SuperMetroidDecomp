namespace SuperMetroid.Core.Audio;

/// <summary>
/// The installed key of each <see cref="RoomSoundStop"/> (#2580), written out so renaming a member never changes a name
/// the installation spells it by.
/// </summary>
internal static class RoomSoundStopKeys
{
    /// <summary>Each RoomSoundStop member's installed key.</summary>
    internal static InstalledKeys<RoomSoundStop> Installed { get; } = new(new Dictionary<RoomSoundStop, string>
    {
        [RoomSoundStop.Any] = "Any",
        [RoomSoundStop.SpeedBoosterOrElevator] = "SpeedBoosterOrElevator",
    });
}
