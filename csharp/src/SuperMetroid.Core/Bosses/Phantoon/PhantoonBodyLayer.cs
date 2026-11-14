namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Whether Phantoon's body is shown while he is not translucent: what his mouth asks of
/// <see cref="PhantoonBodyVisibilityEffect"/>.
/// </summary>
public enum PhantoonBodyLayer
{
    /// <summary>His body is hidden: the state a part's start and a kill leave.</summary>
    Hidden,

    /// <summary>His body is shown as the room shows it, as the wavy fade-in and the first round set.</summary>
    Shown,

    /// <summary>His body is hidden and the visibility effect ends for good, as clearing his body on death sets.</summary>
    Cleared,
}
