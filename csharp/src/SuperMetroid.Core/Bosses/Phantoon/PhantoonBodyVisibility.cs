namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>How Phantoon's body shows in his room.</summary>
public enum PhantoonBodyVisibility
{
    /// <summary>His body shows as the room shows it.</summary>
    Shown,

    /// <summary>His body is hidden.</summary>
    Hidden,

    /// <summary>
    /// His body is translucent: it lies behind the room and brightens the room's front, the actors that blend and the
    /// backdrop wherever it shows, without covering the room's front.
    /// </summary>
    Translucent,
}
