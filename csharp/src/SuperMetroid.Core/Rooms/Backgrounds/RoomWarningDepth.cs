namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>The depth a room's warning shows at (#2573), covering what that depth shows behind it within its pictures.</summary>
public enum RoomWarningDepth
{
    /// <summary>At the foreground's depth, as Ceres's self-destruct warning shows.</summary>
    WithForeground,

    /// <summary>At the background's depth, as Zebes's time-bomb warning shows.</summary>
    WithBackground,
}
