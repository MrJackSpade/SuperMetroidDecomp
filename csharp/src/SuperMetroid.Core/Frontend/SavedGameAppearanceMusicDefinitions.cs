namespace SuperMetroid.Core.Frontend;

/// <summary>The music a saved game's appearance schedules as it loads (#2057).</summary>
internal static partial class SavedGameAppearanceMusicDefinitions
{
    /// <summary>How long Samus's fanfare waits on the music queue: fourteen passes.</summary>
    internal static MusicQueueDelay FanfareDelay { get; } = new(14);

    /// <summary>How long the room's track waits on the music queue behind the fanfare: 365 passes.</summary>
    internal static MusicQueueDelay RoomTrackDelay { get; } = new(365);
}
